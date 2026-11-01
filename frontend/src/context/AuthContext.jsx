import React, { createContext, useContext, useState, useEffect } from 'react';
import { api } from '../services/api';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem('tenis_token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadUser() {
      if (!token) {
        setLoading(false);
        return;
      }

      try {
        const userData = await api.auth.me();
        const fullName = userData.nombreCompleto || `${userData.nombre || ''} ${userData.apellido || ''}`.trim();
        const parts = fullName.split(' ').filter(Boolean);
        const nombre = userData.nombre || parts[0] || '';
        const apellido = userData.apellido || parts.slice(1).join(' ') || '';

        setUser({
          id: userData.id,
          email: userData.email,
          rol: userData.rol,
          nombreCompleto: fullName,
          nombre,
          apellido,
        });
      } catch (err) {
        console.error('Sesión expirada o inválida:', err);
        logout();
      } finally {
        setLoading(false);
      }
    }

    loadUser();
  }, [token]);

  const login = async (email, password) => {
    const data = await api.auth.login({ email, password });
    localStorage.setItem('tenis_token', data.token);
    setToken(data.token);
    const fullName = `${data.nombre || ''} ${data.apellido || ''}`.trim();
    setUser({
      id: data.usuarioId,
      nombre: data.nombre,
      apellido: data.apellido,
      nombreCompleto: fullName || data.email,
      email: data.email,
      rol: data.rol,
    });
    return data;
  };

  const register = async (userData) => {
    const data = await api.auth.register(userData);
    localStorage.setItem('tenis_token', data.token);
    setToken(data.token);
    const fullName = `${data.nombre || ''} ${data.apellido || ''}`.trim();
    setUser({
      id: data.usuarioId,
      nombre: data.nombre,
      apellido: data.apellido,
      nombreCompleto: fullName || data.email,
      email: data.email,
      rol: data.rol,
    });
    return data;
  };

  const logout = () => {
    localStorage.removeItem('tenis_token');
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
