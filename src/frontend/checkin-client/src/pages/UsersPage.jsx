import { useEffect, useState } from 'react'
import { assignUserRoles, createUser, deleteUser, getUsers, updateUser, updateUserStatus } from '../api/usersApi'
import { getRoles } from '../api/rolesApi'
import PageHeader from '../components/PageHeader'

const initialForm = {
  userName: '',
  fullName: '',
  email: '',
  password: '',
  roleIds: [],
  isActive: true,
}

function UsersPage() {
  const [usersResult, setUsersResult] = useState({ items: [], totalCount: 0 })
  const [roles, setRoles] = useState([])
  const [form, setForm] = useState(initialForm)
  const [editingUserId, setEditingUserId] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  async function loadData() {
    setLoading(true)

    try {
      const [usersData, rolesData] = await Promise.all([getUsers({ pageNumber: 1, pageSize: 50 }), getRoles()])
      setUsersResult(usersData)
      setRoles(rolesData)
    } catch (error) {
      setMessage(error.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  function resetForm() {
    setForm(initialForm)
    setEditingUserId('')
  }

  function startEdit(user) {
    setEditingUserId(user.id)
    setForm({
      userName: user.userName,
      fullName: user.fullName,
      email: user.email,
      password: '',
      roleIds: user.roles.map((role) => role.id),
      isActive: user.isActive,
    })
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setMessage('')

    try {
      if (editingUserId) {
        await updateUser(editingUserId, {
          fullName: form.fullName,
          email: form.email,
          password: form.password || null,
        })
        await assignUserRoles(editingUserId, form.roleIds)
        await updateUserStatus(editingUserId, form.isActive)
        setMessage('用户更新成功。')
      } else {
        await createUser(form)
        setMessage('用户创建成功。')
      }

      resetForm()
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  async function handleToggle(user) {
    try {
      await updateUserStatus(user.id, !user.isActive)
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  async function handleDelete(userId) {
    try {
      await deleteUser(userId)
      if (editingUserId === userId) {
        resetForm()
      }
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  async function handleRoleAssignment(userId, roleIds) {
    try {
      await assignUserRoles(userId, roleIds)
      await loadData()
    } catch (error) {
      setMessage(error.message)
    }
  }

  return (
    <section className="page-section">
      <PageHeader eyebrow="Admin" title="用户管理" description="管理后台用户、启用状态、基本信息和角色关联。" />
      {message && <p className="info-banner">{message}</p>}

      <div className="two-column">
        <form className="card grid-form" onSubmit={handleSubmit}>
          <h3>{editingUserId ? '编辑用户' : '新增用户'}</h3>
          <label>
            用户名
            <input
              value={form.userName}
              disabled={Boolean(editingUserId)}
              onChange={(event) => setForm((current) => ({ ...current, userName: event.target.value }))}
              required
            />
          </label>
          <label>
            姓名
            <input value={form.fullName} onChange={(event) => setForm((current) => ({ ...current, fullName: event.target.value }))} required />
          </label>
          <label>
            邮箱
            <input type="email" value={form.email} onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))} required />
          </label>
          <label>
            {editingUserId ? '新密码（留空则不修改）' : '密码'}
            <input
              type="password"
              minLength="6"
              value={form.password}
              onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))}
              required={!editingUserId}
            />
          </label>
          <label>
            角色
            <select
              multiple
              value={form.roleIds}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  roleIds: Array.from(event.target.selectedOptions, (option) => option.value),
                }))
              }
            >
              {roles.map((role) => (
                <option key={role.id} value={role.id}>
                  {role.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            启用状态
            <select value={String(form.isActive)} onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.value === 'true' }))}>
              <option value="true">启用</option>
              <option value="false">禁用</option>
            </select>
          </label>
          <div className="action-row">
            <button className="primary-button">{editingUserId ? '保存修改' : '创建用户'}</button>
            {editingUserId && (
              <button type="button" className="secondary-button" onClick={resetForm}>
                取消编辑
              </button>
            )}
          </div>
        </form>

        <div className="card">
          <h3>用户列表</h3>
          {loading ? (
            <p>加载中...</p>
          ) : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>用户名</th>
                  <th>姓名</th>
                  <th>邮箱</th>
                  <th>角色</th>
                  <th>状态</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                {usersResult.items.map((user) => (
                  <tr key={user.id}>
                    <td>{user.userName}</td>
                    <td>{user.fullName}</td>
                    <td>{user.email}</td>
                    <td>
                      <select
                        multiple
                        value={user.roles.map((role) => role.id)}
                        onChange={(event) =>
                          handleRoleAssignment(user.id, Array.from(event.target.selectedOptions, (option) => option.value))
                        }
                      >
                        {roles.map((role) => (
                          <option key={role.id} value={role.id}>
                            {role.name}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td>
                      <span className={user.isActive ? 'pill success' : 'pill muted'}>
                        {user.isActive ? '启用' : '禁用'}
                      </span>
                    </td>
                    <td className="action-row">
                      <button type="button" className="secondary-button" onClick={() => startEdit(user)}>
                        编辑
                      </button>
                      <button type="button" className="secondary-button" onClick={() => handleToggle(user)}>
                        {user.isActive ? '禁用' : '启用'}
                      </button>
                      <button type="button" className="danger-button" onClick={() => handleDelete(user.id)}>
                        删除
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </section>
  )
}

export default UsersPage
