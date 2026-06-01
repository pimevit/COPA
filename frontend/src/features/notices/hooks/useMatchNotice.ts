import { useMutation, useQuery } from '@tanstack/react-query'

import { fetchMatchNotice, updateMatchNotice } from '../api/noticesApi'
import type { UpdateNoticeRequest } from '../../../types/notices'

export const matchNoticeQueryKey = ['notices', 'matches'] as const

export function useMatchNotice() {
  return useQuery({
    queryKey: matchNoticeQueryKey,
    queryFn: fetchMatchNotice,
    staleTime: 60_000,
  })
}

export function useUpdateMatchNoticeMutation() {
  return useMutation({
    mutationFn: (request: UpdateNoticeRequest) => updateMatchNotice(request),
  })
}
