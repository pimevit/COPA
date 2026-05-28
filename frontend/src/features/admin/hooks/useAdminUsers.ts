import { useMutation, useQuery } from '@tanstack/react-query'

import { fetchAdminUsers, resetUserPassword } from '../api/adminUsersApi'

export const adminUsersQueryKey = ['admin', 'users'] as const

export function useAdminUsers() {
  return useQuery({
    queryKey: adminUsersQueryKey,
    queryFn: fetchAdminUsers,
    staleTime: 60_000,
  })
}

export function useResetUserPasswordMutation() {
  return useMutation({
    mutationFn: resetUserPassword,
  })
}
