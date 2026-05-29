import { useQueryClient } from '@tanstack/react-query'

import { BetVisibilityControl } from '../../bets/components/BetVisibilityControl'
import { betVisibilityQueryKey, publicBetsQueryKey, useBetVisibility, useUpdateBetVisibilityMutation } from '../../bets/hooks/useBets'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import { selectAuthUser, useAuthStore } from '../../../stores/authStore'

export function UserSettingsPage() {
  const queryClient = useQueryClient()
  const user = useAuthStore(selectAuthUser)
  const betVisibilityQuery = useBetVisibility()
  const updateBetVisibilityMutation = useUpdateBetVisibilityMutation()
  const showBetsPublicly = betVisibilityQuery.data?.showBetsPublicly ?? true
  const visibilityError = updateBetVisibilityMutation.error ?? (betVisibilityQuery.isError ? betVisibilityQuery.error : null)

  async function handleVisibilityChange(showBetsPubliclyNext: boolean) {
    try {
      const response = await updateBetVisibilityMutation.mutateAsync({
        showBetsPublicly: showBetsPubliclyNext,
      })

      queryClient.setQueryData(betVisibilityQueryKey, response)

      if (response.showBetsPublicly) {
        await queryClient.invalidateQueries({ queryKey: publicBetsQueryKey() })
      } else {
        queryClient.removeQueries({ queryKey: publicBetsQueryKey() })
      }
    } catch {
      // The mutation exposes the error state rendered by BetVisibilityControl.
    }
  }

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-5xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="user" />
          <div>
            <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
              Bolao Copa
            </p>
            <h1 className="text-2xl font-semibold tracking-normal sm:text-3xl">Usuario</h1>
            <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
              {user?.name ?? user?.email ?? 'Configuracao da conta'}
            </p>
          </div>
        </header>

        <BetVisibilityControl
          error={visibilityError}
          isLoading={betVisibilityQuery.isPending}
          isSaving={updateBetVisibilityMutation.isPending}
          onChange={(nextValue) => void handleVisibilityChange(nextValue)}
          showBetsPublicly={showBetsPublicly}
        />
      </section>
    </main>
  )
}
