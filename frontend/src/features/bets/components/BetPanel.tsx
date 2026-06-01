import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'

import type { MyBet, UpdateBetRequest } from '../../../types/bets'
import type { MatchListItem } from '../../../types/matches'
import { matchesQueryKey } from '../../matches/hooks/useMatches'
import { useCreateBetMutation, useUpdateBetMutation, myBetsQueryKey } from '../hooks/useBets'
import { buildBetSaveOperation } from '../utils/betHistory'
import { mapBetError } from '../utils/betErrors'
import { BetForm } from './BetForm'

type BetPanelProps = {
  match: MatchListItem
  existingBet?: MyBet
  isHistoryLoading?: boolean
}

export function BetPanel({ existingBet, isHistoryLoading = false, match }: BetPanelProps) {
  const queryClient = useQueryClient()
  const createBetMutation = useCreateBetMutation()
  const updateBetMutation = useUpdateBetMutation()
  const [localError, setLocalError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [isClosedByApi, setIsClosedByApi] = useState(false)
  const isSaving = createBetMutation.isPending || updateBetMutation.isPending
  const isReadOnly = !match.isBettingOpen || isClosedByApi
  const isDisabled = isHistoryLoading

  async function handleSubmit(request: UpdateBetRequest) {
    setLocalError(null)
    setSuccessMessage(null)

    const operation = buildBetSaveOperation(match.id, existingBet, request)

    try {
      if (operation.type === 'create') {
        await createBetMutation.mutateAsync(operation.request)
      } else {
        await updateBetMutation.mutateAsync(operation.command)
      }

      await queryClient.invalidateQueries({ queryKey: myBetsQueryKey })
      await queryClient.invalidateQueries({ queryKey: matchesQueryKey() })
      setSuccessMessage('Palpite salvo.')
    } catch (error) {
      const message = mapBetError(error)
      setLocalError(message.message)

      if (message.kind === 'windowClosed') {
        setIsClosedByApi(true)
        await queryClient.invalidateQueries({ queryKey: matchesQueryKey() })
      }

      if (message.kind === 'duplicate') {
        await queryClient.invalidateQueries({ queryKey: myBetsQueryKey })
      }
    }
  }

  if (isReadOnly) {
    return (
      <section className="border-t border-slate-100 pt-4 dark:border-slate-800" aria-label="Palpite da partida">
        <div className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 dark:border-slate-800 dark:bg-slate-900">
          {existingBet ? (
            <p className="text-sm font-semibold text-slate-950 dark:text-slate-50">
              Seu palpite: {existingBet.homeGoalsPrediction} x {existingBet.awayGoalsPrediction}
            </p>
          ) : (
            <p className="text-sm font-medium text-slate-700 dark:text-slate-300">
              Você não registrou palpite para esta partida.
            </p>
          )}
          <p className="mt-1 text-xs font-semibold text-slate-500 dark:text-slate-400">Palpites encerrados</p>
          {localError ? (
            <p className="mt-2 text-xs font-medium text-red-700 dark:text-red-300">{localError}</p>
          ) : null}
        </div>
      </section>
    )
  }

  return (
    <section className="border-t border-slate-100 pt-4 dark:border-slate-800" aria-label="Palpite da partida">
      <div className="mb-3 flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-sm font-semibold text-slate-950 dark:text-slate-50">Seu palpite</h2>
          <p className="text-xs text-slate-500 dark:text-slate-400">
            {existingBet ? 'Edite enquanto a janela estiver aberta.' : 'Informe o placar previsto.'}
          </p>
        </div>
        {existingBet ? (
          <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">
            {existingBet.homeGoalsPrediction} x {existingBet.awayGoalsPrediction}
          </span>
        ) : null}
      </div>

      {isHistoryLoading ? (
        <p className="mb-3 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-xs font-medium text-slate-700 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
          Carregando seu historico para decidir entre criar ou editar.
        </p>
      ) : null}

      <BetForm
        disabled={isDisabled}
        errorMessage={localError}
        existingBet={existingBet}
        isLoading={isSaving}
        match={match}
        onSubmit={handleSubmit}
        successMessage={successMessage}
      />
    </section>
  )
}
