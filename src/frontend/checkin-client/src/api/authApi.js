import apiClient from './client'

export async function login(payload) {
  const response = await apiClient.post('/auth/login', payload)
  return response.data.data
}
