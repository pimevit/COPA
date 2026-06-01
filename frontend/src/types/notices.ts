export type Notice = {
  message: string
  updatedAtUtc?: string | null
}

export type UpdateNoticeRequest = {
  message: string
}
