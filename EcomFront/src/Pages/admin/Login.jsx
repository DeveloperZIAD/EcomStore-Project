// src/pages/admin/Login.js
import React, { useState } from 'react';

import { useNavigate } from 'react-router-dom';
import authService  from '../../API/Services/authService';

const AdminLogin = () => {
  const [email, setEmail] = useState('admin@ecomstore.com');
  const [password, setPassword] = useState('admin123');
  const [error, setError] = useState('');
  const navigate = useNavigate();

const handleLogin = async (e) => {
  e.preventDefault();
  setError('');
  try {
    const response = await authService.login(email, password);
    if (authService.isAdmin()) {
      navigate('/dashboard'); // ده الـ path الصحيح في الـ Template للـ Admin Dashboard
      // أو لو كان '/admin/dashboard' جرب ده
      // navigate('/admin/dashboard');
    } else {
      setError('Access denied. Admin only.');
      authService.logout();
    }
  } catch (err) {
    setError(err.message || 'Invalid email or password.');
    console.error('Login error:', err);
  }
};

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-header">
              <h4>Admin Login</h4>
            </div>
            <div className="card-body">
              {error && <div className="alert alert-danger">{error}</div>}
              <form onSubmit={handleLogin}>
                <div className="mb-3">
                  <label className="form-label">Email</label>
                  <input
                    type="email"
                    className="form-control"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Password</label>
                  <input
                    type="password"
                    className="form-control"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                  />
                </div>
                <button type="submit" className="btn btn-primary w-100">
                  Login
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminLogin;