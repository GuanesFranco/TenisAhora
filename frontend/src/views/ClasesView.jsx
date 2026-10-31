import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import {
  GraduationCap,
  Dumbbell,
  Calendar,
  Clock,
  Users,
  Plus,
  CheckCircle2,
  AlertTriangle,
  X,
  UserCheck,
  Trash2,
  Sparkles,
  Award
} from 'lucide-react';

export default function ClasesView() {
  const { user } = useAuth();
  const isAdmin = user?.rol === 'Administrador';

  const [activeTab, setActiveTab] = useState('clases'); // 'clases' | 'entrenamientos'
  const [clases, setClases] = useState([]);
  const [entrenamientos, setEntrenamientos] = useState([]);
  const [profesores, setProfesores] = useState([]);
  const [entrenadores, setEntrenadores] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // Modales
  const [modalClaseOpen, setModalClaseOpen] = useState(false);
  const [modalEntrenamientoOpen, setModalEntrenamientoOpen] = useState(false);
  const [modalAsistenciaOpen, setModalAsistenciaOpen] = useState(false);
  const [actividadParaAsistencia, setActividadParaAsistencia] = useState(null);

  // Form State Clase
  const [nombreClase, setNombreClase] = useState('');
  const [tipoClase, setTipoClase] = useState(0); // 0=Tecnica, 1=Tactica, 2=Saque, 3=Peloteo
  const [nivelClase, setNivelClase] = useState(0); // 0=Principiante, 1=Intermedio, 2=Avanzado
  const [fechaClase, setFechaClase] = useState(new Date(Date.now() + 86400000).toISOString().slice(0, 10));
  const [horaInicioClase, setHoraInicioClase] = useState('10:00');
  const [duracionMinutosClase, setDuracionMinutosClase] = useState(90);
  const [capacidadClase, setCapacidadClase] = useState(6);
  const [profesorId, setProfesorId] = useState('');

  // Form State Entrenamiento
  const [nombreEntrenamiento, setNombreEntrenamiento] = useState('');
  const [objetivoEntrenamiento, setObjetivoEntrenamiento] = useState(1); // 0=Tecnica, 1=Resistencia, 2=PreparacionCompetitiva
  const [nivelEntrenamiento, setNivelEntrenamiento] = useState(1);
  const [fechaEntrenamiento, setFechaEntrenamiento] = useState(new Date(Date.now() + 86400000).toISOString().slice(0, 10));
  const [horaInicioEntrenamiento, setHoraInicioEntrenamiento] = useState('18:00');
  const [duracionMinutosEntrenamiento, setDuracionMinutosEntrenamiento] = useState(90);
  const [capacidadEntrenamiento, setCapacidadEntrenamiento] = useState(10);
  const [entrenadorId, setEntrenadorId] = useState('');

  // Asistencia Form
  const [asistenciaDni, setAsistenciaDni] = useState('');
  const [asistenciaPresente, setAsistenciaPresente] = useState(true);

  useEffect(() => {
    cargarTodo();
  }, []);

  async function cargarTodo() {
    setLoading(true);
    setError('');
    try {
      const [listaClases, listaEntrenamientos, listaProfs, listaEnts] = await Promise.all([
        api.clases.listar().catch(() => []),
        api.entrenamientos.listar().catch(() => []),
        api.profesores.listar().catch(() => []),
        api.entrenadores.listar().catch(() => []),
      ]);
      setClases(listaClases || []);
      setEntrenamientos(listaEntrenamientos || []);
      setProfesores(listaProfs || []);
      setEntrenadores(listaEnts || []);
      if (listaProfs?.length > 0) setProfesorId(listaProfs[0].id);
      if (listaEnts?.length > 0) setEntrenadorId(listaEnts[0].id);
    } catch (err) {
      setError(err.message || 'Error cargando clases y entrenamientos.');
    } finally {
      setLoading(false);
    }
  }

  function mostrarMensajeExito(msg) {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 5000);
  }

  // Inscripción para Socio
  async function handleInscribirClase(claseId) {
    if (!user) return;
    try {
      setError('');
      await api.clases.inscribir(claseId, user.dni || 40123456);
      mostrarMensajeExito('¡Te inscribiste exitosamente a la clase!');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleInscribirEntrenamiento(entrenamientoId) {
    if (!user) return;
    try {
      setError('');
      await api.entrenamientos.inscribir(entrenamientoId, user.dni || 40123456);
      mostrarMensajeExito('¡Te inscribiste exitosamente al entrenamiento!');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  // Crear Clase (Admin)
  async function handleCrearClase(e) {
    e.preventDefault();
    try {
      setError('');
      const fechaBase = new Date(`${fechaClase}T${horaInicioClase}:00`);
      const fechaFin = new Date(fechaBase.getTime() + duracionMinutosClase * 60000);

      await api.clases.crear({
        nombre: nombreClase,
        tipoDeClase: parseInt(tipoClase, 10),
        nivelActividad: parseInt(nivelClase, 10),
        horaInicio: fechaBase.toISOString(),
        horaFin: fechaFin.toISOString(),
        duracion: parseInt(duracionMinutosClase, 10),
        capacidadMaxma: parseInt(capacidadClase, 10),
        profesorId: profesorId ? parseInt(profesorId, 10) : null,
      });

      mostrarMensajeExito('Clase de tenis creada exitosamente.');
      setModalClaseOpen(false);
      setNombreClase('');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  // Crear Entrenamiento (Admin)
  async function handleCrearEntrenamiento(e) {
    e.preventDefault();
    try {
      setError('');
      const fechaBase = new Date(`${fechaEntrenamiento}T${horaInicioEntrenamiento}:00`);
      const fechaFin = new Date(fechaBase.getTime() + duracionMinutosEntrenamiento * 60000);

      await api.entrenamientos.crear({
        nombre: nombreEntrenamiento,
        objetivoEntrenamiento: parseInt(objetivoEntrenamiento, 10),
        nivelActividad: parseInt(nivelEntrenamiento, 10),
        horaInicio: fechaBase.toISOString(),
        horaFin: fechaFin.toISOString(),
        duracion: parseInt(duracionMinutosEntrenamiento, 10),
        capacidadMaxma: parseInt(capacidadEntrenamiento, 10),
        entrenadorId: entrenadorId ? parseInt(entrenadorId, 10) : null,
      });

      mostrarMensajeExito('Entrenamiento creado exitosamente.');
      setModalEntrenamientoOpen(false);
      setNombreEntrenamiento('');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  // Dar de Baja
  async function handleBajaClase(id) {
    if (!window.confirm('¿Confirma dar de baja esta clase?')) return;
    try {
      await api.clases.darDeBaja(id);
      mostrarMensajeExito('La clase fue dada de baja.');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleBajaEntrenamiento(id) {
    if (!window.confirm('¿Confirma dar de baja este entrenamiento?')) return;
    try {
      await api.entrenamientos.darDeBaja(id);
      mostrarMensajeExito('El entrenamiento fue dado de baja.');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  // Registrar Asistencia (Admin)
  async function handleRegistrarAsistencia(e) {
    e.preventDefault();
    if (!actividadParaAsistencia) return;
    try {
      setError('');
      if (actividadParaAsistencia.tipo === 'clase') {
        await api.clases.registrarAsistencia(actividadParaAsistencia.id, {
          dni: parseInt(asistenciaDni, 10),
          presente: asistenciaPresente,
          fecha: new Date().toISOString(),
        });
      } else {
        await api.entrenamientos.registrarAsistencia(actividadParaAsistencia.id, {
          dni: parseInt(asistenciaDni, 10),
          presente: asistenciaPresente,
          fecha: new Date().toISOString(),
        });
      }
      mostrarMensajeExito(`Asistencia registrada para el DNI ${asistenciaDni}.`);
      setModalAsistenciaOpen(false);
      setAsistenciaDni('');
      cargarTodo();
    } catch (err) {
      setError(err.message);
    }
  }

  const nivelesNombres = ['Principiante', 'Intermedio', 'Avanzado'];
  const tiposClaseNombres = ['Técnica', 'Táctica', 'Saque y Volea', 'Peloteo'];
  const objetivosNombres = ['Técnica', 'Resistencia Aeróbica', 'Preparación Competitiva'];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 16,
        borderBottom: '1px solid var(--border)',
        paddingBottom: 20
      }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
            <GraduationCap size={26} color="var(--accent-volt)" />
            <h1 style={{ fontSize: '2rem' }}>Clases y Entrenamientos</h1>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            {isAdmin
              ? 'Programación de actividades deportivas, profesores, entrenadores y control de asistencias.'
              : 'Inscribite en clases magistrales y sesiones de entrenamiento físico orientadas a tu nivel de juego.'}
          </p>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {/* Tabs Selector */}
          <div style={{
            display: 'flex',
            background: 'var(--bg-elevated)',
            padding: 4,
            borderRadius: 'var(--radius-md)',
            border: '1px solid var(--border)'
          }}>
            <button
              onClick={() => setActiveTab('clases')}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 8,
                padding: '8px 16px',
                borderRadius: 'var(--radius-sm)',
                background: activeTab === 'clases' ? 'var(--primary)' : 'transparent',
                color: activeTab === 'clases' ? '#fff' : 'var(--text-muted)',
                fontWeight: 600,
                fontSize: '0.875rem',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              <GraduationCap size={16} />
              Clases de Tenis ({clases.length})
            </button>
            <button
              onClick={() => setActiveTab('entrenamientos')}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 8,
                padding: '8px 16px',
                borderRadius: 'var(--radius-sm)',
                background: activeTab === 'entrenamientos' ? 'var(--primary)' : 'transparent',
                color: activeTab === 'entrenamientos' ? '#fff' : 'var(--text-muted)',
                fontWeight: 600,
                fontSize: '0.875rem',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              <Dumbbell size={16} />
              Entrenamientos ({entrenamientos.length})
            </button>
          </div>

          {isAdmin && (
            activeTab === 'clases' ? (
              <button onClick={() => setModalClaseOpen(true)} className="btn btn-volt">
                <Plus size={16} />
                Nueva Clase
              </button>
            ) : (
              <button onClick={() => setModalEntrenamientoOpen(true)} className="btn btn-volt">
                <Plus size={16} />
                Nuevo Entrenamiento
              </button>
            )
          )}
        </div>
      </div>

      {/* Alertas */}
      {error && (
        <div style={{
          background: 'rgba(239, 68, 68, 0.1)',
          border: '1px solid rgba(239, 68, 68, 0.3)',
          color: '#f87171',
          padding: '12px 16px',
          borderRadius: 'var(--radius-md)',
          display: 'flex',
          alignItems: 'center',
          gap: 10
        }}>
          <AlertTriangle size={18} />
          <span>{error}</span>
        </div>
      )}

      {successMsg && (
        <div style={{
          background: 'rgba(34, 197, 94, 0.1)',
          border: '1px solid rgba(34, 197, 94, 0.3)',
          color: 'var(--accent-volt)',
          padding: '12px 16px',
          borderRadius: 'var(--radius-md)',
          display: 'flex',
          alignItems: 'center',
          gap: 10
        }}>
          <CheckCircle2 size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {/* Contenido según pestaña */}
      {loading ? (
        <div style={{ padding: 60, textAlign: 'center', color: 'var(--text-dim)' }}>
          Cargando actividades deportivas...
        </div>
      ) : activeTab === 'clases' ? (
        clases.length === 0 ? (
          <div className="card" style={{ padding: 48, textAlign: 'center' }}>
            <GraduationCap size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
            <h3 style={{ marginBottom: 8 }}>No hay clases de tenis programadas</h3>
            <p style={{ color: 'var(--text-muted)' }}>
              {isAdmin ? 'Creá la primera clase del calendario deportivo.' : 'Próximamente se publicarán nuevas clases.'}
            </p>
          </div>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(360px, 1fr))', gap: 20 }}>
            {clases.map((clase) => {
              const cuposLibres = Math.max(0, clase.capacidadMaxma - clase.cantidadDeAlumnos);
              const fechaObj = new Date(clase.horaInicio);

              return (
                <div key={clase.id} className="card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 12 }}>
                      <span className="badge badge-success">
                        {tiposClaseNombres[clase.tipoDeClase] || 'Clase'}
                      </span>
                      <span style={{
                        fontSize: '0.75rem',
                        fontWeight: 700,
                        color: cuposLibres > 0 ? 'var(--accent-volt)' : '#f87171',
                        background: 'var(--bg-elevated)',
                        padding: '4px 8px',
                        borderRadius: 6
                      }}>
                        {cuposLibres > 0 ? `${cuposLibres} vacantes` : 'Completa'}
                      </span>
                    </div>

                    <h3 style={{ fontSize: '1.25rem', marginBottom: 8 }}>{clase.nombre}</h3>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: 8, fontSize: '0.875rem', color: 'var(--text-muted)', marginBottom: 20 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Calendar size={15} color="var(--text-dim)" />
                        <span>{fechaObj.toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'short' })}</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Clock size={15} color="var(--text-dim)" />
                        <span>{fechaObj.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })} ({clase.duracion} min)</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Users size={15} color="var(--text-dim)" />
                        <span>Nivel: <strong>{nivelesNombres[clase.nivelActividad] || 'Todos'}</strong> · Inscriptos: {clase.cantidadDeAlumnos} / {clase.capacidadMaxma}</span>
                      </div>
                      {clase.nombreProfesor && (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                          <Award size={15} color="var(--text-dim)" />
                          <span>Profesor: <strong style={{ color: 'var(--text-main)' }}>{clase.nombreProfesor}</strong></span>
                        </div>
                      )}
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: 8, borderTop: '1px solid var(--border)', paddingTop: 16 }}>
                    {isAdmin ? (
                      <>
                        <button
                          onClick={() => {
                            setActividadParaAsistencia({ id: clase.id, tipo: 'clase', nombre: clase.nombre });
                            setModalAsistenciaOpen(true);
                          }}
                          className="btn btn-secondary"
                          style={{ flex: 1 }}
                        >
                          <UserCheck size={16} />
                          Asistencia
                        </button>
                        <button
                          onClick={() => handleBajaClase(clase.id)}
                          className="btn btn-secondary"
                          style={{ color: '#f87171' }}
                          title="Dar de baja"
                        >
                          <Trash2 size={16} />
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={() => handleInscribirClase(clase.id)}
                        disabled={cuposLibres === 0}
                        className="btn btn-volt"
                        style={{ width: '100%' }}
                      >
                        <Sparkles size={16} />
                        {cuposLibres > 0 ? 'Inscribirme a la Clase' : 'Sin Vacantes'}
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )
      ) : (
        entrenamientos.length === 0 ? (
          <div className="card" style={{ padding: 48, textAlign: 'center' }}>
            <Dumbbell size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
            <h3 style={{ marginBottom: 8 }}>No hay entrenamientos programados</h3>
            <p style={{ color: 'var(--text-muted)' }}>
              {isAdmin ? 'Creá la primera sesión de entrenamiento del complejo.' : 'Próximamente se publicarán nuevos entrenamientos.'}
            </p>
          </div>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(360px, 1fr))', gap: 20 }}>
            {entrenamientos.map((ent) => {
              const cuposLibres = Math.max(0, ent.capacidadMaxma - ent.cantidadDeAlumnos);
              const fechaObj = new Date(ent.horaInicio);

              return (
                <div key={ent.id} className="card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 12 }}>
                      <span className="badge badge-volt">
                        {objetivosNombres[ent.objetivoEntrenamiento] || 'Entrenamiento'}
                      </span>
                      <span style={{
                        fontSize: '0.75rem',
                        fontWeight: 700,
                        color: cuposLibres > 0 ? 'var(--accent-volt)' : '#f87171',
                        background: 'var(--bg-elevated)',
                        padding: '4px 8px',
                        borderRadius: 6
                      }}>
                        {cuposLibres > 0 ? `${cuposLibres} vacantes` : 'Completo'}
                      </span>
                    </div>

                    <h3 style={{ fontSize: '1.25rem', marginBottom: 8 }}>{ent.nombre}</h3>

                    <div style={{ display: 'flex', flexDirection: 'column', gap: 8, fontSize: '0.875rem', color: 'var(--text-muted)', marginBottom: 20 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Calendar size={15} color="var(--text-dim)" />
                        <span>{fechaObj.toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'short' })}</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Clock size={15} color="var(--text-dim)" />
                        <span>{fechaObj.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })} ({ent.duracion} min)</span>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <Users size={15} color="var(--text-dim)" />
                        <span>Nivel: <strong>{nivelesNombres[ent.nivelActividad] || 'Todos'}</strong> · Inscriptos: {ent.cantidadDeAlumnos} / {ent.capacidadMaxma}</span>
                      </div>
                      {ent.nombreEntrenador && (
                        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                          <Award size={15} color="var(--text-dim)" />
                          <span>Entrenador: <strong style={{ color: 'var(--text-main)' }}>{ent.nombreEntrenador}</strong></span>
                        </div>
                      )}
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: 8, borderTop: '1px solid var(--border)', paddingTop: 16 }}>
                    {isAdmin ? (
                      <>
                        <button
                          onClick={() => {
                            setActividadParaAsistencia({ id: ent.id, tipo: 'entrenamiento', nombre: ent.nombre });
                            setModalAsistenciaOpen(true);
                          }}
                          className="btn btn-secondary"
                          style={{ flex: 1 }}
                        >
                          <UserCheck size={16} />
                          Asistencia
                        </button>
                        <button
                          onClick={() => handleBajaEntrenamiento(ent.id)}
                          className="btn btn-secondary"
                          style={{ color: '#f87171' }}
                          title="Dar de baja"
                        >
                          <Trash2 size={16} />
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={() => handleInscribirEntrenamiento(ent.id)}
                        disabled={cuposLibres === 0}
                        className="btn btn-volt"
                        style={{ width: '100%' }}
                      >
                        <Sparkles size={16} />
                        {cuposLibres > 0 ? 'Inscribirme al Entrenamiento' : 'Sin Vacantes'}
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )
      )}

      {/* Modal Crear Clase */}
      {modalClaseOpen && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: 20
        }}>
          <div className="card" style={{ maxWidth: 520, width: '100%', position: 'relative' }}>
            <button
              onClick={() => setModalClaseOpen(false)}
              style={{ position: 'absolute', top: 16, right: 16, background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ marginBottom: 16, display: 'flex', alignItems: 'center', gap: 10 }}>
              <GraduationCap size={22} color="var(--accent-volt)" />
              Nueva Clase de Tenis
            </h3>

            <form onSubmit={handleCrearClase} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <label className="form-label">Nombre de la Clase</label>
                <input
                  type="text"
                  required
                  placeholder="Ej: Clínica de Saque y Volea"
                  value={nombreClase}
                  onChange={(e) => setNombreClase(e.target.value)}
                  className="form-control"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Tipo de Clase</label>
                  <select value={tipoClase} onChange={(e) => setTipoClase(e.target.value)} className="form-control">
                    <option value={0}>Técnica</option>
                    <option value={1}>Táctica</option>
                    <option value={2}>Saque</option>
                    <option value={3}>Peloteo</option>
                  </select>
                </div>
                <div>
                  <label className="form-label">Nivel de Juego</label>
                  <select value={nivelClase} onChange={(e) => setNivelClase(e.target.value)} className="form-control">
                    <option value={0}>Principiante</option>
                    <option value={1}>Intermedio</option>
                    <option value={2}>Avanzado</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Fecha</label>
                  <input
                    type="date"
                    required
                    value={fechaClase}
                    onChange={(e) => setFechaClase(e.target.value)}
                    className="form-control"
                  />
                </div>
                <div>
                  <label className="form-label">Hora Inicio</label>
                  <input
                    type="time"
                    required
                    value={horaInicioClase}
                    onChange={(e) => setHoraInicioClase(e.target.value)}
                    className="form-control"
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Duración (minutos)</label>
                  <input
                    type="number"
                    min="30"
                    max="180"
                    step="15"
                    value={duracionMinutosClase}
                    onChange={(e) => setDuracionMinutosClase(e.target.value)}
                    className="form-control"
                  />
                </div>
                <div>
                  <label className="form-label">Cupo Máximo</label>
                  <input
                    type="number"
                    min="1"
                    max="20"
                    value={capacidadClase}
                    onChange={(e) => setCapacidadClase(e.target.value)}
                    className="form-control"
                  />
                </div>
              </div>

              <div>
                <label className="form-label">Profesor Asignado</label>
                <select value={profesorId} onChange={(e) => setProfesorId(e.target.value)} className="form-control">
                  <option value="">Seleccione un profesor...</option>
                  {profesores.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nombre} {p.apellido} {p.especialidad ? `(${p.especialidad})` : ''}
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalClaseOpen(false)} className="btn btn-secondary" style={{ flex: 1 }}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-volt" style={{ flex: 1 }}>
                  Guardar Clase
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Crear Entrenamiento */}
      {modalEntrenamientoOpen && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: 20
        }}>
          <div className="card" style={{ maxWidth: 520, width: '100%', position: 'relative' }}>
            <button
              onClick={() => setModalEntrenamientoOpen(false)}
              style={{ position: 'absolute', top: 16, right: 16, background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ marginBottom: 16, display: 'flex', alignItems: 'center', gap: 10 }}>
              <Dumbbell size={22} color="var(--accent-volt)" />
              Nuevo Entrenamiento Físico
            </h3>

            <form onSubmit={handleCrearEntrenamiento} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <label className="form-label">Nombre del Entrenamiento</label>
                <input
                  type="text"
                  required
                  placeholder="Ej: Acondicionamiento Físico Competitivo"
                  value={nombreEntrenamiento}
                  onChange={(e) => setNombreEntrenamiento(e.target.value)}
                  className="form-control"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Objetivo Principal</label>
                  <select value={objetivoEntrenamiento} onChange={(e) => setObjetivoEntrenamiento(e.target.value)} className="form-control">
                    <option value={0}>Técnica</option>
                    <option value={1}>Resistencia Aeróbica</option>
                    <option value={2}>Preparación Competitiva</option>
                  </select>
                </div>
                <div>
                  <label className="form-label">Nivel Exigido</label>
                  <select value={nivelEntrenamiento} onChange={(e) => setNivelEntrenamiento(e.target.value)} className="form-control">
                    <option value={0}>Principiante</option>
                    <option value={1}>Intermedio</option>
                    <option value={2}>Avanzado</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Fecha</label>
                  <input
                    type="date"
                    required
                    value={fechaEntrenamiento}
                    onChange={(e) => setFechaEntrenamiento(e.target.value)}
                    className="form-control"
                  />
                </div>
                <div>
                  <label className="form-label">Hora Inicio</label>
                  <input
                    type="time"
                    required
                    value={horaInicioEntrenamiento}
                    onChange={(e) => setHoraInicioEntrenamiento(e.target.value)}
                    className="form-control"
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Duración (minutos)</label>
                  <input
                    type="number"
                    min="30"
                    max="180"
                    step="15"
                    value={duracionMinutosEntrenamiento}
                    onChange={(e) => setDuracionMinutosEntrenamiento(e.target.value)}
                    className="form-control"
                  />
                </div>
                <div>
                  <label className="form-label">Cupo Máximo</label>
                  <input
                    type="number"
                    min="1"
                    max="30"
                    value={capacidadEntrenamiento}
                    onChange={(e) => setCapacidadEntrenamiento(e.target.value)}
                    className="form-control"
                  />
                </div>
              </div>

              <div>
                <label className="form-label">Entrenador Responsable</label>
                <select value={entrenadorId} onChange={(e) => setEntrenadorId(e.target.value)} className="form-control">
                  <option value="">Seleccione un entrenador...</option>
                  {entrenadores.map((ent) => (
                    <option key={ent.id} value={ent.id}>
                      {ent.nombre} {ent.apellido}
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalEntrenamientoOpen(false)} className="btn btn-secondary" style={{ flex: 1 }}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-volt" style={{ flex: 1 }}>
                  Guardar Entrenamiento
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Tomar Asistencia */}
      {modalAsistenciaOpen && actividadParaAsistencia && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: 20
        }}>
          <div className="card" style={{ maxWidth: 460, width: '100%', position: 'relative' }}>
            <button
              onClick={() => setModalAsistenciaOpen(false)}
              style={{ position: 'absolute', top: 16, right: 16, background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ marginBottom: 8, display: 'flex', alignItems: 'center', gap: 10 }}>
              <UserCheck size={22} color="var(--primary)" />
              Registrar Asistencia
            </h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', marginBottom: 16 }}>
              Actividad: <strong style={{ color: 'var(--text-main)' }}>{actividadParaAsistencia.nombre}</strong>
            </p>

            <form onSubmit={handleRegistrarAsistencia} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <label className="form-label">DNI del Alumno / Socio</label>
                <input
                  type="text" inputMode="numeric" maxLength={8} required placeholder="Ej. 12345678" value={asistenciaDni} onChange={(e) => setAsistenciaDni(e.target.value.replace(/\D/g, '').slice(0, 8))}
                  className="form-control"
                />
              </div>

              <div style={{
                display: 'flex',
                alignItems: 'center',
                gap: 12,
                padding: '12px 14px',
                background: 'var(--bg-elevated)',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border)'
              }}>
                <input
                  type="checkbox"
                  id="chkPresente"
                  checked={asistenciaPresente}
                  onChange={(e) => setAsistenciaPresente(e.target.checked)}
                  style={{ width: 18, height: 18, accentColor: 'var(--primary)', cursor: 'pointer' }}
                />
                <label htmlFor="chkPresente" style={{ cursor: 'pointer', fontSize: '0.925rem', fontWeight: 600 }}>
                  Marcar como Presente
                </label>
              </div>

              <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalAsistenciaOpen(false)} className="btn btn-secondary" style={{ flex: 1 }}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-volt" style={{ flex: 1 }}>
                  Confirmar Asistencia
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

