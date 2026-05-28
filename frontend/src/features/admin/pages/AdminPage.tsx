import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useQueryClient } from '@tanstack/react-query'

import { ApiError } from '../../../api/httpClient'
import { myBetsQueryKey } from '../../bets/hooks/useBets'
import { matchesQueryKey } from '../../matches/hooks/useMatches'
import { formatMatchDateTime, parseUtcDate } from '../../matches/utils/dateTime'
import { getStageLabel, getStatusLabel } from '../../matches/utils/labels'
import { rankingQueryKey } from '../../ranking/hooks/useRanking'
import { AuthenticatedNav } from '../../../routes/AuthenticatedNav'
import type { MatchListItem, MatchStage } from '../../../types/matches'
import type { AdminMaintenanceResponse } from '../api/adminMaintenanceApi'
import {
  adminMatchesQueryKey,
  teamsQueryKey,
  useClearApplicationDataMutation,
  useAdminMatches,
  useAdminTeams,
  useCreateMatchMutation,
  useImportBrasileiraoTeamsMutation,
  useImportWorldCupTeamsMutation,
  useUpdateMatchResultMutation,
} from '../hooks/useAdminMatches'

const stages: MatchStage[] = ['Groups', 'RoundOf16', 'QuarterFinals', 'SemiFinals', 'Final']

type ResultRowProps = {
  match: MatchListItem
  isSaving: boolean
  onSave: (matchId: number, homeGoals: number, awayGoals: number) => Promise<void>
}

export function AdminPage() {
  const queryClient = useQueryClient()
  const teamsQuery = useAdminTeams()
  const matchesQuery = useAdminMatches()
  const createMatchMutation = useCreateMatchMutation()
  const updateResultMutation = useUpdateMatchResultMutation()
  const importBrasileiraoMutation = useImportBrasileiraoTeamsMutation()
  const importWorldCupMutation = useImportWorldCupTeamsMutation()
  const clearApplicationDataMutation = useClearApplicationDataMutation()
  const [homeTeamId, setHomeTeamId] = useState('')
  const [awayTeamId, setAwayTeamId] = useState('')
  const [stage, setStage] = useState<MatchStage>('Groups')
  const [matchDate, setMatchDate] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [createSuccess, setCreateSuccess] = useState<string | null>(null)
  const [resultError, setResultError] = useState<string | null>(null)
  const [resultSuccess, setResultSuccess] = useState<string | null>(null)
  const [maintenanceError, setMaintenanceError] = useState<string | null>(null)
  const [maintenanceSuccess, setMaintenanceSuccess] = useState<string | null>(null)

  const teams = teamsQuery.data ?? []
  const matches = useMemo(
    () =>
      [...(matchesQuery.data ?? [])].sort(
        (first, second) => parseUtcDate(first.matchDate).getTime() - parseUtcDate(second.matchDate).getTime(),
      ),
    [matchesQuery.data],
  )

  async function refreshAdminData() {
    await queryClient.invalidateQueries({ queryKey: teamsQueryKey })
    await queryClient.invalidateQueries({ queryKey: adminMatchesQueryKey })
    await queryClient.invalidateQueries({ queryKey: matchesQueryKey() })
    await queryClient.invalidateQueries({ queryKey: rankingQueryKey })
    await queryClient.invalidateQueries({ queryKey: myBetsQueryKey })
  }

  async function handleCreateMatch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)
    setCreateSuccess(null)

    if (!homeTeamId || !awayTeamId || !matchDate || !stage) {
      setFormError('Preencha todos os campos da partida.')
      return
    }

    if (homeTeamId === awayTeamId) {
      setFormError('Selecione times diferentes.')
      return
    }

    try {
      await createMatchMutation.mutateAsync({
        homeTeamId: Number(homeTeamId),
        awayTeamId: Number(awayTeamId),
        matchDate: new Date(matchDate).toISOString(),
        stage,
      })

      await refreshAdminData()
      setHomeTeamId('')
      setAwayTeamId('')
      setStage('Groups')
      setMatchDate('')
      setCreateSuccess('Partida cadastrada.')
    } catch (error) {
      setFormError(mapAdminError(error))
    }
  }

  async function handleSaveResult(matchId: number, homeGoals: number, awayGoals: number) {
    setResultError(null)
    setResultSuccess(null)

    try {
      await updateResultMutation.mutateAsync({
        matchId,
        request: {
          homeGoals,
          awayGoals,
        },
      })

      await refreshAdminData()
      setResultSuccess('Resultado salvo e pontuacao recalculada.')
    } catch (error) {
      setResultError(mapAdminError(error))
    }
  }

  async function runMaintenanceAction(action: () => Promise<AdminMaintenanceResponse>) {
    setMaintenanceError(null)
    setMaintenanceSuccess(null)

    try {
      const result = await action()
      await refreshAdminData()
      setMaintenanceSuccess(formatMaintenanceResult(result))
    } catch (error) {
      setMaintenanceError(mapAdminError(error, 'Nao foi possivel executar a acao.'))
    }
  }

  async function handleImportBrasileiraoTeams() {
    await runMaintenanceAction(() => importBrasileiraoMutation.mutateAsync())
  }

  async function handleImportWorldCupTeams() {
    await runMaintenanceAction(() => importWorldCupMutation.mutateAsync())
  }

  async function handleClearApplicationData() {
    var confirmed = window.confirm('Limpar palpites, partidas e times? Usuarios serao preservados.')

    if (!confirmed) {
      return
    }

    await runMaintenanceAction(() => clearApplicationDataMutation.mutateAsync())
  }

  const isMaintenancePending =
    importBrasileiraoMutation.isPending ||
    importWorldCupMutation.isPending ||
    clearApplicationDataMutation.isPending

  return (
    <main className="min-h-screen bg-slate-50 px-4 py-6 text-slate-950 dark:bg-slate-950 dark:text-slate-50 sm:px-6 lg:px-8">
      <section className="mx-auto flex w-full max-w-6xl flex-col gap-5">
        <header className="flex flex-col gap-3 border-b border-slate-200 pb-4 dark:border-slate-800">
          <AuthenticatedNav activePage="admin" />
          <div>
            <p className="text-sm font-medium uppercase tracking-normal text-emerald-700 dark:text-emerald-300">
              Administracao
            </p>
            <h1 className="text-2xl font-semibold tracking-normal sm:text-3xl">Painel administrativo</h1>
            <p className="mt-1 text-sm text-slate-600 dark:text-slate-300">
              Cadastre partidas e atualize resultados oficiais.
            </p>
          </div>
        </header>

        <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
          <h2 className="text-lg font-semibold tracking-normal">Manutencao de dados</h2>
          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <button
              className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
              disabled={isMaintenancePending}
              onClick={() => void handleImportBrasileiraoTeams()}
              type="button"
            >
              {importBrasileiraoMutation.isPending ? 'Inserindo...' : 'Inserir Brasileirao Serie A 2026'}
            </button>
            <button
              className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
              disabled={isMaintenancePending}
              onClick={() => void handleImportWorldCupTeams()}
              type="button"
            >
              {importWorldCupMutation.isPending ? 'Inserindo...' : 'Inserir Copa 2026'}
            </button>
            <button
              className="rounded-md border border-red-300 px-4 py-2 text-sm font-semibold text-red-700 hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:border-slate-300 disabled:text-slate-400 dark:border-red-900 dark:text-red-300 dark:hover:bg-red-950 dark:focus:ring-offset-slate-950"
              disabled={isMaintenancePending}
              onClick={() => void handleClearApplicationData()}
              type="button"
            >
              {clearApplicationDataMutation.isPending ? 'Limpando...' : 'Limpar dados'}
            </button>
          </div>

          {maintenanceError ? <FeedbackMessage tone="error" message={maintenanceError} /> : null}
          {maintenanceSuccess ? <FeedbackMessage tone="success" message={maintenanceSuccess} /> : null}
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
          <h2 className="text-lg font-semibold tracking-normal">Cadastrar partida</h2>
          <form className="mt-4 grid gap-4 md:grid-cols-2 lg:grid-cols-5" onSubmit={handleCreateMatch}>
            <label className="flex flex-col gap-1 text-sm font-medium text-slate-700 dark:text-slate-200">
              Mandante
              <select
                className="rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
                disabled={teamsQuery.isPending || createMatchMutation.isPending}
                onChange={(event) => setHomeTeamId(event.target.value)}
                value={homeTeamId}
              >
                <option value="">Selecione</option>
                {teams.map((team) => (
                  <option key={team.id} value={team.id}>
                    {team.name} ({team.code})
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm font-medium text-slate-700 dark:text-slate-200">
              Visitante
              <select
                className="rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
                disabled={teamsQuery.isPending || createMatchMutation.isPending}
                onChange={(event) => setAwayTeamId(event.target.value)}
                value={awayTeamId}
              >
                <option value="">Selecione</option>
                {teams.map((team) => (
                  <option key={team.id} value={team.id}>
                    {team.name} ({team.code})
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm font-medium text-slate-700 dark:text-slate-200">
              Fase
              <select
                className="rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
                disabled={createMatchMutation.isPending}
                onChange={(event) => setStage(event.target.value)}
                value={stage}
              >
                {stages.map((stageOption) => (
                  <option key={stageOption} value={stageOption}>
                    {getStageLabel(stageOption)}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm font-medium text-slate-700 dark:text-slate-200">
              Data e hora
              <input
                className="rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
                disabled={createMatchMutation.isPending}
                onChange={(event) => setMatchDate(event.target.value)}
                type="datetime-local"
                value={matchDate}
              />
            </label>

            <div className="flex items-end">
              <button
                className="w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
                disabled={createMatchMutation.isPending}
                type="submit"
              >
                {createMatchMutation.isPending ? 'Salvando...' : 'Cadastrar'}
              </button>
            </div>
          </form>

          {teamsQuery.isError ? (
            <p className="mt-3 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm font-medium text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              Nao foi possivel carregar os times.
            </p>
          ) : null}
          {formError ? <FeedbackMessage tone="error" message={formError} /> : null}
          {createSuccess ? <FeedbackMessage tone="success" message={createSuccess} /> : null}
        </section>

        <section className="rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-800 dark:bg-slate-950">
          <div className="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold tracking-normal">Atualizar resultado</h2>
              <p className="text-sm text-slate-600 dark:text-slate-300">
                Salvar um resultado fecha a partida e recalcula os pontos dos palpites.
              </p>
            </div>
            <button
              className="w-fit rounded-md border border-slate-300 px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-900"
              onClick={() => void matchesQuery.refetch()}
              type="button"
            >
              Recarregar
            </button>
          </div>

          {matchesQuery.isPending ? (
            <p className="mt-4 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Carregando partidas...
            </p>
          ) : null}

          {matchesQuery.isError ? (
            <p className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm font-medium text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              Nao foi possivel carregar as partidas.
            </p>
          ) : null}

          {!matchesQuery.isPending && !matchesQuery.isError && matches.length === 0 ? (
            <p className="mt-4 rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-300">
              Nenhuma partida cadastrada.
            </p>
          ) : null}

          {!matchesQuery.isPending && !matchesQuery.isError && matches.length > 0 ? (
            <ul className="mt-4 grid gap-3" aria-label="Partidas para atualizacao de resultado">
              {matches.map((match) => (
                <li key={match.id}>
                  <ResultRow
                    isSaving={updateResultMutation.isPending}
                    match={match}
                    onSave={handleSaveResult}
                  />
                </li>
              ))}
            </ul>
          ) : null}

          {resultError ? <FeedbackMessage tone="error" message={resultError} /> : null}
          {resultSuccess ? <FeedbackMessage tone="success" message={resultSuccess} /> : null}
        </section>
      </section>
    </main>
  )
}

function ResultRow({ isSaving, match, onSave }: ResultRowProps) {
  const [homeGoals, setHomeGoals] = useState(match.homeGoals?.toString() ?? '')
  const [awayGoals, setAwayGoals] = useState(match.awayGoals?.toString() ?? '')
  const [localError, setLocalError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setLocalError(null)

    if (homeGoals === '' || awayGoals === '') {
      setLocalError('Informe os dois placares.')
      return
    }

    var parsedHomeGoals = Number(homeGoals)
    var parsedAwayGoals = Number(awayGoals)

    if (!Number.isInteger(parsedHomeGoals) || !Number.isInteger(parsedAwayGoals) || parsedHomeGoals < 0 || parsedAwayGoals < 0) {
      setLocalError('Informe placares validos e maiores ou iguais a zero.')
      return
    }

    await onSave(match.id, parsedHomeGoals, parsedAwayGoals)
  }

  return (
    <form
      className="grid gap-3 rounded-lg border border-slate-200 p-3 dark:border-slate-800 md:grid-cols-[1fr_auto]"
      onSubmit={handleSubmit}
    >
      <div className="min-w-0">
        <p className="font-semibold text-slate-950 dark:text-slate-50">
          {match.homeTeam.name} x {match.awayTeam.name}
        </p>
        <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">
          {formatMatchDateTime(match.matchDate)} · {getStageLabel(match.stage)} · {getStatusLabel(match.status)}
        </p>
      </div>

      <div className="flex flex-col gap-2 sm:flex-row sm:items-start">
        <label className="flex flex-col gap-1 text-xs font-semibold text-slate-600 dark:text-slate-300">
          {match.homeTeam.code}
          <input
            className="h-10 w-24 rounded-md border border-slate-300 bg-white px-3 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
            disabled={isSaving}
            min={0}
            onChange={(event) => setHomeGoals(event.target.value)}
            type="number"
            value={homeGoals}
          />
        </label>
        <label className="flex flex-col gap-1 text-xs font-semibold text-slate-600 dark:text-slate-300">
          {match.awayTeam.code}
          <input
            className="h-10 w-24 rounded-md border border-slate-300 bg-white px-3 text-slate-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-50"
            disabled={isSaving}
            min={0}
            onChange={(event) => setAwayGoals(event.target.value)}
            type="number"
            value={awayGoals}
          />
        </label>
        <button
          className="mt-auto h-10 rounded-md bg-emerald-700 px-4 text-sm font-semibold text-white hover:bg-emerald-800 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-slate-400 dark:bg-emerald-500 dark:text-emerald-950 dark:hover:bg-emerald-400 dark:focus:ring-offset-slate-950"
          disabled={isSaving}
          type="submit"
        >
          {isSaving ? 'Salvando...' : 'Salvar'}
        </button>
      </div>

      {localError ? (
        <p className="text-sm font-medium text-red-700 dark:text-red-300 md:col-span-2">{localError}</p>
      ) : null}
    </form>
  )
}

function FeedbackMessage({ message, tone }: { message: string; tone: 'error' | 'success' }) {
  const classes =
    tone === 'error'
      ? 'border-red-200 bg-red-50 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-200'
      : 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-200'

  return <p className={`mt-3 rounded-md border px-3 py-2 text-sm font-medium ${classes}`}>{message}</p>
}

function formatMaintenanceResult(result: AdminMaintenanceResponse): string {
  if (result.action === 'application-data-reset') {
    return `Dados limpos: ${result.deletedBets} palpites, ${result.deletedMatches} partidas e ${result.deletedTeams} times removidos.`
  }

  return `Times processados: ${result.insertedTeams} inseridos e ${result.updatedTeams} atualizados.`
}

function mapAdminError(error: unknown, fallback = 'Nao foi possivel salvar.'): string {
  if (error instanceof ApiError) {
    if (error.status === 403) {
      return 'Acesso restrito a administradores.'
    }

    if (error.status === 409) {
      return 'Esta partida ja esta cadastrada.'
    }

    if (error.status === 404) {
      return 'Time ou partida nao encontrado.'
    }
  }

  return fallback
}
