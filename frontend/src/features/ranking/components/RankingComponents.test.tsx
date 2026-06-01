import '@testing-library/jest-dom/vitest'
import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { RankingList } from './RankingList'
import { RankingRow } from './RankingRow'
import { TopThreeHighlight } from './TopThreeHighlight'
import type { RankingItem, RankingTieBreakers } from '../../../types/ranking'

const rankingItems: RankingItem[] = [
  createRankingItem(1, 10, 'Ana Silva', 20, true, false, {
    exactScores: 3,
    outcomeHits: 6,
    bestHitStreak: 4,
    firstBetCreatedAtUtc: '2026-06-01T12:00:00Z',
  }),
  createRankingItem(2, 20, 'Bruno Costa', 18, true, true, {
    exactScores: 2,
    outcomeHits: 5,
    bestHitStreak: 3,
    firstBetCreatedAtUtc: '2026-06-01T12:05:00Z',
  }),
  createRankingItem(3, 30, 'Carla Souza', 16, true, false, {
    exactScores: 1,
    outcomeHits: 4,
    bestHitStreak: 2,
    firstBetCreatedAtUtc: '2026-06-01T12:10:00Z',
  }),
  createRankingItem(4, 40, 'Diego Lima', 8, false, false, {
    exactScores: 0,
    outcomeHits: 2,
    bestHitStreak: 1,
    firstBetCreatedAtUtc: '2026-06-01T12:15:00Z',
  }),
]

afterEach(() => {
  cleanup()
})

function createRankingItem(
  position: number,
  userId: number,
  name: string,
  points: number,
  isTop3: boolean,
  isCurrentUser: boolean,
  tieBreakers: RankingTieBreakers,
): RankingItem {
  return {
    position,
    userId,
    name,
    points,
    isTop3,
    isCurrentUser,
    tieBreakers,
  }
}

describe('RankingRow', () => {
  it('renders base data and current user marker', () => {
    render(<RankingRow item={rankingItems[1]} />)

    expect(screen.getByText('2o')).toBeInTheDocument()
    expect(screen.getByText('Bruno Costa')).toBeInTheDocument()
    expect(screen.getAllByText('18 pts')).toHaveLength(2)
    expect(screen.getByText('voce')).toBeInTheDocument()
  })

  it('renders tie-breaker tooltip data', () => {
    render(<RankingRow item={rankingItems[1]} />)

    expect(screen.getByRole('button', { name: 'Criterios de desempate de Bruno Costa' })).toBeInTheDocument()
    expect(screen.getByRole('tooltip')).toHaveTextContent('18 pts')
    expect(screen.getByRole('tooltip')).toHaveTextContent('Placares exatos: 2')
    expect(screen.getByRole('tooltip')).toHaveTextContent('Acertos de vencedor/empate: 5')
    expect(screen.getByRole('tooltip')).toHaveTextContent('Melhor sequencia: 3')
    expect(screen.getByRole('tooltip')).toHaveTextContent('Primeiro palpite:')
  })
})

describe('TopThreeHighlight', () => {
  it('renders one, two or three top users without breaking', () => {
    const { rerender } = render(<TopThreeHighlight items={rankingItems.slice(0, 1)} />)

    expect(screen.getByText('Ana Silva')).toBeInTheDocument()
    expect(screen.queryByText('Bruno Costa')).not.toBeInTheDocument()

    rerender(<TopThreeHighlight items={rankingItems.slice(0, 2)} />)

    expect(screen.getByText('Ana Silva')).toBeInTheDocument()
    expect(screen.getByText('Bruno Costa')).toBeInTheDocument()

    rerender(<TopThreeHighlight items={rankingItems} />)

    expect(screen.getByText('Ana Silva')).toBeInTheDocument()
    expect(screen.getByText('Bruno Costa')).toBeInTheDocument()
    expect(screen.getByText('Carla Souza')).toBeInTheDocument()
    expect(screen.queryByText('Diego Lima')).not.toBeInTheDocument()
  })

  it('renders tie-breaker tooltip controls for highlighted users', () => {
    render(<TopThreeHighlight items={rankingItems} />)

    expect(screen.getByRole('button', { name: 'Criterios de desempate de Ana Silva' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Criterios de desempate de Bruno Costa' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Criterios de desempate de Carla Souza' })).toBeInTheDocument()
  })
})

describe('RankingList', () => {
  it('preserves API order and highlights current user', () => {
    render(<RankingList items={rankingItems} />)

    const listItems = screen.getAllByRole('listitem')

    expect(within(listItems[0]).getByText('Ana Silva')).toBeInTheDocument()
    expect(within(listItems[1]).getByText('Bruno Costa')).toBeInTheDocument()
    expect(within(listItems[1]).getByText('voce')).toBeInTheDocument()
    expect(within(listItems[3]).getByText('Diego Lima')).toBeInTheDocument()
  })

  it('renders empty state', () => {
    render(<RankingList items={[]} />)

    expect(screen.getByText('Nenhum usuario no ranking.')).toBeInTheDocument()
  })
})
