type BettingWindowBadgeProps = {
  isBettingOpen: boolean
}

export function BettingWindowBadge({ isBettingOpen }: BettingWindowBadgeProps) {
  const classes = isBettingOpen
    ? 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-700 dark:bg-emerald-950 dark:text-emerald-200'
    : 'border-slate-200 bg-slate-100 text-slate-700 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-300'

  return (
    <span className={`inline-flex min-h-7 items-center rounded-full border px-3 text-xs font-semibold ${classes}`}>
      {isBettingOpen ? 'Janela aberta' : 'Janela fechada'}
    </span>
  )
}
