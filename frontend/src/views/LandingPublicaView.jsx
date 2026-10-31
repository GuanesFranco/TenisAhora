import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useTheme } from '../context/ThemeContext';
import {
  Calendar,
  Trophy,
  Award,
  ShieldCheck,
  CheckCircle2,
  Clock,
  ArrowRight,
  Users,
  Activity,
  Layers
} from 'lucide-react';

export default function LandingPublicaView({ onRequireAuth }) {
  const [canchas, setCanchas] = useState([]);
  const [torneos, setTorneos] = useState([]);
  const [ligas, setLigas] = useState([]);
  const [loading, setLoading] = useState(true);
  const { isDark } = useTheme();

  // Fotografías de canchas reales vacías (sin personas)
  const fotosSuperficie = {
    Ladrillo: '/courts/ladrillo.jpg?v=3',
    Cemento: '/courts/cemento.jpg?v=3',
    Pasto: '/courts/pasto.jpg?v=5',
  };

  useEffect(() => {
    async function cargarDatosPublicos() {
      try {
        const [canchasData, torneosData, ligasData] = await Promise.all([
          api.canchas.listar().catch(() => []),
          api.torneos.listar().catch(() => []),
          api.ligas.listar().catch(() => []),
        ]);
        setCanchas(canchasData || []);
        setTorneos(torneosData || []);
        setLigas(ligasData || []);
      } catch (err) {
        console.error('Error cargando información pública:', err);
      } finally {
        setLoading(false);
      }
    }
    cargarDatosPublicos();
  }, []);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 48, paddingBottom: 48 }}>
      {/* Hero Principal Minimalista con Fondo de Cancha */}
      <section style={{
        position: 'relative',
        borderRadius: 'var(--radius-lg)',
        overflow: 'hidden',
        minHeight: 400,
        display: 'flex',
        alignItems: 'center',
        padding: '56px 40px',
        border: '1px solid var(--border)',
        boxShadow: 'var(--shadow-sm)',
        backgroundImage: isDark
          ? 'linear-gradient(90deg, rgba(11, 15, 20, 0.95) 0%, rgba(11, 15, 20, 0.85) 60%, rgba(11, 15, 20, 0.45) 100%), url("/courts/cemento.jpg")'
          : 'linear-gradient(90deg, rgba(255, 255, 255, 0.95) 0%, rgba(255, 255, 255, 0.85) 60%, rgba(255, 255, 255, 0.45) 100%), url("/courts/cemento.jpg")',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        transition: 'background-image 0.2s ease',
      }}>
        <div style={{ maxWidth: 640, position: 'relative', zIndex: 2 }}>
          <div className="badge badge-primary" style={{ marginBottom: 14 }}>
            Complejo de Canchas de Tenis "El Deporte Saludable"
          </div>

          <h1 style={{
            fontSize: 'clamp(2.1rem, 3.8vw, 3rem)',
            lineHeight: 1.15,
            marginBottom: 16,
            fontWeight: 700,
            letterSpacing: '-0.025em',
            color: 'var(--text-main)'
          }}>
            La excelencia del tenis en su máxima expresión.
          </h1>

          <p style={{
            color: 'var(--text-muted)',
            fontSize: '1.05rem',
            lineHeight: 1.6,
            marginBottom: 28,
            maxWidth: 560
          }}>
            Infraestructura reglamentaria con pistas de polvo de ladrillo, cemento y césped natural. Reservá tu cancha con confirmación inmediata, sumate a torneos oficiales y entrená con nuestro cuerpo técnico.
          </p>

          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
            <button
              onClick={() => onRequireAuth('Para alquilar una cancha online, por favor iniciá sesión con tu cuenta de socio.')}
              className="btn btn-primary"
              style={{ padding: '11px 22px', fontSize: '0.9375rem' }}
            >
              <Calendar size={17} />
              <span>Reservar Cancha Online</span>
            </button>

            <button
              onClick={() => onRequireAuth('Registrate en segundos para asociarte al club y acceder a tarifas preferenciales.')}
              className="btn btn-secondary"
              style={{ padding: '11px 22px', fontSize: '0.9375rem' }}
            >
              <Users size={17} />
              <span>Hacerme Socio</span>
            </button>
          </div>
        </div>
      </section>

      {/* Métricas Principales del Complejo */}
      <section style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(210px, 1fr))',
        gap: 16
      }}>
        <div className="card">
          <div style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em', marginBottom: 4 }}>
            Superficies Homologadas
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--primary)' }}>
            3 Pistas
          </div>
          <p style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', marginTop: 2 }}>
            Polvo de Ladrillo, Cemento y Césped
          </p>
        </div>

        <div className="card">
          <div style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em', marginBottom: 4 }}>
            Reserva Anticipada
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-main)' }}>
            Hasta 30 Días
          </div>
          <p style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', marginTop: 2 }}>
            Confirmación directa con seña del 50%
          </p>
        </div>

        <div className="card">
          <div style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em', marginBottom: 4 }}>
            Competencias
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--primary)' }}>
            Torneos y Ligas
          </div>
          <p style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', marginTop: 2 }}>
            Modalidades Singles y Dobles
          </p>
        </div>

        <div className="card">
          <div style={{ color: 'var(--text-dim)', fontSize: '0.75rem', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.04em', marginBottom: 4 }}>
            Staff Profesional
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--accent-hard)' }}>
            Entrenadores
          </div>
          <p style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', marginTop: 2 }}>
            Profesores y preparadores físicos
          </p>
        </div>
      </section>

      {/* Pistas y Canchas del Predio (Fotografía realista sin personas) */}
      <section id="canchas">
        <div style={{ marginBottom: 20 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <Activity size={18} color="var(--primary)" />
            <h2 style={{ fontSize: '1.5rem' }}>Nuestras Pistas de Tenis</h2>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            Infraestructura homologada con iluminación LED profesional y mantenimiento diario del terreno.
          </p>
        </div>

        {loading ? (
          <div style={{ padding: 40, textAlign: 'center', color: 'var(--text-dim)' }}>
            Cargando disponibilidad de canchas...
          </div>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(310px, 1fr))', gap: 20 }}>
            {canchas.map((cancha) => {
              const foto = cancha.fotoUrl || fotosSuperficie[cancha.superficie] || fotosSuperficie.Ladrillo;

              return (
                <div
                  key={cancha.id}
                  className="card"
                  style={{
                    padding: 0,
                    overflow: 'hidden',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'space-between'
                  }}
                >
                  <div style={{ position: 'relative', height: 180, overflow: 'hidden', background: 'var(--bg-elevated)' }}>
                    <img
                      src={foto}
                      alt={`Cancha de tenis ${cancha.superficie}`}
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                      style={{
                        width: '100%',
                        height: '100%',
                        objectFit: 'cover',
                      }}
                    />
                    <div style={{
                      position: 'absolute',
                      inset: 0,
                      background: 'linear-gradient(to top, rgba(0, 0, 0, 0.75) 0%, rgba(0, 0, 0, 0.1) 60%)'
                    }} />

                    <div style={{ position: 'absolute', top: 12, left: 12 }}>
                      <span className={`badge-surface-overlay ${
                        cancha.superficie === 'Ladrillo' ? 'clay' :
                        cancha.superficie === 'Pasto' ? 'grass' : 'hard'
                      }`}>
                        <span className="surface-dot" />
                        <span>{cancha.superficie}</span>
                      </span>
                    </div>

                    <div style={{ position: 'absolute', bottom: 12, left: 14 }}>
                      <h3 style={{ fontSize: '1.2rem', color: '#ffffff', fontWeight: 600 }}>Pista {cancha.id}</h3>
                    </div>
                  </div>

                  <div style={{ padding: '18px' }}>
                    <div style={{
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 8,
                      fontSize: '0.85rem',
                      color: 'var(--text-muted)',
                      marginBottom: 18
                    }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span>Capacidad:</span>
                        <strong style={{ color: 'var(--text-main)' }}>Hasta {cancha.capacidad || 4} jugadores</strong>
                      </div>
                      <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span>Mantenimiento:</span>
                        <strong style={{ color: 'var(--primary)' }}>Óptimo / Habilitada</strong>
                      </div>
                      <div style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'baseline',
                        borderTop: '1px solid var(--border)',
                        paddingTop: 10
                      }}>
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>Arancel Oficial:</span>
                        <span style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--text-main)' }}>
                          ${(cancha.precio || 12000).toLocaleString('es-AR')}<span style={{ fontSize: '0.75rem', fontWeight: 400, color: 'var(--text-muted)' }}>/hora</span>
                        </span>
                      </div>
                    </div>

                    <button
                      onClick={() => onRequireAuth(`Iniciá sesión para reservar la Pista ${cancha.id} (${cancha.superficie}).`)}
                      className="btn btn-secondary"
                      style={{ width: '100%', justifyContent: 'center' }}
                    >
                      <span>Alquilar esta Cancha</span>
                      <ArrowRight size={14} />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </section>

      {/* Competencias y Torneos del Club */}
      <section id="competencias" style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-md)',
        padding: '28px',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14, marginBottom: 20 }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
              <Trophy size={18} color="var(--primary)" />
              <h2 style={{ fontSize: '1.4rem' }}>Competencias Oficiales</h2>
            </div>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              Torneos eliminatorios y ligas regulares con ranking institucional del complejo.
            </p>
          </div>

          <button
            onClick={() => onRequireAuth('Iniciá sesión para anotarte en los torneos y ligas oficiales del club.')}
            className="btn btn-primary btn-sm"
          >
            <span>Ver Calendario de Competencias</span>
            <ArrowRight size={13} />
          </button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 16 }}>
          <div style={{
            background: 'var(--bg-elevated)',
            border: '1px solid var(--border)',
            borderRadius: 'var(--radius-sm)',
            padding: '18px'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 10 }}>
              <span className="badge badge-primary">Torneo Abierto</span>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>Inscripciones Abiertas</span>
            </div>
            <h4 style={{ fontSize: '1.05rem', marginBottom: 6 }}>Copa Primavera Singles Caballeros</h4>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.8125rem', lineHeight: 1.5, marginBottom: 12 }}>
              Cuadro eliminatorio de 16 jugadores con partidos al mejor de 3 sets reglamentarios.
            </p>
            <div style={{ fontSize: '0.8rem', color: 'var(--primary)', fontWeight: 600 }}>
              Premios oficiales y trofeo institucional
            </div>
          </div>

          <div style={{
            background: 'var(--bg-elevated)',
            border: '1px solid var(--border)',
            borderRadius: 'var(--radius-sm)',
            padding: '18px'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 10 }}>
              <span className="badge badge-grass">Liga Regular</span>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>Temporada 2026</span>
            </div>
            <h4 style={{ fontSize: '1.05rem', marginBottom: 6 }}>Liga Intermedia de Tenis</h4>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.8125rem', lineHeight: 1.5, marginBottom: 12 }}>
              Formato Round Robin (todos contra todos) con tabla de posiciones y ascensos de categoría.
            </p>
            <div style={{ fontSize: '0.8rem', color: 'var(--primary)', fontWeight: 600 }}>
              Fixture semanal programado
            </div>
          </div>
        </div>
      </section>

      {/* Escuela y Beneficios */}
      <section id="escuela" style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
        gap: 16
      }}>
        <div className="card">
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
            <div style={{
              width: 32,
              height: 32,
              borderRadius: 6,
              background: 'var(--primary-subtle)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <CheckCircle2 size={16} color="var(--primary)" />
            </div>
            <h3 style={{ fontSize: '1.05rem' }}>Prioridad de Turnos</h3>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.5 }}>
            Los socios habilitados pueden reservar canchas con hasta 30 días de antelación y abonar el 50% de seña digital con QR.
          </p>
        </div>

        <div className="card">
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
            <div style={{
              width: 32,
              height: 32,
              borderRadius: 6,
              background: 'var(--primary-subtle)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <Award size={16} color="var(--primary)" />
            </div>
            <h3 style={{ fontSize: '1.05rem' }}>Escuela de Alto Rendimiento</h3>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.5 }}>
            Clases magistrales de técnica, saque y volea dictadas por profesores matriculados y entrenamientos físicos personalizados.
          </p>
        </div>

        <div className="card">
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
            <div style={{
              width: 32,
              height: 32,
              borderRadius: 6,
              background: 'var(--accent-hard-bg)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <ShieldCheck size={16} color="var(--accent-hard)" />
            </div>
            <h3 style={{ fontSize: '1.05rem' }}>Equipamiento de Stock</h3>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.5 }}>
            Alquilá raquetas de grafito Wilson y tubos presurizados de pelotas oficiales directamente al momento de confirmar tu turno.
          </p>
        </div>
      </section>
    </div>
  );
}
