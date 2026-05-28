export type AdminUser = {
  id: number
  name: string
  email: string
  createdAt: string
}

export type ResetUserPasswordResponse = {
  userId: number
  name: string
  email: string
  temporaryPassword: string
}
