import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { MatchesPage } from './MatchesPage'
import type { MyBet, PublicBet } from '../../../types/bets'
import type { MatchListItem } from '../../../types/matches'
import {
  useBetVisibility,
  useMyBets,
  usePublicBets,
  useUpdateBetVisibilityMutation,
} from '../../bets/hooks/useBets'
import { useMatches } from '../hooks/useMatches'

vi.mock('../hooks/useMatches', () => ({
  useMatches: vi.fn(),
}))

vi.mock('../../bets/hooks/useBets', async () => {
  const actual = await vi.importActual<typeof import('../../bets/hooks/useBets')>('../../bets/hooks/useBets')

  return {
    ...actual,
    useBetVisibility: vi.fn(),
    useMyBets: vi.fn(),
    usePublicBets: vi.fn(),
    useUpdateBetVisibilityMutation: vi.fn(),
  }
})

const mockedUseMatches = vi.mocked(useMatches)
const mockedUseBetVisibility = vi.mocked(useBetVisibility)
const mockedUseMyBets = vi.mocked(useMyBets)
const mockedUsePublicBets = vi.mocked(usePublicBets)
const mockedUseUpdateBetVisibilityMutation = vi.mocked(useUpdateBetVisibilityMutation)

const baseMatch: MatchListItem = {
  id: 1,
  homeTeam: {
    id: 10,
    name: 'Brazil',
    code: 'BRA',
    flagUrl: 'https://flagcdn.com/w320/br.png',
  },
  awayTeam: {
    id: 20,
    name: 'Argentina',
    code: 'ARG',
    flagUrl: 'https://flagcdn.com/w320/ar.png',
  },
  matchDate: '2026-06-11T19:00:00Z',
  stage: 'Groups',
  status: 'Scheduled',
  homeGoals: null,
  awayGoals: null,
  isBettingOpen: true,
  isBettingLocked: false,
}

const baseBet: MyBet = {
  id: 30,
  matchId: 1,
  homeGoalsPrediction: 2,
  awayGoalsPrediction: 1,
  pointsEarned: 0,
  createdAt: '2026-06-10T12:00:00Z',
  match: {
    id: 1,
    homeTeam: baseMatch.homeTeam,
    awayTeam: baseMatch.awayTeam,
    matchDate: baseMatch.matchDate,
    stage: baseMatch.stage,
    status: baseMatch.status,
    homeGoals: baseMatch.homeGoals,
    awayGoals: baseMatch.awayGoals,
  },
}

const basePublicBet: PublicBet = {
  matchId: 1,
  userId: 40,
  userName: 'Ana',
  homeGoalsPrediction: 1,
  awayGoalsPrediction: 0,
  pointsEarned: 0,
  createdAt: '2026-06-10T12:00:00Z',
  isCurrentUser: false,
}

function mockMatchesState(overrides: Partial<ReturnType<typeof useMatches>>) {
  mockedUseMatches.mockReturnValue({
    data: undefined,
    error: null,
    isError: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof useMatches>)
}

function mockMyBetsState(overrides: Partial<ReturnType<typeof useMyBets>>) {
  mockedUseMyBets.mockReturnValue({
    data: [],
    error: null,
    isError: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof useMyBets>)
}

function mockBetVisibilityState(overrides: Partial<ReturnType<typeof useBetVisibility>>) {
  mockedUseBetVisibility.mockReturnValue({
    data: { showBetsPublicly: false },
    error: null,
    isError: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof useBetVisibility>)
}

function mockPublicBetsState(overrides: Partial<ReturnType<typeof usePublicBets>>) {
  mockedUsePublicBets.mockReturnValue({
    data: [],
    error: null,
    isError: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof usePublicBets>)
}

function mockUpdateVisibilityState(overrides: Partial<ReturnType<typeof useUpdateBetVisibilityMutation>>) {
  mockedUseUpdateBetVisibilityMutation.mockReturnValue({
    error: null,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue({ showBetsPublicly: true }),
    ...overrides,
  } as unknown as ReturnType<typeof useUpdateBetVisibilityMutation>)
}

function renderWithQueryClient(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </MemoryRouter>,
  )
}

describe('MatchesPage', () => {
  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    vi.useRealTimers()
    mockedUseMatches.mockReset()
    mockedUseBetVisibility.mockReset()
    mockedUseMyBets.mockReset()
    mockedUsePublicBets.mockReset()
    mockedUseUpdateBetVisibilityMutation.mockReset()
    mockBetVisibilityState({})
    mockMyBetsState({})
    mockPublicBetsState({})
    mockUpdateVisibilityState({})
  })

  it('renders loading state', () => {
    mockMatchesState({ isPending: true })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Carregando partidas...')).toBeInTheDocument()
  })

  it('renders error state', () => {
    mockMatchesState({ error: new Error('Falha de rede'), isError: true })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Nao foi possivel carregar as partidas.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })

  it('renders empty state', () => {
    mockMatchesState({ data: [] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Nenhuma partida encontrada.')).toBeInTheDocument()
  })

  it('renders matches sorted by date with result and betting badges', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-11T12:00:00-03:00'))

    const laterMatch: MatchListItem = {
      ...baseMatch,
      id: 2,
      matchDate: '2026-06-12T19:00:00Z',
      homeGoals: 1,
      awayGoals: 1,
      isBettingOpen: false,
    }

    mockMatchesState({ data: [laterMatch, baseMatch] })
    mockMyBetsState({ data: [baseBet] })

    renderWithQueryClient(<MatchesPage />)

    const listItems = screen.getAllByRole('listitem')

    expect(listItems[0]).toHaveTextContent('Brazil')
    expect(listItems[0]).toHaveTextContent('Janela aberta')
    expect(listItems[0]).toHaveTextContent('Hoje')
    expect(listItems[0]).toHaveTextContent('2 x 1')
    expect(listItems[1]).toHaveTextContent('1 x 1')
    expect(listItems[1]).toHaveTextContent('Janela fechada')
  })

  it('renders the visibility control and blocks public bets when hidden', () => {
    mockMatchesState({ data: [baseMatch] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByRole('checkbox', { name: 'Publico' })).not.toBeChecked()
    expect(screen.getByText('Seus palpites estao ocultos. A lista dos jogadores tambem fica bloqueada.')).toBeInTheDocument()
    expect(mockedUsePublicBets).toHaveBeenCalledWith(false)
  })

  it('renders public bets when visibility is public', () => {
    mockMatchesState({ data: [baseMatch] })
    mockBetVisibilityState({ data: { showBetsPublicly: true } })
    mockPublicBetsState({ data: [basePublicBet] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByRole('checkbox', { name: 'Publico' })).toBeChecked()
    expect(screen.getByText('Palpites dos jogadores')).toBeInTheDocument()
    expect(screen.getByText('Ana')).toBeInTheDocument()
    expect(screen.getByText('1 x 0')).toBeInTheDocument()
    expect(mockedUsePublicBets).toHaveBeenCalledWith(true)
  })

  it('updates visibility from the control', async () => {
    const mutateAsync = vi.fn().mockResolvedValue({ showBetsPublicly: true })
    mockMatchesState({ data: [baseMatch] })
    mockUpdateVisibilityState({ mutateAsync } as Partial<ReturnType<typeof useUpdateBetVisibilityMutation>>)

    renderWithQueryClient(<MatchesPage />)

    fireEvent.click(screen.getByRole('checkbox', { name: 'Publico' }))

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith({ showBetsPublicly: true })
    })
  })
})
