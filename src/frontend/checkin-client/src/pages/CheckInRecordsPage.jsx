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
      <PageHeader eyebrow="Audit" title="Check-in Records" description="Filter check-in results by user, status, and time range." />
      {message && <p className="info-banner">{message}</p>}

      <form className="card filter-bar" onSubmit={handleSearch}>
        <label>
          User
          <select value={filters.userId} onChange={(event) => setFilters((current) => ({ ...current, userId: event.target.value }))}>
            <option value="">All</option>
            {usersResult.items.map((user) => (
              <option key={user.id} value={user.id}>
                {user.userName}
              </option>
            ))}
          </select>
        </label>
        <label>
          Status
          <select value={filters.status} onChange={(event) => setFilters((current) => ({ ...current, status: event.target.value }))}>
            <option value="">All</option>
            <option value="Success">Success</option>
            <option value="Failed">Failed</option>
          </select>
        </label>
        <label>
          Start Time
          <input type="datetime-local" value={filters.startUtc} onChange={(event) => setFilters((current) => ({ ...current, startUtc: event.target.value }))} />
        </label>
        <label>
          End Time
          <input type="datetime-local" value={filters.endUtc} onChange={(event) => setFilters((current) => ({ ...current, endUtc: event.target.value }))} />
        </label>
        <button className="primary-button">Apply Filters</button>
      </form>

      <div className="card">
        <h3>Record List</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Status</th>
              <th>Failure Reason</th>
              <th>Time</th>
              <th>Source IP</th>
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
