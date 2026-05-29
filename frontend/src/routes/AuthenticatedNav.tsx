import { Link } from 'react-router-dom'

import { useLogout } from '../features/auth/hooks/useLogout'
import { selectAuthUser, selectIsAdmin, selectIsAuthenticated, useAuthStore } from '../stores/authStore'

type AuthenticatedNavProps = {
  activePage: 'matches' | 'ranking' | 'rules' | 'user' | 'admin'
}

const activeLinkClass =
  'rounded-md bg-emerald-700 px-3 py-2 text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950'

const inactiveLinkClass =
  'rounded-md px-3 py-2 text-slate-700 hover:bg-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:text-slate-200 dark:hover:bg-slate-900'

export function AuthenticatedNav({ activePage }: AuthenticatedNavProps) {
  const isAuthenticated = useAuthStore(selectIsAuthenticated)
  const isAdmin = useAuthStore(selectIsAdmin)
  const user = useAuthStore(selectAuthUser)
  const logout = useLogout()

  return (
    <nav
      className="flex flex-col gap-3 text-sm font-semibold sm:flex-row sm:items-center sm:justify-between"
      aria-label="Navegacao principal"
    >
      <div className="flex flex-wrap gap-2">
        <Link className={activePage === 'matches' ? activeLinkClass : inactiveLinkClass} to="/matches">
          Partidas
        </Link>
        <Link className={activePage === 'ranking' ? activeLinkClass : inactiveLinkClass} to="/ranking">
          Ranking
        </Link>
        <Link className={activePage === 'rules' ? activeLinkClass : inactiveLinkClass} to="/rules">
          Regras
        </Link>
        <Link className={activePage === 'user' ? activeLinkClass : inactiveLinkClass} to="/usuario">
          Usuario
        </Link>
        {isAdmin ? (
          <Link className={activePage === 'admin' ? activeLinkClass : inactiveLinkClass} to="/admin">
            Admin
          </Link>
        ) : null}
      </div>

      {isAuthenticated ? (
        <div className="flex flex-wrap items-center gap-2">
          <span className="max-w-full truncate text-slate-500 dark:text-slate-400">
            {user?.name ?? user?.email ?? 'Sessao ativa'}
          </span>
          <button
            className="rounded-md border border-slate-300 px-3 py-2 text-slate-700 hover:bg-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-900 dark:focus:ring-offset-slate-950"
            onClick={logout}
            type="button"
          >
            Sair
          </button>
        </div>
      ) : null}
    </nav>
  )
}
