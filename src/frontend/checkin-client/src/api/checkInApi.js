import apiClient from './client'

export async function submitCheckIn(token) {
  const response = await apiClient.post('/checkin', { token })
  return response.data.data
}

export async function getCheckInRecords(params) {
  const cleanParams = Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== '' && value !== null && value !== undefined),
  )

  const response = await apiClient.get('/checkinrecords', { params: cleanParams })
  return response.data.data
}
