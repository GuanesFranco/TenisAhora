import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { api } from '../services/api';
import { User, Printer, ShieldAlert, QrCode } from 'lucide-react';

export default function MiPerfilSocioView() {
  const { user, logout } = useAuth();
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
      const encontrado = lista.find(u =>
        (user?.id && String(u.id) === String(user.id)) ||
        (user?.email && u.email?.toLowerCase() === user.email.toLowerCase()) ||
        (user?.dni && u.dni === user.dni)
      ) || (user?.email ? null : lista[0]);

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
      const telLimpio = String(formData.telefono || socio.telefono || '1144556677').replace(/\D/g, '').slice(0, 15);
      if (telLimpio.length < 8) {
        alert('El número de teléfono debe tener entre 8 y 15 dígitos numéricos.');
        return;
      }
      const nombreLimpio = (formData.nombre || socio.nombre).trim();
      const apellidoLimpio = (formData.apellido || socio.apellido).trim();
      if (nombreLimpio.length > 50 || apellidoLimpio.length > 50) {
        alert('El nombre y apellido no pueden superar los 50 caracteres.');
        return;
      }
      if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(nombreLimpio) || !/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(apellidoLimpio)) {
        alert('El nombre y apellido solo pueden contener caracteres alfabéticos.');
        return;
      }
      const emailLimpio = (formData.email || socio.email || user?.email || '').trim();

      let fechaNac = socio.fechaDeNacimiento;
      if (!fechaNac || isNaN(new Date(fechaNac).getTime())) {
        fechaNac = '1995-01-01T00:00:00.000Z';
      } else {
        fechaNac = new Date(fechaNac).toISOString();
      }

      await api.usuarios.modificar(socio.dni, {
        nombre: (formData.nombre || socio.nombre).trim(),
        apellido: (formData.apellido || socio.apellido).trim(),
        email: emailLimpio,
        telefono: telLimpio,
        direccion: (formData.direccion || socio.direccion || 'Calle Club 123').trim(),
        codigoPostal: parseInt(socio.codigoPostal, 10) || 1888,
        fechaDeNacimiento: fechaNac,
        estadoMembresia: socio.estadoMembresia !== undefined ? socio.estadoMembresia : 0
      });
      setSuccessMsg('¡Tus datos personales fueron actualizados correctamente!');
      setEditing(false);
      setTimeout(() => setSuccessMsg(''), 3000);
      cargarDatosSocio();
    } catch (err) {
      alert(`Error al actualizar datos: ${err.message}`);
    }
  };

  const handleSolicitarBaja = async () => {
    if (!socio) return;
    const confirma = window.confirm(
      '¿Confirmás que deseás dar de baja tu membresía y cuenta de socio en el Club Tenis Ahora?\n\n' +
      '• Tu cuenta quedará inhabilitada de inmediato en el sistema.\n' +
      '• Se cerrará tu sesión activa y no podrás volver a ingresar hasta que la administración reactive tu cuenta.\n\n' +
      '¿Deseás confirmar la baja de tu cuenta?'
    );
    if (!confirma) return;

    try {
      await api.usuarios.eliminar(socio.dni);
      alert('Tu cuenta de socio fue dada de baja correctamente. Tu sesión se cerrará a continuación.');
      logout();
    } catch (err) {
      alert(`Error al procesar la baja de la cuenta: ${err.message}`);
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
                  maxLength={50}
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
                  maxLength={50}
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
                  type="text"
                  inputMode="numeric"
                  disabled={!editing}
                  className="input"
                  placeholder="Solo números"
                  value={formData.telefono}
                  maxLength={15}
                  onChange={(e) => setFormData({ ...formData, telefono: e.target.value.replace(/\D/g, '').slice(0, 15) })}
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
            <button onClick={handleSolicitarBaja} className="btn btn-danger btn-sm" title="Dar de baja mi cuenta y membresía">
              <ShieldAlert size={14} />
              <span>Dar de Baja Mi Membresía</span>
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
            <span className={`badge ${socio?.activo ? 'badge-success' : 'badge-danger'}`} style={{ fontSize: '0.65rem' }}>
              {socio?.activo ? 'SOCIO ACTIVO' : 'SOCIO INACTIVO (BAJA)'}
            </span>
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
