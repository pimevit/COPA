import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { AdminPage } from './AdminPage'
import { ApiError } from '../../../api/httpClient'
import type { MatchListItem } from '../../../types/matches'
import type { Team } from '../../../types/teams'
import { useMatchNotice, useUpdateMatchNoticeMutation } from '../../notices/hooks/useMatchNotice'
import {
  useClearApplicationDataMutation,
  useAdminMatches,
  useAdminTeams,
  useCreateMatchMutation,
  useDeleteMatchMutation,
  useImportBrasileiraoTeamsMutation,
  useImportWorldCupTeamsMutation,
  useUpdateMatchBettingLockMutation,
  useUpdateMatchResultMutation,
} from '../hooks/useAdminMatches'

vi.mock('../hooks/useAdminMatches', async () => {
  const actual = await vi.importActual<typeof import('../hooks/useAdminMatches')>('../hooks/useAdminMatches')

  return {
    ...actual,
    useAdminTeams: vi.fn(),
    useAdminMatches: vi.fn(),
    useCreateMatchMutation: vi.fn(),
    useDeleteMatchMutation: vi.fn(),
    useUpdateMatchBettingLockMutation: vi.fn(),
    useUpdateMatchResultMutation: vi.fn(),
    useImportBrasileiraoTeamsMutation: vi.fn(),
    useImportWorldCupTeamsMutation: vi.fn(),
    useClearApplicationDataMutation: vi.fn(),
  }
})

vi.mock('../../notices/hooks/useMatchNotice', async () => {
  const actual = await vi.importActual<typeof import('../../notices/hooks/useMatchNotice')>(
    '../../notices/hooks/useMatchNotice',
  )

  return {
    ...actual,
    useMatchNotice: vi.fn(),
    useUpdateMatchNoticeMutation: vi.fn(),
  }
})

const mockedUseClearApplicationDataMutation = vi.mocked(useClearApplicationDataMutation)
const mockedUseMatchNotice = vi.mocked(useMatchNotice)
const mockedUseUpdateMatchNoticeMutation = vi.mocked(useUpdateMatchNoticeMutation)
const mockedUseAdminTeams = vi.mocked(useAdminTeams)
const mockedUseAdminMatches = vi.mocked(useAdminMatches)
const mockedUseCreateMatchMutation = vi.mocked(useCreateMatchMutation)
const mockedUseDeleteMatchMutation = vi.mocked(useDeleteMatchMutation)
const mockedUseImportBrasileiraoTeamsMutation = vi.mocked(useImportBrasileiraoTeamsMutation)
const mockedUseImportWorldCupTeamsMutation = vi.mocked(useImportWorldCupTeamsMutation)
const mockedUseUpdateMatchBettingLockMutation = vi.mocked(useUpdateMatchBettingLockMutation)
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
  isBettingLocked: false,
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
  const clearApplicationData = vi.fn()
  const createMatch = vi.fn()
  const deleteMatch = vi.fn()
  const importBrasileiraoTeams = vi.fn()
  const importWorldCupTeams = vi.fn()
  const updateMatchNotice = vi.fn()
  const updateBettingLock = vi.fn()
  const updateResult = vi.fn()

  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    clearApplicationData.mockReset()
    createMatch.mockReset()
    deleteMatch.mockReset()
    importBrasileiraoTeams.mockReset()
    importWorldCupTeams.mockReset()
    updateMatchNotice.mockReset()
    updateBettingLock.mockReset()
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
    mockedUseMatchNotice.mockReturnValue({
      data: { message: '', updatedAtUtc: null },
      isError: false,
      isPending: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useMatchNotice>)
    mockedUseCreateMatchMutation.mockReturnValue({
      isPending: false,
      mutateAsync: createMatch,
    } as unknown as ReturnType<typeof useCreateMatchMutation>)
    mockedUseDeleteMatchMutation.mockReturnValue({
      isPending: false,
      mutateAsync: deleteMatch,
    } as unknown as ReturnType<typeof useDeleteMatchMutation>)
    mockedUseUpdateMatchBettingLockMutation.mockReturnValue({
      isPending: false,
      mutateAsync: updateBettingLock,
    } as unknown as ReturnType<typeof useUpdateMatchBettingLockMutation>)
    mockedUseUpdateMatchResultMutation.mockReturnValue({
      isPending: false,
      mutateAsync: updateResult,
    } as unknown as ReturnType<typeof useUpdateMatchResultMutation>)
    mockedUseImportBrasileiraoTeamsMutation.mockReturnValue({
      isPending: false,
      mutateAsync: importBrasileiraoTeams,
    } as unknown as ReturnType<typeof useImportBrasileiraoTeamsMutation>)
    mockedUseImportWorldCupTeamsMutation.mockReturnValue({
      isPending: false,
      mutateAsync: importWorldCupTeams,
    } as unknown as ReturnType<typeof useImportWorldCupTeamsMutation>)
    mockedUseClearApplicationDataMutation.mockReturnValue({
      isPending: false,
      mutateAsync: clearApplicationData,
    } as unknown as ReturnType<typeof useClearApplicationDataMutation>)
    mockedUseUpdateMatchNoticeMutation.mockReturnValue({
      isPending: false,
      mutateAsync: updateMatchNotice,
    } as unknown as ReturnType<typeof useUpdateMatchNoticeMutation>)
  })

  it('renders admin create and result forms', () => {
    renderWithProviders(<AdminPage />)

    expect(screen.getByRole('heading', { name: 'Painel administrativo' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Recado das partidas' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Recado exibido nas partidas' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Manutencao de dados' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Inserir Brasileirao Serie A 2026' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Inserir Copa 2026' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Limpar dados' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Cadastrar partida' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Gerenciar partidas' })).toBeInTheDocument()
    expect(screen.getByText('Brazil x Argentina')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Gerenciar usuarios' })).toHaveAttribute('href', '/admin/users')
    expect(screen.getByRole('button', { name: 'Bloquear palpites' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Excluir se sem palpites' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Excluir com palpites' })).toBeInTheDocument()
  })

  it('saves the matches notice', async () => {
    updateMatchNotice.mockResolvedValue({
      message: 'Rodada aberta ate sexta.',
      updatedAtUtc: '2026-06-01T12:00:00Z',
    })

    renderWithProviders(<AdminPage />)

    fireEvent.change(screen.getByRole('textbox', { name: 'Recado exibido nas partidas' }), {
      target: { value: 'Rodada aberta ate sexta.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar recado' }))

    await waitFor(() => expect(updateMatchNotice).toHaveBeenCalledWith({ message: 'Rodada aberta ate sexta.' }))
    expect(screen.getByText('Recado salvo.')).toBeInTheDocument()
  })

  it('clears the matches notice', async () => {
    mockedUseMatchNotice.mockReturnValue({
      data: { message: 'Aviso temporario', updatedAtUtc: '2026-06-01T12:00:00Z' },
      isError: false,
      isPending: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useMatchNotice>)
    updateMatchNotice.mockResolvedValue({
      message: '',
      updatedAtUtc: null,
    })

    renderWithProviders(<AdminPage />)

    expect(await screen.findByDisplayValue('Aviso temporario')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Limpar recado' }))

    await waitFor(() => expect(updateMatchNotice).toHaveBeenCalledWith({ message: '' }))
    expect(screen.getByText('Recado removido.')).toBeInTheDocument()
  })

  it('imports brasileirao teams from admin maintenance', async () => {
    importBrasileiraoTeams.mockResolvedValue({
      action: 'brasileirao-serie-a-2026',
      insertedTeams: 20,
      updatedTeams: 0,
      deletedBets: 0,
      deletedMatches: 0,
      deletedTeams: 0,
    })

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Inserir Brasileirao Serie A 2026' }))

    await waitFor(() => expect(importBrasileiraoTeams).toHaveBeenCalledTimes(1))
    expect(screen.getByText('Times processados: 20 inseridos e 0 atualizados.')).toBeInTheDocument()
  })

  it('imports world cup teams from admin maintenance', async () => {
    importWorldCupTeams.mockResolvedValue({
      action: 'world-cup-2026',
      insertedTeams: 48,
      updatedTeams: 0,
      deletedBets: 0,
      deletedMatches: 0,
      deletedTeams: 0,
    })

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Inserir Copa 2026' }))

    await waitFor(() => expect(importWorldCupTeams).toHaveBeenCalledTimes(1))
    expect(screen.getByText('Times processados: 48 inseridos e 0 atualizados.')).toBeInTheDocument()
  })

  it('does not clear application data when confirmation is cancelled', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false)

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Limpar dados' }))

    expect(clearApplicationData).not.toHaveBeenCalled()
    confirmSpy.mockRestore()
  })

  it('clears application data after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    clearApplicationData.mockResolvedValue({
      action: 'application-data-reset',
      insertedTeams: 0,
      updatedTeams: 0,
      deletedBets: 1,
      deletedMatches: 2,
      deletedTeams: 3,
    })

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Limpar dados' }))

    await waitFor(() => expect(clearApplicationData).toHaveBeenCalledTimes(1))
    expect(screen.getByText('Dados limpos: 1 palpites, 2 partidas e 3 times removidos.')).toBeInTheDocument()
    confirmSpy.mockRestore()
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

  it('blocks betting after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    updateBettingLock.mockResolvedValue({})

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Bloquear palpites' }))

    await waitFor(() =>
      expect(updateBettingLock).toHaveBeenCalledWith({
        matchId: 10,
        request: {
          isBettingLocked: true,
        },
      }),
    )
    expect(screen.getByText('Palpites bloqueados para a partida.')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })

  it('unblocks betting when match is manually locked', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    updateBettingLock.mockResolvedValue({})
    mockedUseAdminMatches.mockReturnValue({
      data: [{ ...match, isBettingOpen: false, isBettingLocked: true }],
      isError: false,
      isPending: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useAdminMatches>)

    renderWithProviders(<AdminPage />)

    expect(screen.getByText('Palpites bloqueados manualmente')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Desbloquear palpites' }))

    await waitFor(() =>
      expect(updateBettingLock).toHaveBeenCalledWith({
        matchId: 10,
        request: {
          isBettingLocked: false,
        },
      }),
    )
    expect(screen.getByText('Bloqueio manual removido.')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })

  it('does not delete match when confirmation is cancelled', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false)

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Excluir se sem palpites' }))

    expect(deleteMatch).not.toHaveBeenCalled()
    confirmSpy.mockRestore()
  })

  it('deletes a match safely after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    deleteMatch.mockResolvedValue({})

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Excluir se sem palpites' }))

    await waitFor(() => expect(deleteMatch).toHaveBeenCalledWith({ matchId: 10, deleteBets: false }))
    expect(screen.getByText('Partida excluida.')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })

  it('deletes a match with related bets after confirmation', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    deleteMatch.mockResolvedValue({})

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Excluir com palpites' }))

    await waitFor(() => expect(deleteMatch).toHaveBeenCalledWith({ matchId: 10, deleteBets: true }))
    expect(screen.getByText('Partida excluida.')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })

  it('shows a specific message when safe delete finds related bets', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    deleteMatch.mockRejectedValue(new ApiError(409, { title: 'Match has related bets.' }))

    renderWithProviders(<AdminPage />)

    fireEvent.click(screen.getByRole('button', { name: 'Excluir se sem palpites' }))

    expect(await screen.findByText('Esta partida tem palpites. Use "Excluir com palpites" para remover tudo.')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })
})
