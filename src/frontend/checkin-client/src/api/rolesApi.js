import apiClient from './client'

export async function getRoles() {
  const response = await apiClient.get('/roles')
  return response.data.data
}

export async function getPermissions() {
  const response = await apiClient.get('/roles/permissions')
  return response.data.data
}

export async function createRole(payload) {
  const response = await apiClient.post('/roles', payload)
  return response.data.data
}

export async function updateRole(roleId, payload) {
  const response = await apiClient.put(`/roles/${roleId}`, payload)
  return response.data.data
}

export async function deleteRole(roleId) {
  const response = await apiClient.delete(`/roles/${roleId}`)
  return response.data
}
