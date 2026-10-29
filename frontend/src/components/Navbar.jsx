import React from 'react';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { Logo } from './Logo';
import { LogIn, LogOut, Sun, Moon } from 'lucide-react';

export default function Navbar({ onOpenAuth, currentTab }) {
  const { user, logout } = useAuth();
  const { theme, toggleTheme, isDark } = useTheme();

  const tabTitles = {
    dashboard: 'Panel de Control',
    usuarios: 'Gestión de Usuarios',
    clases: 'Clases y Entrenamientos',
    productos: 'Inventario y Tienda',
    torneos: user?.rol === 'Administrador' ? 'Gestión de Torneos' : 'Torneos del Club',
    ligas: user?.rol === 'Administrador' ? 'Gestión de Ligas' : 'Ligas y Posiciones',
    reservas: user?.rol === 'Administrador' ? 'Auditoría de Reservas' : 'Alquiler de Canchas',
    perfil: 'Mi Carnet de Socio',
  };

  return (
    <header style={{
      height: 64,
      borderBottom: '1px solid var(--border)',
      background: 'var(--header-bg)',
      backdropFilter: 'blur(12px)',
      WebkitBackdropFilter: 'blur(12px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 24px',
      position: 'sticky',
      top: 0,
      zIndex: 100,
      transition: 'background-color 0.2s ease, border-color 0.2s ease',
    }}>
      {/* Brand & Section Indicator */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        <Logo size="sm" subtitle={false} />
        
        {user && currentTab && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, paddingLeft: 12, borderLeft: '1px solid var(--border)' }}>
            <span style={{ fontSize: '0.875rem', fontWeight: 500, color: 'var(--text-muted)' }}>
              {tabTitles[currentTab] || 'Club'}
            </span>
          </div>
        )}
      </div>

      {/* Navegación, Selector de Modo y Sesión */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        {/* Conmutador Modo Claro / Modo Oscuro */}
        <button
          onClick={toggleTheme}
          className="theme-toggle-btn"
          title={isDark ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro'}
          aria-label={isDark ? 'Activar modo claro' : 'Activar modo oscuro'}
        >
          {isDark ? <Sun size={17} /> : <Moon size={17} />}
        </button>

        {!user ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <nav style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
              <a
                href="#canchas"
                style={{
                  color: 'var(--text-muted)',
                  fontSize: '0.875rem',
                  fontWeight: 500,
                  textDecoration: 'none',
                }}
              >
                Pistas y Canchas
              </a>
              <a
                href="#competencias"
                style={{
                  color: 'var(--text-muted)',
                  fontSize: '0.875rem',
                  fontWeight: 500,
                  textDecoration: 'none',
                }}
              >
                Torneos y Ligas
              </a>
              <a
                href="#escuela"
                style={{
                  color: 'var(--text-muted)',
                  fontSize: '0.875rem',
                  fontWeight: 500,
                  textDecoration: 'none',
                }}
              >
                Escuela de Tenis
              </a>
            </nav>

            <button onClick={() => onOpenAuth()} className="btn btn-primary btn-sm">
              <LogIn size={15} />
              <span>Ingresar</span>
            </button>
          </div>
        ) : (
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              gap: 9,
              background: 'var(--bg-elevated)',
              padding: '5px 12px',
              borderRadius: 'var(--radius-sm)',
              border: '1px solid var(--border)'
            }}>
              <div style={{
                width: 26,
                height: 26,
                borderRadius: '50%',
                background: 'var(--primary)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--primary-text)',
                fontWeight: 600,
                fontSize: '0.75rem'
              }}>
                {user.nombre?.charAt(0) || 'U'}
              </div>
              <div style={{ display: 'flex', flexDirection: 'column' }}>
                <span style={{ fontSize: '0.8125rem', fontWeight: 600, color: 'var(--text-main)', lineHeight: 1.2 }}>
                  {user.nombreCompleto || `${user.nombre} ${user.apellido}`}
                </span>
                <span style={{
                  fontSize: '0.625rem',
                  color: 'var(--primary)',
                  fontWeight: 600,
                  textTransform: 'uppercase',
                  letterSpacing: '0.04em'
                }}>
                  {user.rol === 'Administrador' ? 'Administrador' : 'Socio'}
                </span>
              </div>
            </div>

            <button
              onClick={logout}
              className="btn btn-secondary btn-sm"
              title="Cerrar sesión"
            >
              <LogOut size={14} />
              <span className="no-print">Salir</span>
            </button>
          </div>
        )}
      </div>
    </header>
  );
}
