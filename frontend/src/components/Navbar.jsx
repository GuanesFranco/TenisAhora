import React from 'react';
import { useAuth } from '../context/AuthContext';
import { LogIn, LogOut, User, ShieldCheck, Calendar, Trophy } from 'lucide-react';

export default function Navbar({ onOpenAuth, currentTab }) {
  const { user, logout } = useAuth();

  const tabTitles = {
    dashboard: 'Panel de Control General',
    usuarios: 'Gestión de Usuarios y Socios',
    torneos: user?.rol === 'Administrador' ? 'Gestión de Torneos Oficiales' : 'Torneos del Club',
    ligas: user?.rol === 'Administrador' ? 'Gestión de Ligas Regulares' : 'Ligas y Posiciones',
    reservas: user?.rol === 'Administrador' ? 'Gestión de Reservas del Predio' : 'Alquiler de Canchas',
    perfil: 'Mi Perfil de Socio',
  };

  return (
    <header style={{
      height: 70,
      borderBottom: '1px solid var(--border)',
      background: 'rgba(15, 29, 24, 0.85)',
      backdropFilter: 'blur(12px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 28px',
      position: 'sticky',
      top: 0,
      zIndex: 100,
    }}>
      {/* Brand */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        <div style={{
          width: 36,
          height: 36,
          borderRadius: 8,
          background: 'linear-gradient(135deg, var(--accent-volt), var(--primary))',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontWeight: 900,
          color: '#000',
          fontSize: '1.1rem',
        }}>
          🎾
        </div>
        <h1 style={{ fontSize: '1.25rem', color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{ color: 'var(--accent-volt)', fontWeight: 800 }}>TENIS AHORA</span>
          <span style={{ color: 'var(--text-dim)', fontWeight: 400 }}>|</span>
          <span style={{ fontSize: '0.95rem', color: 'var(--text-muted)' }}>
            {user ? (tabTitles[currentTab] || 'Club') : 'Complejo "El Deporte Saludable"'}
          </span>
        </h1>
      </div>

      {/* Navegación y Sesión */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
        {!user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <nav style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
              <a href="#canchas" style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textDecoration: 'none' }}>
                Pistas y Canchas
              </a>
              <a href="#competencias" style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textDecoration: 'none' }}>
                Torneos y Ligas
              </a>
            </nav>

            <button onClick={() => onOpenAuth()} className="btn btn-volt btn-sm">
              <LogIn size={16} />
              <span>Ingresar / Registrarse</span>
            </button>
          </div>
        ) : (
          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              background: 'var(--bg-card)',
              padding: '6px 14px',
              borderRadius: 'var(--radius-full)',
              border: '1px solid var(--border)'
            }}>
              <div style={{
                width: 28,
                height: 28,
                borderRadius: '50%',
                background: user.rol === 'Administrador'
                  ? 'linear-gradient(135deg, var(--accent-volt), #84cc16)'
                  : 'linear-gradient(135deg, var(--primary), #059669)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#000',
                fontWeight: 'bold',
                fontSize: '0.85rem'
              }}>
                {user.nombre?.charAt(0) || 'U'}
              </div>
              <div style={{ display: 'flex', flexDirection: 'column' }}>
                <span style={{ fontSize: '0.875rem', fontWeight: 600 }}>{user.nombreCompleto || `${user.nombre} ${user.apellido}`}</span>
                <span style={{
                  fontSize: '0.65rem',
                  color: user.rol === 'Administrador' ? 'var(--accent-volt)' : 'var(--primary)',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  letterSpacing: '0.05em'
                }}>
                  {user.rol === 'Administrador' ? 'Empleado Administrativo' : 'Socio Oficial'}
                </span>
              </div>
            </div>

            <button onClick={logout} className="btn btn-secondary btn-sm" title="Cerrar sesión">
              <LogOut size={16} />
              <span className="no-print">Salir</span>
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
