export type FrontendEnv = {
  apiBaseUrl: string
}

export function getApiBaseUrl(): string {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

  if (!apiBaseUrl && import.meta.env.DEV) {
    throw new Error('Missing VITE_API_BASE_URL. Configure it in frontend/.env.local.')
  }

  return apiBaseUrl ?? ''
}

export function getFrontendEnv(): FrontendEnv {
  return {
    apiBaseUrl: getApiBaseUrl(),
  }
}
