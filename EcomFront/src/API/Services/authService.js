// src/services/authService.js
const API_URL = 'http://localhost:5000/api/auth';

const login = async (email, password) => {
  const response = await fetch(`${API_URL}/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || 'Invalid email or password.');
  }

  const data = await response.json();
  if (data.token) {
    localStorage.setItem('token', data.token);
    localStorage.setItem('role', data.role || 'customer');
    localStorage.setItem('userId', data.userId);
  }
  return data;
};

const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('role');
  localStorage.removeItem('userId');
};

const getToken = () => localStorage.getItem('token');

const isAdmin = () => localStorage.getItem('role') === 'admin';

export default { login, logout, getToken, isAdmin };