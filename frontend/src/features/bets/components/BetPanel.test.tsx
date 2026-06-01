import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { afterEach, describe, expect, it } from 'vitest'

import type { MyBet } from '../../../types/bets'
import type { MatchListItem } from '../../../types/matches'
import { BetPanel } from './BetPanel'

const baseMatch: MatchListItem = {
  id: 1,
  homeTeam: {
    id: 10,
    name: 'Brazil',
    code: 'BRA',
    flagUrl: null,
  },
  awayTeam: {
    id: 20,
    name: 'Argentina',
    code: 'ARG',
    flagUrl: null,
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
  homeGoalsPrediction: 1,
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

function renderWithQueryClient(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(<QueryClientProvider client={queryClient}>{children}</QueryClientProvider>)
}

describe('BetPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the bet form when betting is open', () => {
    renderWithQueryClient(<BetPanel existingBet={baseBet} match={baseMatch} />)

    expect(screen.getByLabelText('BRA')).toBeInTheDocument()
    expect(screen.getByLabelText('ARG')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Atualizar palpite' })).toBeInTheDocument()
    expect(screen.queryByText('Palpites encerrados')).not.toBeInTheDocument()
  })

  it('renders a read-only summary with the existing bet when betting is closed', () => {
    renderWithQueryClient(<BetPanel existingBet={baseBet} match={{ ...baseMatch, isBettingOpen: false }} />)

    expect(screen.getByText('Seu palpite: 1 x 1')).toBeInTheDocument()
    expect(screen.getByText('Palpites encerrados')).toBeInTheDocument()
    expect(screen.queryByText('Seu palpite')).not.toBeInTheDocument()
    expect(screen.queryByText('Consulta da partida.')).not.toBeInTheDocument()
    expect(screen.queryByText('1 x 1')).not.toBeInTheDocument()
    expect(screen.queryByLabelText('BRA')).not.toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Atualizar palpite' })).not.toBeInTheDocument()
  })

  it('renders a read-only empty state when betting is closed without a bet', () => {
    renderWithQueryClient(<BetPanel match={{ ...baseMatch, isBettingOpen: false }} />)

    expect(screen.getByText('Você não registrou palpite para esta partida.')).toBeInTheDocument()
    expect(screen.getByText('Palpites encerrados')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Salvar palpite' })).not.toBeInTheDocument()
  })
})
