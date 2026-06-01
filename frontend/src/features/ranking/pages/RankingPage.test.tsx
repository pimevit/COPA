import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import '@testing-library/jest-dom/vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { RankingPage } from './RankingPage'
import type { RankingItem, RankingTieBreakers } from '../../../types/ranking'
import { useRanking } from '../hooks/useRanking'

vi.mock('../hooks/useRanking', async () => {
  const actual = await vi.importActual<typeof import('../hooks/useRanking')>('../hooks/useRanking')

  return {
    ...actual,
    useRanking: vi.fn(),
  }
})

const mockedUseRanking = vi.mocked(useRanking)

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
]

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

function mockRankingState(overrides: Partial<ReturnType<typeof useRanking>>) {
  mockedUseRanking.mockReturnValue({
    data: undefined,
    dataUpdatedAt: 0,
    error: null,
    isError: false,
    isFetching: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof useRanking>)
}

function renderRankingPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <RankingPage />
      </QueryClientProvider>
    </MemoryRouter>,
  )
}

describe('RankingPage', () => {
  beforeEach(() => {
    mockedUseRanking.mockReset()
  })

  it('renders loading state', () => {
    mockRankingState({ isPending: true })

    renderRankingPage()

    expect(screen.getByText('Carregando ranking...')).toBeInTheDocument()
  })

  it('renders error state with retry', () => {
    mockRankingState({ error: new Error('Falha de rede'), isError: true })

    renderRankingPage()

    expect(screen.getByText('Nao foi possivel carregar o ranking.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })

  it('renders empty state', () => {
    mockRankingState({ data: [] })

    renderRankingPage()

    expect(screen.getByText('Nenhum usuario no ranking.')).toBeInTheDocument()
  })

  it('renders ranking, top three and current user highlight', () => {
    mockRankingState({ data: rankingItems, dataUpdatedAt: new Date('2026-06-11T12:00:00Z').getTime() })

    renderRankingPage()

    expect(screen.getByRole('heading', { name: 'Top 3' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Classificacao' })).toBeInTheDocument()
    expect(screen.getAllByText('Bruno Costa')).toHaveLength(2)
    expect(screen.getAllByText('voce')).toHaveLength(2)
    expect(screen.getByText(/Atualizado/)).toBeInTheDocument()
  })
})
