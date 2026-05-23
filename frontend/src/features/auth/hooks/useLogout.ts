import { useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { useNavigate } from 'react-router-dom'

import { useAuthStore } from '../../../stores/authStore'

export function useLogout() {
  const clearSession = useAuthStore((state) => state.clearSession)
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useCallback(() => {
    clearSession()
    queryClient.clear()
    navigate('/login', { replace: true })
  }, [clearSession, navigate, queryClient])
}
