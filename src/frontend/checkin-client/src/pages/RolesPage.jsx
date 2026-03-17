import { useEffect, useState } from 'react'
import { createRole, deleteRole, getPermissions, getRoles, updateRole } from '../api/rolesApi'
import PageHeader from '../components/PageHeader'

const initialForm = {
  name: '',
  description: '',
  isActive: true,
  permissionIds: [],
}

function RolesPage() {
  const [roles, setRoles] = useState([])
  const [permissions, setPermissions] = useState([])
  const [form, setForm] = useState(initialForm)
  const [editingRoleId, setEditingRoleId] = useState('')
  const [message, setMessage] = useState('')

  async function loadData() {
    try {
      const [rolesData, permissionsData] = await Promise.all([getRoles(), getPermissions()])
      setRoles(rolesData)
      setPermissions(permissionsData)
    } catch (error) {
      setMessage(error.message)
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  function resetForm() {
    setForm(initialForm)
    setEditingRoleId('')
  }

  function startEdit(role) {
    setEditingRoleId(role.id)
    setForm({
      name: role.name,
      description: role.description,
      isActive: role.isActive,
      permissionIds: role.permissions.map((permission) => permission.id),
    })
  }

  async function handleSubmit(event) {
    event.preventDefault()

    try {
      if (editingRoleId) {
        await updateRole(editingRoleId, form)
        setMessage('Role updated successfully.')
      } else {
        await createRole(form)
        setMessage('Role created successfully.')
      }

      resetForm()
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  async function handleDelete(roleId) {
    try {
      await deleteRole(roleId)
      if (editingRoleId === roleId) {
        resetForm()
      }
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  return (
    <section className="page-section">
      <PageHeader eyebrow="RBAC" title="Role Management" description="Define roles and assign permission codes for backend authorization." />
      {message && <p className="info-banner">{message}</p>}

      <div className="two-column">
        <form className="card grid-form" onSubmit={handleSubmit}>
          <h3>{editingRoleId ? 'Edit Role' : 'Create Role'}</h3>
          <label>
            Role Name
            <input value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} required />
          </label>
          <label>
            Description
            <textarea value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} rows="4" />
          </label>
          <label>
            Status
            <select value={String(form.isActive)} onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.value === 'true' }))}>
              <option value="true">Active</option>
              <option value="false">Disabled</option>
            </select>
          </label>
          <label>
            Permissions
            <select
              multiple
              value={form.permissionIds}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  permissionIds: Array.from(event.target.selectedOptions, (option) => option.value),
                }))
              }
            >
              {permissions.map((permission) => (
                <option key={permission.id} value={permission.id}>
                  {permission.code}
                </option>
              ))}
            </select>
          </label>
          <div className="action-row">
            <button className="primary-button">{editingRoleId ? 'Save Changes' : 'Create Role'}</button>
            {editingRoleId && (
              <button type="button" className="secondary-button" onClick={resetForm}>
                Cancel
              </button>
            )}
          </div>
        </form>

        <div className="card">
          <h3>Role List</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>Role</th>
                <th>Description</th>
                <th>Status</th>
                <th>Permissions</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {roles.map((role) => (
                <tr key={role.id}>
                  <td>{role.name}</td>
                  <td>{role.description || '-'}</td>
                  <td>{role.isActive ? 'Active' : 'Disabled'}</td>
                  <td>{role.permissions.map((permission) => permission.code).join(', ') || '-'}</td>
                  <td className="action-row">
                    <button type="button" className="secondary-button" onClick={() => startEdit(role)}>
                      Edit
                    </button>
                    {!role.isSystem && (
                      <button type="button" className="danger-button" onClick={() => handleDelete(role.id)}>
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  )
}

export default RolesPage
