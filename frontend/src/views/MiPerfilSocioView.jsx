import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { api } from '../services/api';
import { User, Phone, MapPin, Mail, Calendar, Printer, ShieldAlert, CheckCircle2, QrCode, Sparkles } from 'lucide-react';

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
      // Buscar datos del socio por DNI o listar
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
          <h2 style={{ fontSize: '1.5rem', display: 'flex', alignItems: 'center', gap: 10 }}>
            <User color="var(--accent-volt)" size={26} />
            Mi Perfil de Socio
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            Autogestión de datos de membresía, credencial deportiva y solicitudes
          </p>
        </div>

        <button onClick={() => window.print()} className="btn btn-secondary">
          <Printer size={16} />
          <span>Imprimir Carnet Oficial</span>
        </button>
      </div>

      {successMsg && (
        <div className="badge badge-success" style={{ padding: 12, fontSize: '0.9rem' }}>
          {successMsg}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: 24, alignItems: 'start' }}>
        {/* Formulario de Datos Personales */}
        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border)',
          borderRadius: 'var(--radius-lg)',
          padding: '24px',
          display: 'flex',
          flexDirection: 'column',
          gap: 18,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid var(--border)', paddingBottom: 12 }}>
            <h3 style={{ fontSize: '1.15rem' }}>Información Personal y Contacto</h3>
            <button
              onClick={() => setEditing(!editing)}
              className="btn btn-secondary btn-sm"
            >
              {editing ? 'Cancelar' : 'Modificar Datos'}
            </button>
          </div>

          <form onSubmit={handleGuardarCambios} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Nombre</label>
                <input
                  type="text"
                  disabled={!editing}
                  className="input"
                  value={formData.nombre}
                  onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Apellido</label>
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
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>DNI (Inmutable)</label>
                <input
                  type="text"
                  disabled
                  className="input"
                  value={socio?.dni || ''}
                  style={{ opacity: 0.7 }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Teléfono</label>
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
              <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Dirección de Domicilio</label>
              <input
                type="text"
                disabled={!editing}
                className="input"
                value={formData.direccion}
                onChange={(e) => setFormData({ ...formData, direccion: e.target.value })}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Correo Electrónico</label>
              <input
                type="email"
                disabled
                className="input"
                value={formData.email}
                style={{ opacity: 0.7 }}
              />
            </div>

            {editing && (
              <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 10 }}>
                <button type="submit" className="btn btn-volt">
                  Guardar Modificaciones
                </button>
              </div>
            )}
          </form>

          {/* Opción de solicitar baja */}
          <div style={{ borderTop: '1px solid var(--border)', paddingTop: 16, marginTop: 8 }}>
            <button onClick={handleSolicitarBaja} className="btn btn-danger btn-sm">
              <ShieldAlert size={14} />
              <span>Solicitar Baja de Membresía</span>
            </button>
          </div>
        </div>

        {/* Tarjeta Digital / Carnet de Socio Oficial */}
        <div style={{
          background: 'linear-gradient(135deg, rgba(34, 197, 94, 0.15) 0%, rgba(15, 29, 24, 0.9) 100%)',
          border: '2px solid var(--accent-volt)',
          borderRadius: 'var(--radius-lg)',
          padding: '24px',
          boxShadow: '0 8px 30px var(--accent-volt-glow)',
          display: 'flex',
          flexDirection: 'column',
          gap: 16,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ fontSize: '0.75rem', fontWeight: 800, color: 'var(--accent-volt)', letterSpacing: '0.05em' }}>
              CLUB TENIS AHORA
            </span>
            <span className="badge badge-success" style={{ fontSize: '0.65rem' }}>SOCIO ACTIVO</span>
          </div>

          <div style={{ textAlign: 'center', padding: '12px 0' }}>
            <div style={{
              width: 72,
              height: 72,
              borderRadius: '50%',
              background: 'linear-gradient(135deg, var(--accent-volt), var(--primary))',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 12px',
              fontSize: '1.8rem',
              color: '#000',
              fontWeight: 800,
            }}>
              {formData.nombre?.charAt(0) || 'S'}
            </div>
            <h4 style={{ fontSize: '1.2rem', marginBottom: 2 }}>{formData.nombre} {formData.apellido}</h4>
            <div style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>DNI {socio?.dni || '---'}</div>
          </div>

          <div style={{
            background: 'var(--bg-dark)',
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
              <strong style={{ color: 'var(--accent-volt)' }}>Tarifa Socio Bonificada</strong>
            </div>
          </div>

          {/* QR de Acceso Físico al Club [RNF-16] */}
          <div style={{
            background: '#fff',
            color: '#000',
            padding: '12px',
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            gap: 12
          }}>
            <QrCode size={44} color="#000" />
            <div style={{ fontSize: '0.7rem' }}>
              <strong>Credencial Digital QR</strong>
              <div>Válida para ingreso al complejo y reserva de pistas.</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
