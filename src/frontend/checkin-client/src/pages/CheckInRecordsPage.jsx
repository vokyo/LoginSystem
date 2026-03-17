import { useEffect, useState } from 'react'
import { getCheckInRecords } from '../api/checkInApi'
import { getUsers } from '../api/usersApi'
import PageHeader from '../components/PageHeader'

function CheckInRecordsPage() {
  const [recordsResult, setRecordsResult] = useState({ items: [], totalCount: 0 })
  const [usersResult, setUsersResult] = useState({ items: [] })
  const [filters, setFilters] = useState({ userId: '', status: '', startUtc: '', endUtc: '', pageNumber: 1, pageSize: 50 })
  const [message, setMessage] = useState('')

  async function loadData(currentFilters = filters) {
    const [recordsData, usersData] = await Promise.all([
      getCheckInRecords(currentFilters),
      getUsers({ pageNumber: 1, pageSize: 100 }),
    ])

    setRecordsResult(recordsData)
    setUsersResult(usersData)
  }

  useEffect(() => {
    loadData().catch((error) => setMessage(error.message))
  }, [])

  async function handleSearch(event) {
    event.preventDefault()

    try {
      await loadData(filters)
    } catch (error) {
      setMessage(error.message)
    }
  }

  return (
    <section className="page-section">
      <PageHeader eyebrow="Audit" title="签到记录" description="按用户和状态筛选签到结果，便于管理端审计。" />
      {message && <p className="info-banner">{message}</p>}

      <form className="card filter-bar" onSubmit={handleSearch}>
        <label>
          用户
          <select value={filters.userId} onChange={(event) => setFilters((current) => ({ ...current, userId: event.target.value }))}>
            <option value="">全部</option>
            {usersResult.items.map((user) => (
              <option key={user.id} value={user.id}>
                {user.userName}
              </option>
            ))}
          </select>
        </label>
        <label>
          状态
          <select value={filters.status} onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value }))}>
            <option value="">全部</option>
            <option value="Success">Success</option>
            <option value="Failed">Failed</option>
          </select>
        </label>
        <label>
          开始时间
          <input type="datetime-local" value={filters.startUtc} onChange={(event) => setFilters((current) => ({ ...current, startUtc: event.target.value }))} />
        </label>
        <label>
          结束时间
          <input type="datetime-local" value={filters.endUtc} onChange={(event) => setFilters((current) => ({ ...current, endUtc: event.target.value }))} />
        </label>
        <button className="primary-button">筛选</button>
      </form>

      <div className="card">
        <h3>记录列表</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>用户</th>
              <th>状态</th>
              <th>失败原因</th>
              <th>时间</th>
              <th>来源 IP</th>
            </tr>
          </thead>
          <tbody>
            {recordsResult.items.map((record) => (
              <tr key={record.id}>
                <td>{record.userName || '-'}</td>
                <td>{record.status}</td>
                <td>{record.failureReason || '-'}</td>
                <td>{new Date(record.checkedInAtUtc).toLocaleString()}</td>
                <td>{record.sourceIp || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

export default CheckInRecordsPage
