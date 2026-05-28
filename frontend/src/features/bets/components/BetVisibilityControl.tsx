type BetVisibilityControlProps = {
  error?: unknown
  isLoading?: boolean
  isSaving?: boolean
  onChange: (showBetsPublicly: boolean) => void
  showBetsPublicly: boolean
}

export function BetVisibilityControl({
  error,
  isLoading = false,
  isSaving = false,
  onChange,
  showBetsPublicly,
}: BetVisibilityControlProps) {
  const isDisabled = isLoading || isSaving

  return (
    <section
      className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950"
      aria-label="Privacidade dos palpites"
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-sm font-semibold text-slate-950 dark:text-slate-50">Privacidade dos palpites</h2>
          <p className="mt-1 text-xs text-slate-600 dark:text-slate-300">
            {showBetsPublicly
              ? 'Seus palpites aparecem para jogadores publicos.'
              : 'Seus palpites estao ocultos e a lista dos jogadores fica bloqueada.'}
          </p>
        </div>
        <label className="inline-flex items-center gap-3 text-sm font-semibold text-slate-700 dark:text-slate-200">
          <input
            checked={showBetsPublicly}
            className="h-5 w-5 rounded border-slate-300 text-emerald-700 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={isDisabled}
            onChange={(event) => onChange(event.target.checked)}
            type="checkbox"
          />
          Publico
        </label>
      </div>

      {isLoading ? (
        <p className="mt-3 text-xs font-medium text-slate-500 dark:text-slate-400">Carregando privacidade...</p>
      ) : null}

      {isSaving ? (
        <p className="mt-3 text-xs font-medium text-slate-500 dark:text-slate-400">Salvando privacidade...</p>
      ) : null}

      {error ? (
        <p className="mt-3 text-xs font-semibold text-red-700 dark:text-red-300">
          {error instanceof Error ? error.message : 'Nao foi possivel atualizar a privacidade.'}
        </p>
      ) : null}
    </section>
  )
}
