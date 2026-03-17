import { useState } from 'react'
import { submitCheckIn } from '../api/checkInApi'
import PageHeader from '../components/PageHeader'

function CheckInPage() {
  const [token, setToken] = useState(() => localStorage.getItem('latest-checkin-token') || '')
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setLoading(true)

    try {
      const data = await submitCheckIn(token)
      setResult(data)
    } catch (error) {
      setResult({
        isSuccessful: false,
        status: 'Failed',
        message: error.message,
        checkedInAtUtc: new Date().toISOString(),
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="checkin-page">
      <div className="checkin-card">
        <PageHeader eyebrow="Check-in Website" title="签到验证" description="提交用户 Token，系统会验证签名、过期时间、撤销状态和用户有效性。" />

        <form className="grid-form" onSubmit={handleSubmit}>
          <label>
            JWT Token
            <textarea value={token} onChange={(event) => setToken(event.target.value)} rows="8" required />
          </label>
          <button type="button" className="secondary-button" onClick={() => setToken(localStorage.getItem('latest-checkin-token') || '')}>
            使用最近生成的 Token
          </button>
          <button className="primary-button" disabled={loading}>
            {loading ? '验证中...' : '提交签到'}
          </button>
        </form>

        {result && (
          <div className={result.isSuccessful ? 'result-card success' : 'result-card failed'}>
            <h3>{result.isSuccessful ? '签到成功' : '签到失败'}</h3>
            <p>{result.message}</p>
            {result.failureReason && <p>失败原因: {result.failureReason}</p>}
            <p>记录时间: {new Date(result.checkedInAtUtc).toLocaleString()}</p>
          </div>
        )}
      </div>
    </section>
  )
}

export default CheckInPage
