import { useEffect, useState } from 'react'
import { getUsers } from '../api/usersApi'
import { generateToken, getCurrentToken, getTokenHistory } from '../api/tokensApi'
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
      setMessage('Token 生成成功，旧 Token 已自动失效。')
      await loadTokenData(selectedUserId)
    } catch (error) {
      setMessage(error.message)
    }
  }

  return (
    <section className="page-section">
      <PageHeader eyebrow="JWT" title="Token 管理" description="每个用户始终只保留一个当前有效 Token，重新生成会自动撤销旧 Token。" />
      {message && <p className="info-banner">{message}</p>}

      <div className="card token-toolbar">
        <label>
          选择用户
          <select value={selectedUserId} onChange={(event) => setSelectedUserId(event.target.value)}>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.userName}
              </option>
            ))}
          </select>
        </label>
        <label>
          有效小时数
          <input type="number" min="1" max="720" value={validHours} onChange={(event) => setValidHours(event.target.value)} />
        </label>
        <button className="primary-button" onClick={handleGenerate} disabled={!selectedUserId}>
          生成新 Token
        </button>
      </div>

      {generatedToken && (
        <div className="card">
          <h3>最新生成的原始 Token</h3>
          <textarea className="token-output" value={generatedToken} readOnly rows="6" />
        </div>
      )}

      {currentToken && (
        <div className="card">
          <h3>当前有效 Token</h3>
          <p>TokenId: {currentToken.tokenId}</p>
          <p>签发时间: {new Date(currentToken.issuedAtUtc).toLocaleString()}</p>
          <p>过期时间: {new Date(currentToken.expiresAtUtc).toLocaleString()}</p>
          <p>状态: {currentToken.status}</p>
        </div>
      )}

      <div className="card">
        <h3>Token 历史</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>TokenId</th>
              <th>签发时间</th>
              <th>过期时间</th>
              <th>状态</th>
              <th>撤销原因</th>
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
