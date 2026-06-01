export function formatRankingPosition(position: number): string {
  if (!Number.isFinite(position) || position < 1) {
    return '-'
  }

  return `${position}o`
}

export function formatRankingPoints(points: number): string {
  if (!Number.isFinite(points)) {
    return '0 pts'
  }

  const normalizedPoints = Math.max(0, Math.trunc(points))

  return normalizedPoints === 1 ? '1 pt' : `${normalizedPoints} pts`
}

export function formatRankingDateTime(value: string): string {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return '-'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(date)
}
