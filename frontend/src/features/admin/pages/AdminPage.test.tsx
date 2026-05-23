import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { AdminPage } from './AdminPage'
import type { MatchListItem } from '../../../types/matches'
import type { Team } from '../../../types/teams'
import {
  useAdminMatches,
  useAdminTeams,
  useCreateMatchMutation,
  useUpdateMatchResultMutation,
} from '../hooks/useAdminMatches'

vi.mock('../hooks/useAdminMatches', async () => {
  const actual = await vi.importActual<typeof import('../hooks/useAdminMatches')>('../hooks/useAdminMatches')

  return {
    ...actual,
    useAdminTeams: vi.fn(),
    useAdminMatches: vi.fn(),
    useCreateMatchMutation: vi.fn(),
    useUpdateMatchResultMutation: vi.fn(),
  }
})

const mockedUseAdminTeams = vi.mocked(useAdminTeams)
const mockedUseAdminMatches = vi.mocked(useAdminMatches)
const mockedUseCreateMatchMutation = vi.mocked(useCreateMatchMutation)
const mockedUseUpdateMatchResultMutation = vi.mocked(useUpdateMatchResultMutation)

const teams: Team[] = [
  { id: 1, name: 'Brazil', code: 'BRA', flagUrl: null },
  { id: 2, name: 'Argentina', code: 'ARG', flagUrl: null },
]

const match: MatchListItem = {
  id: 10,
  homeTeam: teams[0],
  awayTeam: teams[1],
  matchDate: '2026-06-11T19:00:00Z',
  stage: 'Groups',
  status: 'Scheduled',
  homeGoals: null,
  awayGoals: null,
  isBettingOpen: true,
}

function renderWithProviders(children: ReactNode) {
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

describe('AdminPage', () => {
  const createMatch = vi.fn()
  const updateResult = vi.fn()

  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    createMatch.mockReset()
    updateResult.mockReset()

    mockedUseAdminTeams.mockReturnValue({
      data: teams,
      isError: false,
      isPending: false,
    } as ReturnType<typeof useAdminTeams>)
    mockedUseAdminMatches.mockReturnValue({
      data: [match],
      isError: false,
      isPending: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useAdminMatches>)
    mockedUseCreateMatchMutation.mockReturnValue({
      isPending: false,
      mutateAsync: createMatch,
    } as unknown as ReturnType<typeof useCreateMatchMutation>)
    mockedUseUpdateMatchResultMutation.mockReturnValue({
      isPending: false,
      mutateAsync: updateResult,
    } as unknown as ReturnType<typeof useUpdateMatchResultMutation>)
  })

  it('renders admin create and result forms', () => {
    renderWithProviders(<AdminPage />)

    expect(screen.getByRole('heading', { name: 'Painel administrativo' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Cadastrar partida' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Atualizar resultado' })).toBeInTheDocument()
    expect(screen.getByText('Brazil x Argentina')).toBeInTheDocument()
  })

  it('validates same team before creating match', () => {
    renderWithProviders(<AdminPage />)

    fireEvent.change(screen.getByLabelText('Mandante'), { target: { value: '1' } })
    fireEvent.change(screen.getByLabelText('Visitante'), { target: { value: '1' } })
    fireEvent.change(screen.getByLabelText('Data e hora'), { target: { value: '2026-06-20T15:00' } })
    fireEvent.click(screen.getByRole('button', { name: 'Cadastrar' }))

    expect(screen.getByText('Selecione times diferentes.')).toBeInTheDocument()
    expect(createMatch).not.toHaveBeenCalled()
  })

  it('saves a match result', async () => {
    updateResult.mockResolvedValue({})

    renderWithProviders(<AdminPage />)

    fireEvent.change(screen.getByLabelText('BRA'), { target: { value: '2' } })
    fireEvent.change(screen.getByLabelText('ARG'), { target: { value: '1' } })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar' }))

    await waitFor(() =>
      expect(updateResult).toHaveBeenCalledWith({
        matchId: 10,
        request: {
          homeGoals: 2,
          awayGoals: 1,
        },
      }),
    )
  })
})
