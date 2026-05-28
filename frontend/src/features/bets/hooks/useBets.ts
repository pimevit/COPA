import { useMutation, useQuery } from '@tanstack/react-query'

import {
  createBet,
  fetchBetVisibility,
  fetchMyBets,
  fetchPublicBets,
  updateBet,
  updateBetVisibility,
} from '../api/betsApi'
import type { CreateBetRequest, UpdateBetCommand, UpdateBetVisibilityRequest } from '../../../types/bets'

export const myBetsQueryKey = ['bets', 'me'] as const
export const betVisibilityQueryKey = ['bets', 'visibility'] as const
export const publicBetsQueryKey = (matchId?: number) =>
  matchId === undefined ? (['bets', 'public'] as const) : (['bets', 'public', matchId] as const)

export function useMyBets() {
  return useQuery({
    queryKey: myBetsQueryKey,
    queryFn: fetchMyBets,
    staleTime: 30_000,
  })
}

export function useBetVisibility() {
  return useQuery({
    queryKey: betVisibilityQueryKey,
    queryFn: fetchBetVisibility,
    staleTime: 30_000,
  })
}

export function usePublicBets(enabled: boolean, matchId?: number) {
  return useQuery({
    queryKey: publicBetsQueryKey(matchId),
    queryFn: () => fetchPublicBets(matchId),
    enabled,
    staleTime: 30_000,
  })
}

export function useCreateBetMutation() {
  return useMutation({
    mutationFn: (request: CreateBetRequest) => createBet(request),
  })
}

export function useUpdateBetMutation() {
  return useMutation({
    mutationFn: ({ id, request }: UpdateBetCommand) => updateBet(id, request),
  })
}

export function useUpdateBetVisibilityMutation() {
  return useMutation({
    mutationFn: (request: UpdateBetVisibilityRequest) => updateBetVisibility(request),
  })
}
