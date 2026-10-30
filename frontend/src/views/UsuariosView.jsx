import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { TennisMark } from '../components/Logo';
import { Search, UserPlus, Printer, Edit2, Trash2, UserCheck, Shield, Award, X, Check, AlertCircle, RotateCcw } from 'lucide-react';

export default function UsuariosView() {
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [filterRole, setFilterRole] = useState('Todos');

  // Modales
  const [modalCrearOpen, setModalCrearOpen] = useState(false);
  const [modalEditarOpen, setModalEditarOpen] = useState(false);
  const [modalImprimirOpen, setModalImprimirOpen] = useState(false);
  const [modalAsignarOpen, setModalAsignarOpen] = useState(false);

  // Selecciones y estados de formulario
  const [usuarioSeleccionado, setUsuarioSeleccionado] = useState(null);
  const [fichaImpresion, setFichaImpresion] = useState(null);
  const [formError, setFormError] = useState('');
  const [formSuccess, setFormSuccess] = useState('');
  const [submitting, setSubmitting] = useState(false);

  // Forms
  const [nuevoUsuario, setNuevoUsuario] = useState({
    nombre: '',
    apellido: '',
    dni: '',
    email: '',
    telefono: '',
    direccion: '',
    codigoPostal: '',
    fechaDeNacimiento: '',
    password: '',
    rol: 2, // 2 = Socio
    tituloHabilitante: '',
    enteEmisor: '',
    especialidad: '',
    antiguedad: '',
  });

  const [asignarData, setAsignarData] = useState({
    instructorId: 1,
    tipo: 'profesor', // 'profesor' o 'entrenador'
    fechaHoraInicio: new Date().toISOString().slice(0, 16),
    duracionMinutos: 60,
  });

  useEffect(() => {
    cargarUsuarios();
  }, []);

  async function cargarUsuarios() {
    setLoading(true);
    try {
      const data = await api.usuarios.listar(false);
      setUsuarios(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  // Normalizador para ignorar tildes/acentos y mayúsculas
  const normalizarTexto = (texto) =>
    (texto || '')
      .toString()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase();

  // Filtrado
  const usuariosFiltrados = usuarios.filter((u) => {
    const searchNorm = normalizarTexto(search);
    const matchesSearch =
      normalizarTexto(u.nombre).includes(searchNorm) ||
      normalizarTexto(u.apellido).includes(searchNorm) ||
      u.dni.toString().includes(search.trim());

    const matchesRole =
      filterRole === 'Todos' ||
      (filterRole === 'Socio' && u.rol === 2) ||
      (filterRole === 'Profesor' && u.rol === 3) ||
      (filterRole === 'Entrenador' && u.rol === 4) ||
      (filterRole === 'Administrador' && u.rol === 1);

    return matchesSearch && matchesRole;
  });

  // RF-01 Crear Usuario
  const handleCrearUsuario = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      if (!nuevoUsuario.nombre?.trim() || !nuevoUsuario.apellido?.trim()) {
        throw new Error('El nombre y apellido son obligatorios.');
      }
      if (!nuevoUsuario.dni) {
        throw new Error('El DNI es obligatorio.');
      }
      if (!nuevoUsuario.email?.trim()) {
        throw new Error('El correo electrónico es obligatorio.');
      }
      if (!nuevoUsuario.telefono?.trim()) {
        throw new Error('El teléfono es obligatorio y debe contener solo números.');
      }

      let fechaNacISO;
      if (nuevoUsuario.fechaDeNacimiento && !isNaN(new Date(nuevoUsuario.fechaDeNacimiento).getTime())) {
        fechaNacISO = new Date(nuevoUsuario.fechaDeNacimiento).toISOString();
      } else {
        fechaNacISO = new Date('1995-01-01T00:00:00.000Z').toISOString();
      }

      await api.usuarios.crear({
        nombre: nuevoUsuario.nombre.trim(),
        apellido: nuevoUsuario.apellido.trim(),
        dni: parseInt(nuevoUsuario.dni, 10),
        email: nuevoUsuario.email.trim(),
        telefono: nuevoUsuario.telefono.trim(),
        direccion: nuevoUsuario.direccion?.trim() || 'Sin especificar',
        codigoPostal: parseInt(nuevoUsuario.codigoPostal, 10) || 1000,
        fechaDeNacimiento: fechaNacISO,
        rol: parseInt(nuevoUsuario.rol, 10),
        tituloHabilitante: nuevoUsuario.tituloHabilitante?.trim() || undefined,
        enteEmisor: nuevoUsuario.enteEmisor?.trim() || undefined,
        antiguedad: nuevoUsuario.antiguedad ? parseInt(nuevoUsuario.antiguedad, 10) : 0,
        especialidad: nuevoUsuario.especialidad?.trim() || undefined,
      });
      setFormSuccess('Usuario registrado exitosamente.');
      setTimeout(() => {
        setModalCrearOpen(false);
        setFormSuccess('');
        setNuevoUsuario({
          nombre: '',
          apellido: '',
          dni: '',
          email: '',
          telefono: '',
          direccion: '',
          codigoPostal: '',
          fechaDeNacimiento: '',
          password: '',
          rol: 2,
          tituloHabilitante: '',
          enteEmisor: '',
          especialidad: '',
          antiguedad: '',
        });
      }, 1200);
      cargarUsuarios();
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  // RF-02 Modificar Usuario
  const handleModificarUsuario = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      let fechaNacISO;
      if (usuarioSeleccionado.fechaDeNacimiento && !isNaN(new Date(usuarioSeleccionado.fechaDeNacimiento).getTime())) {
        fechaNacISO = new Date(usuarioSeleccionado.fechaDeNacimiento).toISOString();
      } else {
        fechaNacISO = new Date('1995-01-01T00:00:00.000Z').toISOString();
      }

      await api.usuarios.modificar(usuarioSeleccionado.dni, {
        nombre: usuarioSeleccionado.nombre,
        apellido: usuarioSeleccionado.apellido,
        email: usuarioSeleccionado.email,
        telefono: usuarioSeleccionado.telefono,
        direccion: usuarioSeleccionado.direccion,
        codigoPostal: parseInt(usuarioSeleccionado.codigoPostal, 10) || 1000,
        fechaDeNacimiento: fechaNacISO,
        estadoMembresia: usuarioSeleccionado.estadoMembresia,
      });
      setFormSuccess('Datos actualizados correctamente.');
      setTimeout(() => {
        setModalEditarOpen(false);
        setFormSuccess('');
      }, 1200);
      cargarUsuarios();
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleAprobarCertificacion = async (u) => {
    if (!window.confirm(`¿Aprobar la certificación de ${u.nombre} ${u.apellido} (${u.tituloHabilitante || 'Docente'}) y habilitar su cuenta en el club?`)) return;
    try {
      if (u.certificacionId) {
        await api.certificaciones.resolver(u.certificacionId, 1);
      } else {
        if (u.rol === 3) await api.profesores.activar(u.id);
        else if (u.rol === 4) await api.entrenadores.activar(u.id);
      }
      cargarUsuarios();
    } catch (err) {
      alert(`Error al aprobar certificación: ${err.message}`);
    }
  };

  const handleRechazarCertificacion = async (u) => {
    if (!window.confirm(`¿Rechazar la certificación deportiva presentada por ${u.nombre} ${u.apellido}? La cuenta permanecerá inhabilitada.`)) return;
    try {
      if (u.certificacionId) {
        await api.certificaciones.resolver(u.certificacionId, 2);
      } else {
        alert('No se encontró el registro de certificación para rechazar.');
      }
      cargarUsuarios();
    } catch (err) {
      alert(`Error al rechazar certificación: ${err.message}`);
    }
  };

  const handleReactivarUsuario = async (u) => {
    if (!window.confirm(`¿Seguro que deseás reactivar a ${u.nombre} ${u.apellido}?`)) return;
    try {
      if (u.rol === 3) {
        await api.profesores.activar(u.id);
      } else if (u.rol === 4) {
        await api.entrenadores.activar(u.id);
      }
      cargarUsuarios();
    } catch (err) {
      alert(`Error al reactivar: ${err.message}`);
    }
  };

  // RF-04 Eliminar / Baja Lógica
  const handleEliminarUsuario = async (dni, nombre) => {
    if (!window.confirm(`¿Seguro que deseás dar de baja lógica al usuario ${nombre}?`)) return;
    try {
      await api.usuarios.eliminar(dni);
      cargarUsuarios();
    } catch (err) {
      alert(`Error: ${err.message}`);
    }
  };

  // RF-05, RF-59 y RF-64 Imprimir Ficha / Credencial Oficial
  const handleAbrirImpresion = async (dni, rol, id) => {
    try {
      if (rol === 3) {
        const fichaProf = await api.profesores.imprimir(id);
        setFichaImpresion({
          ...fichaProf,
          nombreCompleto: `${fichaProf.nombre} ${fichaProf.apellido}`,
          rol: 'Profesor Titular Oficial',
          tituloHabilitante: fichaProf.nombreCertificacion || 'Certificación de Profesor de Tenis',
          enteEmisor: fichaProf.enteEmisor || 'Federación Argentina de Tenis (FAT)',
          fechaEmisionReporte: fichaProf.fechaEmision || new Date().toISOString()
        });
      } else if (rol === 4) {
        const fichaEnt = await api.entrenadores.imprimir(id);
        setFichaImpresion({
          ...fichaEnt,
          nombreCompleto: `${fichaEnt.nombre} ${fichaEnt.apellido}`,
          rol: 'Entrenador Físico Certificado',
          tituloHabilitante: fichaEnt.nombreCertificacion || 'Certificación de Entrenador de Tenis',
          enteEmisor: fichaEnt.enteEmisor || 'Federación Argentina de Tenis (FAT)',
          fechaEmisionReporte: fichaEnt.fechaEmision || new Date().toISOString()
        });
      } else {
        const ficha = await api.usuarios.obtenerFichaImpresion(dni);
        setFichaImpresion(ficha);
      }
      setModalImprimirOpen(true);
    } catch (err) {
      alert(`Error al generar ficha: ${err.message}`);
    }
  };

  // RF-06 y RF-07 Asignar Profesor / Entrenador
  const handleAsignarInstructor = async (e) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      if (asignarData.tipo === 'profesor') {
        await api.usuarios.asignarProfesor(usuarioSeleccionado.dni, {
          profesorId: parseInt(asignarData.instructorId, 10),
          fechaHoraInicio: new Date(asignarData.fechaHoraInicio).toISOString(),
          duracionMinutos: parseInt(asignarData.duracionMinutos, 10),
        });
      } else {
        await api.usuarios.asignarEntrenador(usuarioSeleccionado.dni, {
          entrenadorId: parseInt(asignarData.instructorId, 10),
          fechaHoraInicio: new Date(asignarData.fechaHoraInicio).toISOString(),
          duracionMinutos: parseInt(asignarData.duracionMinutos, 10),
        });
      }
      setFormSuccess(`¡${asignarData.tipo === 'profesor' ? 'Profesor' : 'Entrenador'} asignado con éxito!`);
      setTimeout(() => {
        setModalAsignarOpen(false);
        setFormSuccess('');
      }, 1400);
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const getRoleName = (rol) => {
    switch (rol) {
      case 1: return 'Administrador';
      case 2: return 'Socio';
      case 3: return 'Profesor';
      case 4: return 'Entrenador';
      default: return 'Usuario';
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Top Controls */}
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 16, justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, flex: 1, maxWidth: 500 }}>
          <div style={{ position: 'relative', width: '100%' }}>
            <Search size={18} style={{ position: 'absolute', left: 14, top: 12, color: 'var(--text-dim)' }} />
            <input
              type="text"
              className="input-field"
              style={{ paddingLeft: 42 }}
              placeholder="Buscar por DNI, Nombre o Apellido..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {/* Filtros de Rol */}
          <div style={{ display: 'flex', background: 'var(--bg-card)', padding: 4, borderRadius: 'var(--radius-sm)', border: '1px solid var(--border)' }}>
            {['Todos', 'Socio', 'Profesor', 'Entrenador'].map((role) => (
              <button
                key={role}
                onClick={() => setFilterRole(role)}
                style={{
                  background: filterRole === role ? 'var(--primary)' : 'transparent',
                  color: filterRole === role ? '#041b0d' : 'var(--text-muted)',
                  border: 'none',
                  padding: '6px 12px',
                  borderRadius: 6,
                  fontWeight: 600,
                  fontSize: '0.8rem',
                  cursor: 'pointer',
                }}
              >
                {role}
              </button>
            ))}
          </div>

          <button onClick={() => setModalCrearOpen(true)} className="btn btn-volt">
            <UserPlus size={18} />
            <span>Nuevo Usuario</span>
          </button>
        </div>
      </div>

      {/* Tabla de Usuarios */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        overflow: 'hidden',
        boxShadow: 'var(--shadow-sm)',
      }}>
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ background: 'var(--bg-dark)', borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.785rem', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
                <th style={{ padding: '14px 20px' }}>Usuario / Socio</th>
                <th style={{ padding: '14px 16px' }}>DNI</th>
                <th style={{ padding: '14px 16px' }}>Contacto</th>
                <th style={{ padding: '14px 16px' }}>Rol</th>
                <th style={{ padding: '14px 16px' }}>Estado</th>
                <th style={{ padding: '14px 20px', textAlign: 'right' }}>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="6" style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
                    Cargando usuarios desde la base de datos...
                  </td>
                </tr>
              ) : usuariosFiltrados.length === 0 ? (
                <tr>
                  <td colSpan="6" style={{ padding: 40, textAlign: 'center', color: 'var(--text-muted)' }}>
                    No se encontraron usuarios que coincidan con la búsqueda.
                  </td>
                </tr>
              ) : (
                usuariosFiltrados.map((u) => (
                  <tr
                    key={u.id}
                    style={{
                      borderBottom: '1px solid rgba(31, 56, 47, 0.4)',
                      background: !u.activo ? 'rgba(244, 63, 94, 0.03)' : 'transparent',
                    }}
                  >
                    <td style={{ padding: '14px 20px' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                        <div style={{ width: 36, height: 36, borderRadius: '50%', background: 'var(--bg-elevated)', border: '1px solid var(--border)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold', color: 'var(--accent-volt)' }}>
                          {u.nombre.charAt(0)}{u.apellido.charAt(0)}
                        </div>
                        <div>
                          <strong style={{ display: 'block', color: 'var(--text-main)' }}>
                            {u.apellido}, {u.nombre}
                          </strong>
                          <small style={{ color: 'var(--text-dim)' }}>ID #{u.id}</small>
                        </div>
                      </div>
                    </td>
                    <td style={{ padding: '14px 16px', fontWeight: 600, color: 'var(--text-muted)' }}>
                      {u.dni}
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <div style={{ fontSize: '0.85rem' }}>{u.email}</div>
                      <small style={{ color: 'var(--text-dim)' }}>{u.telefono}</small>
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <span className={`badge ${u.rol === 1 ? 'badge-danger' : u.rol === 2 ? 'badge-success' : 'badge-info'}`}>
                        {getRoleName(u.rol)}
                      </span>
                      {(u.rol === 3 || u.rol === 4) && (u.tituloHabilitante || u.enteEmisor) && (
                        <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', marginTop: 4 }}>
                          <Award size={12} style={{ display: 'inline', marginRight: 4, verticalAlign: 'middle', color: 'var(--accent-volt)' }} />
                          {u.tituloHabilitante || 'Certificación Oficial'}
                          {u.enteEmisor ? ` (${u.enteEmisor})` : ''}
                        </div>
                      )}
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      {(u.rol === 3 || u.rol === 4) ? (
                        (u.estadoCertificacion === 0 || (!u.activo && (u.certificacionId || u.tituloHabilitante) && u.estadoCertificacion !== 2)) ? (
                          <span className="badge" style={{ background: 'rgba(234, 179, 8, 0.15)', color: '#eab308', border: '1px solid rgba(234, 179, 8, 0.3)', display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                            <AlertCircle size={12} /> Certif. Pendiente
                          </span>
                        ) : u.estadoCertificacion === 2 ? (
                          <span className="badge" style={{ background: 'rgba(239, 68, 68, 0.15)', color: '#ef4444', border: '1px solid rgba(239, 68, 68, 0.3)', display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                            <X size={12} /> Rechazada
                          </span>
                        ) : u.activo ? (
                          <span className="badge badge-success" style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                            <Check size={12} /> Activo (Habilitado)
                          </span>
                        ) : (
                          <span className="badge badge-danger">Inactivo (Baja)</span>
                        )
                      ) : (
                        u.activo ? (
                          <span className="badge badge-success">Activo</span>
                        ) : (
                          <span className="badge badge-danger">Inactivo (Baja)</span>
                        )
                      )}
                    </td>
                    <td style={{ padding: '14px 20px', textAlign: 'right' }}>
                      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                        {/* Imprimir Ficha */}
                        <button
                          onClick={() => handleAbrirImpresion(u.dni, u.rol, u.id)}
                          className="btn btn-secondary btn-sm"
                          title="Imprimir Ficha Técnica de Socio"
                        >
                          <Printer size={15} />
                        </button>

                        {/* Asignar Particular (solo si es socio) */}
                        {u.rol === 2 && u.activo && (
                          <button
                            onClick={() => {
                              setUsuarioSeleccionado(u);
                              setModalAsignarOpen(true);
                              setFormError('');
                            }}
                            className="btn btn-secondary btn-sm"
                            title="Asignar Profesor o Entrenador Particular"
                          >
                            <UserCheck size={15} color="var(--accent-volt)" />
                          </button>
                        )}

                        {/* Aprobar / Rechazar Certificación para docentes pendientes */}
                        {(u.rol === 3 || u.rol === 4) && !u.activo && (u.estadoCertificacion === 0 || u.certificacionId || u.tituloHabilitante) && u.estadoCertificacion !== 2 && (
                          <>
                            <button
                              onClick={() => handleAprobarCertificacion(u)}
                              className="btn btn-secondary btn-sm"
                              title="Aprobar certificación oficial y habilitar docente"
                              style={{ color: '#4ade80', borderColor: 'rgba(74, 222, 128, 0.4)' }}
                            >
                              <Check size={15} />
                            </button>
                            <button
                              onClick={() => handleRechazarCertificacion(u)}
                              className="btn btn-secondary btn-sm"
                              title="Rechazar certificación deportiva"
                              style={{ color: '#f87171', borderColor: 'rgba(248, 113, 113, 0.4)' }}
                            >
                              <X size={15} />
                            </button>
                          </>
                        )}

                        {/* Modificar */}
                        <button
                          onClick={() => {
                            setUsuarioSeleccionado({ ...u });
                            setModalEditarOpen(true);
                            setFormError('');
                          }}
                          className="btn btn-secondary btn-sm"
                          title="Modificar Datos"
                        >
                          <Edit2 size={15} />
                        </button>

                        {/* Eliminar (Baja lógica) o Reactivar */}
                        {u.activo ? (
                          <button
                            onClick={() => handleEliminarUsuario(u.dni, `${u.nombre} ${u.apellido}`)}
                            className="btn btn-danger btn-sm"
                            title="Dar de baja usuario"
                          >
                            <Trash2 size={15} />
                          </button>
                        ) : (
                          (u.rol === 3 || u.rol === 4) && u.estadoCertificacion !== 0 && (
                            <button
                              onClick={() => handleReactivarUsuario(u)}
                              className="btn btn-secondary btn-sm"
                              title="Reactivar usuario en el sistema"
                              style={{ color: '#4ade80' }}
                            >
                              <RotateCcw size={15} />
                            </button>
                          )
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* MODAL 1: REGISTRAR USUARIO */}
      {modalCrearOpen && (
        <div className="modal-overlay" onClick={() => setModalCrearOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Registrar Nuevo Usuario / Socio</h3>
              <button onClick={() => setModalCrearOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && (
              <div style={{ background: 'rgba(244, 63, 94, 0.15)', border: '1px solid rgba(244, 63, 94, 0.4)', color: '#f87171', padding: '10px 14px', borderRadius: 8, marginBottom: 16, fontSize: '0.9rem' }}>
                {formError}
              </div>
            )}
            {formSuccess && (
              <div style={{ background: 'rgba(34, 197, 94, 0.15)', border: '1px solid rgba(34, 197, 94, 0.4)', color: '#4ade80', padding: '10px 14px', borderRadius: 8, marginBottom: 16, fontSize: '0.9rem' }}>
                {formSuccess}
              </div>
            )}

            <form onSubmit={handleCrearUsuario}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Nombre</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={nuevoUsuario.nombre}
                    onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, nombre: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Apellido</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={nuevoUsuario.apellido}
                    onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, apellido: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">DNI (Único)</label>
                  <input
                    type="text" inputMode="numeric" maxLength={8} required className="input-field" placeholder="Ej. 12345678" value={nuevoUsuario.dni} onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, dni: e.target.value.replace(/\D/g, '').slice(0, 8) })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Teléfono</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={nuevoUsuario.telefono}
                    onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, telefono: e.target.value })}
                  />
                </div>
              </div>

              <div className="input-group">
                <label className="input-label">Correo Electrónico</label>
                <input
                  type="email"
                  required
                  className="input-field"
                  value={nuevoUsuario.email}
                  onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, email: e.target.value })}
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Dirección</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={nuevoUsuario.direccion}
                    onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, direccion: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Rol en el Club</label>
                  <select
                    className="input-field"
                    value={nuevoUsuario.rol}
                    onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, rol: parseInt(e.target.value, 10) })}
                  >
                    <option value={2}>Socio</option>
                    <option value={3}>Profesor</option>
                    <option value={4}>Entrenador</option>
                    <option value={1}>Administrador</option>
                  </select>
                </div>
              </div>

              {(nuevoUsuario.rol === 3 || nuevoUsuario.rol === 4) && (
                <div style={{ background: 'var(--bg-elevated)', padding: 14, borderRadius: 'var(--radius-sm)', border: '1px solid var(--primary-subtle-border)', marginTop: 10, display: 'flex', flexDirection: 'column', gap: 10 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--primary-subtle-text)', fontSize: '0.8rem', fontWeight: 600 }}>
                    <Award size={16} />
                    <span>Requisito Obligatorio: Título Habilitante y Certificación Oficial</span>
                  </div>
                  <small style={{ color: 'var(--text-dim)', fontSize: '0.785rem' }}>
                    Al registrar un profesor o entrenador, la cuenta se creará en estado <strong>inactiva con certificación pendiente</strong>. Deberá ser aprobada por la administración del club para habilitar sus funciones docentes.
                  </small>
                  <div className="input-group">
                    <label className="input-label">Nombre de Certificación / Título</label>
                    <input
                      type="text"
                      required
                      className="input-field"
                      value={nuevoUsuario.tituloHabilitante}
                      onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, tituloHabilitante: e.target.value })}
                    />
                  </div>
                  <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
                    <div className="input-group">
                      <label className="input-label">Ente Emisor Oficial</label>
                      <input
                        type="text"
                        required
                        className="input-field"
                        value={nuevoUsuario.enteEmisor}
                        onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, enteEmisor: e.target.value })}
                      />
                    </div>
                    <div className="input-group">
                      <label className="input-label">Antigüedad (Años)</label>
                      <input
                        type="number"
                        min={0}
                        required
                        className="input-field"
                        value={nuevoUsuario.antiguedad}
                        onChange={(e) => setNuevoUsuario({ ...nuevoUsuario, antiguedad: parseInt(e.target.value, 10) || 0 })}
                      />
                    </div>
                  </div>
                </div>
              )}

              <button type="submit" disabled={submitting} className="btn btn-primary" style={{ width: '100%', marginTop: 8 }}>
                {submitting ? 'Guardando...' : 'Registrar Usuario'}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* MODAL 2: EDITAR USUARIO */}
      {modalEditarOpen && usuarioSeleccionado && (
        <div className="modal-overlay" onClick={() => setModalEditarOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Modificar Usuario DNI: {usuarioSeleccionado.dni}</h3>
              <button onClick={() => setModalEditarOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
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

            <form onSubmit={handleModificarUsuario}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Nombre</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={usuarioSeleccionado.nombre}
                    onChange={(e) => setUsuarioSeleccionado({ ...usuarioSeleccionado, nombre: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Apellido</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={usuarioSeleccionado.apellido}
                    onChange={(e) => setUsuarioSeleccionado({ ...usuarioSeleccionado, apellido: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Email</label>
                  <input
                    type="email"
                    required
                    className="input-field"
                    value={usuarioSeleccionado.email}
                    onChange={(e) => setUsuarioSeleccionado({ ...usuarioSeleccionado, email: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Teléfono</label>
                  <input
                    type="text"
                    required
                    className="input-field"
                    value={usuarioSeleccionado.telefono}
                    onChange={(e) => setUsuarioSeleccionado({ ...usuarioSeleccionado, telefono: e.target.value })}
                  />
                </div>
              </div>

              <div className="input-group">
                <label className="input-label">Dirección</label>
                <input
                  type="text"
                  required
                  className="input-field"
                  value={usuarioSeleccionado.direccion}
                  onChange={(e) => setUsuarioSeleccionado({ ...usuarioSeleccionado, direccion: e.target.value })}
                />
              </div>

              <button type="submit" disabled={submitting} className="btn btn-primary" style={{ width: '100%', marginTop: 8 }}>
                {submitting ? 'Guardando...' : 'Guardar Modificaciones'}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* MODAL 3: FICHA DE IMPRESIÓN */}
      {modalImprimirOpen && fichaImpresion && (
        <div className="modal-overlay" onClick={() => setModalImprimirOpen(false)}>
          <div className="modal-content print-card" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 480 }}>
            <div className="no-print" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h4>Credencial Imprimible Oficial</h4>
              <button onClick={() => setModalImprimirOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Carnet de Socio Visual */}
            <div style={{
              background: 'linear-gradient(135deg, #091712, #122920)',
              border: '2px solid var(--accent-volt)',
              borderRadius: 16,
              padding: 24,
              color: '#ffffff',
              boxShadow: 'var(--shadow-md)',
              position: 'relative',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', borderBottom: '1px solid rgba(255,255,255,0.15)', paddingBottom: 14, marginBottom: 14 }}>
                <div>
                  <h3 style={{ fontSize: '1.2rem', color: 'var(--accent-volt)', textTransform: 'uppercase' }}>TENIS AHORA</h3>
                  <small style={{ fontSize: '0.75rem', opacity: 0.8 }}>Complejo El Deporte Saludable</small>
                </div>
                <div style={{ background: 'var(--accent-volt)', color: '#000', fontWeight: 800, padding: '2px 8px', borderRadius: 4, fontSize: '0.75rem' }}>
                  {fichaImpresion.rol}
                </div>
              </div>

              <div style={{ display: 'flex', gap: 16, alignItems: 'center', marginBottom: 16 }}>
                <div style={{ width: 64, height: 64, borderRadius: 12, background: 'var(--bg-elevated)', border: '2px solid rgba(255,255,255,0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.6rem' }}>
                  <TennisMark size={32} color="var(--accent-volt)" />
                </div>
                <div>
                  <h2 style={{ fontSize: '1.25rem', marginBottom: 4 }}>{fichaImpresion.nombreCompleto}</h2>
                  <div style={{ fontSize: '0.85rem', opacity: 0.85 }}>DNI: <strong>{fichaImpresion.dni}</strong></div>
                  <div style={{ fontSize: '0.85rem', opacity: 0.85 }}>Edad: <strong>{fichaImpresion.edad} años</strong></div>
                </div>
              </div>

              <div style={{ fontSize: '0.8rem', opacity: 0.75, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6, borderTop: '1px solid rgba(255,255,255,0.1)', paddingTop: 10 }}>
                {fichaImpresion.tituloHabilitante && (
                  <div style={{ gridColumn: 'span 2', background: 'rgba(56, 189, 248, 0.15)', padding: '6px 10px', borderRadius: 4, color: '#7dd3fc', border: '1px solid rgba(56, 189, 248, 0.3)' }}>
                    <strong>Título Habilitante Oficial:</strong> {fichaImpresion.tituloHabilitante} &bull; {fichaImpresion.enteEmisor}
                  </div>
                )}
                <div>Email: {fichaImpresion.email}</div>
                <div>Tel: {fichaImpresion.telefono}</div>
                <div>Dirección: {fichaImpresion.direccion}</div>
                <div>CP: {fichaImpresion.codigoPostal}</div>
              </div>

              <div style={{ marginTop: 12, textAlign: 'right', fontSize: '0.685rem', opacity: 0.5 }}>
                Emitido: {new Date(fichaImpresion.fechaEmisionReporte).toLocaleString()}
              </div>
            </div>

            <div className="no-print" style={{ display: 'flex', gap: 10, marginTop: 20 }}>
              <button onClick={() => window.print()} className="btn btn-primary" style={{ flex: 1 }}>
                <Printer size={18} />
                Imprimir Credencial
              </button>
              <button onClick={() => setModalImprimirOpen(false)} className="btn btn-secondary">
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL 4: ASIGNAR PROFESOR O ENTRENADOR */}
      {modalAsignarOpen && usuarioSeleccionado && (
        <div className="modal-overlay" onClick={() => setModalAsignarOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3>Asignar Clase / Entrenamiento Particular</h3>
              <button onClick={() => setModalAsignarOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: 16 }}>
              Asignando sesión individual para el socio: <strong>{usuarioSeleccionado.nombre} {usuarioSeleccionado.apellido}</strong> (DNI: {usuarioSeleccionado.dni})
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

            <form onSubmit={handleAsignarInstructor}>
              <div className="input-group">
                <label className="input-label">Tipo de Asignación</label>
                <div style={{ display: 'flex', gap: 12 }}>
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontSize: '0.9rem' }}>
                    <input
                      type="radio"
                      name="tipo"
                      value="profesor"
                      checked={asignarData.tipo === 'profesor'}
                      onChange={() => setAsignarData({ ...asignarData, tipo: 'profesor' })}
                    />
                    Profesor (Clase Técnica Particular)
                  </label>
                  <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer', fontSize: '0.9rem' }}>
                    <input
                      type="radio"
                      name="tipo"
                      value="entrenador"
                      checked={asignarData.tipo === 'entrenador'}
                      onChange={() => setAsignarData({ ...asignarData, tipo: 'entrenador' })}
                    />
                    Entrenador (Preparación Física y Táctica)
                  </label>
                </div>
              </div>

              <div className="input-group">
                <label className="input-label">ID del {asignarData.tipo === 'profesor' ? 'Profesor' : 'Entrenador'} habilitado</label>
                <input
                  type="number"
                  required
                  min="1"
                  className="input-field"
                  value={asignarData.instructorId}
                  onChange={(e) => setAsignarData({ ...asignarData, instructorId: e.target.value })}
                />
                <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem' }}>
                  El profesional debe contar con título habilitante y estar activo en el sistema.
                </small>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="input-group">
                  <label className="input-label">Fecha y Hora</label>
                  <input
                    type="datetime-local"
                    required
                    className="input-field"
                    value={asignarData.fechaHoraInicio}
                    onChange={(e) => setAsignarData({ ...asignarData, fechaHoraInicio: e.target.value })}
                  />
                </div>
                <div className="input-group">
                  <label className="input-label">Duración (minutos)</label>
                  <input
                    type="number"
                    min="30"
                    max="180"
                    required
                    className="input-field"
                    value={asignarData.duracionMinutos}
                    onChange={(e) => setAsignarData({ ...asignarData, duracionMinutos: e.target.value })}
                  />
                </div>
              </div>

              <button type="submit" disabled={submitting} className="btn btn-volt" style={{ width: '100%', marginTop: 10 }}>
                {submitting ? 'Asignando...' : `Confirmar Asignación de ${asignarData.tipo}`}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
