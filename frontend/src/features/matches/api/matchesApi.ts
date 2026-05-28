import { apiClient } from '../../../api/httpClient'
import type {
  CreateMatchRequest,
  MatchesQuery,
  MatchListItem,
  UpdateMatchBettingLockRequest,
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

export function deleteMatch(matchId: number, options: { deleteBets: boolean }): Promise<unknown> {
  const searchParams = new URLSearchParams({ deleteBets: String(options.deleteBets) })

  return apiClient.delete(`/matches/${matchId}?${searchParams}`)
}

export function updateMatchBettingLock(matchId: number, request: UpdateMatchBettingLockRequest): Promise<unknown> {
  return apiClient.put(`/matches/${matchId}/betting-lock`, request)
}

export function updateMatchResult(matchId: number, request: UpdateMatchResultRequest): Promise<unknown> {
  return apiClient.put(`/matches/${matchId}/result`, request)
}
