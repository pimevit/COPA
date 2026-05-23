import { describe, expect, it } from 'vitest'

import { formatMatchDateTime, isTodayMatch } from './dateTime'
import { getStageLabel, getStatusLabel } from './labels'
import { formatMatchResult, hasMatchResult } from './result'

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
