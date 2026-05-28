import { useMutation, useQuery } from '@tanstack/react-query'

import { createMatch, fetchMatches, updateMatchResult } from '../../matches/api/matchesApi'
import type { CreateMatchRequest, UpdateMatchResultRequest } from '../../../types/matches'
import {
  clearApplicationData,
  importBrasileiraoSerieA2026Teams,
  importWorldCup2026Teams,
} from '../api/adminMaintenanceApi'
import { fetchTeams } from '../api/teamsApi'

export const teamsQueryKey = ['teams'] as const
export const adminMatchesQueryKey = ['admin', 'matches'] as const

export function useAdminTeams() {
  return useQuery({
    queryKey: teamsQueryKey,
    queryFn: fetchTeams,
    staleTime: 5 * 60_000,
  })
}

export function useAdminMatches() {
  return useQuery({
    queryKey: adminMatchesQueryKey,
    queryFn: () => fetchMatches(),
    staleTime: 30_000,
  })
}

export function useCreateMatchMutation() {
  return useMutation({
    mutationFn: (request: CreateMatchRequest) => createMatch(request),
  })
}

export function useUpdateMatchResultMutation() {
  return useMutation({
    mutationFn: ({ matchId, request }: { matchId: number; request: UpdateMatchResultRequest }) =>
      updateMatchResult(matchId, request),
  })
}

export function useImportBrasileiraoTeamsMutation() {
  return useMutation({
    mutationFn: importBrasileiraoSerieA2026Teams,
  })
}

export function useImportWorldCupTeamsMutation() {
  return useMutation({
    mutationFn: importWorldCup2026Teams,
  })
}

export function useClearApplicationDataMutation() {
  return useMutation({
    mutationFn: clearApplicationData,
  })
}
