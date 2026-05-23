import { BettingWindowBadge } from './BettingWindowBadge'
import { MatchTeams } from './MatchTeams'
import { BetPanel } from '../../bets/components/BetPanel'
import type { MyBet } from '../../../types/bets'
import type { MatchListItem } from '../../../types/matches'
import { formatMatchDateTime } from '../utils/dateTime'
import { getStageLabel, getStatusLabel } from '../utils/labels'
import { formatMatchResult, hasMatchResult } from '../utils/result'

type MatchCardProps = {
  match: MatchListItem
  isToday: boolean
  existingBet?: MyBet
  isBetHistoryLoading?: boolean
}

export function MatchCard({ existingBet, isBetHistoryLoading = false, match, isToday }: MatchCardProps) {
  const cardClasses = isToday
    ? 'border-emerald-300 bg-white shadow-sm shadow-emerald-100 dark:border-emerald-700 dark:bg-slate-950 dark:shadow-none'
    : 'border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-950'

  return (
    <article className={`rounded-lg border p-4 ${cardClasses}`}>
      <div className="flex flex-col gap-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <p className="text-sm font-semibold text-slate-950 dark:text-slate-50">
                {formatMatchDateTime(match.matchDate)}
              </p>
              {isToday ? (
                <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-semibold text-emerald-800 dark:bg-emerald-950 dark:text-emerald-200">
                  Hoje
                </span>
              ) : null}
            </div>
            <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
              {getStageLabel(match.stage)} · {getStatusLabel(match.status)}
            </p>
          </div>
          <BettingWindowBadge isBettingOpen={match.isBettingOpen} />
        </div>

        <MatchTeams awayTeam={match.awayTeam} homeTeam={match.homeTeam} />

        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-3 dark:border-slate-800">
          <span className="text-xs font-medium text-slate-500 dark:text-slate-400">Resultado</span>
          <span
            className={
              hasMatchResult(match)
                ? 'text-base font-semibold text-slate-950 dark:text-slate-50'
                : 'text-sm font-medium text-slate-500 dark:text-slate-400'
            }
          >
            {formatMatchResult(match)}
          </span>
        </div>

        <BetPanel existingBet={existingBet} isHistoryLoading={isBetHistoryLoading} match={match} />
      </div>
    </article>
  )
}
