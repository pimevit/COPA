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
