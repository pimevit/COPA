import { describe, expect, it } from 'vitest'

import type { MyBet } from '../../../types/bets'
import { buildBetSaveOperation, findBetByMatchId, indexBetsByMatchId } from './betHistory'
import { validateBetForm } from './betValidation'

const baseBet: MyBet = {
  id: 10,
  matchId: 5,
  homeGoalsPrediction: 2,
  awayGoalsPrediction: 1,
  pointsEarned: 0,
  createdAt: '2026-06-01T10:00:00Z',
  match: {
    id: 5,
    homeTeam: { id: 1, name: 'Brazil', code: 'BRA', flagUrl: null },
    awayTeam: { id: 2, name: 'Argentina', code: 'ARG', flagUrl: null },
    matchDate: '2026-06-11T19:00:00Z',
    stage: 'Groups',
    status: 'Scheduled',
    homeGoals: null,
    awayGoals: null,
  },
}

describe('bet helpers', () => {
  it('indexes and finds bets by match id', () => {
    const index = indexBetsByMatchId([baseBet])

    expect(index.get(5)).toEqual(baseBet)
    expect(findBetByMatchId([baseBet], 5)).toEqual(baseBet)
    expect(findBetByMatchId([baseBet], 99)).toBeUndefined()
  })

  it('builds a create operation when no bet exists', () => {
    expect(buildBetSaveOperation(5, undefined, {
      homeGoalsPrediction: 1,
      awayGoalsPrediction: 0,
    })).toEqual({
      type: 'create',
      request: {
        matchId: 5,
        homeGoalsPrediction: 1,
        awayGoalsPrediction: 0,
      },
    })
  })

  it('builds an update operation when a bet exists', () => {
    expect(buildBetSaveOperation(5, baseBet, {
      homeGoalsPrediction: 3,
      awayGoalsPrediction: 2,
    })).toEqual({
      type: 'update',
      command: {
        id: 10,
        request: {
          homeGoalsPrediction: 3,
          awayGoalsPrediction: 2,
        },
      },
    })
  })

  it('validates required non-negative integer goals', () => {
    expect(validateBetForm({ homeGoalsPrediction: '', awayGoalsPrediction: '-1' })).toEqual({
      valid: false,
      errors: {
        homeGoalsPrediction: 'Informe os gols.',
        awayGoalsPrediction: 'Use um numero inteiro maior ou igual a zero.',
      },
    })

    expect(validateBetForm({ homeGoalsPrediction: '2', awayGoalsPrediction: '0' })).toEqual({
      valid: true,
      errors: {},
      value: {
        homeGoalsPrediction: 2,
        awayGoalsPrediction: 0,
      },
    })
  })

})
