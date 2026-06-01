import { apiClient } from '../../../api/httpClient'
import type { Notice, UpdateNoticeRequest } from '../../../types/notices'

export function fetchMatchNotice(): Promise<Notice> {
  return apiClient.get<Notice>('/notices/matches')
}

export function updateMatchNotice(request: UpdateNoticeRequest): Promise<Notice> {
  return apiClient.put<Notice>('/admin/notices/matches', request)
}
