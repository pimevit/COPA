import '@testing-library/jest-dom/vitest'
import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { RankingList } from './RankingList'
import { RankingRow } from './RankingRow'
import { TopThreeHighlight } from './TopThreeHighlight'
import type { RankingItem } from '../../../types/ranking'

const rankingItems: RankingItem[] = [
  { position: 1, userId: 10, name: 'Ana Silva', points: 20, isTop3: true, isCurrentUser: false },
  { position: 2, userId: 20, name: 'Bruno Costa', points: 18, isTop3: true, isCurrentUser: true },
  { position: 3, userId: 30, name: 'Carla Souza', points: 16, isTop3: true, isCurrentUser: false },
  { position: 4, userId: 40, name: 'Diego Lima', points: 8, isTop3: false, isCurrentUser: false },
]

afterEach(() => {
  cleanup()
})

describe('RankingRow', () => {
  it('renders base data and current user marker', () => {
    render(<RankingRow item={rankingItems[1]} />)

    expect(screen.getByText('2o')).toBeInTheDocument()
    expect(screen.getByText('Bruno Costa')).toBeInTheDocument()
    expect(screen.getByText('18 pts')).toBeInTheDocument()
    expect(screen.getByText('voce')).toBeInTheDocument()
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
