import { Navigate, Outlet } from 'react-router-dom'

import { selectIsAuthenticated, useAuthStore } from '../stores/authStore'

export function PublicAuthRoute() {
  const isAuthenticated = useAuthStore(selectIsAuthenticated)

  if (isAuthenticated) {
    return <Navigate replace to="/app" />
  }

  return <Outlet />
}
