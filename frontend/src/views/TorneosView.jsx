import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { Trophy, Plus, Calendar, Users, Swords, CheckCircle, Edit, Trash2, X, Play } from 'lucide-react';

export default function TorneosView() {
  const [torneos, setTorneos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [torneoSeleccionado, setTorneoSeleccionado] = useState(null);

  // Modales
  const [modalCrearOpen, setModalCrearOpen] = useState(false);
  const [modalInscribirOpen, setModalInscribirOpen] = useState(false);
  const [modalResultadoOpen, setModalResultadoOpen] = useState(false);

  // Forms
  const [nuevoTorneo, setNuevoTorneo] = useState({
    nombre: '',
    fechaInicio: new Date().toISOString().slice(0, 10),
    fechaFin: new Date(Date.now() + 15 * 86400000).toISOString().slice(0, 10),
    categoriaGenero: 1, // 1 = Masculino, 0 = Femenino
    modalidad: 0, // 0 = Single, 1 = Dobles
    tipoLlave: 'Eliminatoria Simple',
  });

  const [inscribirDni, setInscribirDni] = useState('');
  const [partidoParaResultado, setPartidoParaResultado] = useState(null);
  const [resultadoForm, setResultadoForm] = useState({ resultado: '', ganadorNombre: '' });

  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    cargarTorneos();
  }, []);

  async function cargarTorneos() {
    setLoading(true);
    try {
      const data = await api.torneos.listar(false);
      setTorneos(data);
      if (data.length > 0 && !torneoSeleccionado) {
        setTorneoSeleccionado(data[0]);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  async function seleccionarTorneo(id) {
    try {
      const data = await api.torneos.obtenerPorId(id);
      setTorneoSeleccionado(data);
    } catch (err) {
      alert(`Error al cargar torneo: ${err.message}`);
    }
  }

  // RF-28 Crear Torneo
  const handleCrearTorneo = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      const creado = await api.torneos.crear({
        ...nuevoTorneo,
        categoriaGenero: parseInt(nuevoTorneo.categoriaGenero, 10),
        modalidad: parseInt(nuevoTorneo.modalidad, 10),
        fechaInicio: new Date(nuevoTorneo.fechaInicio).toISOString(),
        fechaFin: new Date(nuevoTorneo.fechaFin).toISOString(),
      });
      setFormSuccess('¡Torneo oficial creado con éxito!');
      setTimeout(() => {
        setModalCrearOpen(false);
        setFormSuccess('');
      }, 1200);
      await cargarTorneos();
      setTorneoSeleccionado(creado);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-31 Eliminar Torneo
  const handleEliminarTorneo = async (id, nombre) => {
    if (!window.confirm(`¿Seguro que deseás eliminar el torneo ${nombre}?`)) return;
    try {
      await api.torneos.eliminar(id);
      setTorneoSeleccionado(null);
      await cargarTorneos();
    } catch (err) {
      alert(`Error al eliminar torneo: ${err.message}`);
    }
  };

  // RF-32 Inscribir Socio
  const handleInscribirSocio = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      await api.torneos.inscribir(torneoSeleccionado.id, parseInt(inscribirDni, 10));
      setFormSuccess('¡Socio inscripto con éxito al torneo!');
      setInscribirDni('');
      setTimeout(() => {
        setModalInscribirOpen(false);
        setFormSuccess('');
      }, 1200);
      seleccionarTorneo(torneoSeleccionado.id);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-33 Generar Fixture / Llaves
  const handleGenerarFixture = async () => {
    if (!window.confirm('¿Deseás generar automáticamente las llaves eliminatorias con los inscriptos actuales?')) return;
    try {
      await api.torneos.generarFixture(torneoSeleccionado.id);
      seleccionarTorneo(torneoSeleccionado.id);
    } catch (err) {
      alert(`Error al generar cruces: ${err.message}`);
    }
  };

  // RF-34 Registrar Resultado
  const handleRegistrarResultado = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      await api.torneos.registrarResultado(partidoParaResultado.id, resultadoForm);
      setFormSuccess('Resultado asentado correctamente.');
      setTimeout(() => {
        setModalResultadoOpen(false);
        setFormSuccess('');
      }, 1200);
      seleccionarTorneo(torneoSeleccionado.id);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', display: 'flex', alignItems: 'center', gap: 10 }}>
            <Trophy color="var(--accent-volt)" size={26} />
            Torneos Oficiales del Club
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            Reglas de la Asociación de Tenis · Cuadros eliminatorios masculinos y femeninos
          </p>
        </div>

        <button onClick={() => setModalCrearOpen(true)} className="btn btn-volt">
          <Plus size={18} />
          <span>Crear Torneo</span>
        </button>
      </div>

      {/* Main split view: Lista de Torneos & Detalle interactivo */}
      <div style={{ display: 'grid', gridTemplateColumns: '320px 1fr', gap: 24, alignItems: 'start' }}>
        {/* Lista Lateral de Torneos */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {loading ? (
            <div style={{ padding: 20, textAlign: 'center', color: 'var(--text-muted)' }}>Cargando torneos...</div>
          ) : torneos.length === 0 ? (
            <div style={{ padding: 24, background: 'var(--bg-card)', borderRadius: 'var(--radius-md)', textAlign: 'center', color: 'var(--text-muted)' }}>
              No hay torneos registrados.
            </div>
          ) : (
            torneos.map((t) => {
              const selected = torneoSeleccionado?.id === t.id;
              return (
                <div
                  key={t.id}
                  onClick={() => seleccionarTorneo(t.id)}
                  style={{
                    background: selected ? 'var(--bg-card-hover)' : 'var(--bg-card)',
                    border: selected ? '2px solid var(--accent-volt)' : '1px solid var(--border)',
                    borderRadius: 'var(--radius-md)',
                    padding: '16px',
                    cursor: 'pointer',
                    boxShadow: selected ? '0 4px 20px var(--accent-volt-glow)' : 'none',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                    <h4 style={{ color: selected ? 'var(--accent-volt)' : 'var(--text-main)', fontSize: '1.05rem' }}>{t.nombre}</h4>
                    <span className="badge badge-warning" style={{ fontSize: '0.65rem' }}>{t.etapaActual || 'Inscripción'}</span>
                  </div>

                  <div style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                    <span className="badge badge-info">{t.modalidad}</span>
                    <span className="badge badge-success">{t.categoriaGenero}</span>
                  </div>

                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                    <span>Inscriptos: <strong>{t.cantidadInscriptos || 0}</strong></span>
                    <span>Partidos: <strong>{t.partidos?.length || 0}</strong></span>
                  </div>
                </div>
              );
            })
          )}
        </div>

        {/* Panel Detalle del Torneo Seleccionado */}
        {torneoSeleccionado ? (
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border)',
            borderRadius: 'var(--radius-lg)',
            padding: 28,
            boxShadow: 'var(--shadow-sm)',
          }}>
            {/* Cabecera del Torneo */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', borderBottom: '1px solid var(--border)', paddingBottom: 20, marginBottom: 24 }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 6 }}>
                  <h3 style={{ fontSize: '1.6rem', color: 'var(--text-main)' }}>{torneoSeleccionado.nombre}</h3>
                  <span className="badge badge-success">{torneoSeleccionado.etapaActual}</span>
                </div>
                <div style={{ display: 'flex', gap: 16, fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                  <span>Inicio: <strong>{new Date(torneoSeleccionado.fechaInicio).toLocaleDateString()}</strong></span>
                  <span>Fin: <strong>{new Date(torneoSeleccionado.fechaFin).toLocaleDateString()}</strong></span>
                  <span>Modalidad: <strong>{torneoSeleccionado.modalidad} ({torneoSeleccionado.categoriaGenero})</strong></span>
                  <span>Formato: <strong>{torneoSeleccionado.tipoLlave}</strong></span>
                </div>
              </div>

              <div style={{ display: 'flex', gap: 10 }}>
                <button
                  onClick={() => setModalInscribirOpen(true)}
                  className="btn btn-volt btn-sm"
                  title="Inscribir Socio por DNI"
                >
                  <Users size={16} />
                  Inscribir Socio
                </button>

                {torneoSeleccionado.partidos?.length === 0 && (
                  <button
                    onClick={handleGenerarFixture}
                    className="btn btn-primary btn-sm"
                    title="Generar Cuadro de Cruces Automático"
                  >
                    <Play size={16} />
                    Generar Fixture
                  </button>
                )}

                <button
                  onClick={() => handleEliminarTorneo(torneoSeleccionado.id, torneoSeleccionado.nombre)}
                  className="btn btn-danger btn-sm"
                  title="Eliminar Torneo"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>

            {/* SECCIÓN 1: INFORMATIVO DE PARTICIPANTES INSCRIPTOS */}
            <div style={{ marginBottom: 30 }}>
              <h4 style={{ fontSize: '1.1rem', marginBottom: 14, display: 'flex', alignItems: 'center', gap: 8 }}>
                <Users size={18} color="var(--primary)" />
                Jugadores Inscriptos ({torneoSeleccionado.inscriptos?.length || 0})
              </h4>

              {torneoSeleccionado.inscriptos?.length === 0 ? (
                <div style={{ background: 'var(--bg-elevated)', padding: 16, borderRadius: 10, color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                  Aún no hay socios inscriptos. Usá el botón "Inscribir Socio" para sumar participantes por su DNI.
                </div>
              ) : (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 10 }}>
                  {torneoSeleccionado.inscriptos.map((ins) => (
                    <div
                      key={ins.inscripcionId}
                      style={{
                        background: 'var(--bg-elevated)',
                        padding: '10px 14px',
                        borderRadius: 8,
                        border: '1px solid var(--border)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                      }}
                    >
                      <div>
                        <strong style={{ fontSize: '0.9rem', display: 'block' }}>{ins.nombreCompleto}</strong>
                        <small style={{ color: 'var(--text-dim)' }}>DNI: {ins.dni}</small>
                      </div>
                      <span className="badge badge-info" style={{ fontSize: '0.7rem' }}>
                        #{ins.numeroParticipante}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* SECCIÓN 2: CUADRO DE CRUCES / PARTIDOS  */}
            <div>
              <h4 style={{ fontSize: '1.1rem', marginBottom: 14, display: 'flex', alignItems: 'center', gap: 8 }}>
                <Swords size={18} color="var(--accent-volt)" />
                Cuadro de Cruces Eliminatorios (Fixture de Tenis)
              </h4>

              {torneoSeleccionado.partidos?.length === 0 ? (
                <div style={{ background: 'var(--bg-elevated)', padding: 24, borderRadius: 10, textAlign: 'center', color: 'var(--text-muted)' }}>
                  <p style={{ marginBottom: 12 }}>El fixture aún no ha sido generado.</p>
                  <button onClick={handleGenerarFixture} className="btn btn-primary btn-sm">
                    Generar Cuadro de Cruces Oficial
                  </button>
                </div>
              ) : (
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: 14 }}>
                  {torneoSeleccionado.partidos.map((partido) => (
                    <div
                      key={partido.id}
                      style={{
                        background: 'var(--bg-elevated)',
                        border: partido.resultado && partido.resultado !== 'Pendiente' ? '1px solid var(--primary)' : '1px solid var(--border)',
                        borderRadius: 12,
                        padding: 16,
                        boxShadow: 'var(--shadow-sm)',
                      }}
                    >
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
                        <span style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--accent-volt)', textTransform: 'uppercase' }}>
                          {partido.ronda}
                        </span>
                        <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                          {new Date(partido.fecha).toLocaleDateString()}
                        </span>
                      </div>

                      <div style={{
                        background: 'var(--bg-dark)',
                        padding: '10px 14px',
                        borderRadius: 8,
                        marginBottom: 12,
                        textAlign: 'center',
                        fontSize: '0.95rem',
                        fontWeight: 700,
                        color: partido.resultado && partido.resultado !== 'Pendiente' ? 'var(--accent-volt)' : 'var(--text-muted)',
                      }}>
                        {partido.resultado || 'Pendiente de disputa'}
                      </div>

                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <small style={{ color: 'var(--text-dim)' }}>Cancha #{partido.canchaId}</small>
                        <button
                          onClick={() => {
                            setPartidoParaResultado(partido);
                            setResultadoForm({ resultado: partido.resultado === 'Pendiente' ? '' : (partido.resultado || ''), ganadorNombre: '' });
                            setModalResultadoOpen(true);
                            setFormError('');
                          }}
                          className="btn btn-secondary btn-sm"
                          style={{ fontSize: '0.75rem', padding: '4px 10px' }}
                        >
                          Cargar Resultado
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        ) : (
          <div style={{ background: 'var(--bg-card)', padding: 40, borderRadius: 'var(--radius-lg)', textAlign: 'center', color: 'var(--text-muted)' }}>
            Seleccioná un torneo para ver su cuadro de llaves y participantes.
          </div>
        )}
      </div>

      {/* MODAL: CREAR TORNEO */}
      {modalCrearOpen && (
        <div className="modal-overlay" onClick={() => setModalCrearOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Registrar Nuevo Torneo</h3>
              <button onClick={() => setModalCrearOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && (
              <div style={{ background: 'rgba(244, 63, 94, 0.15)', border: '1px solid rgba(244, 63, 94, 0.4)', color: '#f87171', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formError}
              </div>
            )}
            {formSuccess && (
              <div style={{ background: 'rgba(34, 197, 94, 0.15)', border: '1px solid rgba(34, 197, 94, 0.4)', color: '#4ade80', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formSuccess}
              </div>
            )}

            <form onSubmit={handleCrearTorneo}>
              <div className="input-group">
                <label className="input-label">Nombre del Torneo</label>
                <input
                  type="text"
                  required
                  className="input-field"
                  placeholder="Ej: Copa Primavera 2026"
                  value={nuevoTorneo.nombre}
                  onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, nombre: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Fecha de Inicio</label>
                  <input
                    type="date"
                    required
                    className="input-field"
                    value={nuevoTorneo.fechaInicio}
                    onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, fechaInicio: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Fecha de Finalización</label>
                  <input
                    type="date"
                    required
                    className="input-field"
                    value={nuevoTorneo.fechaFin}
                    onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, fechaFin: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Categoría / Género</label>
                  <select
                    className="input-field"
                    value={nuevoTorneo.categoriaGenero}
                    onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, categoriaGenero: e.target.value })}
                  >
                    <option value={1}>Masculino</option>
                    <option value={0}>Femenino</option>
                  </select>
                </div>
                <div className="input-group">
                  <label className="input-label">Modalidad</label>
                  <select
                    className="input-field"
                    value={nuevoTorneo.modalidad}
                    onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, modalidad: e.target.value })}
                  >
                    <option value={0}>Single (Individual)</option>
                    <option value={1}>Dobles (En Pareja)</option>
                  </select>
                </div>
              </div>

              <div className="input-group">
                <label className="input-label">Tipo de Llave / Cuadro</label>
                <select
                  className="input-field"
                  value={nuevoTorneo.tipoLlave}
                  onChange={(e) => setNuevoTorneo({ ...nuevoTorneo, tipoLlave: e.target.value })}
                >
                  <option value="Eliminación Simple">Eliminación Simple</option>
                  <option value="Doble Eliminación">Doble Eliminación con Repechaje</option>
                </select>
              </div>

              <button type="submit" disabled={submitting} className="btn btn-volt" style={{ width: '100%', marginTop: 8 }}>
                {submitting ? 'Creando...' : 'Crear Torneo Oficial'}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: INSCRIBIR SOCIO */}
      {modalInscribirOpen && torneoSeleccionado && (
        <div className="modal-overlay" onClick={() => setModalInscribirOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 440 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Inscribir Socio</h3>
              <button onClick={() => setModalInscribirOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: 16 }}>
              Inscripción al torneo <strong>{torneoSeleccionado.nombre}</strong>.
            </p>

            {formError && (
              <div style={{ background: 'rgba(244, 63, 94, 0.15)', border: '1px solid rgba(244, 63, 94, 0.4)', color: '#f87171', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formError}
              </div>
            )}
            {formSuccess && (
              <div style={{ background: 'rgba(34, 197, 94, 0.15)', border: '1px solid rgba(34, 197, 94, 0.4)', color: '#4ade80', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formSuccess}
              </div>
            )}

            <form onSubmit={handleInscribirSocio}>
              <div className="input-group">
                <label className="input-label">DNI del Socio a Inscribir</label>
                <input
                  type="number"
                  required
                  placeholder="Ej: 40123456"
                  className="input-field"
                  value={inscribirDni}
                  onChange={(e) => setInscribirDni(e.target.value)}
                />
              </div>

              <button type="submit" disabled={submitting} className="btn btn-primary" style={{ width: '100%', marginTop: 8 }}>
                {submitting ? 'Inscribiendo...' : 'Confirmar Inscripción'}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: CARGAR RESULTADO OFICIAL */}
      {modalResultadoOpen && partidoParaResultado && (
        <div className="modal-overlay" onClick={() => setModalResultadoOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 460 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Registrar Resultado Oficial</h3>
              <button onClick={() => setModalResultadoOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', marginBottom: 14 }}>
              Partido: <strong>{partidoParaResultado.ronda}</strong>
            </p>

            {formError && (
              <div style={{ background: 'rgba(244, 63, 94, 0.15)', border: '1px solid rgba(244, 63, 94, 0.4)', color: '#f87171', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formError}
              </div>
            )}
            {formSuccess && (
              <div style={{ background: 'rgba(34, 197, 94, 0.15)', border: '1px solid rgba(34, 197, 94, 0.4)', color: '#4ade80', padding: '10px 14px', borderRadius: 8, marginBottom: 16 }}>
                {formSuccess}
              </div>
            )}

            <form onSubmit={handleRegistrarResultado}>
              <div className="input-group">
                <label className="input-label">Tanteador Oficial (Sets y Games)</label>
                <input
                  type="text"
                  required
                  placeholder="Ej: 6-4, 7-5 ó 6-2, 4-6, 7-6"
                  className="input-field"
                  value={resultadoForm.resultado}
                  onChange={(e) => setResultadoForm({ ...resultadoForm, resultado: e.target.value })}
                />
              </div>

              <div className="input-group">
                <label className="input-label">Nombre del Jugador / Pareja Ganadora</label>
                <input
                  type="text"
                  placeholder="Ej: Carlos Alcaraz"
                  className="input-field"
                  value={resultadoForm.ganadorNombre}
                  onChange={(e) => setResultadoForm({ ...resultadoForm, ganadorNombre: e.target.value })}
                />
              </div>

              <button type="submit" disabled={submitting} className="btn btn-primary" style={{ width: '100%', marginTop: 8 }}>
                {submitting ? 'Asentando...' : 'Asentar Resultado Oficial'}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
