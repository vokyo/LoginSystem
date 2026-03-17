const KEY = 'checkin-portal-token'

export function readStoredToken() {
  return localStorage.getItem(KEY) || ''
}

export function writeStoredToken(token) {
  if (token) {
    localStorage.setItem(KEY, token)
  } else {
    localStorage.removeItem(KEY)
  }
}

export function readTokenFromUrl() {
  const params = new URLSearchParams(window.location.search)
  return params.get('token') || ''
}
