import { describe, expect, it } from 'vitest'

import type { MatchListItem } from '../../../types/matches'
import { formatMatchDateTime, isTodayMatch } from './dateTime'
import { getStageLabel, getStatusLabel } from './labels'
import { getPendingBetMatches, isMatchClosedByTime, splitMatchesByClosedState } from './matchLists'
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
})
