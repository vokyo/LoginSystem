import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const navItems = [
  { to: '/users', label: 'User Management', permission: 'users.read' },
  { to: '/roles', label: 'Role Management', permission: 'roles.read' },
  { to: '/tokens', label: 'Token Management', permission: 'tokens.read' },
  { to: '/checkin-records', label: 'Check-in Records', permission: 'checkins.read' },
  { to: '/checkin', label: 'Check-in Page' },
]

function AppShell() {
  const { auth, hasPermission, logout } = useAuth()

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">CheckIn System</p>
          <h1>Admin Console</h1>
          <p className="sidebar-copy">Manage users, roles, permissions, tokens, and check-in audit data from one place.</p>
        </div>

        <nav className="nav-list">
          {navItems.filter((item) => !item.permission || hasPermission(item.permission)).map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div>
            <strong>{auth?.user?.fullName}</strong>
            <p>{auth?.user?.userName}</p>
          </div>
          <button className="secondary-button" onClick={logout}>
            Sign Out
          </button>
        </div>
      </aside>

      <main className="content-panel">
        <Outlet />
      </main>
    </div>
  )
}

export default AppShell
