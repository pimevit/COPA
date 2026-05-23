import type { MatchStage, MatchStatus } from '../../../types/matches'

const stageLabels: Record<string, string> = {
  Groups: 'Grupos',
  RoundOf16: 'Oitavas',
  QuarterFinals: 'Quartas',
  SemiFinals: 'Semifinal',
  Final: 'Final',
}

const statusLabels: Record<string, string> = {
  Scheduled: 'Agendada',
  InProgress: 'Em andamento',
  Finished: 'Encerrada',
}

export function getStageLabel(stage: MatchStage): string {
  return stageLabels[stage] ?? String(stage)
}

export function getStatusLabel(status: MatchStatus): string {
  return statusLabels[status] ?? String(status)
}
