import type { MatchListItem } from '../../../types/matches'
import { parseUtcDate } from './dateTime'

export const CLOSED_MATCH_OFFSET_MS = 5 * 60 * 60 * 1000

type BetsByMatchId = ReadonlyMap<number, unknown>

export type MatchTabGroups = {
  activeMatches: MatchListItem[]
  closedMatches: MatchListItem[]
}

export function getMatchClosedAt(matchDate: string): Date {
  return new Date(parseUtcDate(matchDate).getTime() + CLOSED_MATCH_OFFSET_MS)
}

export function isMatchClosedByTime(match: Pick<MatchListItem, 'matchDate'>, now = new Date()): boolean {
  return now.getTime() >= getMatchClosedAt(match.matchDate).getTime()
}

export function compareMatchesByDate(first: { matchDate: string }, second: { matchDate: string }): number {
  return parseUtcDate(first.matchDate).getTime() - parseUtcDate(second.matchDate).getTime()
}

export function sortMatchesByDate<T extends { matchDate: string }>(matches: readonly T[]): T[] {
  return [...matches].sort(compareMatchesByDate)
}

export function splitMatchesByClosedState(matches: readonly MatchListItem[], now = new Date()): MatchTabGroups {
  const activeMatches: MatchListItem[] = []
  const closedMatches: MatchListItem[] = []

  for (const match of sortMatchesByDate(matches)) {
    if (isMatchClosedByTime(match, now)) {
      closedMatches.push(match)
    } else {
      activeMatches.push(match)
    }
  }

  return { activeMatches, closedMatches }
}

export function isPendingBetMatch(match: MatchListItem, betsByMatchId: BetsByMatchId, now = new Date()): boolean {
  return !isMatchClosedByTime(match, now) && match.isBettingOpen && !betsByMatchId.has(match.id)
}

export function getPendingBetMatches(
  matches: readonly MatchListItem[],
  betsByMatchId: BetsByMatchId,
  now = new Date(),
): MatchListItem[] {
  return sortMatchesByDate(matches.filter((match) => isPendingBetMatch(match, betsByMatchId, now)))
}

export function sortActiveMatchesByPendingAndDate(
  matches: readonly MatchListItem[],
  betsByMatchId: BetsByMatchId,
  now = new Date(),
): MatchListItem[] {
  return [...matches].sort((first, second) => {
    const firstIsPending = isPendingBetMatch(first, betsByMatchId, now)
    const secondIsPending = isPendingBetMatch(second, betsByMatchId, now)

    if (firstIsPending !== secondIsPending) {
      return firstIsPending ? -1 : 1
    }

    return compareMatchesByDate(first, second)
  })
}
