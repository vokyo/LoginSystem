import { Navigate, Route, Routes } from 'react-router-dom'
import AppShell from '../components/AppShell'
import ProtectedRoute from '../components/ProtectedRoute'
import CheckInPage from '../pages/CheckInPage'
import CheckInRecordsPage from '../pages/CheckInRecordsPage'
import LoginPage from '../pages/LoginPage'
import RolesPage from '../pages/RolesPage'
import TokensPage from '../pages/TokensPage'
import UsersPage from '../pages/UsersPage'

function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/checkin" element={<CheckInPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route path="/" element={<Navigate to="/users" replace />} />
          <Route path="/users" element={<ProtectedRoute requiredPermission="users.read"><UsersPage /></ProtectedRoute>} />
          <Route path="/roles" element={<ProtectedRoute requiredPermission="roles.read"><RolesPage /></ProtectedRoute>} />
          <Route path="/tokens" element={<ProtectedRoute requiredPermission="tokens.read"><TokensPage /></ProtectedRoute>} />
          <Route path="/checkin-records" element={<ProtectedRoute requiredPermission="checkins.read"><CheckInRecordsPage /></ProtectedRoute>} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default AppRouter
