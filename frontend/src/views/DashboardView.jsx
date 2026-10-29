import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import {
  Users,
  Trophy,
  Award,
  Calendar,
  ArrowRight,
  ShieldCheck,
  Clock,
  CheckCircle2,
  Sparkles,
  CreditCard,
  UserCheck
} from 'lucide-react';

export default function DashboardView({ onNavigate, role = 'Administrador' }) {
  const [stats, setStats] = useState({
    usuariosCount: 0,
    torneosCount: 0,
    ligasCount: 0,
    reservasCount: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchSummary() {
      try {
        const [usuarios, torneos, ligas, reservas] = await Promise.all([
          api.usuarios.listar().catch(() => []),
          api.torneos.listar().catch(() => []),
          api.ligas.listar().catch(() => []),
          api.reservas.listar().catch(() => []),
        ]);

        setStats({
          usuariosCount: usuarios.length,
          torneosCount: torneos.length,
          ligasCount: ligas.length,
          reservasCount: reservas.length,
        });
      } catch (err) {
        console.error('Error cargando métricas:', err);
      } finally {
        setLoading(false);
      }
    }

    fetchSummary();
  }, []);

  const isAdmin = role === 'Administrador';

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 28 }}>
      {/* Hero Banner adaptativo */}
      <div style={{
        background: 'linear-gradient(135deg, rgba(34, 197, 94, 0.15) 0%, rgba(15, 29, 24, 0.8) 100%)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '36px',
        position: 'relative',
        overflow: 'hidden',
        boxShadow: 'var(--shadow-md)',
      }}>
        <div style={{ maxWidth: 680 }}>
          <div className="badge badge-success" style={{ marginBottom: 12 }}>
            Complejo Deportivo "El Deporte Saludable"
          </div>
          <h1 style={{ fontSize: '2.4rem', lineHeight: 1.15, marginBottom: 12 }}>
            {isAdmin ? (
              <>Panel de Gestión <span style={{ color: 'var(--accent-volt)' }}>Administrativa</span></>
            ) : (
              <>Bienvenido a <span style={{ color: 'var(--accent-volt)' }}>Tenis Ahora</span></>
            )}
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '1.05rem', lineHeight: 1.6, marginBottom: 24 }}>
            {isAdmin ? (
              'Control unificado de socios, habilitaciones de profesores, programación de torneos oficiales, ligas regulares y auditoría de alquileres.'
            ) : (
              'Alquilá canchas de tenis en polvo de ladrillo, cemento o pasto, consultá la disponibilidad en tiempo real e inscribite en torneos y ligas del club.'
            )}
          </p>

          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
            {isAdmin ? (
              <>
                <button onClick={() => onNavigate('usuarios')} className="btn btn-volt">
                  <Users size={18} />
                  Gestionar Usuarios
                </button>
                <button onClick={() => onNavigate('torneos')} className="btn btn-secondary">
                  <Trophy size={18} />
                  Ver Torneos
                </button>
                <button onClick={() => onNavigate('ligas')} className="btn btn-secondary">
                  <Award size={18} />
                  Ver Ligas
                </button>
                <button onClick={() => onNavigate('reservas')} className="btn btn-secondary">
                  <Calendar size={18} />
                  Reservas del Predio
                </button>
              </>
            ) : (
              <>
                <button onClick={() => onNavigate('reservas')} className="btn btn-volt">
                  <Calendar size={18} />
                  Alquilar una Cancha Ahora
                </button>
                <button onClick={() => onNavigate('torneos')} className="btn btn-secondary">
                  <Trophy size={18} />
                  Torneos Abiertos
                </button>
                <button onClick={() => onNavigate('ligas')} className="btn btn-secondary">
                  <Award size={18} />
                  Tabla de Ligas
                </button>
                <button onClick={() => onNavigate('perfil')} className="btn btn-secondary">
                  <UserCheck size={18} />
                  Mi Carnet de Socio
                </button>
              </>
            )}
          </div>
        </div>

        {/* Elemento gráfico decorativo */}
        <div style={{
          position: 'absolute',
          right: -20,
          top: -20,
          opacity: 0.12,
          pointerEvents: 'none',
          userSelect: 'none',
          fontSize: '14rem'
        }}>
          🎾
        </div>
      </div>

      {/* Métricas Principales */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 18 }}>
        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-md)',
          padding: '20px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <div>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              {isAdmin ? 'Socios y Usuarios' : 'Canchas Habilitadas'}
            </span>
            <div style={{ fontSize: '2rem', fontWeight: 800, marginTop: 4 }}>
              {isAdmin ? (loading ? '...' : stats.usuariosCount) : '4 Pistas'}
            </div>
            <small style={{ color: 'var(--primary)', fontSize: '0.8rem' }}>
              {isAdmin ? 'Activos en sistema' : 'Polvo de Ladrillo, Cemento y Pasto'}
            </small>
          </div>
          <div style={{ padding: 12, background: 'rgba(34, 197, 94, 0.12)', borderRadius: 'var(--radius-md)', color: 'var(--primary)' }}>
            <Users size={26} />
          </div>
        </div>

        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-md)',
          padding: '20px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <div>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              Torneos Oficiales
            </span>
            <div style={{ fontSize: '2rem', fontWeight: 800, marginTop: 4 }}>
              {loading ? '...' : stats.torneosCount}
            </div>
            <small style={{ color: 'var(--accent-volt)', fontSize: '0.8rem' }}>Cruces eliminatorios</small>
          </div>
          <div style={{ padding: 12, background: 'rgba(204, 255, 0, 0.12)', borderRadius: 'var(--radius-md)', color: 'var(--accent-volt)' }}>
            <Trophy size={26} />
          </div>
        </div>

        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-md)',
          padding: '20px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <div>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              Ligas Regulares
            </span>
            <div style={{ fontSize: '2rem', fontWeight: 800, marginTop: 4 }}>
              {loading ? '...' : stats.ligasCount}
            </div>
            <small style={{ color: '#38BDF8', fontSize: '0.8rem' }}>Formato Round-Robin</small>
          </div>
          <div style={{ padding: 12, background: 'rgba(56, 189, 248, 0.12)', borderRadius: 'var(--radius-md)', color: '#38BDF8' }}>
            <Award size={26} />
          </div>
        </div>

        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-md)',
          padding: '20px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <div>
            <span style={{ color: 'var(--text-dim)', fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
              Alquileres y Turnos
            </span>
            <div style={{ fontSize: '2rem', fontWeight: 800, marginTop: 4 }}>
              {loading ? '...' : stats.reservasCount}
            </div>
            <small style={{ color: 'var(--primary)', fontSize: '0.8rem' }}>Con seña del 50% validada</small>
          </div>
          <div style={{ padding: 12, background: 'rgba(34, 197, 94, 0.12)', borderRadius: 'var(--radius-md)', color: 'var(--primary)' }}>
            <Calendar size={26} />
          </div>
        </div>
      </div>

      {/* Reglas oficiales y Políticas aprobadas del club */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '28px',
      }}>
        <h3 style={{ fontSize: '1.2rem', marginBottom: 16, display: 'flex', alignItems: 'center', gap: 10 }}>
          <ShieldCheck color="var(--primary)" size={22} />
          Reglamento y Políticas del Complejo Deportivo
        </h3>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 20 }}>
          <div style={{
            background: 'var(--bg-elevated)',
            padding: '16px',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border)',
            display: 'flex',
            flexDirection: 'column',
            gap: 8,
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--accent-volt)', fontWeight: 600 }}>
              <CheckCircle2 size={16} />
              <span>Alquileres y Seña</span>
            </div>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.5 }}>
              Para confirmar un alquiler se debe abonar el 50% en concepto de seña. Duración máxima por turno: 2 horas. Antelación máxima de reserva: 30 días.
            </p>
          </div>

          <div style={{
            background: 'var(--bg-elevated)',
            padding: '16px',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border)',
            display: 'flex',
            flexDirection: 'column',
            gap: 8,
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--primary)', fontWeight: 600 }}>
              <CheckCircle2 size={16} />
              <span>Cancelaciones</span>
            </div>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.5 }}>
              Cancelando con más de 6 horas de antelación se procesa el reembolso completo del 100% de la seña. Fuera de término aplica penalidad.
            </p>
          </div>

          <div style={{
            background: 'var(--bg-elevated)',
            padding: '16px',
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border)',
            display: 'flex',
            flexDirection: 'column',
            gap: 8,
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: '#38BDF8', fontWeight: 600 }}>
              <CheckCircle2 size={16} />
              <span>Reglamento Oficial de Competiciones</span>
            </div>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.5 }}>
              Rige la reglamentación oficial de la Asociación de Tenis: sets, games y tanteadores auditados, y acumulación de puntos por victoria.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
