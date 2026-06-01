export type RankingTieBreakers = {
  exactScores: number
  outcomeHits: number
  bestHitStreak: number
  firstBetCreatedAtUtc: string
}

export type RankingItem = {
  position: number
  userId: number
  name: string
  points: number
  isTop3: boolean
  isCurrentUser: boolean
  tieBreakers: RankingTieBreakers
}
