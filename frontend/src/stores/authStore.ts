import { create } from 'zustand'
import { createJSONStorage, persist } from 'zustand/middleware'

import type { AuthSession, AuthUser } from '../types/auth'

type AuthState = {
  token: string | null
  expiresAtUtc: string | null
  user: AuthUser | null
  setSession: (session: AuthSession) => void
  clearSession: () => void
}

export const authStorageKey = 'bolao-copa-auth'

const createAuthStore = () => create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      expiresAtUtc: null,
      user: null,
      setSession: (session) =>
        set({
          token: session.accessToken,
          expiresAtUtc: session.expiresAtUtc,
          user: session.user,
        }),
      clearSession: () => {
        set({
          token: null,
          expiresAtUtc: null,
          user: null,
        })

        localStorage.removeItem(authStorageKey)
      },
    }),
    {
      name: authStorageKey,
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        token: state.token,
        expiresAtUtc: state.expiresAtUtc,
        user: state.user,
      }),
    },
  ),
)

declare global {
  var __bolaoCopaAuthStore: ReturnType<typeof createAuthStore> | undefined
}

export const useAuthStore =
  globalThis.__bolaoCopaAuthStore ?? (globalThis.__bolaoCopaAuthStore = createAuthStore())

export const selectAuthToken = (state: AuthState) => state.token
export const selectAuthUser = (state: AuthState) => state.user
export const selectIsAuthenticated = (state: AuthState) => Boolean(state.token)
export const selectIsAdmin = (state: AuthState) => Boolean(state.user?.roles?.includes('Admin'))
