import type { MatchListItem } from '../../../types/matches'
import { parseUtcDate } from './dateTime'
import { getStageLabel } from './labels'

export const CLOSED_MATCH_OFFSET_MS = 5 * 60 * 60 * 1000

type BetsByMatchId = ReadonlyMap<number, unknown>
type RoundGroupDraft = {
  key: string
  label: string
  matches: MatchListItem[]
  order: number
}

export type MatchTabGroups = {
  activeMatches: MatchListItem[]
  closedMatches: MatchListItem[]
}

export type MatchRoundGroup = {
  key: string
  label: string
  matches: MatchListItem[]
  pendingCount: number
  isComplete: boolean
}

const knockoutStageOrder = ['RoundOf16', 'QuarterFinals', 'SemiFinals', 'Final']

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

function hasRepeatedRoundTeam(match: MatchListItem, roundTeamIds: ReadonlySet<number>): boolean {
  return roundTeamIds.has(match.homeTeam.id) || roundTeamIds.has(match.awayTeam.id)
}

function buildGroupStageRoundGroups(matches: readonly MatchListItem[]): RoundGroupDraft[] {
  const roundGroups: RoundGroupDraft[] = []
  let roundMatches: MatchListItem[] = []
  let roundTeamIds = new Set<number>()

  function pushCurrentRound() {
    if (roundMatches.length === 0) {
      return
    }

    const roundNumber = roundGroups.length + 1

    roundGroups.push({
      key: `groups:${roundNumber}`,
      label: `Rodada ${roundNumber}`,
      matches: roundMatches,
      order: roundNumber,
    })
    roundMatches = []
    roundTeamIds = new Set<number>()
  }

  for (const match of sortMatchesByDate(matches)) {
    if (roundMatches.length > 0 && hasRepeatedRoundTeam(match, roundTeamIds)) {
      pushCurrentRound()
    }

    roundMatches.push(match)
    roundTeamIds.add(match.homeTeam.id)
    roundTeamIds.add(match.awayTeam.id)
  }

  pushCurrentRound()

  return roundGroups
}

function buildStageRoundGroups(matches: readonly MatchListItem[]): RoundGroupDraft[] {
  const groups = new Map<string, MatchListItem[]>()

  for (const match of sortMatchesByDate(matches)) {
    const key = `stage:${match.stage}`
    groups.set(key, [...(groups.get(key) ?? []), match])
  }

  return [...groups.entries()]
    .map(([key, stageMatches]) => {
      const stage = stageMatches[0]?.stage ?? key
      const knownStageIndex = knockoutStageOrder.indexOf(stage)
      const firstMatchTime = parseUtcDate(stageMatches[0]?.matchDate ?? '').getTime()

      return {
        key,
        label: getStageLabel(stage),
        matches: stageMatches,
        order: knownStageIndex >= 0 ? 100 + knownStageIndex : 1000 + firstMatchTime,
      }
    })
    .sort((first, second) => first.order - second.order)
}

export function groupMatchesByRound(
  matches: readonly MatchListItem[],
  betsByMatchId: BetsByMatchId,
  now = new Date(),
): MatchRoundGroup[] {
  const groupStageMatches = matches.filter((match) => match.stage === 'Groups')
  const stageMatches = matches.filter((match) => match.stage !== 'Groups')
  const roundGroups = [...buildGroupStageRoundGroups(groupStageMatches), ...buildStageRoundGroups(stageMatches)]

  return roundGroups
    .sort((first, second) => first.order - second.order)
    .map((group) => {
      const sortedMatches = sortMatchesByDate(group.matches)
      const pendingCount = sortedMatches.filter((match) => isPendingBetMatch(match, betsByMatchId, now)).length

      return {
        key: group.key,
        label: group.label,
        matches: sortedMatches,
        pendingCount,
        isComplete: pendingCount === 0,
      }
    })
}
