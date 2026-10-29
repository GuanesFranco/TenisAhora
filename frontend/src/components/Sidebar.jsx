import React from 'react';
import { Logo } from './Logo';
import { useTheme } from '../context/ThemeContext';
import {
  LayoutDashboard,
  Users,
  Trophy,
  Award,
  Calendar,
  ShieldCheck,
  User,
  GraduationCap,
  Dumbbell,
  CheckCircle2,
  ShoppingBag,
  Sun,
  Moon
} from 'lucide-react';

export default function Sidebar({ currentTab, onSelectTab, role = 'Administrador' }) {
  const { isDark, toggleTheme } = useTheme();

  // Menú para Empleado Administrativo
  const menuAdmin = [
    { id: 'dashboard', label: 'Panel Principal', icon: LayoutDashboard },
    { id: 'usuarios', label: 'Socios y Personal', icon: Users, badge: 'Gestión' },
    { id: 'reservas', label: 'Reservas y Canchas', icon: Calendar, badge: 'Predio' },
    { id: 'clases', label: 'Clases y Entrenamientos', icon: GraduationCap, badge: 'Deporte' },
    { id: 'productos', label: 'Catálogo y Stock', icon: ShoppingBag, badge: 'Tienda' },
    { id: 'torneos', label: 'Torneos Oficiales', icon: Trophy, badge: 'Oficial' },
    { id: 'ligas', label: 'Ligas Regulares', icon: Award, badge: 'Torneo' },
  ];

  // Menú específico para Profesor de Tenis (Enunciado / RF-43, RF-47, RF-48, RF-59)
  const menuProfesor = [
    { id: 'portal-docente', label: 'Mis Clases Asignadas', icon: GraduationCap, badge: 'Agenda' },
    { id: 'asistencia', label: 'Toma de Asistencia', icon: CheckCircle2, badge: 'Nominal' },
    { id: 'clases-particulares', label: 'Clases Particulares', icon: Users, badge: '1 a 1' },
    { id: 'credencial-docente', label: 'Título y Credencial', icon: Award, badge: 'FAT' },
    { id: 'reservas', label: 'Disponibilidad Canchas', icon: Calendar, badge: 'Predio' },
  ];

  // Menú específico para Entrenador Deportivo (Enunciado / RF-50, RF-54, RF-07, RF-64)
  const menuEntrenador = [
    { id: 'portal-docente', label: 'Mis Entrenamientos', icon: Dumbbell, badge: 'Agenda' },
    { id: 'asistencia', label: 'Toma de Asistencia', icon: CheckCircle2, badge: 'Nominal' },
    { id: 'clases-particulares', label: 'Sesiones Particulares', icon: Users, badge: '1 a 1' },
    { id: 'credencial-docente', label: 'Título y Credencial', icon: Award, badge: 'AAT' },
    { id: 'reservas', label: 'Disponibilidad Canchas', icon: Calendar, badge: 'Predio' },
  ];

  // Menú para Socio / Cliente
  const menuSocio = [
    { id: 'reservas', label: 'Alquilar Canchas', icon: Calendar, badge: 'Turnos' },
    { id: 'clases', label: 'Clases y Entrenamientos', icon: GraduationCap, badge: 'Actividades' },
    { id: 'productos', label: 'Tienda Oficial', icon: ShoppingBag, badge: 'Equipamiento' },
    { id: 'torneos', label: 'Torneos Disponibles', icon: Trophy, badge: 'Competir' },
    { id: 'ligas', label: 'Ligas y Posiciones', icon: Award, badge: 'En Vivo' },
    { id: 'perfil', label: 'Mi Carnet de Socio', icon: User, badge: 'Perfil' },
  ];

  const menuItems =
    role === 'Administrador' ? menuAdmin :
    role === 'Profesor' ? menuProfesor :
    role === 'Entrenador' ? menuEntrenador : menuSocio;

  const roleTitle =
    role === 'Administrador' ? 'Administrador' :
    role === 'Profesor' ? 'Profesor Titular' :
    role === 'Entrenador' ? 'Entrenador Físico' : 'Socio Habilitado';

  return (
    <aside style={{
      width: 250,
      background: 'var(--bg-card)',
      borderRight: '1px solid var(--border)',
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'space-between',
      height: 'calc(100vh - 64px)',
      position: 'sticky',
      top: 64,
      padding: '20px 14px',
      transition: 'background-color 0.2s ease, border-color 0.2s ease',
    }}>
      <div>
        {/* Brand Header */}
        <div style={{ padding: '0 8px', marginBottom: 20 }}>
          <Logo size="md" subtitle={true} />
        </div>

        {/* Badge de rol activo */}
        <div style={{
          background: 'var(--primary-subtle)',
          border: '1px solid var(--primary-subtle-border)',
          padding: '8px 12px',
          borderRadius: 'var(--radius-sm)',
          marginBottom: 18,
          display: 'flex',
          alignItems: 'center',
          gap: 10,
        }}>
          <ShieldCheck size={16} color="var(--primary)" />
          <div>
            <div style={{ color: 'var(--text-dim)', fontSize: '0.625rem', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Rol Activo
            </div>
            <strong style={{
              color: 'var(--primary-subtle-text)',
              fontSize: '0.8125rem',
              fontWeight: 600
            }}>
              {roleTitle}
            </strong>
          </div>
        </div>

        {/* Navegación Principal */}
        <nav style={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
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
                  padding: '9px 12px',
                  borderRadius: 'var(--radius-sm)',
                  background: active ? 'var(--primary-subtle)' : 'transparent',
                  border: active ? '1px solid var(--primary-subtle-border)' : '1px solid transparent',
                  color: active ? 'var(--primary-subtle-text)' : 'var(--text-muted)',
                  cursor: 'pointer',
                  textAlign: 'left',
                  fontWeight: active ? 600 : 500,
                  fontSize: '0.875rem',
                  transition: 'all 0.12s ease'
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <Icon size={16} color={active ? 'var(--primary)' : 'var(--text-dim)'} />
                  <span>{item.label}</span>
                </div>
                {item.badge && (
                  <span style={{
                    fontSize: '0.625rem',
                    background: active ? 'var(--primary)' : 'var(--bg-elevated)',
                    color: active ? 'var(--primary-text)' : 'var(--text-dim)',
                    padding: '2px 6px',
                    borderRadius: 4,
                    fontWeight: 600,
                  }}>
                    {item.badge}
                  </span>
                )}
              </button>
            );
          })}
        </nav>
      </div>

      {/* Control Inferior de Tema y Estado */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
        <button
          onClick={toggleTheme}
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            width: '100%',
            padding: '8px 12px',
            background: 'var(--bg-elevated)',
            border: '1px solid var(--border)',
            borderRadius: 'var(--radius-sm)',
            color: 'var(--text-muted)',
            fontSize: '0.8125rem',
            cursor: 'pointer',
          }}
        >
          <span style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            {isDark ? <Sun size={15} /> : <Moon size={15} />}
            <span>Tema {isDark ? 'Oscuro' : 'Claro'}</span>
          </span>
          <span style={{ fontSize: '0.7rem', color: 'var(--primary)', fontWeight: 600 }}>Cambiar</span>
        </button>

        <div style={{
          background: 'var(--bg-elevated)',
          padding: '10px 12px',
          borderRadius: 'var(--radius-sm)',
          border: '1px solid var(--border)',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          fontSize: '0.75rem',
          color: 'var(--text-dim)'
        }}>
          <span style={{
            width: 7,
            height: 7,
            borderRadius: '50%',
            background: 'var(--primary)',
            display: 'inline-block'
          }} />
          <span>Servidor y DB en línea</span>
        </div>
      </div>
    </aside>
  );
}
