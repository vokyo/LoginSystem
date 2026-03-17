import axios from 'axios'
import { clearStoredAuth, readStoredAuth } from '../utils/auth'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5246/api',
  timeout: 15000,
})

apiClient.interceptors.request.use((config) => {
  const auth = readStoredAuth()
  if (auth?.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }

  return config
})

apiClient.interceptors.response.use(
  (response) => {
    if (response.data && response.data.success === false) {
      return Promise.reject(new Error(response.data.message || 'Request failed.'))
    }

    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      clearStoredAuth()
      window.location.href = '/login'
    }

    return Promise.reject(new Error(error.response?.data?.message || error.message || 'Unexpected request error.'))
  },
)

export default apiClient
