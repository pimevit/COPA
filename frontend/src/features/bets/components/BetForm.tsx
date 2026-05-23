import { useEffect, useId, useState } from 'react'
import type { FormEvent } from 'react'

import type { MyBet, UpdateBetRequest } from '../../../types/bets'
import type { MatchListItem } from '../../../types/matches'
import { validateBetForm, type BetFieldErrors, type BetFormValues } from '../utils/betValidation'

type BetFormProps = {
  match: MatchListItem
  existingBet?: MyBet
  disabled?: boolean
  isLoading?: boolean
  errorMessage?: string | null
  successMessage?: string | null
  onSubmit: (request: UpdateBetRequest) => void
}

function toInitialValues(existingBet?: MyBet): BetFormValues {
  return {
    homeGoalsPrediction: existingBet ? String(existingBet.homeGoalsPrediction) : '',
    awayGoalsPrediction: existingBet ? String(existingBet.awayGoalsPrediction) : '',
  }
}

function FieldError({ id, message }: { id: string; message?: string }) {
  if (!message) {
    return null
  }

  return (
    <p className="mt-1 text-xs font-medium text-red-700 dark:text-red-300" id={id}>
      {message}
    </p>
  )
}

export function BetForm({
  match,
  existingBet,
  disabled = false,
  errorMessage,
  isLoading = false,
  onSubmit,
  successMessage,
}: BetFormProps) {
  const homeInputId = useId()
  const awayInputId = useId()
  const [values, setValues] = useState<BetFormValues>(() => toInitialValues(existingBet))
  const [fieldErrors, setFieldErrors] = useState<BetFieldErrors>({})
  const isFormDisabled = disabled || isLoading
  const submitLabel = existingBet ? 'Atualizar palpite' : 'Salvar palpite'

  useEffect(() => {
    setValues(toInitialValues(existingBet))
    setFieldErrors({})
  }, [existingBet])

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (isFormDisabled) {
      return
    }

    const result = validateBetForm(values)
    setFieldErrors(result.errors)

    if (!result.valid) {
      return
    }

    onSubmit(result.value)
  }

  return (
    <form className="space-y-3" noValidate onSubmit={handleSubmit}>
      <div className="grid grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] items-start gap-3">
        <div className="min-w-0">
          <label className="block text-xs font-semibold text-slate-600 dark:text-slate-300" htmlFor={homeInputId}>
            {match.homeTeam.code}
          </label>
          <input
            aria-describedby={fieldErrors.homeGoalsPrediction ? `${homeInputId}-error` : undefined}
            aria-invalid={Boolean(fieldErrors.homeGoalsPrediction)}
            className="mt-1 h-11 w-full rounded-md border border-slate-300 bg-white px-3 text-center text-base font-semibold text-slate-950 shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500 dark:border-slate-700 dark:bg-slate-950 dark:text-slate-50 dark:disabled:bg-slate-900"
            disabled={isFormDisabled}
            id={homeInputId}
            inputMode="numeric"
            min={0}
            name="homeGoalsPrediction"
            onChange={(event) =>
              setValues((current) => ({ ...current, homeGoalsPrediction: event.target.value }))
            }
            step={1}
            type="number"
            value={values.homeGoalsPrediction}
          />
          <FieldError id={`${homeInputId}-error`} message={fieldErrors.homeGoalsPrediction} />
        </div>

        <span className="mt-8 text-sm font-semibold text-slate-400 dark:text-slate-500">x</span>

        <div className="min-w-0">
          <label className="block text-xs font-semibold text-slate-600 dark:text-slate-300" htmlFor={awayInputId}>
            {match.awayTeam.code}
          </label>
          <input
            aria-describedby={fieldErrors.awayGoalsPrediction ? `${awayInputId}-error` : undefined}
            aria-invalid={Boolean(fieldErrors.awayGoalsPrediction)}
            className="mt-1 h-11 w-full rounded-md border border-slate-300 bg-white px-3 text-center text-base font-semibold text-slate-950 shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500 dark:border-slate-700 dark:bg-slate-950 dark:text-slate-50 dark:disabled:bg-slate-900"
            disabled={isFormDisabled}
            id={awayInputId}
            inputMode="numeric"
            min={0}
            name="awayGoalsPrediction"
            onChange={(event) =>
              setValues((current) => ({ ...current, awayGoalsPrediction: event.target.value }))
            }
            step={1}
            type="number"
            value={values.awayGoalsPrediction}
          />
          <FieldError id={`${awayInputId}-error`} message={fieldErrors.awayGoalsPrediction} />
        </div>
      </div>

      {errorMessage ? (
        <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs font-medium text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
          {errorMessage}
        </p>
      ) : null}

      {successMessage ? (
        <p className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-xs font-medium text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-200">
          {successMessage}
        </p>
      ) : null}

      <button
        className="min-h-11 w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600 dark:focus:ring-offset-slate-950 dark:disabled:bg-slate-800 dark:disabled:text-slate-500"
        disabled={isFormDisabled}
        type="submit"
      >
        {isLoading ? 'Salvando...' : submitLabel}
      </button>
    </form>
  )
}
