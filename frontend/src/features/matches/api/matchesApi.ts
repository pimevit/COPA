import { apiClient } from '../../../api/httpClient'
import type {
  CreateMatchRequest,
  MatchesQuery,
  MatchListItem,
  UpdateMatchResultRequest,
} from '../../../types/matches'

function buildMatchesPath(query?: MatchesQuery): string {
  const searchParams = new URLSearchParams()

  if (query?.stage) {
    searchParams.set('stage', query.stage)
  }

  if (query?.status) {
    searchParams.set('status', query.status)
  }

  const queryString = searchParams.toString()

  return queryString ? `/matches?${queryString}` : '/matches'
}

export function fetchMatches(query?: MatchesQuery): Promise<MatchListItem[]> {
  return apiClient.get<MatchListItem[]>(buildMatchesPath(query))
}

export function createMatch(request: CreateMatchRequest): Promise<MatchListItem> {
  return apiClient.post<MatchListItem>('/matches', request)
}

export function updateMatchResult(matchId: number, request: UpdateMatchResultRequest): Promise<unknown> {
  return apiClient.put(`/matches/${matchId}/result`, request)
}
