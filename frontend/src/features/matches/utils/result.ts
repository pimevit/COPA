import type { MatchListItem } from '../../../types/matches'

export function hasMatchResult(match: Pick<MatchListItem, 'homeGoals' | 'awayGoals'>): boolean {
  return match.homeGoals !== null && match.homeGoals !== undefined
    && match.awayGoals !== null && match.awayGoals !== undefined
}

export function formatMatchResult(match: Pick<MatchListItem, 'homeGoals' | 'awayGoals'>): string {
  return hasMatchResult(match) ? `${match.homeGoals} x ${match.awayGoals}` : 'Sem resultado'
}
