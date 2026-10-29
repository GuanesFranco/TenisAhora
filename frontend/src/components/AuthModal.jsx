import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { X, Lock, Mail, User, Phone, MapPin, Calendar, Award, ShieldAlert, Sparkles, Key } from 'lucide-react';

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
    codigoPostal: 1888,
    fechaDeNacimiento: '1995-01-01',
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

  const fillQuickLogin = (email, password) => {
    setLoginForm({ email, password });
    setIsRegister(false);
    setError('');
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-content" style={{ maxWidth: isRegister ? 560 : 440 }} onClick={(e) => e.stopPropagation()}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{ background: 'rgba(34, 197, 94, 0.15)', padding: 8, borderRadius: 10, color: 'var(--primary)' }}>
              <Lock size={22} />
            </div>
            <h2 style={{ fontSize: '1.3rem' }}>
              {isRegister ? 'Registro Oficial de Socio' : 'Ingresar a Tenis Ahora'}
            </h2>
          </div>
          <button onClick={onClose} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
            <X size={20} />
          </button>
        </div>

        {/* Notificación de acción restringida */}
        {notice && (
          <div style={{
            background: 'rgba(204, 255, 0, 0.1)',
            border: '1px solid rgba(204, 255, 0, 0.3)',
            color: 'var(--accent-volt)',
            padding: '10px 14px',
            borderRadius: 'var(--radius-sm)',
            marginBottom: 16,
            fontSize: '0.85rem',
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
            background: 'rgba(244, 63, 94, 0.15)',
            border: '1px solid rgba(244, 63, 94, 0.4)',
            color: '#f87171',
            padding: '10px 14px',
            borderRadius: 8,
            marginBottom: 16,
            fontSize: '0.85rem'
          }}>
            {error}
          </div>
        )}

        {!isRegister ? (
          <form onSubmit={handleLoginSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: 6 }}>Correo Electrónico</label>
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
              <label style={{ display: 'block', fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: 6 }}>Contraseña</label>
              <input
                type="password"
                required
                className="input"
                placeholder="••••••••"
                value={loginForm.password}
                onChange={(e) => setLoginForm({ ...loginForm, password: e.target.value })}
              />
            </div>

            <button type="submit" disabled={loading} className="btn btn-volt" style={{ width: '100%', marginTop: 6 }}>
              {loading ? 'Validando...' : 'Iniciar Sesión'}
            </button>

            <div style={{ marginTop: 12, textAlign: 'center', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
              ¿Todavía no sos socio del club?{' '}
              <button
                type="button"
                onClick={() => { setIsRegister(true); setError(''); }}
                style={{ background: 'none', border: 'none', color: 'var(--accent-volt)', fontWeight: 600, cursor: 'pointer', textDecoration: 'underline' }}
              >
                Registrate acá
              </button>
            </div>
          </form>
        ) : (
          <form onSubmit={handleRegisterSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Nombre</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Franco"
                  value={registerForm.nombre}
                  onChange={(e) => setRegisterForm({ ...registerForm, nombre: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Apellido</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Guanes"
                  value={registerForm.apellido}
                  onChange={(e) => setRegisterForm({ ...registerForm, apellido: e.target.value })}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>DNI</label>
                <input
                  type="number"
                  required
                  className="input"
                  placeholder="43249050"
                  value={registerForm.dni}
                  onChange={(e) => setRegisterForm({ ...registerForm, dni: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Teléfono</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="11 2345-6789"
                  value={registerForm.telefono}
                  onChange={(e) => setRegisterForm({ ...registerForm, telefono: e.target.value })}
                />
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Correo Electrónico</label>
              <input
                type="email"
                required
                className="input"
                placeholder="franco@correo.com"
                value={registerForm.email}
                onChange={(e) => setRegisterForm({ ...registerForm, email: e.target.value })}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Dirección de Residencia</label>
                <input
                  type="text"
                  required
                  className="input"
                  placeholder="Av. Calchaquí 1200"
                  value={registerForm.direccion}
                  onChange={(e) => setRegisterForm({ ...registerForm, direccion: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Cód. Postal</label>
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
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Fecha de Nacimiento</label>
                <input
                  type="date"
                  required
                  className="input"
                  value={registerForm.fechaDeNacimiento}
                  onChange={(e) => setRegisterForm({ ...registerForm, fechaDeNacimiento: e.target.value })}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Contraseña</label>
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

            <button type="submit" disabled={loading} className="btn btn-volt" style={{ width: '100%', marginTop: 8 }}>
              {loading ? 'Creando socio...' : 'Completar Registro Oficial'}
            </button>

            <div style={{ marginTop: 10, textAlign: 'center', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
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
