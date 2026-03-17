import { useEffect, useState } from 'react'
import { getUsers } from '../api/usersApi'
import { generateToken, getCurrentToken, getTokenHistory, revokeCurrentToken } from '../api/tokensApi'
import PageHeader from '../components/PageHeader'

function TokensPage() {
  const [users, setUsers] = useState([])
  const [selectedUserId, setSelectedUserId] = useState('')
  const [validHours, setValidHours] = useState(24)
  const [generatedToken, setGeneratedToken] = useState('')
  const [currentToken, setCurrentToken] = useState(null)
  const [history, setHistory] = useState([])
  const [message, setMessage] = useState('')

  async function loadUsers() {
    const usersData = await getUsers({ pageNumber: 1, pageSize: 100 })
    setUsers(usersData.items)

    if (!selectedUserId && usersData.items.length > 0) {
      setSelectedUserId(usersData.items[0].id)
    }
  }

  async function loadTokenData(userId) {
    if (!userId) return

    const [current, tokenHistory] = await Promise.all([getCurrentToken(userId), getTokenHistory(userId)])
    setCurrentToken(current)
    setHistory(tokenHistory)
  }

  useEffect(() => {
    loadUsers().catch((error) => setMessage(error.message))
  }, [])

  useEffect(() => {
    loadTokenData(selectedUserId).catch((error) => setMessage(error.message))
  }, [selectedUserId])

  async function handleGenerate() {
    try {
      const data = await generateToken(selectedUserId, Number(validHours))
      setGeneratedToken(data.token)
      localStorage.setItem('latest-checkin-token', data.token)
      setMessage('Token generated successfully. Previous token was revoked automatically.')
      await loadTokenData(selectedUserId)
    } catch (error) {
      setMessage(error.message)
    }
  }

  async function handleRevokeCurrent() {
    try {
      await revokeCurrentToken(selectedUserId)
      setGeneratedToken('')
      setMessage('Current token revoked successfully.')
      await loadTokenData(selectedUserId)
    } catch (error) {
      setMessage(error.message)
    }
  }

  return (
    <section className="page-section">
      <PageHeader
        eyebrow="JWT"
        title="Token Management"
        description="Each user keeps at most one active check-in token. You can regenerate or revoke it manually."
      />

      {message && <p className="info-banner">{message}</p>}

      <div className="card token-toolbar">
        <label>
          Select User
          <select value={selectedUserId} onChange={(event) => setSelectedUserId(event.target.value)}>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.userName}
              </option>
            ))}
          </select>
        </label>

        <label>
          Valid Hours
          <input type="number" min="1" max="720" value={validHours} onChange={(event) => setValidHours(event.target.value)} />
        </label>

        <button type="button" className="primary-button" onClick={handleGenerate} disabled={!selectedUserId}>
          Generate New Token
        </button>

        <button
          type="button"
          className="danger-button"
          onClick={handleRevokeCurrent}
          disabled={!selectedUserId || !currentToken || currentToken.status !== 'Valid'}
        >
          Revoke Current Token
        </button>
      </div>

      {generatedToken && (
        <div className="card">
          <h3>Latest Raw Token</h3>
          <textarea className="token-output" value={generatedToken} readOnly rows="6" />
        </div>
      )}

      {currentToken ? (
        <div className="card">
          <h3>Current Token</h3>
          <p>TokenId: {currentToken.tokenId}</p>
          <p>Issued At: {new Date(currentToken.issuedAtUtc).toLocaleString()}</p>
          <p>Expires At: {new Date(currentToken.expiresAtUtc).toLocaleString()}</p>
          <p>Status: {currentToken.status}</p>
        </div>
      ) : (
        <div className="card">
          <h3>Current Token</h3>
          <p>No active token.</p>
        </div>
      )}

      <div className="card">
        <h3>Token History</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>TokenId</th>
              <th>Issued At</th>
              <th>Expires At</th>
              <th>Status</th>
              <th>Revoked Reason</th>
            </tr>
          </thead>
          <tbody>
            {history.map((token) => (
              <tr key={token.id}>
                <td>{token.tokenId}</td>
                <td>{new Date(token.issuedAtUtc).toLocaleString()}</td>
                <td>{new Date(token.expiresAtUtc).toLocaleString()}</td>
                <td>{token.status}</td>
                <td>{token.revokedReason || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

export default TokensPage
