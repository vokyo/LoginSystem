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
        setMessage('角色更新成功。')
      } else {
        await createRole(form)
        setMessage('角色创建成功。')
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
      <PageHeader eyebrow="RBAC" title="角色与权限管理" description="定义角色并配置权限点，后端接口基于权限声明控制访问。" />
      {message && <p className="info-banner">{message}</p>}

      <div className="two-column">
        <form className="card grid-form" onSubmit={handleSubmit}>
          <h3>{editingRoleId ? '编辑角色' : '新增角色'}</h3>
          <label>
            角色名
            <input value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} required />
          </label>
          <label>
            描述
            <textarea value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} rows="4" />
          </label>
          <label>
            状态
            <select value={String(form.isActive)} onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.value === 'true' }))}>
              <option value="true">启用</option>
              <option value="false">禁用</option>
            </select>
          </label>
          <label>
            权限
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
            <button className="primary-button">{editingRoleId ? '保存修改' : '创建角色'}</button>
            {editingRoleId && (
              <button type="button" className="secondary-button" onClick={resetForm}>
                取消编辑
              </button>
            )}
          </div>
        </form>

        <div className="card">
          <h3>角色列表</h3>
          <table className="data-table">
            <thead>
              <tr>
                <th>角色</th>
                <th>描述</th>
                <th>状态</th>
                <th>权限</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              {roles.map((role) => (
                <tr key={role.id}>
                  <td>{role.name}</td>
                  <td>{role.description || '-'}</td>
                  <td>{role.isActive ? '启用' : '禁用'}</td>
                  <td>{role.permissions.map((permission) => permission.code).join(', ') || '-'}</td>
                  <td className="action-row">
                    <button type="button" className="secondary-button" onClick={() => startEdit(role)}>
                      编辑
                    </button>
                    {!role.isSystem && (
                      <button type="button" className="danger-button" onClick={() => handleDelete(role.id)}>
                        删除
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
