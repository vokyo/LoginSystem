import { useEffect, useState } from 'react'
import { submitCheckIn } from '../api/checkInApi'
import { readStoredToken, readTokenFromUrl, writeStoredToken } from '../utils/token'

function CheckInPortalPage() {
  const [token, setToken] = useState(() => readTokenFromUrl() || readStoredToken())
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const urlToken = readTokenFromUrl()
    if (urlToken) {
      setToken(urlToken)
      writeStoredToken(urlToken)
    }
  }, [])

  async function handleSubmit(event) {
    event.preventDefault()
    setLoading(true)
    writeStoredToken(token)

    try {
      const data = await submitCheckIn(token)
      setResult(data)
    } catch (error) {
      setResult({
        isSuccessful: false,
        status: 'Failed',
        message: 'Check-in failed.',
        failureReason: error.message,
        checkedInAtUtc: new Date().toISOString(),
      })
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className="portal-shell">
      <section className="portal-card">
        <p className="eyebrow">Independent Check-in Website</p>
        <h1>Token Check-in Portal</h1>
        <p className="description">
          Submit a token manually, or open this page with <code>?token=...</code> to prefill the form.
        </p>

        <form className="portal-form" onSubmit={handleSubmit}>
          <label>
            Token
            <textarea value={token} onChange={(event) => setToken(event.target.value)} rows="8" required />
          </label>

          <div className="button-row">
            <button type="button" className="secondary-button" onClick={() => setToken(readStoredToken())}>
              Use Stored Token
            </button>
            <button className="primary-button" disabled={loading}>
              {loading ? 'Checking...' : 'Submit Check-in'}
            </button>
          </div>
        </form>

        {result && (
          <div className={result.isSuccessful ? 'result-card success' : 'result-card failed'}>
            <h2>{result.isSuccessful ? 'Check-in Succeeded' : 'Check-in Failed'}</h2>
            <p>{result.message}</p>
            {result.failureReason && <p>Reason: {result.failureReason}</p>}
            <p>Time: {new Date(result.checkedInAtUtc).toLocaleString()}</p>
          </div>
        )}
      </section>
    </main>
  )
}

export default CheckInPortalPage
