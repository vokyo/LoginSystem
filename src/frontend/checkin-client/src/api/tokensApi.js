import apiClient from './client'

export async function generateToken(userId, validHours) {
  const response = await apiClient.post(`/tokens/users/${userId}/generate`, { validHours })
  return response.data.data
}

export async function getCurrentToken(userId) {
  const response = await apiClient.get(`/tokens/users/${userId}/current`)
  return response.data.data
}

export async function getTokenHistory(userId) {
  const response = await apiClient.get(`/tokens/users/${userId}`)
  return response.data.data
}

export async function revokeCurrentToken(userId) {
  const response = await apiClient.post(`/tokens/users/${userId}/revoke-current`)
  return response.data.data
}
