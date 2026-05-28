import type { MatchStage, MatchStatus, MatchTeamSummary } from './matches'

export type CreateBetRequest = {
  matchId: number
  homeGoalsPrediction: number
  awayGoalsPrediction: number
}

export type UpdateBetRequest = {
  homeGoalsPrediction: number
  awayGoalsPrediction: number
}

export type BetMatchSummary = {
  id: number
  homeTeam: MatchTeamSummary
  awayTeam: MatchTeamSummary
  matchDate: string
  stage: MatchStage
  status: MatchStatus
  homeGoals?: number | null
  awayGoals?: number | null
}

export type BetResponse = {
  id: number
  matchId: number
  homeGoalsPrediction: number
  awayGoalsPrediction: number
  pointsEarned: number
  createdAt: string
  match: BetMatchSummary
}

export type MyBet = BetResponse

export type BetVisibilityResponse = {
  showBetsPublicly: boolean
}

export type UpdateBetVisibilityRequest = {
  showBetsPublicly: boolean
}

export type PublicBet = {
  matchId: number
  userId: number
  userName: string
  homeGoalsPrediction: number
  awayGoalsPrediction: number
  pointsEarned: number
  createdAt: string
  isCurrentUser: boolean
}

export type UpdateBetCommand = {
  id: number
  request: UpdateBetRequest
}
