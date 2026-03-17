import { createContext, useEffect, useMemo, useState } from 'react'
import { clearStoredAuth, hasPermission, isTokenExpired, normalizeAuth, persistAuth, readStoredAuth } from '../utils/auth'

export const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(() => {
    const stored = readStoredAuth()
    if (stored && !isTokenExpired(stored.expiresAtUtc)) {
      return stored
    }

    if (stored) {
      clearStoredAuth()
    }

    return null
  })

  useEffect(() => {
    if (auth) {
      persistAuth(auth)
    } else {
      clearStoredAuth()
    }
  }, [auth])

  const value = useMemo(
    () => ({
      auth,
      isAuthenticated: Boolean(auth && !isTokenExpired(auth.expiresAtUtc)),
      login(loginResponse) {
        setAuth(normalizeAuth(loginResponse))
      },
      logout() {
        setAuth(null)
      },
      hasPermission(permission) {
        return hasPermission(auth, permission)
      },
    }),
    [auth],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
