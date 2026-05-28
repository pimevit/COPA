import '@testing-library/jest-dom/vitest'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { RulesPage } from './RulesPage'

function renderRulesPage() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <RulesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('RulesPage', () => {
  it('renders betting rules from the backlog', () => {
    renderRulesPage()

    expect(screen.getByRole('heading', { name: 'Regras' })).toBeInTheDocument()
    expect(screen.getByText('Placar exato')).toBeInTheDocument()
    expect(screen.getByText('Final')).toBeInTheDocument()
    expect(
      screen.getByText(
        '* Os palpites sao bloqueados automaticamente 15 minutos antes na fase de grupos e 30 minutos antes no mata-mata.',
      ),
    ).toBeInTheDocument()
    expect(screen.getByText('Mais placares exatos.')).toBeInTheDocument()
  })
})
