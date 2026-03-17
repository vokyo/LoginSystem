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
        <PageHeader
          eyebrow="Check-in Website"
          title="Check-in Validation"
          description="Validate token signature, expiry, revocation state, and linked user activity."
        />

        <form className="grid-form" onSubmit={handleSubmit}>
          <label>
            JWT Token
            <textarea value={token} onChange={(event) => setToken(event.target.value)} rows="8" required />
          </label>
          <button type="button" className="secondary-button" onClick={() => setToken(localStorage.getItem('latest-checkin-token') || '')}>
            Use Latest Generated Token
          </button>
          <button className="primary-button" disabled={loading}>
            {loading ? 'Validating...' : 'Submit Check-in'}
          </button>
        </form>

        {result && (
          <div className={result.isSuccessful ? 'result-card success' : 'result-card failed'}>
            <h3>{result.isSuccessful ? 'Check-in Succeeded' : 'Check-in Failed'}</h3>
            <p>{result.message}</p>
            {result.failureReason && <p>Reason: {result.failureReason}</p>}
            <p>Recorded At: {new Date(result.checkedInAtUtc).toLocaleString()}</p>
          </div>
        )}
      </div>
    </section>
  )
}

export default CheckInPage
