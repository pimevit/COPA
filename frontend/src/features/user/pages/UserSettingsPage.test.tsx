import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { UserSettingsPage } from './UserSettingsPage'
import { useBetVisibility, useUpdateBetVisibilityMutation } from '../../bets/hooks/useBets'
import { useAuthStore } from '../../../stores/authStore'

vi.mock('../../bets/hooks/useBets', async () => {
  const actual = await vi.importActual<typeof import('../../bets/hooks/useBets')>('../../bets/hooks/useBets')

  return {
    ...actual,
    useBetVisibility: vi.fn(),
    useUpdateBetVisibilityMutation: vi.fn(),
  }
})

const mockedUseBetVisibility = vi.mocked(useBetVisibility)
const mockedUseUpdateBetVisibilityMutation = vi.mocked(useUpdateBetVisibilityMutation)

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

function mockUpdateVisibilityState(overrides: Partial<ReturnType<typeof useUpdateBetVisibilityMutation>>) {
  mockedUseUpdateBetVisibilityMutation.mockReturnValue({
    error: null,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue({ showBetsPublicly: false }),
    ...overrides,
  } as unknown as ReturnType<typeof useUpdateBetVisibilityMutation>)
}

function renderWithProviders(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('UserSettingsPage', () => {
  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    useAuthStore.getState().clearSession()
    useAuthStore.getState().setSession({
      accessToken: 'token',
      expiresAtUtc: '2099-01-01T00:00:00Z',
      user: {
        id: 1,
        name: 'Felipe',
        email: 'felipe@example.com',
        createdAt: '2026-01-01T00:00:00Z',
        roles: [],
      },
    })
    mockedUseBetVisibility.mockReset()
    mockedUseUpdateBetVisibilityMutation.mockReset()
    mockBetVisibilityState({})
    mockUpdateVisibilityState({})
  })

  it('renders user settings with public bets enabled by default', () => {
    renderWithProviders(<UserSettingsPage />)

    expect(screen.getByRole('heading', { name: 'Usuario' })).toBeInTheDocument()
    expect(screen.getAllByText('Felipe')).toHaveLength(2)
    expect(screen.getByRole('checkbox', { name: 'Publico' })).toBeChecked()
  })

  it('updates bet visibility from the user page', async () => {
    const mutateAsync = vi.fn().mockResolvedValue({ showBetsPublicly: false })
    mockUpdateVisibilityState({ mutateAsync } as Partial<ReturnType<typeof useUpdateBetVisibilityMutation>>)

    renderWithProviders(<UserSettingsPage />)

    fireEvent.click(screen.getByRole('checkbox', { name: 'Publico' }))

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalledWith({ showBetsPublicly: false })
    })
  })
})
