import { apiClient } from '../../../api/httpClient'
import type { AdminUser, ResetUserPasswordResponse } from '../../../types/adminUsers'

export function fetchAdminUsers(): Promise<AdminUser[]> {
  return apiClient.get<AdminUser[]>('/admin/users')
}

export function resetUserPassword(userId: number): Promise<ResetUserPasswordResponse> {
  return apiClient.post<ResetUserPasswordResponse>(`/admin/users/${userId}/reset-password`)
}
