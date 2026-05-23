import '@testing-library/jest-dom/vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'

import { AdminRoute } from './AdminRoute'
import { useAuthStore } from '../stores/authStore'

function setSession(roles: string[]) {
  useAuthStore.getState().setSession({
    accessToken: 'valid-token',
    expiresAtUtc: '2026-06-11T12:00:00Z',
    user: {
      id: 1,
      name: 'Felipe',
      email: 'felipe@example.com',
      createdAt: '2026-06-01T12:00:00Z',
      roles,
    },
  })
}

function renderRoute() {
  return render(
    <MemoryRouter initialEntries={['/admin']}>
      <Routes>
        <Route element={<AdminRoute />}>
          <Route element={<div>Admin content</div>} path="/admin" />
        </Route>
        <Route element={<div>Login route</div>} path="/login" />
      </Routes>
    </MemoryRouter>,
  )
}

describe('AdminRoute', () => {
  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    useAuthStore.getState().clearSession()
    localStorage.clear()
  })

  it('redirects anonymous users to login', () => {
    renderRoute()

    expect(screen.getByText('Login route')).toBeInTheDocument()
  })

  it('blocks authenticated users without admin role', () => {
    setSession([])

    renderRoute()

    expect(screen.getByText('Acesso restrito a administradores')).toBeInTheDocument()
    expect(screen.queryByText('Admin content')).not.toBeInTheDocument()
  })

  it('renders admin content for users with admin role', () => {
    setSession(['Admin'])

    renderRoute()

    expect(screen.getByText('Admin content')).toBeInTheDocument()
  })
})
