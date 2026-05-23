import type { UpdateBetRequest } from '../../../types/bets'

export type BetFormValues = {
  homeGoalsPrediction: string
  awayGoalsPrediction: string
}

export type BetFieldErrors = Partial<Record<keyof BetFormValues, string>>

export type BetValidationResult =
  | {
      valid: true
      value: UpdateBetRequest
      errors: BetFieldErrors
    }
  | {
      valid: false
      errors: BetFieldErrors
    }

function validateGoals(value: string): string | null {
  const trimmedValue = value.trim()

  if (!trimmedValue) {
    return 'Informe os gols.'
  }

  if (!/^\d+$/.test(trimmedValue)) {
    return 'Use um numero inteiro maior ou igual a zero.'
  }

  return null
}

export function validateBetForm(values: BetFormValues): BetValidationResult {
  const errors: BetFieldErrors = {
    homeGoalsPrediction: validateGoals(values.homeGoalsPrediction) ?? undefined,
    awayGoalsPrediction: validateGoals(values.awayGoalsPrediction) ?? undefined,
  }

  if (errors.homeGoalsPrediction || errors.awayGoalsPrediction) {
    return {
      valid: false,
      errors,
    }
  }

  return {
    valid: true,
    errors: {},
    value: {
      homeGoalsPrediction: Number(values.homeGoalsPrediction),
      awayGoalsPrediction: Number(values.awayGoalsPrediction),
    },
  }
}
