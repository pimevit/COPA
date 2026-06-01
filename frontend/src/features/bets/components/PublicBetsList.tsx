import type { PublicBet } from '../../../types/bets'

type PublicBetsListProps = {
  bets: readonly PublicBet[]
  error?: unknown
  id: string
  isBlocked: boolean
  isError?: boolean
  isExpanded: boolean
  isLoading?: boolean
  onToggleExpanded: () => void
}

function getToggleLabel({
  betsCount,
  isBlocked,
  isError,
  isExpanded,
  isLoading,
}: {
  betsCount: number
  isBlocked: boolean
  isError: boolean
  isExpanded: boolean
  isLoading: boolean
}) {
  if (isExpanded) {
    return 'Ocultar palpites'
  }

  if (isBlocked || isLoading || isError) {
    return 'Ver palpites'
  }

  return betsCount === 1 ? 'Ver 1 palpite' : `Ver ${betsCount} palpites`
}

export function PublicBetsList({
  bets,
  error,
  id,
  isBlocked,
  isError = false,
  isExpanded,
  isLoading = false,
  onToggleExpanded,
}: PublicBetsListProps) {
  const toggleLabel = getToggleLabel({
    betsCount: bets.length,
    isBlocked,
    isError,
    isExpanded,
    isLoading,
  })

  return (
    <section className="border-t border-slate-100 pt-4 dark:border-slate-800" aria-label="Palpites dos jogadores">
      <div className="mb-3 flex items-center justify-between gap-3">
        <div className="flex min-w-0 items-center gap-2">
          <h2 className="text-sm font-semibold text-slate-950 dark:text-slate-50">Palpites dos jogadores</h2>
          {!isBlocked && !isLoading && !isError ? (
            <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">{bets.length}</span>
          ) : null}
        </div>
        <button
          aria-controls={id}
          aria-expanded={isExpanded}
          className="rounded-md border border-slate-300 px-3 py-1.5 text-xs font-semibold text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-900 dark:focus:ring-offset-slate-950"
          onClick={onToggleExpanded}
          type="button"
        >
          {toggleLabel}
        </button>
      </div>

      {isExpanded ? (
        <div id={id}>
          {isBlocked ? (
            <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-medium text-slate-700 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Seus palpites estao ocultos. A lista dos jogadores tambem fica bloqueada.
            </p>
          ) : null}

          {!isBlocked && isLoading ? (
            <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-medium text-slate-700 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Carregando palpites dos jogadores...
            </p>
          ) : null}

          {!isBlocked && isError ? (
            <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs font-semibold text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              {error instanceof Error ? error.message : 'Nao foi possivel carregar os palpites dos jogadores.'}
            </p>
          ) : null}

          {!isBlocked && !isLoading && !isError && bets.length === 0 ? (
            <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-medium text-slate-700 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Nenhum palpite publico nesta partida.
            </p>
          ) : null}

          {!isBlocked && !isLoading && !isError && bets.length > 0 ? (
            <ul className="grid gap-2">
              {bets.map((bet) => (
                <li
                  className="flex items-center justify-between gap-3 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 dark:border-slate-800 dark:bg-slate-900"
                  key={`${bet.matchId}-${bet.userId}`}
                >
                  <div className="min-w-0">
                    <p className="truncate text-xs font-semibold text-slate-950 dark:text-slate-50">
                      {bet.isCurrentUser ? 'Voce' : bet.userName}
                    </p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">{bet.pointsEarned} pts</p>
                  </div>
                  <span className="shrink-0 text-sm font-semibold text-slate-950 dark:text-slate-50">
                    {bet.homeGoalsPrediction} x {bet.awayGoalsPrediction}
                  </span>
                </li>
              ))}
            </ul>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
