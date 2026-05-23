type RankingErrorStateProps = {
  error: unknown
  onRetry: () => void
}

export function RankingLoadingState() {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
      Carregando ranking...
    </div>
  )
}

export function RankingErrorState({ error, onRetry }: RankingErrorStateProps) {
  return (
    <div className="rounded-lg border border-red-200 bg-red-50 p-5 text-sm text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
      <p className="font-semibold">Nao foi possivel carregar o ranking.</p>
      <p className="mt-1">{error instanceof Error ? error.message : 'Tente novamente em instantes.'}</p>
      <button
        className="mt-4 rounded-md bg-red-700 px-4 py-2 text-sm font-semibold text-white hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 dark:focus:ring-offset-red-950"
        onClick={onRetry}
        type="button"
      >
        Tentar novamente
      </button>
    </div>
  )
}
