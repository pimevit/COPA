import { useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { useNavigate } from 'react-router-dom'

import { useAuthStore } from '../../../stores/authStore'
import { logout } from '../api/authApi'

export function useLogout() {
  const clearSession = useAuthStore((state) => state.clearSession)
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useCallback(async () => {
    try {
      await logout()
    } catch {
      // Logout local deve continuar mesmo se a revogacao remota falhar.
    } finally {
      clearSession()
      queryClient.clear()
      navigate('/login', { replace: true })
    }
  }, [clearSession, navigate, queryClient])
}
