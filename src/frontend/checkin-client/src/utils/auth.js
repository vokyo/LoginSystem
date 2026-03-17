import { jwtDecode } from 'jwt-decode'

const AUTH_STORAGE_KEY = 'checkin-admin-auth'

export function readStoredAuth() {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (!raw) return null

  try {
    const parsed = JSON.parse(raw)
    if (!parsed?.token) return null
    return parsed
  } catch {
    localStorage.removeItem(AUTH_STORAGE_KEY)
    return null
  }
}

export function persistAuth(auth) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth))
}

export function clearStoredAuth() {
  localStorage.removeItem(AUTH_STORAGE_KEY)
}

export function decodeToken(token) {
  try {
    return jwtDecode(token)
  } catch {
    return null
  }
}

export function normalizeAuth(loginResponse) {
  const decoded = decodeToken(loginResponse.token)
  const permissionClaims = Array.isArray(decoded?.permission)
    ? decoded.permission
    : decoded?.permission
      ? [decoded.permission]
      : loginResponse.permissions ?? []

  const roleClaims = Array.isArray(decoded?.role)
    ? decoded.role
    : decoded?.role
      ? [decoded.role]
      : loginResponse.roles ?? []

  return {
    token: loginResponse.token,
    expiresAtUtc: loginResponse.expiresAtUtc,
    user: {
      id: loginResponse.userId,
      userName: loginResponse.userName,
      fullName: loginResponse.fullName,
      roles: roleClaims,
      permissions: permissionClaims,
    },
  }
}

export function hasPermission(auth, permission) {
  if (!permission) return true
  return auth?.user?.permissions?.includes(permission) ?? false
}

export function isTokenExpired(expiresAtUtc) {
  return !expiresAtUtc || new Date(expiresAtUtc).getTime() <= Date.now()
}
