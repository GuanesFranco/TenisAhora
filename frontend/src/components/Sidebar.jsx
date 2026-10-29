import React from 'react';
import {
  LayoutDashboard,
  Users,
  Trophy,
  Award,
  Calendar,
  ShieldCheck,
  User,
  Clock,
  Sparkles
} from 'lucide-react';

export default function Sidebar({ currentTab, onSelectTab, role = 'Administrador' }) {
  // Menú para Empleado Administrativo (permiso para todo)
  const menuAdmin = [
    { id: 'dashboard', label: 'Panel Principal', icon: LayoutDashboard },
    { id: 'usuarios', label: 'Usuarios y Socios', icon: Users, badge: 'Gestión' },
    { id: 'torneos', label: 'Torneos Oficiales', icon: Trophy, badge: 'Oficial' },
    { id: 'ligas', label: 'Ligas Regulares', icon: Award, badge: 'Temporada' },
    { id: 'reservas', label: 'Reservas del Predio', icon: Calendar, badge: 'Predio' },
  ];

  // Menú para Socio / Cliente (portal autogestionable de reservas y participación)
  const menuSocio = [
    { id: 'reservas', label: 'Alquilar Canchas', icon: Calendar, badge: 'Reservas' },
    { id: 'torneos', label: 'Torneos Disponibles', icon: Trophy, badge: 'Inscripción' },
    { id: 'ligas', label: 'Ligas y Posiciones', icon: Award, badge: 'En Vivo' },
    { id: 'perfil', label: 'Mi Perfil de Socio', icon: User, badge: 'Carnet' },
  ];

  const menuItems = role === 'Administrador' ? menuAdmin : menuSocio;

  return (
    <aside style={{
      width: 260,
      background: 'var(--bg-card)',
      borderRight: '1px solid var(--border)',
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'space-between',
      height: 'calc(100vh - 70px)',
      position: 'sticky',
      top: 70,
      padding: '24px 16px',
    }}>
      <div>
        {/* Brand / Logo Mini */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '0 8px', marginBottom: 20 }}>
          <div style={{
            width: 38,
            height: 38,
            borderRadius: 10,
            background: 'linear-gradient(135deg, var(--accent-volt), var(--primary))',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            boxShadow: '0 4px 15px var(--accent-volt-glow)'
          }}>
            <span style={{ fontSize: '1.2rem' }}>🎾</span>
          </div>
          <div>
            <h2 style={{ fontSize: '1.1rem', lineHeight: 1.1 }}>Tenis Ahora</h2>
            <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600 }}>El Deporte Saludable</small>
          </div>
        </div>

        {/* Badge de rol actual */}
        <div style={{
          background: role === 'Administrador' ? 'rgba(204, 255, 0, 0.1)' : 'rgba(34, 197, 94, 0.1)',
          border: `1px solid ${role === 'Administrador' ? 'rgba(204, 255, 0, 0.3)' : 'rgba(34, 197, 94, 0.3)'}`,
          padding: '8px 12px',
          borderRadius: 'var(--radius-sm)',
          marginBottom: 20,
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          fontSize: '0.75rem'
        }}>
          <ShieldCheck size={14} color={role === 'Administrador' ? 'var(--accent-volt)' : 'var(--primary)'} />
          <div>
            <div style={{ color: 'var(--text-dim)', fontSize: '0.65rem' }}>Perfil Activo:</div>
            <strong style={{ color: role === 'Administrador' ? 'var(--accent-volt)' : 'var(--primary)' }}>
              {role === 'Administrador' ? 'Empleado Administrativo' : 'Socio / Cliente'}
            </strong>
          </div>
        </div>

        {/* Menu Navigation */}
        <nav style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {menuItems.map((item) => {
            const Icon = item.icon;
            const active = currentTab === item.id;

            return (
              <button
                key={item.id}
                onClick={() => onSelectTab(item.id)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  width: '100%',
                  padding: '12px 14px',
                  borderRadius: 'var(--radius-md)',
                  background: active ? 'linear-gradient(90deg, rgba(34, 197, 94, 0.18), rgba(34, 197, 94, 0.05))' : 'transparent',
                  border: active ? '1px solid rgba(34, 197, 94, 0.35)' : '1px solid transparent',
                  color: active ? 'var(--accent-volt)' : 'var(--text-muted)',
                  cursor: 'pointer',
                  textAlign: 'left',
                  fontWeight: active ? 700 : 500,
                  fontSize: '0.925rem',
                  transition: 'all 0.15s ease'
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <Icon size={19} color={active ? 'var(--accent-volt)' : 'var(--text-dim)'} />
                  <span>{item.label}</span>
                </div>
                {item.badge && (
                  <span style={{
                    fontSize: '0.65rem',
                    background: active ? 'rgba(204, 255, 0, 0.2)' : 'var(--bg-elevated)',
                    color: active ? 'var(--accent-volt)' : 'var(--text-dim)',
                    padding: '2px 6px',
                    borderRadius: 4,
                    fontWeight: 700,
                  }}>
                    {item.badge}
                  </span>
                )}
              </button>
            );
          })}
        </nav>
      </div>

      {/* System Status info */}
      <div style={{
        background: 'var(--bg-dark)',
        padding: '14px',
        borderRadius: 'var(--radius-md)',
        border: '1px solid var(--border)',
        fontSize: '0.8rem',
        display: 'flex',
        flexDirection: 'column',
        gap: 6
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <span style={{
            width: 8,
            height: 8,
            borderRadius: '50%',
            background: 'var(--primary)',
            boxShadow: '0 0 10px var(--primary)'
          }}></span>
          <span style={{ fontWeight: 600, color: 'var(--text-main)', fontSize: '0.75rem' }}>Backend Conectado</span>
        </div>
        <span style={{ color: 'var(--text-dim)', fontSize: '0.7rem' }}>API REST .NET 8 · Clean Architecture</span>
      </div>
    </aside>
  );
}
