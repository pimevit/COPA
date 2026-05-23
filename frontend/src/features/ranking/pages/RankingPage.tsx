import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import { RankingErrorState, RankingLoadingState } from '../components/RankingFeedback'
import { RankingList } from '../components/RankingList'
import { TopThreeHighlight } from '../components/TopThreeHighlight'
import { rankingRefetchIntervalMs, useRanking } from '../hooks/useRanking'

function formatLastUpdatedAt(date: Date): string {
  return new Intl.DateTimeFormat(undefined, {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).format(date)
}

export function RankingPage() {
  const { data, dataUpdatedAt, error, isError, isFetching, isPending, refetch } = useRanking()
  const ranking = data ?? []
  const intervalSeconds = rankingRefetchIntervalMs / 1000
  const lastUpdatedLabel = dataUpdatedAt > 0 ? formatLastUpdatedAt(new Date(dataUpdatedAt)) : null

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-5xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="ranking" />
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
                Bolao Copa
              </p>
              <h1 className="mt-1 text-2xl font-semibold tracking-normal sm:text-3xl">Ranking</h1>
              <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
                Classificacao global atualizada automaticamente a cada {intervalSeconds} segundos.
              </p>
            </div>
            <span className="text-sm font-medium text-slate-500 dark:text-slate-400" aria-live="polite">
              {isFetching && !isPending ? 'Atualizando...' : lastUpdatedLabel ? `Atualizado ${lastUpdatedLabel}` : ''}
            </span>
          </div>
        </header>

        {isPending ? <RankingLoadingState /> : null}

        {isError ? <RankingErrorState error={error} onRetry={() => void refetch()} /> : null}

        {!isPending && !isError ? (
          <>
            <TopThreeHighlight items={ranking} />
            <RankingList items={ranking} />
          </>
        ) : null}
      </section>
    </main>
  )
}
