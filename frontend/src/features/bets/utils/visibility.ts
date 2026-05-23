import type { MatchStatus } from '../../../types/matches'
import { parseUtcDate } from '../../matches/utils/dateTime'

type MatchStartInput = {
  matchDate: string
  status?: MatchStatus | null
}

export function hasMatchStarted(match: MatchStartInput, now = new Date()): boolean {
  if (match.status === 'InProgress' || match.status === 'Finished') {
    return true
  }

  if (match.status === 'Scheduled') {
    return false
  }

  return parseUtcDate(match.matchDate).getTime() <= now.getTime()
}

export function canShowExternalBets(match: MatchStartInput, now = new Date()): boolean {
  return hasMatchStarted(match, now)
}
