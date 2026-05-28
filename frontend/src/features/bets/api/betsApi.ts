import { apiClient } from '../../../api/httpClient'
import type {
  BetResponse,
  BetVisibilityResponse,
  CreateBetRequest,
  MyBet,
  PublicBet,
  UpdateBetRequest,
  UpdateBetVisibilityRequest,
} from '../../../types/bets'

export function fetchMyBets(): Promise<MyBet[]> {
  return apiClient.get<MyBet[]>('/bets/me')
}

export function fetchBetVisibility(): Promise<BetVisibilityResponse> {
  return apiClient.get<BetVisibilityResponse>('/bets/visibility')
}

export function updateBetVisibility(request: UpdateBetVisibilityRequest): Promise<BetVisibilityResponse> {
  return apiClient.put<BetVisibilityResponse>('/bets/visibility', request)
}

export function fetchPublicBets(matchId?: number): Promise<PublicBet[]> {
  const query = matchId === undefined ? '' : `?matchId=${matchId}`

  return apiClient.get<PublicBet[]>(`/bets/public${query}`)
}

export function createBet(request: CreateBetRequest): Promise<BetResponse> {
  return apiClient.post<BetResponse>('/bets', request)
}

export function updateBet(id: number, request: UpdateBetRequest): Promise<BetResponse> {
  return apiClient.put<BetResponse>(`/bets/${id}`, request)
}
