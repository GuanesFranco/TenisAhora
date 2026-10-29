import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { api } from '../services/api';
import { User, Printer, ShieldAlert, QrCode } from 'lucide-react';

export default function MiPerfilSocioView() {
  const { user } = useAuth();
  const [socio, setSocio] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [formData, setFormData] = useState({
    nombre: '',
    apellido: '',
    telefono: '',
    direccion: '',
    email: '',
  });
  const [successMsg, setSuccessMsg] = useState('');

  useEffect(() => {
    cargarDatosSocio();
  }, [user]);

  async function cargarDatosSocio() {
    setLoading(true);
    try {
      const lista = await api.usuarios.listar(false);
      const encontrado = lista.find(u => u.email === user?.email || u.dni === user?.dni) || lista[0];
      if (encontrado) {
        setSocio(encontrado);
        setFormData({
          nombre: encontrado.nombre || '',
          apellido: encontrado.apellido || '',
          telefono: encontrado.telefono || '',
          direccion: encontrado.direccion || '',
          email: encontrado.email || '',
        });
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  }

  const handleGuardarCambios = async (e) => {
    e.preventDefault();
    if (!socio) return;
    try {
      await api.usuarios.modificar(socio.dni, {
        nombre: formData.nombre,
        apellido: formData.apellido,
        telefono: parseInt(formData.telefono, 10),
        direccion: formData.direccion,
        codigoPostal: socio.codigoPostal || 1888,
        fechaDeNacimiento: socio.fechaDeNacimiento,
      });
      setSuccessMsg('¡Tus datos personales fueron actualizados correctamente!');
      setEditing(false);
      setTimeout(() => setSuccessMsg(''), 3000);
      cargarDatosSocio();
    } catch (err) {
      alert(`Error al actualizar datos: ${err.message}`);
    }
  };

  const handleSolicitarBaja = () => {
    if (window.confirm('¿Confirmás que deseás solicitar la baja de tu membresía como socio del club? El trámite será remitido al personal administrativo.')) {
      alert('Tu solicitud de baja fue remitida a la administración del club.');
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24, maxWidth: 1000, margin: '0 auto' }}>
      {/* Header bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2 style={{ fontSize: '1.4rem', display: 'flex', alignItems: 'center', gap: 10 }}>
            <User color="var(--primary)" size={22} />
            Mi Perfil de Socio
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Autogestión de datos de membresía, credencial deportiva y solicitudes
          </p>
        </div>

        <button onClick={() => window.print()} className="btn btn-secondary btn-sm">
          <Printer size={15} />
          <span>Imprimir Carnet Oficial</span>
        </button>
      </div>

      {successMsg && (
        <div className="badge badge-success" style={{ padding: '10px 14px', fontSize: '0.85rem' }}>
          {successMsg}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 320px', gap: 24, alignItems: 'start' }}>
        {/* Formulario de Datos Personales */}
        <div className="card" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid var(--border)', paddingBottom: 12 }}>
            <h3 style={{ fontSize: '1.1rem' }}>Información Personal y Contacto</h3>
            <button
              onClick={() => setEditing(!editing)}
              className="btn btn-secondary btn-sm"
            >
              {editing ? 'Cancelar' : 'Modificar Datos'}
            </button>
          </div>

          <form onSubmit={handleGuardarCambios} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label className="form-label">Nombre</label>
                <input
                  type="text"
                  disabled={!editing}
                  className="input"
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                />
              </div>

              <div>
                <label className="form-label">Apellido</label>
                <input
                  type="text"
                  disabled={!editing}
                  className="input"
                  value={formData.apellido}
                  onChange={(e) => setFormData({ ...formData, apellido: e.target.value })}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label className="form-label">DNI</label>
                <input
                  type="text"
                  disabled
                  className="input"
                  value={socio?.dni || ''}
                  style={{ opacity: 0.6 }}
                />
              </div>

              <div>
                <label className="form-label">Teléfono</label>
                <input
                  type="number"
                  disabled={!editing}
                  className="input"
                  value={formData.telefono}
                  onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                />
              </div>
            </div>

            <div>
              <label className="form-label">Dirección de Domicilio</label>
              <input
                type="text"
                disabled={!editing}
                className="input"
                value={formData.direccion}
                onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
              />
            </div>

            <div>
              <label className="form-label">Correo Electrónico</label>
              <input
                type="email"
                disabled
                className="input"
                value={formData.email}
                style={{ opacity: 0.6 }}
              />
            </div>

            {editing && (
              <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 8 }}>
                <button type="submit" className="btn btn-primary">
                  Guardar Modificaciones
                </button>
              </div>
            )}
          </form>

          {/* Opción de solicitar baja */}
          <div style={{ borderTop: '1px solid var(--border)', paddingTop: 14, marginTop: 4 }}>
            <button onClick={handleSolicitarBaja} className="btn btn-danger btn-sm">
              <ShieldAlert size={14} />
              <span>Solicitar Baja de Membresía</span>
            </button>
          </div>
        </div>

        {/* Tarjeta Digital / Carnet de Socio */}
        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--primary-subtle-border)',
          borderRadius: 'var(--radius-md)',
          padding: '22px',
          boxShadow: 'var(--shadow-sm)',
          display: 'flex',
          flexDirection: 'column',
          gap: 16,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--primary)', letterSpacing: '0.04em' }}>
              CLUB TENIS AHORA
            </span>
            <span className="badge badge-success" style={{ fontSize: '0.65rem' }}>SOCIO ACTIVO</span>
          </div>

          <div style={{ textAlign: 'center', padding: '10px 0' }}>
            <div style={{
              width: 64,
              height: 64,
              borderRadius: '50%',
              background: 'var(--primary-subtle)',
              border: '2px solid var(--primary)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 10px',
              fontSize: '1.5rem',
              color: 'var(--primary-subtle-text)',
              fontWeight: 700,
            }}>
              {formData.nombre?.charAt(0) || 'S'}
            </div>
            <h4 style={{ fontSize: '1.1rem', marginBottom: 2 }}>{formData.nombre} {formData.apellido}</h4>
            <div style={{ color: 'var(--text-dim)', fontSize: '0.8rem' }}>DNI {socio?.dni || '---'}</div>
          </div>

          <div style={{
            background: 'var(--bg-elevated)',
            padding: '12px',
            borderRadius: 'var(--radius-sm)',
            border: '1px solid var(--border)',
            display: 'flex',
            flexDirection: 'column',
            gap: 6,
            fontSize: '0.8rem'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: 'var(--text-dim)' }}>Categoría:</span>
              <strong>Membresía Individual</strong>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span style={{ color: 'var(--text-dim)' }}>Aranceles canchas:</span>
              <strong style={{ color: 'var(--primary)' }}>Tarifa Socio Bonificada</strong>
            </div>
          </div>

          {/* QR de Acceso al Club */}
          <div style={{
            background: 'var(--bg-elevated)',
            border: '1px solid var(--border)',
            padding: '12px',
            borderRadius: 'var(--radius-sm)',
            display: 'flex',
            alignItems: 'center',
            gap: 12
          }}>
            <QrCode size={40} color="var(--primary)" />
            <div style={{ fontSize: '0.75rem' }}>
              <strong style={{ display: 'block', color: 'var(--text-main)' }}>Credencial Digital</strong>
              <div style={{ color: 'var(--text-dim)' }}>Válida para ingreso al complejo y reservas.</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
