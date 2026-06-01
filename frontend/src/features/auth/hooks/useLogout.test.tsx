import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { useLogout } from './useLogout'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import { ProtectedRoute } from '../../../routes/ProtectedRoute'
import { authStorageKey, useAuthStore } from '../../../stores/authStore'
import { logout } from '../api/authApi'

vi.mock('../api/authApi', () => ({
  logout: vi.fn(),
}))

const mockedLogout = vi.mocked(logout)

const session = {
  accessToken: 'valid-token',
  expiresAtUtc: '2026-06-11T12:00:00Z',
  user: {
    id: 1,
    name: 'Felipe',
    email: 'felipe@example.com',
    createdAt: '2026-06-01T12:00:00Z',
    roles: [],
  },
}

function renderWithProviders(children: ReactNode, queryClient: QueryClient, initialEntries = ['/matches']) {
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>{children}</MemoryRouter>
    </QueryClientProvider>,
  )
}

function LogoutButton() {
  const logout = useLogout()

  return (
    <button onClick={logout} type="button">
      Logout helper
    </button>
  )
}

describe('logout flow', () => {
  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    mockedLogout.mockReset()
    mockedLogout.mockResolvedValue()
    useAuthStore.getState().clearSession()
    localStorage.clear()
  })

  it('clears session, persisted auth data and query cache when clicked from authenticated nav', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })

    useAuthStore.getState().setSession(session)
    queryClient.setQueryData(['bets', 'me'], [{ id: 10 }])

    expect(localStorage.getItem(authStorageKey)).toContain('valid-token')
    expect(queryClient.getQueryData(['bets', 'me'])).toEqual([{ id: 10 }])

    renderWithProviders(
      <Routes>
        <Route
          element={
            <>
              <AuthenticatedNav activePage="matches" />
              <div>Protected content</div>
            </>
          }
          path="/matches"
        />
        <Route element={<div>Login route</div>} path="/login" />
      </Routes>,
      queryClient,
    )

    expect(screen.getByRole('button', { name: 'Sair' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Sair' }))

    await waitFor(() => expect(screen.getByText('Login route')).toBeInTheDocument())
    expect(mockedLogout).toHaveBeenCalledTimes(1)
    expect(useAuthStore.getState().token).toBeNull()
    expect(useAuthStore.getState().user).toBeNull()
    expect(useAuthStore.getState().expiresAtUtc).toBeNull()
    expect(localStorage.getItem(authStorageKey)).toBeNull()
    expect(queryClient.getQueryData(['bets', 'me'])).toBeUndefined()
  })

  it('keeps protected routes blocked after logout', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })

    renderWithProviders(
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route element={<div>Protected content</div>} path="/matches" />
        </Route>
        <Route element={<div>Login route</div>} path="/login" />
      </Routes>,
      queryClient,
    )

    await waitFor(() => expect(screen.getByText('Login route')).toBeInTheDocument())
    expect(screen.queryByText('Protected content')).not.toBeInTheDocument()
  })

  it('allows the logout helper to run without an active session', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })

    renderWithProviders(
      <Routes>
        <Route element={<LogoutButton />} path="/logout-test" />
        <Route element={<div>Login route</div>} path="/login" />
      </Routes>,
      queryClient,
      ['/logout-test'],
    )

    fireEvent.click(screen.getByRole('button', { name: 'Logout helper' }))

    await waitFor(() => expect(screen.getByText('Login route')).toBeInTheDocument())
    expect(mockedLogout).toHaveBeenCalledTimes(1)
    expect(localStorage.getItem(authStorageKey)).toBeNull()
  })

  it('clears local session even when server logout fails', async () => {
    mockedLogout.mockRejectedValue(new Error('network'))
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })

    useAuthStore.getState().setSession(session)

    renderWithProviders(
      <Routes>
        <Route element={<LogoutButton />} path="/logout-test" />
        <Route element={<div>Login route</div>} path="/login" />
      </Routes>,
      queryClient,
      ['/logout-test'],
    )

    fireEvent.click(screen.getByRole('button', { name: 'Logout helper' }))

    await waitFor(() => expect(screen.getByText('Login route')).toBeInTheDocument())
    expect(useAuthStore.getState().token).toBeNull()
    expect(localStorage.getItem(authStorageKey)).toBeNull()
  })
})
