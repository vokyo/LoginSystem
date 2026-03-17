import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const navItems = [
  { to: '/users', label: '用户管理', permission: 'users.read' },
  { to: '/roles', label: '角色管理', permission: 'roles.read' },
  { to: '/tokens', label: 'Token 管理', permission: 'tokens.read' },
  { to: '/checkin-records', label: '签到记录', permission: 'checkins.read' },
  { to: '/checkin', label: '签到页' },
]

function AppShell() {
  const { auth, hasPermission, logout } = useAuth()

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">CheckIn System</p>
          <h1>Admin Console</h1>
          <p className="sidebar-copy">统一管理用户、角色、权限、Token 与签到数据。</p>
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
            退出登录
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
