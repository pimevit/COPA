import type { MatchTeamSummary } from '../../../types/matches'

type MatchTeamsProps = {
  homeTeam: MatchTeamSummary
  awayTeam: MatchTeamSummary
}

function TeamFlag({ team }: { team: MatchTeamSummary }) {
  if (team.flagUrl) {
    return (
      <img
        alt={`Bandeira de ${team.name}`}
        className="h-8 w-10 rounded border border-slate-200 object-cover dark:border-slate-700"
        loading="lazy"
        src={team.flagUrl}
      />
    )
  }

  return (
    <span
      aria-label={`Bandeira nao informada para ${team.name}`}
      className="flex h-8 w-10 items-center justify-center rounded border border-slate-200 bg-slate-100 text-[10px] font-semibold text-slate-700 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
      role="img"
    >
      {team.code}
    </span>
  )
}

function TeamName({ team }: { team: MatchTeamSummary }) {
  return (
    <div className="flex min-w-0 items-center gap-3">
      <TeamFlag team={team} />
      <div className="min-w-0">
        <p className="truncate text-sm font-semibold text-slate-950 dark:text-slate-50">{team.name}</p>
        <p className="text-xs font-medium text-slate-500 dark:text-slate-400">{team.code}</p>
      </div>
    </div>
  )
}

export function MatchTeams({ homeTeam, awayTeam }: MatchTeamsProps) {
  return (
    <div className="grid min-w-0 grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] items-center gap-3">
      <TeamName team={homeTeam} />
      <span className="text-xs font-semibold uppercase text-slate-400 dark:text-slate-500">vs</span>
      <div className="min-w-0 justify-self-end text-right">
        <TeamName team={awayTeam} />
      </div>
    </div>
  )
}
