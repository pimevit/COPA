import { apiClient } from '../../../api/httpClient'
import type { RankingItem } from '../../../types/ranking'

export function fetchRanking(): Promise<RankingItem[]> {
  return apiClient.get<RankingItem[]>('/ranking')
}
