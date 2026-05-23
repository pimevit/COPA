import type { CreateBetRequest, MyBet, UpdateBetCommand, UpdateBetRequest } from '../../../types/bets'

export function indexBetsByMatchId(bets: readonly MyBet[] = []): Map<number, MyBet> {
  return new Map(bets.map((bet) => [bet.matchId, bet]))
}

export function findBetByMatchId(bets: readonly MyBet[] = [], matchId: number): MyBet | undefined {
  return indexBetsByMatchId(bets).get(matchId)
}

export type BetSaveOperation =
  | {
      type: 'create'
      request: CreateBetRequest
    }
  | {
      type: 'update'
      command: UpdateBetCommand
    }

export function buildBetSaveOperation(
  matchId: number,
  existingBet: MyBet | undefined,
  goals: UpdateBetRequest,
): BetSaveOperation {
  if (existingBet) {
    return {
      type: 'update',
      command: {
        id: existingBet.id,
        request: goals,
      },
    }
  }

  return {
    type: 'create',
    request: {
      matchId,
      ...goals,
    },
  }
}
