import { apiClient } from '../../../api/httpClient'

export type AdminMaintenanceResponse = {
  action: string
  insertedTeams: number
  updatedTeams: number
  deletedBets: number
  deletedMatches: number
  deletedTeams: number
}

export function importBrasileiraoSerieA2026Teams(): Promise<AdminMaintenanceResponse> {
  return apiClient.post<AdminMaintenanceResponse>('/admin/maintenance/teams/brasileirao-serie-a-2026')
}

export function importWorldCup2026Teams(): Promise<AdminMaintenanceResponse> {
  return apiClient.post<AdminMaintenanceResponse>('/admin/maintenance/teams/world-cup-2026')
}

export function clearApplicationData(): Promise<AdminMaintenanceResponse> {
  return apiClient.delete<AdminMaintenanceResponse>('/admin/maintenance/application-data')
}
