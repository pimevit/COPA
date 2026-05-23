import { useQuery } from '@tanstack/react-query'

import { fetchRanking } from '../api/rankingApi'

export const rankingQueryKey = ['ranking'] as const
export const rankingRefetchIntervalMs = 30_000

export function useRanking() {
  return useQuery({
    queryKey: rankingQueryKey,
    queryFn: fetchRanking,
    refetchInterval: rankingRefetchIntervalMs,
    refetchIntervalInBackground: false,
    staleTime: 15_000,
  })
}
