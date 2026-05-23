export type AuthUser = {
  id: number
  name: string
  email: string
  createdAt: string
  roles: string[]
}

export type AuthSession = {
  accessToken: string
  expiresAtUtc: string
  user: AuthUser
}

export type LoginRequest = {
  email: string
  password: string
}

export type LoginResponse = AuthSession

export type RegisterRequest = {
  name: string
  email: string
  password: string
}

export type RegisterResponse = AuthSession
