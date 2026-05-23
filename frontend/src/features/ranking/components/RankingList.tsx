import { RankingRow } from './RankingRow'
import type { RankingItem } from '../../../types/ranking'

type RankingListProps = {
  items: readonly RankingItem[]
}

export function RankingList({ items }: RankingListProps) {
  if (items.length === 0) {
    return (
      <p className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
        Nenhum usuario no ranking.
      </p>
    )
  }

  return (
    <section aria-labelledby="ranking-list-title">
      <div className="mb-3 flex items-center justify-between gap-3">
        <h2 className="text-lg font-semibold text-slate-950 dark:text-slate-50" id="ranking-list-title">
          Classificacao
        </h2>
        <span className="text-sm font-medium text-slate-500 dark:text-slate-400">{items.length} usuarios</span>
      </div>
      <ol className="grid gap-3" aria-label="Ranking completo">
        {items.map((item) => (
          <li key={item.userId}>
            <RankingRow item={item} />
          </li>
        ))}
      </ol>
    </section>
  )
}
