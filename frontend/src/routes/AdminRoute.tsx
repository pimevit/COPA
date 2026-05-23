import { Link, Navigate, Outlet, useLocation } from 'react-router-dom'

import { selectIsAdmin, selectIsAuthenticated, useAuthStore } from '../stores/authStore'

export function AdminRoute() {
  const isAuthenticated = useAuthStore(selectIsAuthenticated)
  const isAdmin = useAuthStore(selectIsAdmin)
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (!isAdmin) {
    return (
      <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
        <section className="mx-auto flex w-full max-w-3xl flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-800 dark:bg-slate-950">
          <h1 className="text-xl font-semibold tracking-normal">Acesso restrito a administradores</h1>
          <p className="text-sm text-slate-600 dark:text-slate-300">
            Sua sessao atual nao possui permissao para acessar o painel administrativo.
          </p>
          <Link
            className="inline-flex w-fit rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
            to="/matches"
          >
            Voltar para partidas
          </Link>
        </section>
      </main>
    )
  }

  return <Outlet />
}
