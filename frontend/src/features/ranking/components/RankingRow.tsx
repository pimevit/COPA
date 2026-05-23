import type { RankingItem } from '../../../types/ranking'
import { formatRankingPoints, formatRankingPosition } from '../utils/formatting'

type RankingRowProps = {
  item: RankingItem
}

export function RankingRow({ item }: RankingRowProps) {
  const rowClasses = item.isCurrentUser
    ? 'border-emerald-300 bg-emerald-50 text-emerald-950 dark:border-emerald-700 dark:bg-emerald-950 dark:text-emerald-50'
    : 'border-slate-200 bg-white text-slate-950 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-50'

  return (
    <article
      className={`rounded-lg border p-4 ${rowClasses}`}
      aria-label={`${item.name}, posicao ${item.position}, ${formatRankingPoints(item.points)}`}
    >
      <div className="grid grid-cols-[3.5rem_minmax(0,1fr)_auto] items-center gap-3">
        <span className="text-base font-semibold tabular-nums">{formatRankingPosition(item.position)}</span>
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold sm:text-base">{item.name}</p>
          <div className="mt-1 flex flex-wrap items-center gap-2">
            {item.isCurrentUser ? (
              <span className="rounded-full bg-emerald-700 px-2 py-0.5 text-xs font-semibold text-white dark:bg-emerald-300 dark:text-emerald-950">
                voce
              </span>
            ) : null}
            {item.isTop3 ? (
              <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-semibold text-amber-900 dark:bg-amber-300 dark:text-amber-950">
                Top 3
              </span>
            ) : null}
          </div>
        </div>
        <span className="min-w-20 rounded-md bg-slate-100 px-2 py-1 text-center text-sm font-semibold text-slate-800 dark:bg-slate-900 dark:text-slate-100">
          {formatRankingPoints(item.points)}
        </span>
      </div>
    </article>
  )
}
