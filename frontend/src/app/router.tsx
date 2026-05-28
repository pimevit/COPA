import { createBrowserRouter, RouterProvider } from 'react-router-dom'

import { LoginPage } from '../features/auth/pages/LoginPage'
import { RegisterPage } from '../features/auth/pages/RegisterPage'
import { AdminPage } from '../features/admin/pages/AdminPage'
import { AdminUsersPage } from '../features/admin/pages/AdminUsersPage'
import { MatchesPage } from '../features/matches/pages/MatchesPage'
import { RankingPage } from '../features/ranking/pages/RankingPage'
import { RulesPage } from '../features/rules/pages/RulesPage'
import { AdminRoute } from '../routes/AdminRoute'
import { PlaceholderPage } from '../routes/PlaceholderPage'
import { ProtectedRoute } from '../routes/ProtectedRoute'
import { PublicAuthRoute } from '../routes/PublicAuthRoute'

export const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <PlaceholderPage
        title="Base do frontend"
        description="Scaffold tecnico com Tailwind, React Router, Zustand, cliente HTTP e TanStack Query."
      />
    ),
  },
  {
    element: <PublicAuthRoute />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
      },
      {
        path: '/register',
        element: <RegisterPage />,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/app',
        element: <MatchesPage />,
      },
      {
        path: '/matches',
        element: <MatchesPage />,
      },
      {
        path: '/ranking',
        element: <RankingPage />,
      },
      {
        path: '/rules',
        element: <RulesPage />,
      },
    ],
  },
  {
    element: <AdminRoute />,
    children: [
      {
        path: '/admin',
        element: <AdminPage />,
      },
      {
        path: '/admin/users',
        element: <AdminUsersPage />,
      },
    ],
  },
])

export function AppRouter() {
  return <RouterProvider router={router} />
}
