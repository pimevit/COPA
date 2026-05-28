import '@testing-library/jest-dom/vitest'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { MatchListItem } from '../../../types/matches'
import { BetForm } from './BetForm'

const match: MatchListItem = {
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

describe('BetForm', () => {
  afterEach(() => {
    cleanup()
  })

  it('submits valid goals', () => {
    const onSubmit = vi.fn()

    render(<BetForm match={match} onSubmit={onSubmit} />)

    fireEvent.change(screen.getByLabelText('BRA'), { target: { value: '2' } })
    fireEvent.change(screen.getByLabelText('ARG'), { target: { value: '1' } })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar palpite' }))

    expect(onSubmit).toHaveBeenCalledWith({
      homeGoalsPrediction: 2,
      awayGoalsPrediction: 1,
    })
  })

  it('blocks invalid goals', () => {
    const onSubmit = vi.fn()

    render(<BetForm match={match} onSubmit={onSubmit} />)

    fireEvent.change(screen.getByLabelText('BRA'), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText('ARG'), { target: { value: '-1' } })
    fireEvent.click(screen.getByRole('button', { name: 'Salvar palpite' }))

    expect(screen.getByText('Informe os gols.')).toBeInTheDocument()
    expect(screen.getByText('Use um numero inteiro maior ou igual a zero.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('disables fields and submit when blocked by betting window', () => {
    render(<BetForm disabled match={match} onSubmit={vi.fn()} />)

    expect(screen.getByLabelText('BRA')).toBeDisabled()
    expect(screen.getByLabelText('ARG')).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Salvar palpite' })).toBeDisabled()
  })
})
