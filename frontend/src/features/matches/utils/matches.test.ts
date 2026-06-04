import { describe, expect, it } from 'vitest'

import type { MatchListItem } from '../../../types/matches'
import { formatMatchDateTime, isTodayMatch } from './dateTime'
import { getStageLabel, getStatusLabel } from './labels'
import { getPendingBetMatches, groupMatchesByRound, isMatchClosedByTime, splitMatchesByClosedState } from './matchLists'
import { formatMatchResult, hasMatchResult } from './result'

function buildMatch(overrides: Partial<MatchListItem> = {}): MatchListItem {
  const id = overrides.id ?? 1

  return {
    id,
    homeTeam: { id: id * 10, name: `Home ${id}`, code: `H${id}`, flagUrl: null },
    awayTeam: { id: id * 10 + 1, name: `Away ${id}`, code: `A${id}`, flagUrl: null },
    matchDate: '2026-06-11T19:00:00Z',
    stage: 'Groups',
    status: 'Scheduled',
    homeGoals: null,
    awayGoals: null,
    isBettingOpen: true,
    isBettingLocked: false,
    ...overrides,
  }
}

describe('match display helpers', () => {
  it('formats match date using Intl and the selected timezone', () => {
    expect(formatMatchDateTime('2026-06-11T19:00:00', 'pt-BR', 'America/Sao_Paulo')).toContain('16:00')
  })

  it('identifies today matches using the displayed local date', () => {
    const now = new Date('2026-06-11T12:00:00-03:00')

    expect(isTodayMatch('2026-06-11T23:00:00Z', now, 'pt-BR', 'America/Sao_Paulo')).toBe(true)
    expect(isTodayMatch('2026-06-12T03:00:00Z', now, 'pt-BR', 'America/Sao_Paulo')).toBe(false)
  })

  it('formats only complete results', () => {
    expect(hasMatchResult({ homeGoals: 2, awayGoals: 1 })).toBe(true)
    expect(formatMatchResult({ homeGoals: 2, awayGoals: 1 })).toBe('2 x 1')
    expect(hasMatchResult({ homeGoals: 2, awayGoals: null })).toBe(false)
    expect(formatMatchResult({ homeGoals: 2, awayGoals: null })).toBe('Sem resultado')
  })

  it('maps known stage and status labels with safe fallback', () => {
    expect(getStageLabel('RoundOf16')).toBe('Oitavas')
    expect(getStageLabel('UnknownStage')).toBe('UnknownStage')
    expect(getStatusLabel('Finished')).toBe('Encerrada')
    expect(getStatusLabel('UnknownStatus')).toBe('UnknownStatus')
  })
})

describe('match list helpers', () => {
  it('moves matches to closed at the exact five-hour boundary', () => {
    const match = buildMatch({ matchDate: '2026-06-11T19:00:00Z' })

    expect(isMatchClosedByTime(match, new Date('2026-06-11T23:59:59Z'))).toBe(false)
    expect(isMatchClosedByTime(match, new Date('2026-06-12T00:00:00Z'))).toBe(true)
  })

  it('splits active and closed matches by match date plus five hours', () => {
    const activeMatch = buildMatch({ id: 1, matchDate: '2026-06-11T20:00:00Z' })
    const closedMatch = buildMatch({ id: 2, matchDate: '2026-06-11T10:00:00Z' })

    expect(splitMatchesByClosedState([activeMatch, closedMatch], new Date('2026-06-11T15:00:00Z'))).toEqual({
      activeMatches: [activeMatch],
      closedMatches: [closedMatch],
    })
  })

  it('finds pending bets only for active open matches without the current user bet', () => {
    const openWithoutBet = buildMatch({ id: 1, isBettingOpen: true, matchDate: '2026-06-11T19:00:00Z' })
    const openWithBet = buildMatch({ id: 2, isBettingOpen: true, matchDate: '2026-06-11T20:00:00Z' })
    const closedWindow = buildMatch({ id: 3, isBettingOpen: false, matchDate: '2026-06-11T21:00:00Z' })
    const closedByTime = buildMatch({ id: 4, isBettingOpen: true, matchDate: '2026-06-11T10:00:00Z' })

    const pendingMatches = getPendingBetMatches(
      [openWithBet, closedWindow, closedByTime, openWithoutBet],
      new Map([[2, {}]]),
      new Date('2026-06-11T15:00:00Z'),
    )

    expect(pendingMatches.map((match) => match.id)).toEqual([1])
  })

  it('groups group-stage matches into rodadas and then knockout stages', () => {
    const roundOneFirstMatch = buildMatch({ id: 1, matchDate: '2026-06-11T15:00:00Z' })
    const roundOneSecondMatch = buildMatch({ id: 2, matchDate: '2026-06-11T18:00:00Z' })
    const roundTwoFirstMatch = buildMatch({
      id: 3,
      homeTeam: roundOneFirstMatch.homeTeam,
      awayTeam: roundOneSecondMatch.homeTeam,
      matchDate: '2026-06-18T15:00:00Z',
    })
    const roundTwoSecondMatch = buildMatch({
      id: 4,
      homeTeam: roundOneFirstMatch.awayTeam,
      awayTeam: roundOneSecondMatch.awayTeam,
      matchDate: '2026-06-18T18:00:00Z',
    })
    const roundOf16Match = buildMatch({ id: 5, matchDate: '2026-06-28T15:00:00Z', stage: 'RoundOf16' })

    const roundGroups = groupMatchesByRound(
      [roundTwoSecondMatch, roundOf16Match, roundOneSecondMatch, roundTwoFirstMatch, roundOneFirstMatch],
      new Map(),
      new Date('2026-06-08T12:00:00Z'),
    )

    expect(roundGroups.map((group) => group.label)).toEqual(['Rodada 1', 'Rodada 2', 'Oitavas'])
    expect(roundGroups[0].matches.map((match) => match.id)).toEqual([1, 2])
    expect(roundGroups[1].matches.map((match) => match.id)).toEqual([3, 4])
    expect(roundGroups[2].matches.map((match) => match.id)).toEqual([5])
  })

  it('marks round groups as complete only when there are no open pending bets', () => {
    const openWithoutBet = buildMatch({ id: 1, isBettingOpen: true, matchDate: '2026-06-11T19:00:00Z' })
    const openWithBet = buildMatch({ id: 2, isBettingOpen: true, matchDate: '2026-06-12T19:00:00Z' })
    const closedWindowWithoutBet = buildMatch({ id: 3, isBettingOpen: false, matchDate: '2026-06-13T19:00:00Z' })
    const now = new Date('2026-06-11T15:00:00Z')

    const pendingGroup = groupMatchesByRound(
      [openWithBet, closedWindowWithoutBet, openWithoutBet],
      new Map([[2, {}]]),
      now,
    )[0]

    expect(pendingGroup.pendingCount).toBe(1)
    expect(pendingGroup.isComplete).toBe(false)
    expect(pendingGroup.matches.map((match) => match.id)).toEqual([1, 2, 3])

    const completeGroup = groupMatchesByRound(
      [openWithBet, closedWindowWithoutBet, openWithoutBet],
      new Map([
        [1, {}],
        [2, {}],
      ]),
      now,
    )[0]

    expect(completeGroup.pendingCount).toBe(0)
    expect(completeGroup.isComplete).toBe(true)
  })
})
