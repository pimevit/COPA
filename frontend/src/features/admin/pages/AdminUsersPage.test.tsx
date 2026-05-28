import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { AdminUsersPage } from './AdminUsersPage'
import type { AdminUser } from '../../../types/adminUsers'
import { useAdminUsers, useResetUserPasswordMutation } from '../hooks/useAdminUsers'

vi.mock('../hooks/useAdminUsers', async () => {
  const actual = await vi.importActual<typeof import('../hooks/useAdminUsers')>('../hooks/useAdminUsers')

  return {
    ...actual,
    useAdminUsers: vi.fn(),
    useResetUserPasswordMutation: vi.fn(),
  }
})

const mockedUseAdminUsers = vi.mocked(useAdminUsers)
const mockedUseResetUserPasswordMutation = vi.mocked(useResetUserPasswordMutation)

const users: AdminUser[] = [
  {
    id: 1,
    name: 'Admin User',
    email: 'admin@example.com',
    createdAt: '2026-05-27T12:00:00Z',
  },
  {
    id: 2,
    name: 'Regular User',
    email: 'user@example.com',
    createdAt: '2026-05-27T12:30:00Z',
  },
]

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

describe('AdminUsersPage', () => {
  const refetch = vi.fn()
  const resetPassword = vi.fn()

  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    refetch.mockReset()
    resetPassword.mockReset()

    mockedUseAdminUsers.mockReturnValue({
      data: users,
      isError: false,
      isPending: false,
      refetch,
    } as unknown as ReturnType<typeof useAdminUsers>)
    mockedUseResetUserPasswordMutation.mockReturnValue({
      isPending: false,
      mutateAsync: resetPassword,
    } as unknown as ReturnType<typeof useResetUserPasswordMutation>)
  })

  it('renders admin users with reset actions', () => {
    renderWithProviders(<AdminUsersPage />)

    expect(screen.getByRole('heading', { name: 'Usuarios' })).toBeInTheDocument()
    expect(screen.getByText('Admin User')).toBeInTheDocument()
    expect(screen.getByText('user@example.com')).toBeInTheDocument()
    expect(screen.getAllByRole('button', { name: 'Resetar senha' })).toHaveLength(2)
  })

  it('does not reset password when confirmation is cancelled', () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false)

    renderWithProviders(<AdminUsersPage />)

    fireEvent.click(screen.getAllByRole('button', { name: 'Resetar senha' })[1])

    expect(resetPassword).not.toHaveBeenCalled()
    confirmSpy.mockRestore()
  })

  it('resets password and shows temporary password once', async () => {
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true)
    resetPassword.mockResolvedValue({
      userId: 2,
      name: 'Regular User',
      email: 'user@example.com',
      temporaryPassword: 'Aa2!zzzz',
    })

    renderWithProviders(<AdminUsersPage />)

    fireEvent.click(screen.getAllByRole('button', { name: 'Resetar senha' })[1])

    await waitFor(() => expect(resetPassword).toHaveBeenCalledWith(2))
    expect(screen.getByText('Senha temporaria gerada')).toBeInTheDocument()
    expect(screen.getByText('Aa2!zzzz')).toBeInTheDocument()
    confirmSpy.mockRestore()
  })
})
