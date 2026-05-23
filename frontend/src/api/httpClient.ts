import { useAuthStore } from '../stores/authStore'
import { getApiBaseUrl } from './env'

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
}

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

export async function request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  const response = await fetch(buildUrl(path), {
    ...options,
    headers: buildHeaders(options),
    body:
      options.body === undefined || options.body instanceof FormData
        ? options.body
        : JSON.stringify(options.body),
  })

  const body = (await parseResponseBody(response)) as ApiErrorBody

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
