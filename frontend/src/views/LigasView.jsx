import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import {
  Award,
  Plus,
  Calendar,
  Users,
  Swords,
  CheckCircle,
  Edit,
  Trash2,
  X,
  Play,
  RotateCcw,
  Trophy,
  Table,
  CheckCircle2,
  Search,
  Hash,
  Star
} from 'lucide-react';

export default function LigasView() {
  const [ligas, setLigas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [ligaSeleccionada, setLigaSeleccionada] = useState(null);
  const [activeTab, setActiveTab] = useState('posiciones'); // 'posiciones' | 'partidos' | 'participantes'
  const [posiciones, setPosiciones] = useState([]);
  const [loadingPosiciones, setLoadingPosiciones] = useState(false);

  // Modales
  const [modalCrearOpen, setModalCrearOpen] = useState(false);
  const [modalModificarOpen, setModalModificarOpen] = useState(false);
  const [modalInscribirOpen, setModalInscribirOpen] = useState(false);
  const [modalResultadoOpen, setModalResultadoOpen] = useState(false);

  // Forms
  const [nuevaLiga, setNuevaLiga] = useState({
    nombre: '',
    fechaInicio: new Date().toISOString().slice(0, 10),
    fechaFin: new Date(Date.now() + 60 * 86400000).toISOString().slice(0, 10),
    categoriaGenero: 1, // 1 = Masculino, 0 = Femenino
    modalidad: 0, // 0 = Single, 1 = Dobles
    cantidadFechas: 10,
    puntosPorPartido: 3,
    reglamentoId: 1,
  });

  const [ligaEdicion, setLigaEdicion] = useState({
    nombre: '',
    fechaInicio: '',
    fechaFin: '',
    categoriaGenero: 1,
    modalidad: 0,
    cantidadFechas: 10,
    puntosPorPartido: 3,
  });

  const [inscribirDni, setInscribirDni] = useState('');
  const [partidoParaResultado, setPartidoParaResultado] = useState(null);
  const [resultadoForm, setResultadoForm] = useState({ resultado: '', ganadorNombre: '' });

  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    cargarLigas();
  }, []);

  useEffect(() => {
    if (ligaSeleccionada?.id) {
      cargarPosiciones(ligaSeleccionada.id);
    }
  }, [ligaSeleccionada?.id]);

  async function cargarLigas() {
    setLoading(true);
    try {
      const data = await api.ligas.listar(false);
      setLigas(data);
      if (data.length > 0 && !ligaSeleccionada) {
        seleccionarLiga(data[0].id);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  async function seleccionarLiga(id) {
    try {
      const data = await api.ligas.obtenerPorId(id);
      setLigaSeleccionada(data);
    } catch (err) {
      alert(`Error al cargar datos de la liga: ${err.message}`);
    }
  }

  async function cargarPosiciones(id) {
    setLoadingPosiciones(true);
    try {
      const data = await api.ligas.obtenerTablaPosiciones(id);
      setPosiciones(data || []);
    } catch (err) {
      console.error('Error al cargar posiciones:', err);
      setPosiciones([]);
    } finally {
      setLoadingPosiciones(false);
    }
  }

  // RF-35 Crear Liga
  const handleCrearLiga = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      const creada = await api.ligas.crear({
        ...nuevaLiga,
        categoriaGenero: parseInt(nuevaLiga.categoriaGenero, 10),
        modalidad: parseInt(nuevaLiga.modalidad, 10),
        cantidadFechas: parseInt(nuevaLiga.cantidadFechas, 10),
        puntosPorPartido: parseInt(nuevaLiga.puntosPorPartido, 10),
        fechaInicio: new Date(nuevaLiga.fechaInicio).toISOString(),
        fechaFin: new Date(nuevaLiga.fechaFin).toISOString(),
      });
      setFormSuccess('¡Liga regular creada exitosamente!');
      setTimeout(() => {
        setModalCrearOpen(false);
        setFormSuccess('');
      }, 1200);
      await cargarLigas();
      setLigaSeleccionada(creada);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-37 Modificar Liga
  const abrirModalModificar = () => {
    if (!ligaSeleccionada) return;
    setLigaEdicion({
      nombre: ligaSeleccionada.nombre,
      fechaInicio: ligaSeleccionada.fechaInicio?.slice(0, 10) || '',
      fechaFin: ligaSeleccionada.fechaFin?.slice(0, 10) || '',
      categoriaGenero: ligaSeleccionada.categoriaGenero === 'Femenino' ? 0 : 1,
      modalidad: ligaSeleccionada.modalidad === 'Dobles' ? 1 : 0,
      cantidadFechas: ligaSeleccionada.cantidadFechas || 10,
      puntosPorPartido: ligaSeleccionada.puntosPorPartido || 3,
    });
    setFormError('');
    setFormSuccess('');
    setModalModificarOpen(true);
  };

  const handleModificarLiga = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      const modificada = await api.ligas.modificar(ligaSeleccionada.id, {
        ...ligaEdicion,
        categoriaGenero: parseInt(ligaEdicion.categoriaGenero, 10),
        modalidad: parseInt(ligaEdicion.modalidad, 10),
        cantidadFechas: parseInt(ligaEdicion.cantidadFechas, 10),
        puntosPorPartido: parseInt(ligaEdicion.puntosPorPartido, 10),
        fechaInicio: new Date(ligaEdicion.fechaInicio).toISOString(),
        fechaFin: new Date(ligaEdicion.fechaFin).toISOString(),
      });
      setFormSuccess('¡Liga actualizada con éxito!');
      setTimeout(() => {
        setModalModificarOpen(false);
        setFormSuccess('');
      }, 1200);
      await cargarLigas();
      setLigaSeleccionada(modificada);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-38 Eliminar Liga
  const handleEliminarLiga = async (id, nombre) => {
    if (!window.confirm(`¿Seguro que deseás eliminar la liga "${nombre}"? Esta acción no se puede deshacer.`)) return;
    try {
      await api.ligas.eliminar(id);
      setLigaSeleccionada(null);
      await cargarLigas();
    } catch (err) {
      alert(`Error al eliminar liga: ${err.message}`);
    }
  };

  // RF-39 Inscribir Socio
  const handleInscribirSocio = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      await api.ligas.inscribir(ligaSeleccionada.id, parseInt(inscribirDni, 10));
      setFormSuccess('¡Socio inscripto con éxito a la liga!');
      setInscribirDni('');
      setTimeout(() => {
        setModalInscribirOpen(false);
        setFormSuccess('');
      }, 1200);
      await seleccionarLiga(ligaSeleccionada.id);
      await cargarPosiciones(ligaSeleccionada.id);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-40 Generar Fixture Round-Robin
  const handleGenerarFixture = async () => {
    if (!window.confirm('¿Deseás generar el fixture Round-Robin (todos contra todos) con los inscriptos actuales?')) return;
    try {
      await api.ligas.generarFixture(ligaSeleccionada.id);
      await seleccionarLiga(ligaSeleccionada.id);
      await cargarPosiciones(ligaSeleccionada.id);
    } catch (err) {
      alert(`Error al generar fixture: ${err.message}`);
    }
  };

  // RF-41 Registrar Resultado
  const abrirModalResultado = (partido) => {
    setPartidoParaResultado(partido);
    setResultadoForm({
      resultado: partido.resultado || '',
      ganadorNombre: '',
    });
    setFormError('');
    setFormSuccess('');
    setModalResultadoOpen(true);
  };

  const handleRegistrarResultado = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      await api.ligas.registrarResultado(partidoParaResultado.id, resultadoForm);
      setFormSuccess('¡Resultado guardado y tabla de posiciones actualizada!');
      setTimeout(() => {
        setModalResultadoOpen(false);
        setFormSuccess('');
      }, 1200);
      await seleccionarLiga(ligaSeleccionada.id);
      await cargarPosiciones(ligaSeleccionada.id);
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
            <Award color="var(--accent-volt)" size={26} />
            Ligas Regulares del Club
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            Formato Round-Robin (Todos contra Todos) · Acumulación de puntos por victoria
          </p>
        </div>

        <button onClick={() => setModalCrearOpen(true)} className="btn btn-volt">
          <Plus size={18} />
          <span>Crear Liga</span>
        </button>
      </div>

      {/* Main split view: Lista de Ligas & Detalle interactivo */}
      <div style={{ display: 'grid', gridTemplateColumns: '320px 1fr', gap: 24, alignItems: 'start' }}>
        {/* Lista Lateral de Ligas */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          {loading ? (
            <div style={{ padding: 20, textAlign: 'center', color: 'var(--text-muted)' }}>Cargando ligas...</div>
          ) : ligas.length === 0 ? (
            <div style={{ padding: 24, background: 'var(--bg-card)', borderRadius: 'var(--radius-md)', textAlign: 'center', color: 'var(--text-muted)' }}>
              No hay ligas registradas en el sistema.
            </div>
          ) : (
            ligas.map((l) => {
              const selected = ligaSeleccionada?.id === l.id;
              return (
                <div
                  key={l.id}
                  onClick={() => seleccionarLiga(l.id)}
                  style={{
                    background: selected ? 'var(--bg-card-hover)' : 'var(--bg-card)',
                    border: selected ? '2px solid var(--accent-volt)' : '1px solid var(--border)',
                    borderRadius: 'var(--radius-md)',
                    padding: '16px',
                    cursor: 'pointer',
                    boxShadow: selected ? '0 4px 20px var(--accent-volt-glow)' : 'none',
                    transition: 'all 0.2s ease',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                    <h4 style={{ color: selected ? 'var(--accent-volt)' : 'var(--text-main)', fontSize: '1.05rem' }}>{l.nombre}</h4>
                    <span className="badge badge-success" style={{ fontSize: '0.65rem' }}>{l.estado || 'Activa'}</span>
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <Calendar size={13} />
                      <span>{new Date(l.fechaInicio).toLocaleDateString()} - {new Date(l.fechaFin).toLocaleDateString()}</span>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <Users size={13} />
                      <span>{l.cantidadInscriptos || 0} inscriptos · {l.puntosPorPartido || 3} pts/victoria</span>
                    </div>
                  </div>
                </div>
              );
            })
          )}
        </div>

        {/* Detalle de Liga Seleccionada */}
        {ligaSeleccionada ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
            {/* Header de la Liga */}
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border)',
              borderRadius: 'var(--radius-lg)',
              padding: '24px',
              display: 'flex',
              flexDirection: 'column',
              gap: 16
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 6 }}>
                    <span className="badge badge-volt">Liga Oficial</span>
                    <span className="badge badge-secondary">{ligaSeleccionada.categoriaGenero} · {ligaSeleccionada.modalidad}</span>
                    <span className="badge badge-success">{ligaSeleccionada.puntosPorPartido} Puntos por Triunfo</span>
                  </div>
                  <h3 style={{ fontSize: '1.6rem', color: 'var(--text-main)' }}>{ligaSeleccionada.nombre}</h3>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 14, color: 'var(--text-muted)', fontSize: '0.85rem', marginTop: 4 }}>
                    <span>Fechas estipuladas: <strong>{ligaSeleccionada.cantidadFechas}</strong></span>
                    <span>·</span>
                    <span>Inscriptos actuales: <strong style={{ color: 'var(--accent-volt)' }}>{ligaSeleccionada.cantidadInscriptos}</strong></span>
                    <span>·</span>
                    <span>Partidos programados: <strong>{ligaSeleccionada.partidos?.length || 0}</strong></span>
                  </div>
                </div>

                {/* Acciones principales */}
                <div style={{ display: 'flex', gap: 8 }}>
                  <button
                    onClick={() => setModalInscribirOpen(true)}
                    className="btn btn-volt btn-sm"
                  >
                    <Users size={16} />
                    <span>Inscribir Socio</span>
                  </button>

                  <button
                    onClick={handleGenerarFixture}
                    className="btn btn-primary btn-sm"
                    title="Generar Calendario Round-Robin"
                  >
                    <Play size={16} />
                    <span>Generar Fixture</span>
                  </button>

                  <button
                    onClick={abrirModalModificar}
                    className="btn btn-secondary btn-sm"
                    title="Modificar parámetros"
                  >
                    <Edit size={16} />
                  </button>

                  <button
                    onClick={() => handleEliminarLiga(ligaSeleccionada.id, ligaSeleccionada.nombre)}
                    className="btn btn-danger btn-sm"
                    title="Eliminar liga"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>

              {/* Sub-tabs: Posiciones, Fixture, Participantes */}
              <div style={{ display: 'flex', gap: 10, borderBottom: '1px solid var(--border)', paddingTop: 10 }}>
                <button
                  onClick={() => setActiveTab('posiciones')}
                  style={{
                    padding: '8px 16px',
                    background: 'transparent',
                    border: 'none',
                    borderBottom: activeTab === 'posiciones' ? '2px solid var(--accent-volt)' : '2px solid transparent',
                    color: activeTab === 'posiciones' ? 'var(--accent-volt)' : 'var(--text-muted)',
                    fontWeight: activeTab === 'posiciones' ? 700 : 500,
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                  }}
                >
                  <Table size={16} />
                  <span>Tabla de Posiciones ({posiciones.length})</span>
                </button>

                <button
                  onClick={() => setActiveTab('partidos')}
                  style={{
                    padding: '8px 16px',
                    background: 'transparent',
                    border: 'none',
                    borderBottom: activeTab === 'partidos' ? '2px solid var(--accent-volt)' : '2px solid transparent',
                    color: activeTab === 'partidos' ? 'var(--accent-volt)' : 'var(--text-muted)',
                    fontWeight: activeTab === 'partidos' ? 700 : 500,
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                  }}
                >
                  <Swords size={16} />
                  <span>Fixture de Partidos ({ligaSeleccionada.partidos?.length || 0})</span>
                </button>

                <button
                  onClick={() => setActiveTab('participantes')}
                  style={{
                    padding: '8px 16px',
                    background: 'transparent',
                    border: 'none',
                    borderBottom: activeTab === 'participantes' ? '2px solid var(--accent-volt)' : '2px solid transparent',
                    color: activeTab === 'participantes' ? 'var(--accent-volt)' : 'var(--text-muted)',
                    fontWeight: activeTab === 'participantes' ? 700 : 500,
                    cursor: 'pointer',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                  }}
                >
                  <Users size={16} />
                  <span>Inscriptos ({ligaSeleccionada.inscriptos?.length || 0})</span>
                </button>
              </div>
            </div>

            {/* CONTENIDO DE TABS */}

            {/* TAB 1: TABLA DE POSICIONES (RF-36 & RF-41) */}
            {activeTab === 'posiciones' && (
              <div style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-lg)',
                padding: '24px',
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                  <div>
                    <h4 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                      <Trophy size={18} color="var(--accent-volt)" />
                      Clasificación General y Puntos
                    </h4>
                    <p style={{ fontSize: '0.825rem', color: 'var(--text-muted)' }}>
                      Cálculo en vivo: Cada victoria suma {ligaSeleccionada.puntosPorPartido} puntos en la tabla.
                    </p>
                  </div>

                  <button
                    onClick={() => cargarPosiciones(ligaSeleccionada.id)}
                    className="btn btn-secondary btn-sm"
                    title="Actualizar tabla"
                  >
                    <RotateCcw size={14} />
                    <span>Actualizar</span>
                  </button>
                </div>

                {loadingPosiciones ? (
                  <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-muted)' }}>
                    Calculando posiciones...
                  </div>
                ) : posiciones.length === 0 ? (
                  <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
                    No hay participantes inscriptos o partidos con resultado aún. Inscriba socios y genere el fixture para ver la tabla.
                  </div>
                ) : (
                  <div style={{ overflowX: 'auto' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
                      <thead>
                        <tr style={{ borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                          <th style={{ padding: '12px 16px', width: 60 }}>Pos</th>
                          <th style={{ padding: '12px 16px' }}>Jugador / Socio</th>
                          <th style={{ padding: '12px 16px', textAlign: 'center', width: 80 }}>PJ</th>
                          <th style={{ padding: '12px 16px', textAlign: 'center', width: 80 }}>PG</th>
                          <th style={{ padding: '12px 16px', textAlign: 'center', width: 80 }}>PP</th>
                          <th style={{ padding: '12px 16px', textAlign: 'right', width: 100 }}>PTS</th>
                        </tr>
                      </thead>
                      <tbody>
                        {posiciones.map((pos, idx) => {
                          const isTop = idx === 0;
                          const isPodium = idx < 3;
                          return (
                            <tr
                              key={pos.socioId || idx}
                              style={{
                                borderBottom: '1px solid var(--border)',
                                background: isTop ? 'rgba(204, 255, 0, 0.05)' : 'transparent',
                              }}
                            >
                              <td style={{ padding: '14px 16px', fontWeight: 700 }}>
                                <div style={{
                                  width: 28,
                                  height: 28,
                                  borderRadius: '50%',
                                  background: idx === 0 ? 'var(--accent-volt)' : idx === 1 ? '#94A3B8' : idx === 2 ? '#B45309' : 'var(--bg-elevated)',
                                  color: idx === 0 ? '#000' : '#fff',
                                  display: 'flex',
                                  alignItems: 'center',
                                  justifyContent: 'center',
                                  fontSize: '0.85rem'
                                }}>
                                  {pos.posicion || idx + 1}
                                </div>
                              </td>
                              <td style={{ padding: '14px 16px', fontWeight: isPodium ? 700 : 500 }}>
                                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                  {isTop && <Star size={16} fill="var(--accent-volt)" color="var(--accent-volt)" />}
                                  <span>{pos.nombreJugador}</span>
                                </div>
                              </td>
                              <td style={{ padding: '14px 16px', textAlign: 'center', color: 'var(--text-muted)' }}>
                                {pos.partidosJugados}
                              </td>
                              <td style={{ padding: '14px 16px', textAlign: 'center', color: 'var(--primary)', fontWeight: 600 }}>
                                {pos.partidosGanados}
                              </td>
                              <td style={{ padding: '14px 16px', textAlign: 'center', color: 'var(--text-dim)' }}>
                                {pos.partidosPerdidos}
                              </td>
                              <td style={{ padding: '14px 16px', textAlign: 'right', fontWeight: 800, fontSize: '1.05rem', color: isTop ? 'var(--accent-volt)' : 'var(--text-main)' }}>
                                {pos.puntos}
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )}

            {/* TAB 2: FIXTURE ROUND-ROBIN (RF-40 & RF-41) */}
            {activeTab === 'partidos' && (
              <div style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-lg)',
                padding: '24px',
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                  <h4 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                    <Swords size={18} color="var(--accent-volt)" />
                    Cruces Programados
                  </h4>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                    Total: {ligaSeleccionada.partidos?.length || 0} partidos
                  </span>
                </div>

                {!ligaSeleccionada.partidos || ligaSeleccionada.partidos.length === 0 ? (
                  <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
                    Aún no se ha generado el fixture. Haga clic en <strong>"Generar Fixture"</strong> arriba para sortear los cruces entre todos los inscriptos.
                  </div>
                ) : (
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 14 }}>
                    {ligaSeleccionada.partidos.map((partido) => {
                      const jugado = Boolean(partido.resultado);
                      return (
                        <div
                          key={partido.id}
                          style={{
                            background: 'var(--bg-elevated)',
                            border: jugado ? '1px solid rgba(34, 197, 94, 0.4)' : '1px solid var(--border)',
                            borderRadius: 'var(--radius-md)',
                            padding: '16px',
                            display: 'flex',
                            flexDirection: 'column',
                            gap: 12,
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <span className="badge badge-secondary" style={{ fontSize: '0.7rem' }}>
                              {partido.ronda || 'Fecha regular'}
                            </span>
                            <span className={jugado ? 'badge badge-success' : 'badge badge-warning'} style={{ fontSize: '0.7rem' }}>
                              {jugado ? 'Finalizado' : 'Pendiente'}
                            </span>
                          </div>

                          <div style={{ display: 'flex', flexDirection: 'column', gap: 6, padding: '8px 0' }}>
                            <div style={{ fontSize: '0.95rem', fontWeight: 600, color: 'var(--text-main)' }}>
                              Partido #{partido.id}
                            </div>
                            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: 6 }}>
                              <Calendar size={13} />
                              <span>{new Date(partido.fecha).toLocaleDateString()}</span>
                            </div>
                          </div>

                          <div style={{
                            background: 'var(--bg-dark)',
                            padding: '10px 14px',
                            borderRadius: 'var(--radius-sm)',
                            border: '1px solid var(--border)',
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                          }}>
                            <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>Resultado:</span>
                            <span style={{
                              fontWeight: 700,
                              color: jugado ? 'var(--accent-volt)' : 'var(--text-muted)',
                              fontSize: '0.95rem'
                            }}>
                              {partido.resultado || 'Sin disputar'}
                            </span>
                          </div>

                          <button
                            onClick={() => abrirModalResultado(partido)}
                            className="btn btn-secondary btn-sm"
                            style={{ width: '100%', justifyContent: 'center' }}
                          >
                            <Edit size={14} />
                            <span>{jugado ? 'Editar Resultado' : 'Cargar Resultado'}</span>
                          </button>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            )}

            {/* TAB 3: PARTICIPANTES / INSCRIPTOS (RF-39) */}
            {activeTab === 'participantes' && (
              <div style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-lg)',
                padding: '24px',
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                  <h4 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                    <Users size={18} color="var(--accent-volt)" />
                    Nómina Oficial de Inscriptos
                  </h4>
                  <button onClick={() => setModalInscribirOpen(true)} className="btn btn-volt btn-sm">
                    <Plus size={14} />
                    <span>Inscribir Socio</span>
                  </button>
                </div>

                {!ligaSeleccionada.inscriptos || ligaSeleccionada.inscriptos.length === 0 ? (
                  <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
                    No hay socios inscriptos aún en esta liga. Inscriba jugadores para habilitar el sorteo del fixture.
                  </div>
                ) : (
                  <div style={{ overflowX: 'auto' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
                      <thead>
                        <tr style={{ borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                          <th style={{ padding: '12px 16px' }}># Dorsal</th>
                          <th style={{ padding: '12px 16px' }}>Nombre y Apellido</th>
                          <th style={{ padding: '12px 16px' }}>DNI</th>
                          <th style={{ padding: '12px 16px' }}>Fecha Inscripción</th>
                        </tr>
                      </thead>
                      <tbody>
                        {ligaSeleccionada.inscriptos.map((ins) => (
                          <tr key={ins.inscripcionId || ins.socioId} style={{ borderBottom: '1px solid var(--border)' }}>
                            <td style={{ padding: '12px 16px' }}>
                              <span className="badge badge-secondary">#{ins.numeroParticipante}</span>
                            </td>
                            <td style={{ padding: '12px 16px', fontWeight: 600 }}>{ins.nombreCompleto}</td>
                            <td style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>{ins.dni}</td>
                            <td style={{ padding: '12px 16px', color: 'var(--text-dim)', fontSize: '0.85rem' }}>
                              {new Date(ins.fechaInscripcion).toLocaleDateString()}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            )}
          </div>
        ) : (
          <div style={{ padding: 48, background: 'var(--bg-card)', borderRadius: 'var(--radius-lg)', textAlign: 'center', color: 'var(--text-muted)' }}>
            Seleccioná una liga de la lista para ver su clasificación, fixture y participantes.
          </div>
        )}
      </div>

      {/* MODAL CREAR LIGA (RF-35) */}
      {modalCrearOpen && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 540 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Award color="var(--accent-volt)" size={22} />
                Crear Nueva Liga Regular
              </h3>
              <button onClick={() => setModalCrearOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && <div className="badge badge-danger" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formError}</div>}
            {formSuccess && <div className="badge badge-success" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formSuccess}</div>}

            <form onSubmit={handleCrearLiga} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Nombre de la Liga</label>
                <input
                  type="text"
                  required
                  placeholder="Ej: Liga Apertura Caballeros 2026"
                  className="input"
                  value={nuevaLiga.nombre}
                  onChange={(e) => setNuevaLiga({ ...nuevaLiga, nombre: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Fecha Inicio</label>
                  <input
                    type="date"
                    required
                    className="input"
                    value={nuevaLiga.fechaInicio}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, fechaInicio: e.target.value })}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Fecha Fin</label>
                  <input
                    type="date"
                    required
                    className="input"
                    value={nuevaLiga.fechaFin}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, fechaFin: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Género</label>
                  <select
                    className="input"
                    value={nuevaLiga.categoriaGenero}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, categoriaGenero: e.target.value })}
                  >
                    <option value={1}>Masculino</option>
                    <option value={0}>Femenino</option>
                  </select>
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Modalidad</label>
                  <select
                    className="input"
                    value={nuevaLiga.modalidad}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, modalidad: e.target.value })}
                  >
                    <option value={0}>Single (Individual)</option>
                    <option value={1}>Dobles (En parejas)</option>
                  </select>
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Cantidad de Fechas</label>
                  <input
                    type="number"
                    min="1"
                    max="38"
                    required
                    className="input"
                    value={nuevaLiga.cantidadFechas}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, cantidadFechas: e.target.value })}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Puntos por Triunfo</label>
                  <input
                    type="number"
                    min="1"
                    max="10"
                    required
                    className="input"
                    value={nuevaLiga.puntosPorPartido}
                    onChange={(e) => setNuevaLiga({ ...nuevaLiga, puntosPorPartido: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 12 }}>
                <button type="button" onClick={() => setModalCrearOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submitting} className="btn btn-volt">
                  {submitting ? 'Creando...' : 'Crear Liga'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL MODIFICAR LIGA (RF-37) */}
      {modalModificarOpen && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 540 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Edit color="var(--accent-volt)" size={22} />
                Modificar Parámetros de Liga
              </h3>
              <button onClick={() => setModalModificarOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && <div className="badge badge-danger" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formError}</div>}
            {formSuccess && <div className="badge badge-success" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formSuccess}</div>}

            <form onSubmit={handleModificarLiga} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Nombre</label>
                <input
                  type="text"
                  required
                  className="input"
                  value={ligaEdicion.nombre}
                  onChange={(e) => setLigaEdicion({ ...ligaEdicion, nombre: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Fecha Inicio</label>
                  <input
                    type="date"
                    required
                    className="input"
                    value={ligaEdicion.fechaInicio}
                    onChange={(e) => setLigaEdicion({ ...ligaEdicion, fechaInicio: e.target.value })}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Fecha Fin</label>
                  <input
                    type="date"
                    required
                    className="input"
                    value={ligaEdicion.fechaFin}
                    onChange={(e) => setLigaEdicion({ ...ligaEdicion, fechaFin: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Fechas</label>
                  <input
                    type="number"
                    min="1"
                    max="38"
                    required
                    className="input"
                    value={ligaEdicion.cantidadFechas}
                    onChange={(e) => setLigaEdicion({ ...ligaEdicion, cantidadFechas: e.target.value })}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>Puntos por Triunfo</label>
                  <input
                    type="number"
                    min="1"
                    max="10"
                    required
                    className="input"
                    value={ligaEdicion.puntosPorPartido}
                    onChange={(e) => setLigaEdicion({ ...ligaEdicion, puntosPorPartido: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 12 }}>
                <button type="button" onClick={() => setModalModificarOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submitting} className="btn btn-volt">
                  {submitting ? 'Guardando...' : 'Guardar Cambios'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL INSCRIBIR SOCIO (RF-39) */}
      {modalInscribirOpen && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 420 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Users color="var(--accent-volt)" size={22} />
                Inscribir Socio a la Liga
              </h3>
              <button onClick={() => setModalInscribirOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && <div className="badge badge-danger" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formError}</div>}
            {formSuccess && <div className="badge badge-success" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formSuccess}</div>}

            <form onSubmit={handleInscribirSocio} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                  DNI del Socio
                </label>
                <div style={{ position: 'relative' }}>
                  <Search size={16} style={{ position: 'absolute', left: 12, top: 12, color: 'var(--text-dim)' }} />
                  <input
                    type="number"
                    required
                    placeholder="Ej: 38999888"
                    className="input"
                    style={{ paddingLeft: 38 }}
                    value={inscribirDni}
                    onChange={(e) => setInscribirDni(e.target.value)}
                  />
                </div>
                <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                  El socio debe estar registrado en el club y no estar previamente inscripto.
                </small>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 12 }}>
                <button type="button" onClick={() => setModalInscribirOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submitting} className="btn btn-volt">
                  {submitting ? 'Inscribiendo...' : 'Confirmar Inscripción'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL REGISTRAR RESULTADO DE PARTIDO */}
      {modalResultadoOpen && partidoParaResultado && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 440 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <CheckCircle color="var(--accent-volt)" size={22} />
                Registrar Resultado Oficial
              </h3>
              <button onClick={() => setModalResultadoOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && <div className="badge badge-danger" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formError}</div>}
            {formSuccess && <div className="badge badge-success" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formSuccess}</div>}

            <form onSubmit={handleRegistrarResultado} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div style={{ background: 'var(--bg-elevated)', padding: '12px 16px', borderRadius: 'var(--radius-md)' }}>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)', display: 'block' }}>Partido ID #{partidoParaResultado.id}</span>
                <span style={{ fontWeight: 600, color: 'var(--text-main)' }}>{partidoParaResultado.ronda || 'Fecha regular'}</span>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                  Marcador Oficial (Sets y Games)
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ej: 6-4, 6-3"
                  className="input"
                  value={resultadoForm.resultado}
                  onChange={(e) => setResultadoForm({ ...resultadoForm, resultado: e.target.value })}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                  Nombre del Ganador (para asignación de puntos)
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ej: Juan Pérez"
                  className="input"
                  value={resultadoForm.ganadorNombre}
                  onChange={(e) => setResultadoForm({ ...resultadoForm, ganadorNombre: e.target.value })}
                />
                <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                  Debe coincidir con el nombre de uno de los jugadores para otorgarle los puntos de la victoria en la tabla.
                </small>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 12 }}>
                <button type="button" onClick={() => setModalResultadoOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submitting} className="btn btn-volt">
                  {submitting ? 'Guardando...' : 'Asentar Resultado'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
