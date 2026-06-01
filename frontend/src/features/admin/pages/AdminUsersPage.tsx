import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'

import { ApiError } from '../../../api/httpClient'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import type { AdminUser, ResetUserPasswordResponse } from '../../../types/adminUsers'
import { adminUsersQueryKey, useAdminUsers, useResetUserPasswordMutation } from '../hooks/useAdminUsers'

export function AdminUsersPage() {
  const queryClient = useQueryClient()
  const usersQuery = useAdminUsers()
  const resetPasswordMutation = useResetUserPasswordMutation()
  const [resetResult, setResetResult] = useState<ResetUserPasswordResponse | null>(null)
  const [resetError, setResetError] = useState<string | null>(null)
  const [resettingUserId, setResettingUserId] = useState<number | null>(null)

  const users = usersQuery.data ?? []

  async function handleResetPassword(user: AdminUser) {
    const confirmed = window.confirm(`Resetar a senha de ${user.name}?`)

    if (!confirmed) {
      return
    }

    setResetResult(null)
    setResetError(null)
    setResettingUserId(user.id)

    try {
      const result = await resetPasswordMutation.mutateAsync(user.id)
      await queryClient.invalidateQueries({ queryKey: adminUsersQueryKey })
      setResetResult(result)
    } catch (error) {
      setResetError(mapAdminUserError(error))
    } finally {
      setResettingUserId(null)
    }
  }

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-5xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="admin" />
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
                Administracao
              </p>
              <h1 className="text-2xl font-semibold tracking-normal sm:text-3xl">Usuarios</h1>
              <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
                Liste participantes e gere senhas temporarias.
              </p>
            </div>
            <Link
              className="w-fit rounded-md border border-slate-300 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-900"
              to="/admin"
            >
              Voltar ao admin
            </Link>
          </div>
        </header>

        {resetResult ? (
          <section className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-900 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-100">
            <h2 className="text-base font-semibold tracking-normal">Senha temporaria gerada</h2>
            <p className="mt-1">
              {resetResult.name} ({resetResult.email})
            </p>
            <p className="mt-3 font-mono text-lg font-semibold tracking-normal">{resetResult.temporaryPassword}</p>
          </section>
        ) : null}

        {resetError ? <FeedbackMessage tone="error" message={resetError} /> : null}

        <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
          <div className="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold tracking-normal">Usuarios cadastrados</h2>
              <p className="text-sm text-slate-600 dark:text-slate-300">Nome, e-mail e manutencao de senha.</p>
            </div>
            <button
              className="w-fit rounded-md border border-slate-300 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-900"
              onClick={() => void usersQuery.refetch()}
              type="button"
            >
              Recarregar
            </button>
          </div>

          {usersQuery.isPending ? (
            <p className="mt-4 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Carregando usuarios...
            </p>
          ) : null}

          {usersQuery.isError ? <FeedbackMessage tone="error" message="Nao foi possivel carregar os usuarios." /> : null}

          {!usersQuery.isPending && !usersQuery.isError && users.length === 0 ? (
            <p className="mt-4 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Nenhum usuario cadastrado.
            </p>
          ) : null}

          {!usersQuery.isPending && !usersQuery.isError && users.length > 0 ? (
            <ul className="mt-4 grid gap-3" aria-label="Usuarios para manutencao">
              {users.map((user) => (
                <li
                  className="grid gap-3 rounded-lg border border-slate-200 p-3 dark:border-slate-800 sm:grid-cols-[1fr_auto] sm:items-center"
                  key={user.id}
                >
                  <div className="min-w-0">
                    <p className="truncate font-semibold text-slate-950 dark:text-slate-50">{user.name}</p>
                    <p className="truncate text-sm text-slate-600 dark:text-slate-300">{user.email}</p>
                    <p className="mt-1 text-xs font-medium text-slate-500 dark:text-slate-400">
                      Último login: {formatLastLogin(user.lastLoginAtUtc)}
                    </p>
                  </div>
                  <button
                    className="h-10 rounded-md bg-emerald-700 px-4 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
                    disabled={resetPasswordMutation.isPending}
                    onClick={() => void handleResetPassword(user)}
                    type="button"
                  >
                    {resettingUserId === user.id ? 'Resetando...' : 'Resetar senha'}
                  </button>
                </li>
              ))}
            </ul>
          ) : null}
        </section>
      </section>
    </main>
  )
}

function formatLastLogin(value?: string | null): string {
  if (!value) {
    return 'Sem registro'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

function FeedbackMessage({ message, tone }: { message: string; tone: 'error' | 'success' }) {
  const classes =
    tone === 'error'
      ? 'border-red-200 bg-red-50 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200'
      : 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-200'

  return <p className={`mt-3 rounded-md border px-3 py-2 text-sm font-medium ${classes}`}>{message}</p>
}

function mapAdminUserError(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.status === 403) {
      return 'Acesso restrito a administradores.'
    }

    if (error.status === 404) {
      return 'Usuario nao encontrado.'
    }
  }

  return 'Nao foi possivel resetar a senha.'
}
