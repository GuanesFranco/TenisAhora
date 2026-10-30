import React, { useState, useEffect } from 'react';
import { ThemeProvider } from './context/ThemeContext';
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
import ClasesView from './views/ClasesView';
import ProductosView from './views/ProductosView';
import MiPerfilSocioView from './views/MiPerfilSocioView';
import PortalDocenteView from './views/PortalDocenteView';

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
      } else if (user.rol === 'Profesor' || user.rol === 'Entrenador') {
        // Para Docentes y Entrenadores: portal académico directo
        setCurrentTab('portal-docente');
      } else {
        // Para Socio / Cliente: portal directo de alquiler de canchas y actividades
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
        fontSize: '1rem',
      }}>
        Cargando sistema Tenis Ahora...
      </div>
    );
  }

  // 1. CASO VISITANTE: NO HAY SESIÓN INICIADA
  if (!user) {
    return (
      <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--bg-dark)' }}>
        <Navbar
          onOpenAuth={() => handleOpenAuth()}
          currentTab={null}
        />

        <main style={{
          flex: 1,
          maxWidth: 1240,
          width: '100%',
          margin: '0 auto',
          padding: '32px 24px',
        }}>
          <LandingPublicaView
            onRequireAuth={(msg) => handleOpenAuth(msg)}
          />
        </main>

        <footer style={{
          borderTop: '1px solid var(--border)',
          padding: '24px',
          textAlign: 'center',
          color: 'var(--text-dim)',
          fontSize: '0.8125rem',
          background: 'var(--bg-card)',
        }}>
          Club de Tenis Tenis Ahora &bull; Complejo Deportivo El Deporte Saludable &bull; Temporada 2026
        </footer>

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

  // 2. CASO USUARIO AUTENTICADO (ADMINISTRADOR O SOCIO)
  const isAdmin = user.rol === 'Administrador';

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--bg-dark)' }}>
      <Navbar
        onOpenAuth={() => handleOpenAuth()}
        currentTab={currentTab}
      />

      <div style={{ display: 'flex', flex: 1 }}>
        <Sidebar
          currentTab={currentTab}
          onSelectTab={(tabId) => setCurrentTab(tabId)}
          role={user.rol}
        />

        <main style={{
          flex: 1,
          padding: '28px 32px',
          overflowY: 'auto',
          maxWidth: 1400,
          width: '100%',
        }}>
          {/* VISTAS COMUNES Y SEGÚN ROL */}
          {isAdmin && currentTab === 'dashboard' && (
            <DashboardView onNavigate={(tab) => setCurrentTab(tab)} />
          )}

          {isAdmin && currentTab === 'usuarios' && (
            <UsuariosView />
          )}

          {/* PORTAL PARA PROFESORES Y ENTRENADORES (RF-43, RF-47, RF-50, RF-54, RF-59, RF-64) */}
          {(currentTab === 'portal-docente' ||
            currentTab === 'asistencia' ||
            currentTab === 'clases-particulares' ||
            currentTab === 'credencial-docente') && (
            <PortalDocenteView activeSubTab={currentTab} />
          )}

          {currentTab === 'reservas' && (
            <ReservasView />
          )}

          {currentTab === 'clases' && (
            <ClasesView />
          )}

          {currentTab === 'productos' && (
            <ProductosView />
          )}

          {currentTab === 'torneos' && (
            <TorneosView />
          )}

          {currentTab === 'ligas' && (
            <LigasView />
          )}

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
    <ThemeProvider>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </ThemeProvider>
  );
}
