import type { MyBet } from '../../../types/bets'
import { formatMatchDateTime } from '../../matches/utils/dateTime'
import { getStageLabel, getStatusLabel } from '../../matches/utils/labels'

type BetHistoryProps = {
  bets: readonly MyBet[]
  error?: unknown
  isError?: boolean
  isLoading?: boolean
  onRetry?: () => void
}

export function BetHistory({ bets, error, isError = false, isLoading = false, onRetry }: BetHistoryProps) {
  return (
    <section className="border-t border-slate-200 pt-5 dark:border-slate-800" aria-labelledby="bet-history-title">
      <div className="mb-3 flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950 dark:text-slate-50" id="bet-history-title">
            Historico de palpites
          </h2>
          <p className="text-sm text-slate-600 dark:text-slate-300">
            Apenas seus palpites autenticados via GET /bets/me.
          </p>
        </div>
        <span className="text-sm font-medium text-slate-500 dark:text-slate-400">{bets.length} palpites</span>
      </div>

      {isLoading ? (
        <p className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
          Carregando historico...
        </p>
      ) : null}

      {isError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
          <p className="font-semibold">Nao foi possivel carregar seu historico.</p>
          <p className="mt-1">{error instanceof Error ? error.message : 'Tente novamente em instantes.'}</p>
          {onRetry ? (
            <button
              className="mt-3 rounded-md bg-red-700 px-4 py-2 text-sm font-semibold text-white hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 dark:focus:ring-offset-red-950"
              onClick={onRetry}
              type="button"
            >
              Tentar novamente
            </button>
          ) : null}
        </div>
      ) : null}

      {!isLoading && !isError && bets.length === 0 ? (
        <p className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
          Voce ainda nao fez palpites.
        </p>
      ) : null}

      {!isLoading && !isError && bets.length > 0 ? (
        <ul className="grid gap-3" aria-label="Historico do usuario">
          {bets.map((bet) => (
            <li
              className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950"
              key={bet.id}
            >
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold text-slate-950 dark:text-slate-50">
                    {bet.match.homeTeam.name} x {bet.match.awayTeam.name}
                  </p>
                  <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                    {formatMatchDateTime(bet.match.matchDate)} · {getStageLabel(bet.match.stage)} ·{' '}
                    {getStatusLabel(bet.match.status)}
                  </p>
                </div>
                <div className="flex shrink-0 items-center justify-between gap-4 sm:justify-end">
                  <span className="text-base font-semibold text-slate-950 dark:text-slate-50">
                    {bet.homeGoalsPrediction} x {bet.awayGoalsPrediction}
                  </span>
                  <span className="min-w-16 rounded-md bg-slate-100 px-2 py-1 text-center text-xs font-semibold text-slate-700 dark:bg-slate-900 dark:text-slate-200">
                    {bet.pointsEarned} pts
                  </span>
                </div>
              </div>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  )
}
