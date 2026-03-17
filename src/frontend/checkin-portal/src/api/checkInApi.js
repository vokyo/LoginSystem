import client from './client'

export async function submitCheckIn(token) {
  const response = await client.post('/checkin', { token })
  return response.data.data
}
