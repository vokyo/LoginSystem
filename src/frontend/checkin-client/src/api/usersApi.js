import apiClient from './client'

export async function getUsers(params) {
  const response = await apiClient.get('/users', { params })
  return response.data.data
}

export async function createUser(payload) {
  const response = await apiClient.post('/users', payload)
  return response.data.data
}

export async function updateUser(userId, payload) {
  const response = await apiClient.put(`/users/${userId}`, payload)
  return response.data.data
}

export async function updateUserStatus(userId, isActive) {
  const response = await apiClient.patch(`/users/${userId}/status`, { isActive })
  return response.data.data
}

export async function assignUserRoles(userId, roleIds) {
  const response = await apiClient.put(`/users/${userId}/roles`, { roleIds })
  return response.data.data
}

export async function deleteUser(userId) {
  const response = await apiClient.delete(`/users/${userId}`)
  return response.data
}
