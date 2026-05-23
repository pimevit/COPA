import { useMutation, useQuery } from '@tanstack/react-query'

import { createBet, fetchMyBets, updateBet } from '../api/betsApi'
import type { CreateBetRequest, UpdateBetCommand } from '../../../types/bets'

export const myBetsQueryKey = ['bets', 'me'] as const

export function useMyBets() {
  return useQuery({
    queryKey: myBetsQueryKey,
    queryFn: fetchMyBets,
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
