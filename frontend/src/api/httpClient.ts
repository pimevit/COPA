import { useAuthStore } from '../stores/authStore'
import { getApiBaseUrl } from './env'
import type { AuthSession } from '../types/auth'

export type ProblemDetails = {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export type ApiErrorBody = ProblemDetails | Record<string, unknown> | string | null

export class ApiError extends Error {
  readonly status: number
  readonly body: ApiErrorBody

  constructor(status: number, body: ApiErrorBody) {
    const title = typeof body === 'object' && body && 'title' in body ? String(body.title) : undefined

    super(title || `HTTP request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.body = body
  }
}

type RequestOptions = Omit<RequestInit, 'body' | 'headers'> & {
  body?: unknown
  headers?: HeadersInit
  skipAuthRefresh?: boolean
}

let refreshSessionPromise: Promise<boolean> | null = null

async function parseResponseBody(response: Response): Promise<unknown> {
  const contentType = response.headers.get('content-type') ?? ''

  if (response.status === 204) {
    return null
  }

  if (contentType.includes('application/json') || contentType.includes('application/problem+json')) {
    return response.json()
  }

  return response.text()
}

function buildHeaders(options: RequestOptions): Headers {
  const headers = new Headers(options.headers)
  const token = useAuthStore.getState().token

  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json')
  }

  if (options.body !== undefined && !(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  return headers
}

function buildUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  const baseUrl = getApiBaseUrl().replace(/\/$/, '')
  const normalizedPath = path.startsWith('/') ? path : `/${path}`

  return `${baseUrl}${normalizedPath}`
}

async function sendRequest(path: string, options: RequestOptions): Promise<Response> {
  const { body, skipAuthRefresh, ...requestInit } = options
  void skipAuthRefresh

  return fetch(buildUrl(path), {
    ...requestInit,
    credentials: 'include',
    headers: buildHeaders(options),
    body:
      body === undefined || body instanceof FormData
        ? body
        : JSON.stringify(body),
  })
}

function shouldRefreshAuth(path: string, options: RequestOptions): boolean {
  if (options.skipAuthRefresh || !useAuthStore.getState().token) {
    return false
  }

  const normalizedPath = path.toLowerCase()

  return !/\/auth\/(login|register|refresh|logout)(?:\?|$)/.test(normalizedPath)
}

async function refreshSession(): Promise<boolean> {
  refreshSessionPromise ??= request<AuthSession>('/auth/refresh', {
    method: 'POST',
    skipAuthRefresh: true,
  })
    .then((session) => {
      useAuthStore.getState().setSession(session)
      return true
    })
    .catch(() => {
      useAuthStore.getState().clearSession()
      return false
    })
    .finally(() => {
      refreshSessionPromise = null
    })

  return refreshSessionPromise
}

async function refreshExpiringSession(path: string, options: RequestOptions): Promise<void> {
  if (!shouldRefreshAuth(path, options)) {
    return
  }

  const expiresAtUtc = useAuthStore.getState().expiresAtUtc
  const expiresAtMs = expiresAtUtc ? Date.parse(expiresAtUtc) : Number.NaN
  if (!Number.isNaN(expiresAtMs) && expiresAtMs - Date.now() <= 60_000) {
    await refreshSession()
  }
}

export async function request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  await refreshExpiringSession(path, options)

  let response = await sendRequest(path, options)
  let body = (await parseResponseBody(response)) as ApiErrorBody

  if (response.status === 401 && shouldRefreshAuth(path, options)) {
    const refreshed = await refreshSession()
    if (refreshed) {
      response = await sendRequest(path, options)
      body = (await parseResponseBody(response)) as ApiErrorBody
    }
  }

  if (response.status === 401) {
    useAuthStore.getState().clearSession()
  }

  if (!response.ok) {
    throw new ApiError(response.status, body)
  }

  return body as TResponse
}

export const apiClient = {
  get: <TResponse>(path: string, options?: RequestOptions) =>
    request<TResponse>(path, { ...options, method: 'GET' }),
  post: <TResponse>(path: string, body?: unknown, options?: RequestOptions) =>
    request<TResponse>(path, { ...options, method: 'POST', body }),
  put: <TResponse>(path: string, body?: unknown, options?: RequestOptions) =>
    request<TResponse>(path, { ...options, method: 'PUT', body }),
  delete: <TResponse>(path: string, options?: RequestOptions) =>
    request<TResponse>(path, { ...options, method: 'DELETE' }),
}
