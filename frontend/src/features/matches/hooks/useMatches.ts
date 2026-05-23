import { useQuery } from '@tanstack/react-query'

import { fetchMatches } from '../api/matchesApi'
import type { MatchesQuery } from '../../../types/matches'

export const matchesQueryKey = (query?: MatchesQuery) => ['matches', query ?? {}] as const

export function useMatches(query?: MatchesQuery) {
  return useQuery({
    queryKey: matchesQueryKey(query),
    queryFn: () => fetchMatches(query),
    staleTime: 60_000,
  })
}
