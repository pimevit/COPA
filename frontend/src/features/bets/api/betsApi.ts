import { apiClient } from '../../../api/httpClient'
import type { BetResponse, CreateBetRequest, MyBet, UpdateBetRequest } from '../../../types/bets'

export function fetchMyBets(): Promise<MyBet[]> {
  return apiClient.get<MyBet[]>('/bets/me')
}

export function createBet(request: CreateBetRequest): Promise<BetResponse> {
  return apiClient.post<BetResponse>('/bets', request)
}

export function updateBet(id: number, request: UpdateBetRequest): Promise<BetResponse> {
  return apiClient.put<BetResponse>(`/bets/${id}`, request)
}
