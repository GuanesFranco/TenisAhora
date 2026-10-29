import React, { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import Sidebar from './components/Sidebar';
import AuthModal from './components/AuthModal';
import LandingPublicaView from './views/LandingPublicaView';
import DashboardView from './views/DashboardView';
import UsuariosView from './views/UsuariosView';
import TorneosView from './views/TorneosView';
import LigasView from './views/LigasView';
import ReservasView from './views/ReservasView';
import MiPerfilSocioView from './views/MiPerfilSocioView';

function AppContent() {
  const { user, loading } = useAuth();
  const [currentTab, setCurrentTab] = useState('dashboard');
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [authNotice, setAuthNotice] = useState('');

  // Sincronizar pestaña predeterminada según el rol autenticado
  useEffect(() => {
    if (user) {
      if (user.rol === 'Administrador') {
        setCurrentTab('dashboard');
      } else {
        // Para Socio / Cliente: portal directo de alquiler de canchas y competencias
        setCurrentTab('reservas');
      }
    }
  }, [user]);

  const handleOpenAuth = (noticeMessage = '') => {
    setAuthNotice(noticeMessage);
    setAuthModalOpen(true);
  };

  if (loading) {
    return (
      <div style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--bg-dark)',
        color: 'var(--text-muted)',
        fontSize: '1.1rem',
      }}>
        Cargando sistema Tenis Ahora...
      </div>
    );
  }

  // 1. CASO VISITANTE: NO HAY SESIÓN INICIADA
  // No se muestra ninguna información interna ni administrativa del club.
  // Solo se visualiza la Landing Page Pública del predio deportivo con opciones para ingresar.
  if (!user) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--bg-dark)' }}>
        <Navbar
          onOpenAuth={() => handleOpenAuth()}
          currentTab=""
        />

        <main style={{
          flex: 1,
          padding: '36px 32px',
          maxWidth: 1400,
          margin: '0 auto',
          width: '100%',
        }}>
          <LandingPublicaView onRequireAuth={handleOpenAuth} />
        </main>

        <AuthModal
          isOpen={authModalOpen}
          notice={authNotice}
          onClose={() => {
            setAuthModalOpen(false);
            setAuthNotice('');
          }}
        />
      </div>
    );
  }

  // 2. CASO AUTENTICADO: SESIÓN VÁLIDA
  const isAdmin = user.rol === 'Administrador';

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--bg-dark)' }}>
      {/* Barra superior de navegación */}
      <Navbar
        currentTab={currentTab}
        onOpenAuth={() => handleOpenAuth()}
      />

      <div style={{ display: 'flex', flex: 1 }}>
        {/* Barra lateral protegida según rol */}
        <Sidebar
          currentTab={currentTab}
          role={isAdmin ? 'Administrador' : 'Socio'}
          onSelectTab={(tab) => setCurrentTab(tab)}
        />

        {/* Área principal protegida */}
        <main style={{
          flex: 1,
          padding: '28px 36px',
          maxWidth: 1600,
          margin: '0 auto',
          width: '100%',
        }}>
          {/* VISTAS DE EMPLEADO ADMINISTRATIVO */}
          {isAdmin && currentTab === 'dashboard' && (
            <DashboardView
              role="Administrador"
              onNavigate={(tab) => setCurrentTab(tab)}
            />
          )}

          {isAdmin && currentTab === 'usuarios' && (
            <UsuariosView />
          )}

          {/* VISTAS DE COMPETENCIAS (ACCESIBLES CON CONTROLES SEGÚN ROL) */}
          {currentTab === 'torneos' && (
            <TorneosView />
          )}

          {currentTab === 'ligas' && (
            <LigasView />
          )}

          {/* ALQUILER Y RESERVAS DE CANCHAS */}
          {currentTab === 'reservas' && (
            <ReservasView />
          )}

          {/* VISTA EXCLUSIVA DE SOCIO / CLIENTE */}
          {!isAdmin && currentTab === 'perfil' && (
            <MiPerfilSocioView />
          )}
        </main>
      </div>

      <AuthModal
        isOpen={authModalOpen}
        notice={authNotice}
        onClose={() => {
          setAuthModalOpen(false);
          setAuthNotice('');
        }}
      />
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}
