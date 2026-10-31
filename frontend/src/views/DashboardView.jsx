import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import {
  Users,
  Trophy,
  Award,
  Calendar,
  ArrowRight,
  ShieldCheck,
  Clock,
  CheckCircle2,
  GraduationCap,
  ShoppingBag,
  FileText,
  Download,
  Activity
} from 'lucide-react';

export default function DashboardView({ onNavigate, role = 'Administrador' }) {
  const { user } = useAuth();
  const { isDark } = useTheme();
  const [stats, setStats] = useState({
    usuariosCount: 0,
    canchasCount: 0,
    reservasCount: 0,
    clasesCount: 0,
    entrenamientosCount: 0,
    productosCount: 0,
    torneosCount: 0,
    ligasCount: 0,
  });
  const [loading, setLoading] = useState(true);
  const [descargandoPdf, setDescargandoPdf] = useState(false);
  const [pdfMsg, setPdfMsg] = useState('');

  useEffect(() => {
    async function fetchSummary() {
      try {
        const [usuarios, canchas, reservas, clases, entrenamientos, productos, torneos, ligas] = await Promise.all([
          api.usuarios.listar(false).catch(() => []),
          api.canchas.listar().catch(() => []),
          api.reservas.listar().catch(() => []),
          api.clases.listar().catch(() => []),
          api.entrenamientos.listar().catch(() => []),
          api.productos.listar().catch(() => []),
          api.torneos.listar(false).catch(() => []),
          api.ligas.listar(false).catch(() => []),
        ]);

        setStats({
          usuariosCount: Array.isArray(usuarios) ? usuarios.length : 0,
          canchasCount: Array.isArray(canchas) ? canchas.length : 0,
          reservasCount: Array.isArray(reservas) ? reservas.length : 0,
          clasesCount: Array.isArray(clases) ? clases.length : 0,
          entrenamientosCount: Array.isArray(entrenamientos) ? entrenamientos.length : 0,
          productosCount: Array.isArray(productos) ? productos.length : 0,
          torneosCount: Array.isArray(torneos) ? torneos.length : 0,
          ligasCount: Array.isArray(ligas) ? ligas.length : 0,
        });
      } catch (err) {
        console.error('Error al cargar métricas del dashboard:', err);
      } finally {
        setLoading(false);
      }
    }

    fetchSummary();
  }, []);

  const isAdmin = user?.rol === 'Administrador';

  // Descarga real de reportes binarios QuestPDF
  const handleDescargarReporte = async (tipoReporte) => {
    setDescargandoPdf(true);
    setPdfMsg(`Generando y descargando reporte de ${tipoReporte} en PDF...`);
    try {
      const blob = await api.reportes.descargarReporte(tipoReporte, user?.id || 1);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Reporte_${tipoReporte}_${new Date().toISOString().slice(0, 10)}.pdf`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      setPdfMsg(`¡Reporte de ${tipoReporte} descargado exitosamente!`);
      setTimeout(() => setPdfMsg(''), 3000);
    } catch (err) {
      setPdfMsg(`Error al generar reporte: ${err.message}`);
      setTimeout(() => setPdfMsg(''), 4000);
    } finally {
      setDescargandoPdf(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 28 }}>
      {/* Hero Banner Minimalista con Pista Vacía */}
      <div style={{
        position: 'relative',
        borderRadius: 'var(--radius-lg)',
        overflow: 'hidden',
        minHeight: 240,
        display: 'flex',
        alignItems: 'center',
        padding: '36px 40px',
        border: '1px solid var(--border)',
        boxShadow: 'var(--shadow-sm)',
        backgroundImage: isDark
          ? 'linear-gradient(90deg, rgba(11, 15, 20, 0.95) 0%, rgba(11, 15, 20, 0.85) 60%, rgba(11, 15, 20, 0.45) 100%), url("/courts/cemento.jpg")'
          : 'linear-gradient(90deg, rgba(255, 255, 255, 0.95) 0%, rgba(255, 255, 255, 0.85) 60%, rgba(255, 255, 255, 0.45) 100%), url("/courts/cemento.jpg")',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        transition: 'background-image 0.2s ease',
      }}>
        <div style={{ maxWidth: 660, position: 'relative', zIndex: 2 }}>
          <div className="badge badge-primary" style={{ marginBottom: 12 }}>
            Complejo Deportivo "El Deporte Saludable"
          </div>

          <h1 style={{
            fontSize: 'clamp(1.8rem, 2.8vw, 2.4rem)',
            lineHeight: 1.2,
            marginBottom: 10,
            fontWeight: 700,
            letterSpacing: '-0.02em'
          }}>
            {isAdmin ? (
              <>Panel de Control <span style={{ color: 'var(--primary)' }}>Administrativo</span></>
            ) : (
              <>Bienvenido a <span style={{ color: 'var(--primary)' }}>Tenis Ahora</span></>
            )}
          </h1>

          <p style={{
            color: 'var(--text-muted)',
            fontSize: '0.9375rem',
            lineHeight: 1.6,
            marginBottom: 20,
            maxWidth: 580
          }}>
            {isAdmin ? (
              'Control unificado de socios, canchas, alquileres con seña del 50%, escuela deportiva, catálogo de stock y emisión de reportes institucionales.'
            ) : (
              'Alquilá canchas de polvo de ladrillo, cemento o pasto, reservá equipamiento de stock e inscribite en clases de tenis y competencias oficiales.'
            )}
          </p>

          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 10 }}>
            {isAdmin ? (
              <>
                <button onClick={() => onNavigate('reservas')} className="btn btn-primary">
                  <Calendar size={15} />
                  <span>Ver Reservas</span>
                </button>
                <button onClick={() => onNavigate('clases')} className="btn btn-secondary">
                  <GraduationCap size={15} />
                  <span>Clases y Entrenamientos</span>
                </button>
                <button onClick={() => onNavigate('productos')} className="btn btn-secondary">
                  <ShoppingBag size={15} />
                  <span>Inventario y Stock</span>
                </button>
                <button onClick={() => onNavigate('usuarios')} className="btn btn-secondary">
                  <Users size={15} />
                  <span>Socios y Personal</span>
                </button>
              </>
            ) : (
              <>
                <button onClick={() => onNavigate('reservas')} className="btn btn-primary">
                  <Calendar size={15} />
                  <span>Alquilar Cancha</span>
                </button>
                <button onClick={() => onNavigate('clases')} className="btn btn-secondary">
                  <GraduationCap size={15} />
                  <span>Inscribirme a Clases</span>
                </button>
                <button onClick={() => onNavigate('torneos')} className="btn btn-secondary">
                  <Trophy size={15} />
                  <span>Torneos y Ligas</span>
                </button>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Notificación de reporte PDF */}
      {pdfMsg && (
        <div style={{
          background: pdfMsg.includes('Error') ? 'var(--accent-red-bg)' : 'var(--primary-subtle)',
          border: `1px solid ${pdfMsg.includes('Error') ? 'var(--accent-red-border)' : 'var(--primary-subtle-border)'}`,
          color: pdfMsg.includes('Error') ? 'var(--accent-red)' : 'var(--primary-subtle-text)',
          padding: '10px 16px',
          borderRadius: 'var(--radius-sm)',
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          fontSize: '0.875rem'
        }}>
          <CheckCircle2 size={16} />
          <span>{pdfMsg}</span>
        </div>
      )}

      {/* Tarjetas de Métricas en Vivo desde la DB */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))',
        gap: 16,
      }}>
        {/* Canchas */}
        <div className="card" onClick={() => onNavigate('reservas')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Canchas Habilitadas
            </span>
            <div style={{ padding: 5, background: 'var(--primary-subtle)', borderRadius: 6 }}>
              <Activity size={15} color="var(--primary)" />
            </div>
          </div>
          <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--primary)', lineHeight: 1, marginBottom: 4 }}>
            {loading ? '...' : stats.canchasCount}
          </div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Ladrillo, Cemento y Pasto</span>
        </div>

        {/* Reservas */}
        <div className="card" onClick={() => onNavigate('reservas')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Reservas Registradas
            </span>
            <div style={{ padding: 5, background: 'var(--primary-subtle)', borderRadius: 6 }}>
              <Calendar size={15} color="var(--primary)" />
            </div>
          </div>
          <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--text-main)', lineHeight: 1, marginBottom: 4 }}>
            {loading ? '...' : stats.reservasCount}
          </div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Con seña del 50% y stock</span>
        </div>

        {/* Clases & Entrenamientos */}
        <div className="card" onClick={() => onNavigate('clases')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Actividades Deportivas
            </span>
            <div style={{ padding: 5, background: 'var(--accent-hard-bg)', borderRadius: 6 }}>
              <GraduationCap size={15} color="var(--accent-hard)" />
            </div>
          </div>
          <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--accent-hard)', lineHeight: 1, marginBottom: 4 }}>
            {loading ? '...' : stats.clasesCount + stats.entrenamientosCount}
          </div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{stats.clasesCount} clases · {stats.entrenamientosCount} entrenamientos</span>
        </div>

        {/* Productos / Stock */}
        <div className="card" onClick={() => onNavigate('productos')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Catálogo de Stock
            </span>
            <div style={{ padding: 5, background: 'var(--accent-clay-bg)', borderRadius: 6 }}>
              <ShoppingBag size={15} color="var(--accent-clay)" />
            </div>
          </div>
          <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--accent-clay)', lineHeight: 1, marginBottom: 4 }}>
            {loading ? '...' : stats.productosCount}
          </div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Pelotas, raquetas y redes</span>
        </div>

        {/* Usuarios (Solo Admin) */}
        {isAdmin && (
          <div className="card" onClick={() => onNavigate('usuarios')} style={{ cursor: 'pointer' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
              <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                Socios y Personal
              </span>
              <div style={{ padding: 5, background: 'var(--primary-subtle)', borderRadius: 6 }}>
                <Users size={15} color="var(--primary)" />
              </div>
            </div>
            <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--text-main)', lineHeight: 1, marginBottom: 4 }}>
              {loading ? '...' : stats.usuariosCount}
            </div>
            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Cuentas registradas</span>
          </div>
        )}

        {/* Torneos & Ligas */}
        <div className="card" onClick={() => onNavigate('torneos')} style={{ cursor: 'pointer' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              Competencias Activas
            </span>
            <div style={{ padding: 5, background: 'var(--primary-subtle)', borderRadius: 6 }}>
              <Trophy size={15} color="var(--primary)" />
            </div>
          </div>
          <div style={{ fontSize: '1.9rem', fontWeight: 700, color: 'var(--primary)', lineHeight: 1, marginBottom: 4 }}>
            {loading ? '...' : stats.torneosCount + stats.ligasCount}
          </div>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{stats.torneosCount} torneos · {stats.ligasCount} ligas</span>
        </div>
      </div>

      {/* Sección Reportes Oficiales en PDF (Para Empleado Administrativo) */}
      {isAdmin && (
        <div className="card" style={{ padding: 20 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14 }}>
            <div>
              <h3 style={{ fontSize: '1.15rem', marginBottom: 3, display: 'flex', alignItems: 'center', gap: 8 }}>
                <FileText size={17} color="var(--primary)" />
                Generador de Reportes Institucionales en PDF
              </h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.8125rem' }}>
                Emisión de reportes oficiales auditables con firma del club para control administrativo.
              </p>
            </div>

            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
              <button
                onClick={() => handleDescargarReporte('Canchas')}
                disabled={descargandoPdf}
                className="btn btn-secondary btn-sm"
              >
                <Download size={13} />
                <span>Canchas (PDF)</span>
              </button>
              <button
                onClick={() => handleDescargarReporte('Reservas')}
                disabled={descargandoPdf}
                className="btn btn-secondary btn-sm"
              >
                <Download size={13} />
                <span>Reservas (PDF)</span>
              </button>
              <button
                onClick={() => handleDescargarReporte('Ingresos')}
                disabled={descargandoPdf}
                className="btn btn-secondary btn-sm"
              >
                <Download size={13} />
                <span>Ingresos (PDF)</span>
              </button>
              <button
                onClick={() => handleDescargarReporte('TiposDeCancha')}
                disabled={descargandoPdf}
                className="btn btn-secondary btn-sm"
              >
                <Download size={13} />
                <span>Superficies (PDF)</span>
              </button>
              <button
                onClick={() => handleDescargarReporte('Asistencias')}
                disabled={descargandoPdf}
                className="btn btn-secondary btn-sm"
              >
                <Download size={13} />
                <span>Asistencias (PDF)</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
