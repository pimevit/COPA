import { describe, expect, it } from 'vitest'

import { formatRankingPoints, formatRankingPosition } from './formatting'

describe('ranking formatting', () => {
  it('formats positions', () => {
    expect(formatRankingPosition(1)).toBe('1o')
    expect(formatRankingPosition(12)).toBe('12o')
    expect(formatRankingPosition(0)).toBe('-')
  })

  it('formats points', () => {
    expect(formatRankingPoints(0)).toBe('0 pts')
    expect(formatRankingPoints(1)).toBe('1 pt')
    expect(formatRankingPoints(15)).toBe('15 pts')
    expect(formatRankingPoints(-2)).toBe('0 pts')
  })
})
