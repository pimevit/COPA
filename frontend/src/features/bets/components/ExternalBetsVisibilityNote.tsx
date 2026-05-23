import type { MatchListItem } from '../../../types/matches'
import { canShowExternalBets } from '../utils/visibility'

type ExternalBetsVisibilityNoteProps = {
  match: MatchListItem
}

export function ExternalBetsVisibilityNote({ match }: ExternalBetsVisibilityNoteProps) {
  const canShow = canShowExternalBets(match)

  return (
    <section className="border-t border-slate-100 pt-3 dark:border-slate-800" aria-label="Visibilidade de terceiros">
      <p className="text-xs text-slate-500 dark:text-slate-400">
        {canShow
          ? 'Palpites de terceiros podem ser exibidos apos o inicio, mas nao ha endpoint aprovado para carregar esses dados.'
          : 'Palpites de terceiros ficam ocultos antes do inicio da partida.'}
      </p>
    </section>
  )
}
