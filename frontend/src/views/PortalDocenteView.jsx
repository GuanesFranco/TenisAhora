import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { TennisMark } from '../components/Logo';
import {
  GraduationCap,
  Dumbbell,
  CheckCircle2,
  Calendar,
  Clock,
  Users,
  Award,
  Printer,
  ShieldCheck,
  Check,
  X,
  AlertCircle,
  FileCheck,
  UserCheck,
  Search,
  ArrowRight
} from 'lucide-react';

export default function PortalDocenteView({ activeSubTab = 'portal-docente' }) {
  const { user } = useAuth();
  const isProfesor = user?.rol === 'Profesor';
  const isEntrenador = user?.rol === 'Entrenador';
  const esDocente = isProfesor || isEntrenador;

  const [currentSection, setCurrentSection] = useState(activeSubTab || 'portal-docente');
  const [clases, setClases] = useState([]);
  const [entrenamientos, setEntrenamientos] = useState([]);
  const [credencialData, setCredencialData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [mensajeExito, setMensajeExito] = useState('');
  const [mensajeError, setMensajeError] = useState('');

  // Estados para toma de asistencia (RF-47 y RF-54)
  const [actividadSeleccionada, setActividadSeleccionada] = useState(null);
  const [fechaAsistencia, setFechaAsistencia] = useState(new Date().toISOString().slice(0, 10));
  const [dniAlumno, setDniAlumno] = useState('');
  const [alumnoPresente, setAlumnoPresente] = useState(true);
  const [guardandoAsistencia, setGuardandoAsistencia] = useState(false);
  const [asistenciasRegistradas, setAsistenciasRegistradas] = useState([]);

  // Mantener sincronizado si cambia la prop
  useEffect(() => {
    if (activeSubTab) {
      setCurrentSection(activeSubTab);
    }
  }, [activeSubTab]);

  useEffect(() => {
    cargarDatos();
  }, [user]);

  async function cargarDatos() {
    setLoading(true);
    setMensajeError('');
    try {
      const [listaClases, listaEntrenamientos] = await Promise.all([
        api.clases.listar().catch(() => []),
        api.entrenamientos.listar().catch(() => []),
      ]);

      // Filtrar o mostrar las clases/entrenamientos asignados
      setClases(listaClases || []);
      setEntrenamientos(listaEntrenamientos || []);

      if (listaClases && listaClases.length > 0 && isProfesor) {
        setActividadSeleccionada({ tipo: 'clase', ...listaClases[0] });
      } else if (listaEntrenamientos && listaEntrenamientos.length > 0 && isEntrenador) {
        setActividadSeleccionada({ tipo: 'entrenamiento', ...listaEntrenamientos[0] });
      }

      // Cargar credencial y título habilitante (RF-59 / RF-64 / RD-04 / RD-05)
      try {
        if (isProfesor) {
          const profData = await api.profesores.imprimir(user?.id || 3);
          setCredencialData(profData);
        } else if (isEntrenador) {
          const entData = await api.entrenadores.imprimir(user?.id || 4);
          setCredencialData(entData);
        }
      } catch (err) {
        console.warn('No se pudo cargar la credencial completa desde el endpoint, usando datos de sesión:', err);
      }
    } catch (err) {
      setMensajeError('Error cargando la agenda del portal docente: ' + (err.message || 'Error de conexión'));
    } finally {
      setLoading(false);
    }
  }

  // Enviar toma de asistencia (RF-47 / RF-54)
  const handleRegistrarAsistencia = async (e) => {
    e.preventDefault();
    if (!actividadSeleccionada) {
      setMensajeError('Por favor seleccioná una clase o sesión de entrenamiento.');
      return;
    }
    const dniParsed = parseInt(dniAlumno, 10);
    if (!dniParsed || isNaN(dniParsed)) {
      setMensajeError('Ingresá un número de DNI válido del alumno.');
      return;
    }

    setGuardandoAsistencia(true);
    setMensajeError('');
    setMensajeExito('');

    try {
      let respuesta;
      if (actividadSeleccionada.tipo === 'clase' || isProfesor) {
        respuesta = await api.clases.registrarAsistencia(actividadSeleccionada.id, {
          dni: dniParsed,
          presente: alumnoPresente,
          fecha: new Date(fechaAsistencia).toISOString()
        });
      } else {
        respuesta = await api.entrenamientos.registrarAsistencia(actividadSeleccionada.id, {
          dni: dniParsed,
          presente: alumnoPresente,
          fecha: new Date(fechaAsistencia).toISOString()
        });
      }

      const nuevoRegistro = {
        id: Date.now(),
        dni: dniParsed,
        nombre: respuesta.nombreSocio ? `${respuesta.nombreSocio} ${respuesta.apellidoSocio}` : `Alumno DNI ${dniParsed}`,
        presente: alumnoPresente,
        fecha: fechaAsistencia,
        actividad: actividadSeleccionada.nombre
      };

      setAsistenciasRegistradas(prev => [nuevoRegistro, ...prev]);
      setMensajeExito(`¡Asistencia de ${nuevoRegistro.nombre} asentada correctamente como ${alumnoPresente ? 'PRESENTE' : 'AUSENTE'}!`);
      setDniAlumno('');
    } catch (err) {
      setMensajeError(err.message || 'Error al asentar asistencia del alumno.');
    } finally {
      setGuardandoAsistencia(false);
    }
  };

  const handlePrintCredencial = () => {
    window.print();
  };

  // Sesiones activas según el rol
  const misSesiones = isProfesor
    ? clases.filter(c => c.profesorId === user?.id || !c.profesorId || c.nombreProfesor?.includes(user?.nombre || ''))
    : entrenamientos.filter(e => e.entrenadorId === user?.id || !e.entrenadorId || e.nombreEntrenador?.includes(user?.nombre || ''));

  const listaCompleta = misSesiones.length > 0 ? misSesiones : (isProfesor ? clases : entrenamientos);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 28 }}>
      {/* Banner de Bienvenida del Docente / Entrenador */}
      <section className="card" style={{
        padding: '24px 28px',
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 20,
        background: 'linear-gradient(135deg, var(--bg-card) 0%, var(--bg-elevated) 100%)',
        border: '1px solid var(--border)'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 18 }}>
          <div style={{
            width: 56,
            height: 56,
            borderRadius: 'var(--radius-md)',
            background: 'var(--primary-subtle)',
            border: '1px solid var(--primary-subtle-border)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--primary)',
            boxShadow: 'var(--shadow-sm)'
          }}>
            {isProfesor ? <GraduationCap size={30} /> : <Dumbbell size={30} />}
          </div>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
              <span className="badge badge-success">
                {isProfesor ? 'Profesor Titular Oficial' : 'Entrenador Físico Certificado'}
              </span>
              <span className="badge badge-info" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                <ShieldCheck size={13} />
                <span>Título Habilitante</span>
              </span>
            </div>
            <h1 style={{ fontSize: '1.45rem', fontWeight: 700, color: 'var(--text-main)', letterSpacing: '-0.02em' }}>
              Portal Académico &bull; {user?.nombre || 'Docente'} {user?.apellido || 'Titular'}
            </h1>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              Gestión nominal de presentismo, agenda de turnos y certificación oficial habilitante para impartir clases.
            </p>
          </div>
        </div>

        {/* Botones de acción rápida */}
        <div style={{ display: 'flex', gap: 10 }}>
          <button
            onClick={() => setCurrentSection('asistencia')}
            className={`btn ${currentSection === 'asistencia' ? 'btn-primary' : 'btn-secondary'}`}
          >
            <CheckCircle2 size={16} />
            <span>Toma de Asistencia</span>
          </button>
          <button
            onClick={() => setCurrentSection('credencial-docente')}
            className={`btn ${currentSection === 'credencial-docente' ? 'btn-primary' : 'btn-secondary'}`}
          >
            <Award size={16} />
            <span>Ver Mi Credencial</span>
          </button>
        </div>
      </section>

      {/* Tarjetas de Métricas Académicas */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 16 }}>
        <div className="card" style={{ padding: '18px 20px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem', fontWeight: 500, textTransform: 'uppercase' }}>
              {isProfesor ? 'Clases a Cargo' : 'Entrenamientos a Cargo'}
            </span>
            <Calendar size={18} color="var(--primary)" />
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-main)', marginTop: 8 }}>
            {listaCompleta.length}
          </div>
          <small style={{ color: 'var(--text-dim)', fontSize: '0.785rem' }}>
            Sesiones activas en la temporada 2026
          </small>
        </div>

        <div className="card" style={{ padding: '18px 20px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem', fontWeight: 500, textTransform: 'uppercase' }}>
              Cupo Reglamentario
            </span>
            <Users size={18} color="var(--primary)" />
          </div>
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-main)', marginTop: 8 }}>
            Hasta 30
          </div>
          <small style={{ color: 'var(--text-dim)', fontSize: '0.785rem' }}>
            Límite máximo estricto de alumnos por sesión
          </small>
        </div>

        <div className="card" style={{ padding: '18px 20px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem', fontWeight: 500, textTransform: 'uppercase' }}>
              Título Habilitante
            </span>
            <Award size={18} color="var(--primary)" />
          </div>
          <div style={{ fontSize: '1.15rem', fontWeight: 700, color: 'var(--primary-subtle-text)', marginTop: 12 }}>
            Federación Arg. de Tenis
          </div>
          <small style={{ color: 'var(--text-dim)', fontSize: '0.785rem' }}>
            Certificación deportiva homologada y vigente
          </small>
        </div>
      </div>

      {/* Navegación por Pestañas del Portal Docente */}
      <div style={{
        display: 'flex',
        gap: 8,
        borderBottom: '1px solid var(--border)',
        paddingBottom: 10,
        overflowX: 'auto'
      }}>
        <button
          onClick={() => setCurrentSection('portal-docente')}
          style={{
            background: currentSection === 'portal-docente' ? 'var(--primary-subtle)' : 'transparent',
            color: currentSection === 'portal-docente' ? 'var(--primary-subtle-text)' : 'var(--text-muted)',
            border: currentSection === 'portal-docente' ? '1px solid var(--primary-subtle-border)' : '1px solid transparent',
            padding: '8px 16px',
            borderRadius: 'var(--radius-sm)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: 8
          }}
        >
          {isProfesor ? <GraduationCap size={16} /> : <Dumbbell size={16} />}
          <span>{isProfesor ? 'Mis Clases Asignadas' : 'Mis Entrenamientos'}</span>
        </button>

        <button
          onClick={() => setCurrentSection('asistencia')}
          style={{
            background: currentSection === 'asistencia' ? 'var(--primary-subtle)' : 'transparent',
            color: currentSection === 'asistencia' ? 'var(--primary-subtle-text)' : 'var(--text-muted)',
            border: currentSection === 'asistencia' ? '1px solid var(--primary-subtle-border)' : '1px solid transparent',
            padding: '8px 16px',
            borderRadius: 'var(--radius-sm)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: 8
          }}
        >
          <CheckCircle2 size={16} />
          <span>Toma de Asistencia</span>
        </button>

        <button
          onClick={() => setCurrentSection('clases-particulares')}
          style={{
            background: currentSection === 'clases-particulares' ? 'var(--primary-subtle)' : 'transparent',
            color: currentSection === 'clases-particulares' ? 'var(--primary-subtle-text)' : 'var(--text-muted)',
            border: currentSection === 'clases-particulares' ? '1px solid var(--primary-subtle-border)' : '1px solid transparent',
            padding: '8px 16px',
            borderRadius: 'var(--radius-sm)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: 8
          }}
        >
          <Users size={16} />
          <span>Clases Particulares</span>
        </button>

        <button
          onClick={() => setCurrentSection('credencial-docente')}
          style={{
            background: currentSection === 'credencial-docente' ? 'var(--primary-subtle)' : 'transparent',
            color: currentSection === 'credencial-docente' ? 'var(--primary-subtle-text)' : 'var(--text-muted)',
            border: currentSection === 'credencial-docente' ? '1px solid var(--primary-subtle-border)' : '1px solid transparent',
            padding: '8px 16px',
            borderRadius: 'var(--radius-sm)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: 8
          }}
        >
          <Award size={16} />
          <span>Título y Credencial</span>
        </button>
      </div>

      {/* Alertas */}
      {mensajeExito && (
        <div style={{
          background: 'var(--primary-subtle)',
          border: '1px solid var(--primary-subtle-border)',
          color: 'var(--primary-subtle-text)',
          padding: '12px 16px',
          borderRadius: 'var(--radius-sm)',
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          fontSize: '0.875rem'
        }}>
          <CheckCircle2 size={18} />
          <span>{mensajeExito}</span>
        </div>
      )}

      {mensajeError && (
        <div style={{
          background: 'rgba(239, 68, 68, 0.1)',
          border: '1px solid rgba(239, 68, 68, 0.3)',
          color: '#f87171',
          padding: '12px 16px',
          borderRadius: 'var(--radius-sm)',
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          fontSize: '0.875rem'
        }}>
          <AlertCircle size={18} />
          <span>{mensajeError}</span>
        </div>
      )}

      {/* SECCIÓN 1: MIS CLASES / ENTRENAMIENTOS ASIGNADOS */}
      {currentSection === 'portal-docente' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <h2 style={{ fontSize: '1.2rem', fontWeight: 600, color: 'var(--text-main)' }}>
                {isProfesor ? 'Clases de Tenis Programadas' : 'Sesiones de Entrenamiento Programadas'}
              </h2>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
                Agenda de turnos según los requerimientos académicos del club (Cupo máx. 30 alumnos ).
              </p>
            </div>
            <button
              onClick={() => setCurrentSection('asistencia')}
              className="btn btn-volt btn-sm"
            >
              <CheckCircle2 size={15} />
              <span>Pasar Lista a una Clase</span>
            </button>
          </div>

          {loading ? (
            <div className="card" style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
              Cargando cronograma oficial...
            </div>
          ) : listaCompleta.length === 0 ? (
            <div className="card" style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
              No hay actividades programadas asignadas en este momento.
            </div>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 18 }}>
              {listaCompleta.map((act) => {
                const cupoMax = act.capacidadMaxma || act.capacidadMaxima || 30;
                const inscriptos = act.cantidadDeAlumnos || 0;
                const porcentaje = Math.min(100, Math.round((inscriptos / cupoMax) * 100));

                return (
                  <div key={act.id} className="card" style={{
                    padding: 20,
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'space-between',
                    gap: 16
                  }}>
                    <div>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
                        <span className="badge badge-success">
                          {isProfesor ? 'Clase de Tenis' : 'Entrenamiento Físico'}
                        </span>
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)', fontWeight: 600 }}>
                          ID #{act.id}
                        </span>
                      </div>

                      <h3 style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-main)', marginBottom: 8 }}>
                        {act.nombre}
                      </h3>

                      <div style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                          <Calendar size={14} color="var(--primary)" />
                          <span>
                            {act.horaInicio ? new Date(act.horaInicio).toLocaleDateString('es-AR', {
                              weekday: 'long',
                              day: 'numeric',
                              month: 'long',
                              year: 'numeric'
                            }) : 'Fecha a definir'}
                          </span>
                        </div>
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                          <Clock size={14} color="var(--primary)" />
                          <span>
                            {act.horaInicio ? new Date(act.horaInicio).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '10:00'} hs &bull; {act.duracion || 90} minutos
                          </span>
                        </div>
                      </div>
                    </div>

                    {/* Barra de Cupo de Alumnos (RD-01 / RD-02 máx 30) */}
                    <div>
                      <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.785rem', marginBottom: 6 }}>
                        <span style={{ color: 'var(--text-muted)' }}>Ocupación de vacantes</span>
                        <strong style={{ color: 'var(--text-main)' }}>{inscriptos} / {cupoMax} alumnos (Máx. 30)</strong>
                      </div>
                      <div style={{
                        width: '100%',
                        height: 6,
                        background: 'var(--bg-elevated)',
                        borderRadius: 3,
                        overflow: 'hidden'
                      }}>
                        <div style={{
                          width: `${porcentaje}%`,
                          height: '100%',
                          background: 'var(--primary)',
                          borderRadius: 3
                        }} />
                      </div>
                    </div>

                    <div style={{ display: 'flex', gap: 10, paddingTop: 10, borderTop: '1px solid var(--border)' }}>
                      <button
                        onClick={() => {
                          setActividadSeleccionada({ tipo: isProfesor ? 'clase' : 'entrenamiento', ...act });
                          setCurrentSection('asistencia');
                        }}
                        className="btn btn-volt btn-sm"
                        style={{ flex: 1 }}
                      >
                        <CheckCircle2 size={15} />
                        <span>Tomar Asistencia</span>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* SECCIÓN 2: TOMA DE ASISTENCIA NOMINAL (RF-47 y RF-54) */}
      {currentSection === 'asistencia' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'minmax(300px, 420px) 1fr', gap: 24, alignItems: 'start' }}>
          {/* Panel de Registro */}
          <div className="card" style={{ padding: 24 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 16 }}>
              <div style={{
                width: 36,
                height: 36,
                borderRadius: '50%',
                background: 'var(--primary-subtle)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--primary)'
              }}>
                <CheckCircle2 size={18} />
              </div>
              <div>
                <h3 style={{ fontSize: '1.05rem', fontWeight: 600, color: 'var(--text-main)' }}>
                  Presentismo Nominal
                </h3>
                <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>
                  Control de Presentismo
                </small>
              </div>
            </div>

            <form onSubmit={handleRegistrarAsistencia} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div>
                <label className="form-label">Actividad o Clase a Cargo</label>
                <select
                  className="form-control"
                  value={actividadSeleccionada?.id || ''}
                  onChange={(e) => {
                    const id = parseInt(e.target.value, 10);
                    const found = listaCompleta.find(item => item.id === id);
                    if (found) {
                      setActividadSeleccionada({ tipo: isProfesor ? 'clase' : 'entrenamiento', ...found });
                    }
                  }}
                >
                  {listaCompleta.map(a => (
                    <option key={a.id} value={a.id}>
                      #{a.id} - {a.nombre}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="form-label">Fecha de la Sesión</label>
                <input
                  type="date"
                  className="form-control"
                  value={fechaAsistencia}
                  onChange={(e) => setFechaAsistencia(e.target.value)}
                  required
                />
              </div>

              <div>
                <label className="form-label">DNI del Alumno Inscripto</label>
                <input
                  type="number"
                  className="form-control"
                  type="text" inputMode="numeric" maxLength={8} placeholder="Ej. 12345678" value={dniAlumno} onChange={(e) => setDniAlumno(e.target.value.replace(/\D/g, '').slice(0, 8))}
                  required
                />
                <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                  Ingresá el DNI del socio inscripto en esta clase.
                </small>
              </div>

              <div>
                <label className="form-label">Estado de Asistencia</label>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
                  <button
                    type="button"
                    onClick={() => setAlumnoPresente(true)}
                    style={{
                      background: alumnoPresente ? 'var(--primary)' : 'var(--bg-elevated)',
                      color: alumnoPresente ? 'var(--primary-text)' : 'var(--text-muted)',
                      border: '1px solid ' + (alumnoPresente ? 'var(--primary)' : 'var(--border)'),
                      padding: '10px',
                      borderRadius: 'var(--radius-sm)',
                      fontWeight: 600,
                      fontSize: '0.85rem',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      gap: 6
                    }}
                  >
                    <Check size={16} />
                    <span>Presente</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => setAlumnoPresente(false)}
                    style={{
                      background: !alumnoPresente ? '#ef4444' : 'var(--bg-elevated)',
                      color: !alumnoPresente ? '#ffffff' : 'var(--text-muted)',
                      border: '1px solid ' + (!alumnoPresente ? '#ef4444' : 'var(--border)'),
                      padding: '10px',
                      borderRadius: 'var(--radius-sm)',
                      fontWeight: 600,
                      fontSize: '0.85rem',
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      gap: 6
                    }}
                  >
                    <X size={16} />
                    <span>Ausente</span>
                  </button>
                </div>
              </div>

              <button
                type="submit"
                disabled={guardandoAsistencia}
                className="btn btn-volt"
                style={{ width: '100%', marginTop: 8 }}
              >
                <CheckCircle2 size={16} />
                <span>{guardandoAsistencia ? 'Guardando en Base de Datos...' : 'Asentar Asistencia Oficial'}</span>
              </button>
            </form>
          </div>

          {/* Historial de Asistencias Registradas en esta Sesión */}
          <div className="card" style={{ padding: 24 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <div>
                <h3 style={{ fontSize: '1.05rem', fontWeight: 600, color: 'var(--text-main)' }}>
                  Registros de Presentismo de Hoy
                </h3>
                <small style={{ color: 'var(--text-muted)', fontSize: '0.785rem' }}>
                  Asistencias validadas por el docente a cargo
                </small>
              </div>
              <span className="badge badge-success">
                {asistenciasRegistradas.length} registradas
              </span>
            </div>

            {asistenciasRegistradas.length === 0 ? (
              <div style={{
                padding: 36,
                textAlign: 'center',
                color: 'var(--text-dim)',
                background: 'var(--bg-elevated)',
                borderRadius: 'var(--radius-sm)',
                border: '1px dashed var(--border)'
              }}>
                <UserCheck size={32} style={{ margin: '0 auto 10px', opacity: 0.5 }} />
                <p style={{ margin: 0, fontSize: '0.875rem' }}>
                  Aún no se han asentado asistencias en esta sesión. Ingresá el DNI del alumno para comenzar.
                </p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                {asistenciasRegistradas.map((asist) => (
                  <div
                    key={asist.id}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      padding: '12px 14px',
                      background: 'var(--bg-elevated)',
                      borderRadius: 'var(--radius-sm)',
                      border: '1px solid var(--border)'
                    }}
                  >
                    <div>
                      <strong style={{ display: 'block', color: 'var(--text-main)', fontSize: '0.9rem' }}>
                        {asist.nombre}
                      </strong>
                      <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem' }}>
                        DNI: {asist.dni} &bull; {asist.actividad}
                      </small>
                    </div>

                    <div>
                      {asist.presente ? (
                        <span className="badge badge-success" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                          <Check size={12} /> Presente
                        </span>
                      ) : (
                        <span className="badge badge-danger" style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                          <X size={12} /> Ausente
                        </span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      {/* SECCIÓN 3: CLASES PARTICULARES ASIGNADAS (RF-06, RF-07, RF-48) */}
      {currentSection === 'clases-particulares' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div>
            <h2 style={{ fontSize: '1.2rem', fontWeight: 600, color: 'var(--text-main)' }}>
              {isProfesor ? 'Clases Particulares 1 a 1 Asignadas' : 'Entrenamientos Individuales Asignados'}
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              Asignaciones realizadas por la administración del club autorizadas por la administración del club..
            </p>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 16 }}>
            <div className="card" style={{ padding: 20 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                <span className="badge badge-volt">Particular 1 a 1</span>
                <span className="badge badge-success">Confirmada</span>
              </div>
              <h3 style={{ fontSize: '1.05rem', fontWeight: 600, color: 'var(--text-main)', marginBottom: 6 }}>
                Clase Particular &bull; Juan Pérez (Socio #2)
              </h3>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginBottom: 12 }}>
                Enfoque: Corrección de empuñadura y perfeccionamiento de revés a dos manos.
              </p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 6, fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Calendar size={14} color="var(--primary)" />
                  <span>Próximo Lunes 18:00 hs</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Clock size={14} color="var(--primary)" />
                  <span>Duración: 60 minutos &bull; Cancha 1 (Ladrillo)</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* SECCIÓN 4: TÍTULO HABILITANTE Y CREDENCIAL OFICIAL (RD-04/05 y RF-59/64) */}
      {currentSection === 'credencial-docente' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <h2 style={{ fontSize: '1.2rem', fontWeight: 600, color: 'var(--text-main)' }}>
                Título Habilitante y Credencial Oficial
              </h2>
              <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
                Acreditación deportiva homologada por la Asociación y Federación Argentina de Tenis .
              </p>
            </div>
            <button
              onClick={handlePrintCredencial}
              className="btn btn-volt"
            >
              <Printer size={16} />
              <span>Imprimir Credencial Oficial</span>
            </button>
          </div>

          {/* Tarjeta de Credencial Física / Digital Homologada */}
          <div style={{ maxWidth: 640, margin: '0 auto', width: '100%' }}>
            <div
              className="card credential-card-print"
              style={{
                background: 'linear-gradient(145deg, #0d151c 0%, #14212e 100%)',
                border: '2px solid var(--primary)',
                borderRadius: 'var(--radius-lg)',
                padding: '28px 32px',
                color: '#ffffff',
                boxShadow: 'var(--shadow-lg)',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              {/* Marca de agua Tenis Ahora */}
              <div style={{
                position: 'absolute',
                right: -20,
                bottom: -20,
                opacity: 0.08,
                pointerEvents: 'none'
              }}>
                <TennisMark size={220} />
              </div>

              {/* Cabecera de la Credencial */}
              <div style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                borderBottom: '1px solid rgba(255, 255, 255, 0.15)',
                paddingBottom: 16,
                marginBottom: 20
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <TennisMark size={32} />
                  <div>
                    <strong style={{ display: 'block', fontSize: '0.95rem', letterSpacing: '0.04em', color: 'var(--primary)' }}>
                      CLUB DE TENIS TENIS AHORA
                    </strong>
                    <span style={{ fontSize: '0.7rem', color: '#94a3b8', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                      Complejo El Deporte Saludable &bull; Afiliado FAT / AAT
                    </span>
                  </div>
                </div>

                <div style={{ textAlign: 'right' }}>
                  <span style={{
                    fontSize: '0.65rem',
                    background: 'rgba(16, 185, 129, 0.2)',
                    border: '1px solid #10b981',
                    color: '#34d399',
                    padding: '3px 8px',
                    borderRadius: 4,
                    fontWeight: 700,
                    textTransform: 'uppercase'
                  }}>
                    Homologado
                  </span>
                </div>
              </div>

              {/* Contenido Central de la Credencial */}
              <div style={{ display: 'flex', gap: 24, alignItems: 'center' }}>
                <div style={{
                  width: 90,
                  height: 110,
                  borderRadius: 8,
                  background: 'var(--bg-elevated)',
                  border: '1px solid var(--border)',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--primary)',
                  boxShadow: 'inset 0 0 10px rgba(0,0,0,0.5)'
                }}>
                  {isProfesor ? <GraduationCap size={44} /> : <Dumbbell size={44} />}
                  <span style={{ fontSize: '0.625rem', marginTop: 6, color: '#94a3b8', textTransform: 'uppercase' }}>
                    Foto Oficial
                  </span>
                </div>

                <div style={{ flex: 1 }}>
                  <span style={{
                    fontSize: '0.7rem',
                    color: 'var(--primary)',
                    fontWeight: 600,
                    textTransform: 'uppercase',
                    letterSpacing: '0.08em'
                  }}>
                    {isProfesor ? 'Docente Titular Acreditado' : 'Entrenador Físico Homologado'}
                  </span>
                  <h3 style={{ fontSize: '1.4rem', fontWeight: 700, margin: '2px 0 8px', color: '#ffffff' }}>
                    {credencialData?.nombre || user?.nombre || 'Carlos'} {credencialData?.apellido || user?.apellido || 'Peralta'}
                  </h3>

                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '6px 16px', fontSize: '0.8rem', color: '#cbd5e1' }}>
                    <div>
                      <span style={{ color: '#64748b', display: 'block', fontSize: '0.675rem' }}>DNI / MATRÍCULA</span>
                      <strong>{credencialData?.dni || user?.dni || '32.123.456'}</strong>
                    </div>
                    <div>
                      <span style={{ color: '#64748b', display: 'block', fontSize: '0.675rem' }}>LEGAJO DOCENTE</span>
                      <strong>ID #{credencialData?.id || user?.id || 3}</strong>
                    </div>
                    <div>
                      <span style={{ color: '#64748b', display: 'block', fontSize: '0.675rem' }}>TÍTULO HABILITANTE OFICIAL</span>
                      <strong style={{ color: '#38bdf8' }}>
                        {credencialData?.nombreCertificacion || (isProfesor ? 'Certificación de Profesor de Tenis' : 'Certificación de Entrenador de Tenis')}
                      </strong>
                    </div>
                    <div>
                      <span style={{ color: '#64748b', display: 'block', fontSize: '0.675rem' }}>ENTE EMISOR</span>
                      <strong style={{ color: '#38bdf8' }}>
                        {credencialData?.enteEmisor || 'Federación Argentina de Tenis (FAT)'}
                      </strong>
                    </div>
                  </div>
                </div>
              </div>

              {/* Pie de la Credencial con Código de Barras / Validación */}
              <div style={{
                marginTop: 22,
                paddingTop: 14,
                borderTop: '1px solid rgba(255, 255, 255, 0.12)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                fontSize: '0.75rem',
                color: '#94a3b8'
              }}>
                <div>
                  <span>Emisión: {credencialData?.fechaEmision ? new Date(credencialData.fechaEmision).toLocaleDateString('es-AR') : '01/03/2024'} &bull; Vigencia 2026</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                  <FileCheck size={14} color="#34d399" />
                  <span style={{ color: '#34d399', fontWeight: 600 }}>Firma y Sello Registrados</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

