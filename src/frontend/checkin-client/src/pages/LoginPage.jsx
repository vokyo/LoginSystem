import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { login } from '../api/authApi'
import { useAuth } from '../hooks/useAuth'
import PageHeader from '../components/PageHeader'

function LoginPage() {
  const navigate = useNavigate()
  const { login: saveLogin } = useAuth()
  const [form, setForm] = useState({ userName: 'admin', password: 'Admin123!' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setLoading(true)

    try {
      const data = await login(form)
      saveLogin(data)
      navigate('/users')
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <PageHeader
          eyebrow="Admin Management System"
          title="登录管理后台"
          description="默认种子账号为 admin / Admin123!。首次启动后可直接登录。"
        />

        <form className="grid-form" onSubmit={handleSubmit}>
          <label>
            用户名
            <input
              value={form.userName}
              onChange={(event) => setForm((current) => ({ ...current, userName: event.target.value }))}
              required
            />
          </label>

          <label>
            密码
            <input
              type="password"
              value={form.password}
              onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))}
              required
            />
          </label>

          {error && <p className="error-text">{error}</p>}

          <button className="primary-button" disabled={loading}>
            {loading ? '登录中...' : '登录'}
          </button>
        </form>
      </div>
    </div>
  )
}

export default LoginPage
