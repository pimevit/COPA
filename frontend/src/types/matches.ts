export type MatchTeamSummary = {
  id: number
  name: string
  code: string
  flagUrl?: string | null
}

export type MatchStage = 'Groups' | 'RoundOf16' | 'QuarterFinals' | 'SemiFinals' | 'Final' | string

export type MatchStatus = 'Scheduled' | 'InProgress' | 'Finished' | string

export type MatchListItem = {
  id: number
  homeTeam: MatchTeamSummary
  awayTeam: MatchTeamSummary
  matchDate: string
  stage: MatchStage
  status: MatchStatus
  homeGoals?: number | null
  awayGoals?: number | null
  isBettingOpen: boolean
}

export type MatchesQuery = {
  stage?: MatchStage
  status?: MatchStatus
}

export type CreateMatchRequest = {
  homeTeamId: number
  awayTeamId: number
  matchDate: string
  stage: MatchStage
}

export type UpdateMatchResultRequest = {
  homeGoals: number
  awayGoals: number
}
