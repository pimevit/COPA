import { apiClient } from '../../../api/httpClient'
import type { Team } from '../../../types/teams'

export function fetchTeams(): Promise<Team[]> {
  return apiClient.get<Team[]>('/teams')
}
