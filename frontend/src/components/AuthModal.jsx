import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { X, Lock, ShieldAlert } from 'lucide-react';

export default function AuthModal({ isOpen, onClose, notice = '' }) {
  const { login, register } = useAuth();
  const [isRegister, setIsRegister] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Form states
  const [loginForm, setLoginForm] = useState({ email: '', password: '' });
  const [registerForm, setRegisterForm] = useState({
    nombre: '',
    apellido: '',
    dni: '',
    email: '',
    telefono: '',
    direccion: '',
    codigoPostal: '',
    fechaDeNacimiento: '',
    password: '',
  });

  if (!isOpen) return null;

  const handleLoginSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(loginForm.email, loginForm.password);
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRegisterSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register({
        ...registerForm,
        dni: parseInt(registerForm.dni, 10),
        codigoPostal: parseInt(registerForm.codigoPostal, 10),
        fechaDeNacimiento: new Date(registerForm.fechaDeNacimiento).toISOString(),
      });
      onClose();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-content" style={{ maxWidth: isRegister ? 520 : 420 }} onClick={(e) => e.stopPropagation()}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{ background: 'var(--primary-subtle)', padding: 7, borderRadius: 8, color: 'var(--primary)' }}>
              <Lock size={18} />
            </div>
            <h2 style={{ fontSize: '1.2rem', fontWeight: 600 }}>
              {isRegister ? 'Registro de Socio' : 'Ingresar a Tenis Ahora'}
            </h2>
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
            <X size={18} />
          </button>
        </div>

        {/* Notificación de acción restringida */}
        {notice && (
          <div style={{
            background: 'var(--primary-subtle)',
            border: '1px solid var(--primary-subtle-border)',
            color: 'var(--primary-subtle-text)',
            padding: '10px 12px',
            borderRadius: 'var(--radius-sm)',
            marginBottom: 14,
            fontSize: '0.8125rem',
            display: 'flex',
            alignItems: 'center',
            gap: 8,
          }}>
            <ShieldAlert size={16} />
            <span>{notice}</span>
          </div>
        )}

        {error && (
          <div style={{
            background: 'var(--accent-red-bg)',
            border: '1px solid var(--accent-red-border)',
            color: 'var(--accent-red)',
            padding: '10px 12px',
            borderRadius: 'var(--radius-sm)',
            marginBottom: 14,
            fontSize: '0.8125rem'
          }}>
            {error}
          </div>
        )}

        {!isRegister ? (
          <form onSubmit={handleLoginSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: 5 }}>Correo Electrónico</label>
              <input
                type="email"
                required
                className="input"
                placeholder="ejemplo@tenisahora.com"
                value={loginForm.email}
                onChange={(e) => setLoginForm({ ...loginForm, email: e.target.value })}
              />
            </div>
            <div>
              <label style={{ display: 'block', fontSize: '0.8125rem', color: 'var(--text-muted)', marginBottom: 5 }}>Contraseña</label>
              <input
                type="password"
                required
                className="input"
                placeholder="••••••••"
                value={loginForm.password}
                onChange={(e) => setLoginForm({ ...loginForm, password: e.target.value })}
              />
            </div>

            <button type="submit" disabled={loading} className="btn btn-primary" style={{ width: '100%', marginTop: 4 }}>
              {loading ? 'Validando...' : 'Iniciar Sesión'}
            </button>

            <div style={{ marginTop: 10, textAlign: 'center', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
              ¿Todavía no sos socio del club?{' '}
              <button
                type="button"
                onClick={() => { setIsRegister(true); setError(''); }}
                style={{ background: 'none', border: 'none', color: 'var(--primary)', fontWeight: 600, cursor: 'pointer', textDecoration: 'underline' }}
              >
                Registrate acá
              </button>
            </div>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Nombre</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Ingresá tu nombre"
                  value={registerForm.nombre}
                  onChange={(e) => setRegisterForm({ ...registerForm, nombre: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Apellido</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Ingresá tu apellido"
                  value={registerForm.apellido}
                  onChange={(e) => setRegisterForm({ ...registerForm, apellido: e.target.value })}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>DNI</label>
                <input
                  type="text" inputMode="numeric" maxLength={8} required className="input" placeholder="Ej. 12345678" value={registerForm.dni} onChange={(e) => setRegisterForm({ ...registerForm, dni: e.target.value.replace(/\D/g, '').slice(0, 8) })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Teléfono</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Ej. 11 1234-5678"
                  value={registerForm.telefono}
                  onChange={(e) => setRegisterForm({ ...registerForm, telefono: e.target.value })}
                />
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Correo Electrónico</label>
              <input
                type="email"
                required
                className="input"
                placeholder="correo@ejemplo.com"
                value={registerForm.email}
                onChange={(e) => setRegisterForm({ ...registerForm, email: e.target.value })}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Dirección</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Calle y número"
                  value={registerForm.direccion}
                  onChange={(e) => setRegisterForm({ ...registerForm, direccion: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Cód. Postal</label>
                <input
                  type="number"
                  required
                  className="input"
                  value={registerForm.codigoPostal}
                  onChange={(e) => setRegisterForm({ ...registerForm, codigoPostal: e.target.value })}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Fecha de Nacimiento</label>
                <input
                  type="date"
                  required
                  className="input"
                  value={registerForm.fechaDeNacimiento}
                  onChange={(e) => setRegisterForm({ ...registerForm, fechaDeNacimiento: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: 4 }}>Contraseña</label>
                <input
                  type="password"
                  required
                  className="input"
                  placeholder="Mínimo 6 caracteres"
                  value={registerForm.password}
                  onChange={(e) => setRegisterForm({ ...registerForm, password: e.target.value })}
                />
              </div>
            </div>

            <button type="submit" disabled={loading} className="btn btn-primary" style={{ width: '100%', marginTop: 6 }}>
              {loading ? 'Creando socio...' : 'Completar Registro'}
            </button>

            <div style={{ marginTop: 8, textAlign: 'center', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
              ¿Ya estás registrado?{' '}
              <button
                type="button"
                onClick={() => { setIsRegister(false); setError(''); }}
                style={{ background: 'none', border: 'none', color: 'var(--primary)', fontWeight: 600, cursor: 'pointer', textDecoration: 'underline' }}
              >
                Iniciá sesión acá
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
