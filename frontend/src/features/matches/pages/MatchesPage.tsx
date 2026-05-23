import { MatchCard } from '../components/MatchCard'
import { BetHistory } from '../../bets/components/BetHistory'
import { useMyBets } from '../../bets/hooks/useBets'
import { indexBetsByMatchId } from '../../bets/utils/betHistory'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import { useMatches } from '../hooks/useMatches'
import { isTodayMatch, parseUtcDate } from '../utils/dateTime'

export function MatchesPage() {
  const { data, error, isError, isPending, refetch } = useMatches()
  const {
    data: myBetsData,
    error: myBetsError,
    isError: isMyBetsError,
    isPending: isMyBetsPending,
    refetch: refetchMyBets,
  } = useMyBets()
  const matches = [...(data ?? [])].sort(
    (first, second) => parseUtcDate(first.matchDate).getTime() - parseUtcDate(second.matchDate).getTime(),
  )
  const myBets = [...(myBetsData ?? [])].sort(
    (first, second) => parseUtcDate(first.match.matchDate).getTime() - parseUtcDate(second.match.matchDate).getTime(),
  )
  const betsByMatchId = indexBetsByMatchId(myBetsData ?? [])
  const hasTodayMatches = matches.some((match) => isTodayMatch(match.matchDate))

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-5xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="matches" />
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
                Bolao Copa
              </p>
              <h1 className="text-2xl font-semibold tracking-normal sm:text-3xl">Partidas</h1>
              <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
                Jogos ordenados por data, com fase, status e janela de palpite.
              </p>
            </div>
            <span className="text-sm font-medium text-slate-500 dark:text-slate-400">
              {hasTodayMatches ? 'Ha jogos hoje' : 'Sem jogos hoje'}
            </span>
          </div>
        </header>

        {isPending ? (
          <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
            Carregando partidas...
          </div>
        ) : null}

        {isError ? (
          <div className="rounded-lg border border-red-200 bg-red-50 p-5 text-sm text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
            <p className="font-semibold">Nao foi possivel carregar as partidas.</p>
            <p className="mt-1">{error instanceof Error ? error.message : 'Tente novamente em instantes.'}</p>
            <button
              className="mt-4 rounded-md bg-red-700 px-4 py-2 text-sm font-semibold text-white hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 dark:focus:ring-offset-red-950"
              onClick={() => void refetch()}
              type="button"
            >
              Tentar novamente
            </button>
          </div>
        ) : null}

        {!isPending && !isError && matches.length === 0 ? (
          <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
            Nenhuma partida encontrada.
          </div>
        ) : null}

        {!isPending && !isError && matches.length > 0 ? (
          <ul className="grid gap-4" aria-label="Lista de partidas">
            {matches.map((match) => (
              <li key={match.id}>
                <MatchCard
                  existingBet={betsByMatchId.get(match.id)}
                  isBetHistoryLoading={isMyBetsPending}
                  isToday={isTodayMatch(match.matchDate)}
                  match={match}
                />
              </li>
            ))}
          </ul>
        ) : null}

        <BetHistory
          bets={myBets}
          error={myBetsError}
          isError={isMyBetsError}
          isLoading={isMyBetsPending}
          onRetry={() => void refetchMyBets()}
        />
      </section>
    </main>
  )
}
