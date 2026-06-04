import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
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
} from '../../bets/hooks/useBets'
import { useMatchNotice } from '../../notices/hooks/useMatchNotice'
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
  }
})

vi.mock('../../notices/hooks/useMatchNotice', async () => {
  const actual = await vi.importActual<typeof import('../../notices/hooks/useMatchNotice')>(
    '../../notices/hooks/useMatchNotice',
  )

  return {
    ...actual,
    useMatchNotice: vi.fn(),
  }
})

const mockedUseMatches = vi.mocked(useMatches)
const mockedUseBetVisibility = vi.mocked(useBetVisibility)
const mockedUseMyBets = vi.mocked(useMyBets)
const mockedUseMatchNotice = vi.mocked(useMatchNotice)
const mockedUsePublicBets = vi.mocked(usePublicBets)

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

function buildMatch(overrides: Partial<MatchListItem> = {}): MatchListItem {
  const id = overrides.id ?? baseMatch.id

  return {
    ...baseMatch,
    id,
    homeTeam: { id: id * 10, name: `Home ${id}`, code: `H${id}`, flagUrl: null },
    awayTeam: { id: id * 10 + 1, name: `Away ${id}`, code: `A${id}`, flagUrl: null },
    ...overrides,
  }
}

function buildBet(match: MatchListItem, overrides: Partial<MyBet> = {}): MyBet {
  return {
    ...baseBet,
    matchId: match.id,
    match: {
      id: match.id,
      homeTeam: match.homeTeam,
      awayTeam: match.awayTeam,
      matchDate: match.matchDate,
      stage: match.stage,
      status: match.status,
      homeGoals: match.homeGoals,
      awayGoals: match.awayGoals,
    },
    ...overrides,
  }
}

function buildPublicBet(overrides: Partial<PublicBet> = {}): PublicBet {
  return {
    ...basePublicBet,
    ...overrides,
  }
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
    data: { showBetsPublicly: true },
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

function mockMatchNoticeState(overrides: Partial<ReturnType<typeof useMatchNotice>>) {
  mockedUseMatchNotice.mockReturnValue({
    data: { message: '', updatedAtUtc: null },
    error: null,
    isError: false,
    isPending: false,
    refetch: vi.fn(),
    ...overrides,
  } as ReturnType<typeof useMatchNotice>)
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

function getRoundMatchesList(label = 'Rodada 1') {
  return screen.getByRole('list', { name: `Jogos da ${label}` })
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
    mockedUseMatchNotice.mockReset()
    mockedUsePublicBets.mockReset()
    mockBetVisibilityState({})
    mockMatchNoticeState({})
    mockMyBetsState({})
    mockPublicBetsState({})
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

  it('renders the matches notice when it has a message', () => {
    mockMatchesState({ data: [] })
    mockMatchNoticeState({
      data: {
        message: 'Palpites da rodada liberados ate sexta.',
        updatedAtUtc: '2026-06-01T12:00:00Z',
      },
    })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByRole('region', { name: 'Recado das partidas' })).toBeInTheDocument()
    expect(screen.getByText('Palpites da rodada liberados ate sexta.')).toBeInTheDocument()
  })

  it('does not block matches when the notice request fails', () => {
    mockMatchesState({ data: [baseMatch] })
    mockMatchNoticeState({
      data: undefined,
      error: new Error('Falha no recado'),
      isError: true,
    })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Brazil')).toBeInTheDocument()
    expect(screen.queryByRole('region', { name: 'Recado das partidas' })).not.toBeInTheDocument()
  })

  it('renders matches sorted by date with result and betting badges', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-11T12:00:00-03:00'))

    const laterMatch = buildMatch({
      id: 3,
      matchDate: '2026-06-12T19:00:00Z',
      homeGoals: 1,
      awayGoals: 1,
      isBettingOpen: false,
    })

    mockMatchesState({ data: [laterMatch, baseMatch] })
    mockMyBetsState({ data: [baseBet] })

    renderWithQueryClient(<MatchesPage />)

    const listItems = within(getRoundMatchesList()).getAllByRole('listitem')

    expect(listItems[0]).toHaveTextContent('Brazil')
    expect(listItems[0]).toHaveTextContent('Janela aberta')
    expect(listItems[0]).toHaveTextContent('Hoje')
    expect(listItems[0]).toHaveTextContent('2 x 1')
    expect(listItems[1]).toHaveTextContent('1 x 1')
    expect(listItems[1]).toHaveTextContent('Janela fechada')
  })

  it('shows pending count and orders active pending matches first', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-11T12:00:00-03:00'))

    const withBet = buildMatch({
      id: 1,
      homeTeam: { id: 10, name: 'Com palpite', code: 'CMP', flagUrl: null },
      matchDate: '2026-06-11T19:00:00Z',
    })
    const withoutBet = buildMatch({
      id: 2,
      homeTeam: { id: 20, name: 'Sem palpite', code: 'SEM', flagUrl: null },
      matchDate: '2026-06-12T19:00:00Z',
    })

    mockMatchesState({ data: [withBet, withoutBet] })
    mockMyBetsState({ data: [buildBet(withBet)] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Falta 1 jogo para voce palpitar.')).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveTextContent('1 para palpitar')
    expect(screen.getByRole('tab', { name: 'Jogos (2)' })).toHaveAttribute('aria-selected', 'true')

    const matchList = getRoundMatchesList()
    const listItems = within(matchList).getAllByRole('listitem')

    expect(listItems[0]).toHaveTextContent('Sem palpite')
    expect(listItems[0]).toHaveTextContent('Falta seu palpite')
    expect(listItems[1]).toHaveTextContent('Com palpite')
  })

  it('shows a positive message when there are no pending open bets', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-11T12:00:00-03:00'))

    mockMatchesState({ data: [baseMatch] })
    mockMyBetsState({ data: [baseBet] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Voce ja fez todos os palpites disponiveis.')).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveTextContent('0 para palpitar')
    expect(getRoundMatchesList()).toBeInTheDocument()
    expect(screen.queryByText('Historico de palpites')).not.toBeInTheDocument()
  })

  it('renders rodada and knockout tabs above active and closed games tabs', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-11T12:00:00-03:00'))

    const pendingMatch = buildMatch({
      id: 1,
      homeTeam: { id: 10, name: 'Rodada pendente', code: 'RPD', flagUrl: null },
      awayTeam: { id: 11, name: 'Visitante 1', code: 'V1', flagUrl: null },
      matchDate: '2026-06-11T19:00:00Z',
    })
    const completeMatch = buildMatch({
      id: 2,
      homeTeam: pendingMatch.homeTeam,
      awayTeam: { id: 20, name: 'Rodada completa', code: 'RCO', flagUrl: null },
      matchDate: '2026-06-18T19:00:00Z',
    })
    const roundOf16Match = buildMatch({
      id: 3,
      homeTeam: { id: 30, name: 'Oitavas mandante', code: 'OIM', flagUrl: null },
      awayTeam: { id: 31, name: 'Oitavas visitante', code: 'OIV', flagUrl: null },
      matchDate: '2026-06-28T19:00:00Z',
      stage: 'RoundOf16',
    })

    mockMatchesState({ data: [roundOf16Match, completeMatch, pendingMatch] })
    mockMyBetsState({ data: [buildBet(completeMatch)] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByRole('tab', { name: /Rodada 1/ })).toHaveTextContent('1 para palpitar')
    expect(screen.getByRole('tab', { name: /Rodada 2/ })).toHaveTextContent('0 para palpitar')
    expect(screen.getByRole('tab', { name: /Oitavas/ })).toHaveTextContent('1 para palpitar')
    expect(screen.getByRole('tab', { name: 'Jogos (1)' })).toHaveAttribute('aria-selected', 'true')
    expect(within(getRoundMatchesList()).getByText('Rodada pendente')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('tab', { name: /Rodada 2/ }))

    expect(screen.getByRole('tab', { name: /Rodada 2/ })).toHaveAttribute('aria-selected', 'true')
    expect(within(getRoundMatchesList('Rodada 2')).getByText('Rodada completa')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('tab', { name: /Oitavas/ }))

    expect(within(getRoundMatchesList('Oitavas')).getByText('Oitavas mandante')).toBeInTheDocument()
  })

  it('moves matches older than five hours to closed games tab', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-12T00:00:00Z'))

    const closedMatch = buildMatch({
      id: 1,
      homeTeam: { id: 10, name: 'Jogo encerrado', code: 'JEN', flagUrl: null },
      matchDate: '2026-06-11T19:00:00Z',
    })
    const activeMatch = buildMatch({
      id: 2,
      homeTeam: { id: 20, name: 'Jogo ativo', code: 'JAT', flagUrl: null },
      matchDate: '2026-06-12T19:00:00Z',
    })

    mockMatchesState({ data: [closedMatch, activeMatch] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByRole('tab', { name: 'Jogos (1)' })).toHaveAttribute('aria-selected', 'true')
    expect(screen.getByText('Jogo ativo')).toBeInTheDocument()
    expect(screen.queryByText('Jogo encerrado')).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('tab', { name: 'Jogos encerrados (1)' }))

    expect(screen.getByText('Jogo encerrado')).toBeInTheDocument()
    expect(screen.queryByText('Jogo ativo')).not.toBeInTheDocument()
  })

  it('renders a read-only bet summary for a closed betting window with an existing bet', () => {
    const closedMatch = buildMatch({ isBettingOpen: false })

    mockMatchesState({ data: [closedMatch] })
    mockMyBetsState({ data: [buildBet(closedMatch, { homeGoalsPrediction: 1, awayGoalsPrediction: 1 })] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Seu palpite: 1 x 1')).toBeInTheDocument()
    expect(screen.getByText('Palpites encerrados')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Atualizar palpite' })).not.toBeInTheDocument()
  })

  it('renders a read-only empty bet state for a closed betting window without a bet', () => {
    mockMatchesState({ data: [buildMatch({ isBettingOpen: false })] })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Você não registrou palpite para esta partida.')).toBeInTheDocument()
    expect(screen.getByText('Palpites encerrados')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Salvar palpite' })).not.toBeInTheDocument()
  })

  it('blocks public bets when user privacy is hidden', () => {
    mockMatchesState({ data: [baseMatch] })
    mockBetVisibilityState({ data: { showBetsPublicly: false } })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.queryByRole('checkbox', { name: 'Publico' })).not.toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Ver palpites' }))

    expect(screen.getByText('Seus palpites estao ocultos. A lista dos jogadores tambem fica bloqueada.')).toBeInTheDocument()
    expect(mockedUsePublicBets).toHaveBeenCalledWith(false)
  })

  it('renders public bets when visibility is public', () => {
    mockMatchesState({ data: [baseMatch] })
    mockBetVisibilityState({ data: { showBetsPublicly: true } })
    mockPublicBetsState({
      data: Array.from({ length: 6 }, (_, index) =>
        buildPublicBet({
          userId: 40 + index,
          userName: index === 0 ? 'Ana' : `Jogador ${index + 1}`,
          homeGoalsPrediction: index === 0 ? 1 : index + 1,
          awayGoalsPrediction: index === 0 ? 0 : index,
        }),
      ),
    })

    renderWithQueryClient(<MatchesPage />)

    expect(screen.getByText('Palpites dos jogadores')).toBeInTheDocument()
    expect(screen.queryByText('Ana')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Ver 6 palpites' })).toHaveAttribute('aria-expanded', 'false')

    fireEvent.click(screen.getByRole('button', { name: 'Ver 6 palpites' }))

    expect(screen.getByText('Ana')).toBeInTheDocument()
    expect(screen.getByText('1 x 0')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Ocultar palpites' })).toHaveAttribute('aria-expanded', 'true')

    fireEvent.click(screen.getByRole('button', { name: 'Ocultar palpites' }))

    expect(screen.queryByText('Ana')).not.toBeInTheDocument()
    expect(mockedUsePublicBets).toHaveBeenCalledWith(true)
  })
})
