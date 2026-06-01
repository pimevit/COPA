import { useEffect, useMemo, useState } from 'react'

import { MatchCard } from '../components/MatchCard'
import { BetHistory } from '../../bets/components/BetHistory'
import type { PublicBet } from '../../../types/bets'
import {
  useBetVisibility,
  useMyBets,
  usePublicBets,
} from '../../bets/hooks/useBets'
import { indexBetsByMatchId } from '../../bets/utils/betHistory'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import { useMatchNotice } from '../../notices/hooks/useMatchNotice'
import { useMatches } from '../hooks/useMatches'
import { isTodayMatch, parseUtcDate } from '../utils/dateTime'
import {
  getPendingBetMatches,
  sortActiveMatchesByPendingAndDate,
  sortMatchesByDate,
  splitMatchesByClosedState,
} from '../utils/matchLists'

type MatchesTab = 'active' | 'closed'

const nowRefreshIntervalMs = 60_000

function groupPublicBetsByMatchId(bets: readonly PublicBet[]) {
  const index = new Map<number, PublicBet[]>()

  for (const bet of bets) {
    const existing = index.get(bet.matchId) ?? []
    existing.push(bet)
    index.set(bet.matchId, existing)
  }

  return index
}

function getTabButtonClasses(isSelected: boolean): string {
  return isSelected
    ? 'rounded-md bg-emerald-700 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950'
    : 'rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-200 dark:hover:bg-slate-900 dark:focus:ring-offset-slate-950'
}

type PendingBetsSummaryProps = {
  error?: unknown
  isError: boolean
  isLoading: boolean
  pendingCount: number
}

function PendingBetsSummary({ error, isError, isLoading, pendingCount }: PendingBetsSummaryProps) {
  if (isLoading) {
    return (
      <section
        aria-live="polite"
        className="rounded-lg border border-slate-200 bg-white p-5 text-slate-800 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-200"
      >
        <p className="text-lg font-semibold">Conferindo seus palpites pendentes...</p>
        <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
          Aguarde enquanto carregamos seu historico.
        </p>
      </section>
    )
  }

  if (isError) {
    return (
      <section
        aria-live="polite"
        className="rounded-lg border border-red-200 bg-red-50 p-5 text-red-900 dark:border-red-900 dark:bg-red-950 dark:text-red-100"
      >
        <p className="text-lg font-semibold">Nao foi possivel conferir seus palpites pendentes.</p>
        <p className="mt-1 text-sm">{error instanceof Error ? error.message : 'Tente novamente em instantes.'}</p>
      </section>
    )
  }

  if (pendingCount > 0) {
    const pendingMessage =
      pendingCount === 1 ? 'Falta 1 jogo para voce palpitar.' : `Faltam ${pendingCount} jogos para voce palpitar.`

    return (
      <section
        aria-live="polite"
        className="rounded-lg border border-amber-200 bg-amber-50 p-5 text-amber-950 dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-100"
      >
        <p className="text-lg font-semibold">{pendingMessage}</p>
        <p className="mt-1 text-sm">Eles aparecem primeiro na aba Jogos.</p>
      </section>
    )
  }

  return (
    <section
      aria-live="polite"
      className="rounded-lg border border-emerald-200 bg-emerald-50 p-5 text-emerald-950 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-100"
    >
      <p className="text-lg font-semibold">Voce ja fez todos os palpites disponiveis.</p>
      <p className="mt-1 text-sm">Novos jogos abertos para palpite aparecerao em destaque aqui.</p>
    </section>
  )
}

export function MatchesPage() {
  const [selectedTab, setSelectedTab] = useState<MatchesTab>('active')
  const [now, setNow] = useState(() => new Date())
  const [expandedPublicBetsMatchIds, setExpandedPublicBetsMatchIds] = useState<ReadonlySet<number>>(() => new Set())
  const { data, error, isError, isPending, refetch } = useMatches()
  const matchNoticeQuery = useMatchNotice()
  const betVisibilityQuery = useBetVisibility()
  const isVisibilityLoaded = !betVisibilityQuery.isPending
  const showBetsPublicly = betVisibilityQuery.data?.showBetsPublicly === true
  const publicBetsQuery = usePublicBets(showBetsPublicly)
  const {
    data: myBetsData,
    error: myBetsError,
    isError: isMyBetsError,
    isPending: isMyBetsPending,
    refetch: refetchMyBets,
  } = useMyBets()
  const matches = useMemo(() => sortMatchesByDate(data ?? []), [data])
  const matchGroups = useMemo(() => splitMatchesByClosedState(matches, now), [matches, now])
  const myBets = useMemo(
    () =>
      [...(myBetsData ?? [])].sort(
        (first, second) => parseUtcDate(first.match.matchDate).getTime() - parseUtcDate(second.match.matchDate).getTime(),
      ),
    [myBetsData],
  )
  const betsByMatchId = useMemo(() => indexBetsByMatchId(myBetsData ?? []), [myBetsData])
  const pendingMatches = useMemo(
    () => getPendingBetMatches(matchGroups.activeMatches, betsByMatchId, now),
    [betsByMatchId, matchGroups.activeMatches, now],
  )
  const pendingMatchIds = useMemo(() => new Set(pendingMatches.map((match) => match.id)), [pendingMatches])
  const activeMatches = useMemo(
    () => sortActiveMatchesByPendingAndDate(matchGroups.activeMatches, betsByMatchId, now),
    [betsByMatchId, matchGroups.activeMatches, now],
  )
  const publicBetsByMatchId = useMemo(
    () => groupPublicBetsByMatchId(showBetsPublicly ? (publicBetsQuery.data ?? []) : []),
    [publicBetsQuery.data, showBetsPublicly],
  )
  const hasTodayMatches = matches.some((match) => isTodayMatch(match.matchDate))
  const visibleMatches = selectedTab === 'active' ? activeMatches : matchGroups.closedMatches
  const emptyTabMessage = selectedTab === 'active' ? 'Nenhum jogo ativo encontrado.' : 'Nenhum jogo encerrado.'
  const tabPanelLabel = selectedTab === 'active' ? 'tab-jogos' : 'tab-jogos-encerrados'
  const matchNoticeMessage = matchNoticeQuery.isError ? '' : matchNoticeQuery.data?.message.trim() ?? ''

  useEffect(() => {
    const intervalId = window.setInterval(() => setNow(new Date()), nowRefreshIntervalMs)

    return () => window.clearInterval(intervalId)
  }, [])

  function togglePublicBets(matchId: number) {
    setExpandedPublicBetsMatchIds((current) => {
      const next = new Set(current)

      if (next.has(matchId)) {
        next.delete(matchId)
      } else {
        next.add(matchId)
      }

      return next
    })
  }

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

        {matchNoticeMessage ? (
          <section
            aria-label="Recado das partidas"
            className="rounded-lg border border-amber-200 bg-amber-50 p-5 text-amber-950 dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-100"
          >
            <h2 className="text-lg font-semibold tracking-normal">Recado</h2>
            <p className="mt-1 whitespace-pre-wrap text-sm">{matchNoticeMessage}</p>
          </section>
        ) : null}

        {!isPending && !isError && matches.length > 0 ? (
          <>
            <PendingBetsSummary
              error={myBetsError}
              isError={isMyBetsError}
              isLoading={isMyBetsPending}
              pendingCount={pendingMatches.length}
            />

            <div
              aria-label="Abas de partidas"
              className="flex flex-wrap gap-2 border-b border-slate-200 pb-3 dark:border-slate-800"
              role="tablist"
            >
              <button
                aria-controls="matches-tab-panel"
                aria-selected={selectedTab === 'active'}
                className={getTabButtonClasses(selectedTab === 'active')}
                id="tab-jogos"
                onClick={() => setSelectedTab('active')}
                role="tab"
                type="button"
              >
                Jogos ({activeMatches.length})
              </button>
              <button
                aria-controls="matches-tab-panel"
                aria-selected={selectedTab === 'closed'}
                className={getTabButtonClasses(selectedTab === 'closed')}
                id="tab-jogos-encerrados"
                onClick={() => setSelectedTab('closed')}
                role="tab"
                type="button"
              >
                Jogos encerrados ({matchGroups.closedMatches.length})
              </button>
            </div>

            <section aria-labelledby={tabPanelLabel} id="matches-tab-panel" role="tabpanel">
              {visibleMatches.length === 0 ? (
                <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
                  {emptyTabMessage}
                </div>
              ) : (
                <ul className="grid gap-4" aria-label="Lista de partidas">
                  {visibleMatches.map((match) => (
                    <li key={match.id}>
                      <MatchCard
                        existingBet={betsByMatchId.get(match.id)}
                        isBetHistoryLoading={isMyBetsPending}
                        isPendingBet={pendingMatchIds.has(match.id)}
                        isPublicBetsExpanded={expandedPublicBetsMatchIds.has(match.id)}
                        isToday={isTodayMatch(match.matchDate)}
                        match={match}
                        onTogglePublicBets={() => togglePublicBets(match.id)}
                        publicBets={publicBetsByMatchId.get(match.id) ?? []}
                        publicBetsError={betVisibilityQuery.error ?? publicBetsQuery.error}
                        publicBetsIsBlocked={isVisibilityLoaded && !showBetsPublicly}
                        publicBetsIsError={betVisibilityQuery.isError || (showBetsPublicly && publicBetsQuery.isError)}
                        publicBetsIsLoading={betVisibilityQuery.isPending || (showBetsPublicly && publicBetsQuery.isPending)}
                      />
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
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
