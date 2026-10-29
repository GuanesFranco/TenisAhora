import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import {
  Calendar,
  Trophy,
  Award,
  Sparkles,
  ShieldCheck,
  CheckCircle2,
  Clock,
  MapPin,
  Lock,
  ArrowRight,
  LogIn,
  Users
} from 'lucide-react';

export default function LandingPublicaView({ onRequireAuth }) {
  const [canchas, setCanchas] = useState([]);
  const [torneos, setTorneos] = useState([]);
  const [ligas, setLigas] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function cargarDatosPublicos() {
      try {
        const [canchasData, torneosData, ligasData] = await Promise.all([
          api.canchas.listar().catch(() => []),
          api.torneos.listar().catch(() => []),
          api.ligas.listar().catch(() => []),
        ]);
        setCanchas(canchasData);
        setTorneos(torneosData);
        setLigas(ligasData);
      } catch (err) {
        console.error('Error cargando información pública:', err);
      } finally {
        setLoading(false);
      }
    }
    cargarDatosPublicos();
  }, []);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 48, paddingBottom: 60 }}>
      {/* Hero Principal de Bienvenida al Club */}
      <section style={{
        background: 'linear-gradient(135deg, rgba(34, 197, 94, 0.18) 0%, rgba(15, 29, 24, 0.95) 100%)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '56px 40px',
        position: 'relative',
        overflow: 'hidden',
        boxShadow: 'var(--shadow-md)',
      }}>
        <div style={{ maxWidth: 780, position: 'relative', zIndex: 2 }}>
          <div className="badge badge-volt" style={{ marginBottom: 16 }}>
            Complejo Deportivo "El Deporte Saludable"
          </div>
          <h1 style={{ fontSize: '3rem', lineHeight: 1.15, marginBottom: 16, fontWeight: 900 }}>
            Viví el mejor tenis en <span style={{ color: 'var(--accent-volt)' }}>Tenis Ahora</span>
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '1.2rem', lineHeight: 1.6, marginBottom: 32 }}>
            Pistas reglamentarias de polvo de ladrillo, cemento y césped natural. Alquilá tu cancha online con reserva asegurada, participá en torneos y ligas oficiales o sumate a la escuela del club.
          </p>

          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 14 }}>
            <button
              onClick={() => onRequireAuth('Para alquilar una cancha online, por favor iniciá sesión o registrate como socio.')}
              className="btn btn-volt"
              style={{ fontSize: '1rem', padding: '14px 28px' }}
            >
              <Calendar size={20} />
              <span>Reservar una Cancha</span>
            </button>

            <button
              onClick={() => onRequireAuth('Para asociarte al club y obtener tarifas bonificadas, registrate en segundos.')}
              className="btn btn-secondary"
              style={{ fontSize: '1rem', padding: '14px 28px' }}
            >
              <Users size={20} />
              <span>Hacerme Socio del Club</span>
            </button>
          </div>
        </div>

        {/* Decoración gráfica de tenis */}
        <div style={{
          position: 'absolute',
          right: -30,
          bottom: -40,
          fontSize: '18rem',
          opacity: 0.1,
          pointerEvents: 'none',
          userSelect: 'none',
        }}>
          🎾
        </div>
      </section>

      {/* SECCIÓN 1: CANCHAS E INFRAESTRUCTURA (PÚBLICO) */}
      <section id="canchas">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginBottom: 20 }}>
          <div>
            <span className="badge badge-success" style={{ marginBottom: 6 }}>Infraestructura Oficial</span>
            <h2 style={{ fontSize: '1.8rem', display: 'flex', alignItems: 'center', gap: 10 }}>
              <Sparkles size={24} color="var(--accent-volt)" />
              Nuestras Canchas de Tenis
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
              Superficies profesionales habilitadas para partidos de singles y dobles
            </p>
          </div>

          <div style={{ fontSize: '0.85rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: 6 }}>
            <Lock size={14} />
            <span>Alquiler exclusivo para socios registrados</span>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 20 }}>
          {canchas.map((c) => (
            <div
              key={c.id}
              style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-lg)',
                padding: '24px',
                display: 'flex',
                flexDirection: 'column',
                gap: 16,
                transition: 'transform 0.2s ease, border-color 0.2s ease',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className={`badge ${c.tipo === 'Ladrillo' ? 'badge-warning' : c.tipo === 'Pasto' ? 'badge-success' : 'badge-volt'}`}>
                  {c.tipo}
                </span>
                <span className="badge badge-secondary" style={{ fontSize: '0.7rem' }}>Singles / Dobles</span>
              </div>

              <div>
                <h3 style={{ fontSize: '1.25rem', marginBottom: 6, color: 'var(--text-main)' }}>{c.nombre}</h3>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  Superficie: {c.superficie || c.tipo}. Incluye red oficial tensada y equipamiento de stock disponible en recepción.
                </p>
              </div>

              <div style={{
                background: 'var(--bg-dark)',
                padding: '12px 16px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}>
                <span style={{ fontSize: '0.85rem', color: 'var(--text-dim)' }}>Arancel por hora:</span>
                <strong style={{ fontSize: '1.15rem', color: 'var(--accent-volt)' }}>
                  ${(c.precioHora || 10000).toLocaleString('es-AR')}
                </strong>
              </div>

              <button
                onClick={() => onRequireAuth(`Para reservar la ${c.nombre}, por favor iniciá sesión con tu cuenta de socio o registrate.`)}
                className="btn btn-secondary"
                style={{ width: '100%', justifyContent: 'center' }}
              >
                <Lock size={15} />
                <span>Alquilar esta Cancha</span>
              </button>
            </div>
          ))}
        </div>
      </section>

      {/* SECCIÓN 2: TORNEOS Y LIGAS PÚBLICAS */}
      <section id="competencias" style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '36px',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 24 }}>
          <div>
            <span className="badge badge-volt" style={{ marginBottom: 6 }}>Competencias del Club</span>
            <h2 style={{ fontSize: '1.8rem', display: 'flex', alignItems: 'center', gap: 10 }}>
              <Trophy size={26} color="var(--accent-volt)" />
              Torneos Oficiales y Ligas Regulares
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
              Competencias auditadas bajo el reglamento oficial de la Asociación de Tenis
            </p>
          </div>

          <button
            onClick={() => onRequireAuth('Iniciá sesión para anotarte en los torneos y ligas con tu DNI.')}
            className="btn btn-volt btn-sm"
          >
            <span>Inscribirme en una Competencia</span>
          </button>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
          {/* Torneos */}
          <div style={{ background: 'var(--bg-elevated)', borderRadius: 'var(--radius-md)', padding: '20px', border: '1px solid var(--border)' }}>
            <h3 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <Trophy size={18} color="var(--accent-volt)" />
              Cuadros Eliminatorios (Torneos)
            </h3>
            {torneos.length === 0 ? (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Próximamente se anunciará el calendario del nuevo torneo.</p>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {torneos.slice(0, 3).map((t) => (
                  <div key={t.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'var(--bg-dark)', padding: '12px 14px', borderRadius: 'var(--radius-sm)' }}>
                    <div>
                      <strong style={{ fontSize: '0.95rem' }}>{t.nombre}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{t.categoriaGenero} · {t.modalidad}</div>
                    </div>
                    <button
                      onClick={() => onRequireAuth(`Iniciá sesión como socio para inscribirte en el torneo ${t.nombre}.`)}
                      className="btn btn-secondary btn-sm"
                    >
                      Inscribirme
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Ligas */}
          <div style={{ background: 'var(--bg-elevated)', borderRadius: 'var(--radius-md)', padding: '20px', border: '1px solid var(--border)' }}>
            <h3 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12 }}>
              <Award size={18} color="#38BDF8" />
              Ligas Regulares (Round-Robin)
            </h3>
            {ligas.length === 0 ? (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>Próximamente se publicará la nueva liga regular de temporada.</p>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {ligas.slice(0, 3).map((l) => (
                  <div key={l.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', background: 'var(--bg-dark)', padding: '12px 14px', borderRadius: 'var(--radius-sm)' }}>
                    <div>
                      <strong style={{ fontSize: '0.95rem' }}>{l.nombre}</strong>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{l.cantidadFechas} fechas · {l.puntosPorPartido} pts por victoria</div>
                    </div>
                    <button
                      onClick={() => onRequireAuth(`Iniciá sesión como socio para sumarte a la liga ${l.nombre}.`)}
                      className="btn btn-secondary btn-sm"
                    >
                      Inscribirme
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </section>

      {/* SECCIÓN 3: POLÍTICAS TRANSPARENTES DEL COMPLEJO */}
      <section style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 20 }}>
        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-lg)',
          padding: '24px',
          display: 'flex',
          flexDirection: 'column',
          gap: 10,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--accent-volt)', fontWeight: 700 }}>
            <CheckCircle2 size={20} />
            <span>Seña del 50% Obligatoria</span>
          </div>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.6 }}>
            Todas las reservas requieren el abono del 50% inicial mediante Mercado Pago (QR) o tarjetas para quedar confirmadas en el sistema. El saldo restante se cancela al finalizar el partido.
          </p>
        </div>

        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-lg)',
          padding: '24px',
          display: 'flex',
          flexDirection: 'column',
          gap: 10,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--primary)', fontWeight: 700 }}>
            <Clock size={20} />
            <span>Turnos de hasta 2 Horas</span>
          </div>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.6 }}>
            Los alquileres de cancha tienen una duración máxima reglamentaria de 2 horas continuas, garantizando rotación y disponibilidad para todos los socios del predio.
          </p>
        </div>

        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-lg)',
          padding: '24px',
          display: 'flex',
          flexDirection: 'column',
          gap: 10,
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: '#38BDF8', fontWeight: 700 }}>
            <ShieldCheck size={20} />
            <span>Política de Cancelación</span>
          </div>
          <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.6 }}>
            Cancelaciones realizadas con más de 6 horas de antelación al inicio del turno reciben el reembolso del 100% de la seña abonada.
          </p>
        </div>
      </section>

      {/* FOOTER INFORMACIONAL */}
      <footer style={{
        borderTop: '1px solid var(--border)',
        paddingTop: 28,
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        color: 'var(--text-dim)',
        fontSize: '0.85rem'
      }}>
        <div>
          <strong>Tenis Ahora</strong> · Complejo Deportivo "El Deporte Saludable"
        </div>
        <div>
          <button
            onClick={() => onRequireAuth('Ingresá con tus credenciales de empleado administrativo para acceder al panel de gestión.')}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              textDecoration: 'underline',
              fontSize: '0.85rem',
            }}
          >
            Acceso Personal Administrativo
          </button>
        </div>
      </footer>
    </div>
  );
}
