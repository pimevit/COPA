import { apiClient } from '../../../api/httpClient'
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse } from '../../../types/auth'

export function login(request: LoginRequest): Promise<LoginResponse> {
  return apiClient.post<LoginResponse>('/auth/login', request)
}

export function register(request: RegisterRequest): Promise<RegisterResponse> {
  return apiClient.post<RegisterResponse>('/auth/register', request)
}

export function logout(): Promise<void> {
  return apiClient.post<void>('/auth/logout')
}
