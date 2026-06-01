import { useId } from 'react'

import type { RankingItem } from '../../../types/ranking'
import { formatRankingDateTime, formatRankingPoints } from '../utils/formatting'

type RankingTieBreakersTooltipProps = {
  item: RankingItem
}

export function RankingTieBreakersTooltip({ item }: RankingTieBreakersTooltipProps) {
  const { tieBreakers } = item
  const tooltipId = useId()

  return (
    <div className="group relative inline-flex shrink-0">
      <button
        type="button"
        className="flex h-7 w-7 items-center justify-center rounded-full border border-slate-300 bg-white text-xs font-bold text-slate-700 shadow-sm transition hover:border-emerald-500 hover:text-emerald-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-500 dark:border-slate-700 dark:bg-slate-950 dark:text-slate-200 dark:hover:border-emerald-300 dark:hover:text-emerald-200"
        aria-label={`Criterios de desempate de ${item.name}`}
        aria-describedby={tooltipId}
      >
        i
      </button>
      <div
        id={tooltipId}
        role="tooltip"
        className="pointer-events-none absolute right-0 top-full z-20 mt-2 w-64 max-w-[calc(100vw-2rem)] rounded-md border border-slate-200 bg-white p-3 text-left text-xs text-slate-700 opacity-0 shadow-lg transition group-focus-within:opacity-100 group-hover:opacity-100 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
      >
        <p className="font-semibold text-slate-950 dark:text-slate-50">Criterios de desempate</p>
        <p className="mt-2">{formatRankingPoints(item.points)}</p>
        <p>Placares exatos: {tieBreakers.exactScores}</p>
        <p>Acertos de vencedor/empate: {tieBreakers.outcomeHits}</p>
        <p>Melhor sequencia: {tieBreakers.bestHitStreak}</p>
        <p>Primeiro palpite: {formatRankingDateTime(tieBreakers.firstBetCreatedAtUtc)}</p>
      </div>
    </div>
  )
}
