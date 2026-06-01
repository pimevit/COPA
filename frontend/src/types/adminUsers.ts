export type AdminUser = {
  id: number
  name: string
  email: string
  createdAt: string
  lastLoginAtUtc?: string | null
}

export type ResetUserPasswordResponse = {
  userId: number
  name: string
  email: string
  temporaryPassword: string
}
