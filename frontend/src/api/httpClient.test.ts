import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { request } from './httpClient'
import { useAuthStore } from '../stores/authStore'
import type { AuthSession } from '../types/auth'

const refreshedSession: AuthSession = {
  accessToken: 'refreshed-token',
  expiresAtUtc: '2099-01-01T00:00:00Z',
  user: {
    id: 1,
    name: 'Felipe',
    email: 'felipe@example.com',
    createdAt: '2026-06-01T12:00:00Z',
    roles: [],
  },
}

const expiredSession: AuthSession = {
  ...refreshedSession,
  accessToken: 'expired-token',
}

function jsonResponse(body: unknown, status = 200): Promise<Response> {
  return Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  )
}

describe('httpClient auth refresh flow', () => {
  const fetchMock = vi.fn()

  beforeEach(() => {
    fetchMock.mockReset()
    vi.stubEnv('VITE_API_BASE_URL', 'http://api.test')
    vi.stubGlobal('fetch', fetchMock)
    useAuthStore.getState().clearSession()
    localStorage.clear()
  })

  afterEach(() => {
    vi.unstubAllEnvs()
    vi.unstubAllGlobals()
  })

  it('sends requests with credentials included', async () => {
    fetchMock.mockResolvedValueOnce(new Response(JSON.stringify({ ok: true }), {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    }))

    await request('/matches')

    expect(fetchMock).toHaveBeenCalledWith(
      'http://api.test/matches',
      expect.objectContaining({ credentials: 'include' }),
    )
  })

  it('refreshes the session once after a 401 and retries the original request', async () => {
    useAuthStore.getState().setSession(expiredSession)
    fetchMock
      .mockResolvedValueOnce(new Response(JSON.stringify({ title: 'Unauthorized' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }))
      .mockResolvedValueOnce(new Response(JSON.stringify(refreshedSession), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }))
      .mockResolvedValueOnce(new Response(JSON.stringify([{ id: 10 }]), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }))

    const result = await request<Array<{ id: number }>>('/bets/me')

    expect(result).toEqual([{ id: 10 }])
    expect(useAuthStore.getState().token).toBe('refreshed-token')
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      'http://api.test/auth/refresh',
      expect.objectContaining({ credentials: 'include', method: 'POST' }),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      3,
      'http://api.test/bets/me',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )
  })

  it('clears the session when refresh fails', async () => {
    useAuthStore.getState().setSession(expiredSession)
    fetchMock
      .mockResolvedValueOnce(new Response(JSON.stringify({ title: 'Unauthorized' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ title: 'Invalid credentials.' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }))

    await expect(request('/bets/me')).rejects.toMatchObject({ status: 401 })

    expect(useAuthStore.getState().token).toBeNull()
    expect(fetchMock).toHaveBeenCalledTimes(2)
  })

  it('shares a single refresh request across concurrent 401 responses', async () => {
    useAuthStore.getState().setSession(expiredSession)

    let firstAttempts = 0
    let secondAttempts = 0
    let resolveRefresh!: (response: Response) => void
    const refreshResponse = new Promise<Response>((resolve) => {
      resolveRefresh = resolve
    })

    fetchMock.mockImplementation((input: RequestInfo | URL) => {
      const url = String(input)

      if (url.endsWith('/auth/refresh')) {
        return refreshResponse
      }

      if (url.endsWith('/first') && firstAttempts === 0) {
        firstAttempts += 1
        return jsonResponse({ title: 'Unauthorized' }, 401)
      }

      if (url.endsWith('/second') && secondAttempts === 0) {
        secondAttempts += 1
        return jsonResponse({ title: 'Unauthorized' }, 401)
      }

      return jsonResponse({ ok: url.endsWith('/first') ? 'first' : 'second' })
    })

    const firstRequest = request<{ ok: string }>('/first')
    const secondRequest = request<{ ok: string }>('/second')

    await new Promise((resolve) => setTimeout(resolve, 0))

    const refreshCalls = fetchMock.mock.calls.filter(([url]) => String(url).endsWith('/auth/refresh'))
    expect(refreshCalls).toHaveLength(1)

    resolveRefresh(new Response(JSON.stringify(refreshedSession), {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    }))

    await expect(Promise.all([firstRequest, secondRequest])).resolves.toEqual([
      { ok: 'first' },
      { ok: 'second' },
    ])
  })
})
