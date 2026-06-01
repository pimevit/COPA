import type { RankingItem } from '../../../types/ranking'
import { formatRankingPoints, formatRankingPosition } from '../utils/formatting'
import { RankingTieBreakersTooltip } from './RankingTieBreakersTooltip'

type TopThreeHighlightProps = {
  items: readonly RankingItem[]
}

function getPositionClasses(position: number): string {
  if (position === 1) {
    return 'border-amber-300 bg-amber-50 text-amber-950 dark:border-amber-500 dark:bg-amber-950 dark:text-amber-50'
  }

  if (position === 2) {
    return 'border-zinc-300 bg-zinc-50 text-zinc-950 dark:border-zinc-600 dark:bg-zinc-900 dark:text-zinc-50'
  }

  return 'border-sky-300 bg-sky-50 text-sky-950 dark:border-sky-700 dark:bg-sky-950 dark:text-sky-50'
}

export function TopThreeHighlight({ items }: TopThreeHighlightProps) {
  const topThree = items.filter((item) => item.isTop3).slice(0, 3)

  if (topThree.length === 0) {
    return null
  }

  return (
    <section aria-labelledby="ranking-top-three-title">
      <div className="mb-3 flex items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-slate-950 dark:text-slate-50" id="ranking-top-three-title">
          Top 3
        </h2>
        <span className="text-sm font-medium text-slate-500 dark:text-slate-400">{topThree.length} destaques</span>
      </div>
      <ol className="grid gap-3 sm:grid-cols-3" aria-label="Top 3 do ranking">
        {topThree.map((item) => (
          <li key={item.userId}>
            <article className={`rounded-lg border p-4 ${getPositionClasses(item.position)}`}>
              <div className="flex items-start justify-between gap-3">
                <span className="text-lg font-semibold tabular-nums">{formatRankingPosition(item.position)}</span>
                <div className="flex items-center gap-2">
                  <span className="rounded-md bg-white/70 px-2 py-1 text-xs font-semibold text-slate-900 dark:bg-black/20 dark:text-white">
                    {formatRankingPoints(item.points)}
                  </span>
                  <RankingTieBreakersTooltip item={item} />
                </div>
              </div>
              <p className="mt-4 truncate text-base font-semibold">{item.name}</p>
              {item.isCurrentUser ? (
                <p className="mt-2 text-xs font-semibold uppercase tracking-normal">voce</p>
              ) : null}
            </article>
          </li>
        ))}
      </ol>
    </section>
  )
}
