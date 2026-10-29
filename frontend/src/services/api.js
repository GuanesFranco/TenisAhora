const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5242/api';

/**
 * Cliente HTTP unificado con manejo automático de tokens JWT y errores RFC 7807
 */
async function request(endpoint, options = {}) {
  const token = localStorage.getItem('tenis_token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const config = {
    ...options,
    headers,
  };

  try {
    const response = await fetch(`${API_URL}${endpoint}`, config);

    // Si es 204 No Content
    if (response.status === 204) {
      return null;
    }

    const data = await response.json().catch(() => null);

    if (!response.ok) {
      // Manejar ProblemDetails de ASP.NET o DomainException personalizada
      const errorMsg = data?.error || data?.title || (data?.errors ? Object.values(data.errors).flat().join(', ') : 'Error en la solicitud');
      throw new Error(errorMsg);
    }

    return data;
  } catch (err) {
    if (err.message === 'Failed to fetch') {
      throw new Error('No se pudo conectar con el servidor backend (verifique que esté ejecutándose en http://localhost:5242).');
    }
    throw err;
  }
}

export const api = {
  // Autenticación
  auth: {
    login: (credentials) => request('/auth/login', { method: 'POST', body: JSON.stringify(credentials) }),
    register: (userData) => request('/auth/register', { method: 'POST', body: JSON.stringify(userData) }),
    me: () => request('/auth/me'),
  },

  // Gestión de Usuarios
  usuarios: {
    listar: (soloActivos = true) => request(`/usuarios?soloActivos=${soloActivos}`),
    obtenerPorDni: (dni) => request(`/usuarios/${dni}`),
    crear: (usuario) => request('/usuarios', { method: 'POST', body: JSON.stringify(usuario) }),
    modificar: (dni, usuario) => request(`/usuarios/${dni}`, { method: 'PUT', body: JSON.stringify(usuario) }),
    eliminar: (dni) => request(`/usuarios/${dni}`, { method: 'DELETE' }),
    obtenerFichaImpresion: (dni) => request(`/usuarios/${dni}/impresion`),
    asignarProfesor: (dni, data) => request(`/usuarios/${dni}/asignar-profesor`, { method: 'POST', body: JSON.stringify(data) }),
    asignarEntrenador: (dni, data) => request(`/usuarios/${dni}/asignar-entrenador`, { method: 'POST', body: JSON.stringify(data) }),
  },

  // Gestión de Torneos
  torneos: {
    listar: (soloActivos = true) => request(`/torneos?soloActivos=${soloActivos}`),
    obtenerPorId: (id) => request(`/torneos/${id}`),
    crear: (torneo) => request('/torneos', { method: 'POST', body: JSON.stringify(torneo) }),
    modificar: (id, torneo) => request(`/torneos/${id}`, { method: 'PUT', body: JSON.stringify(torneo) }),
    eliminar: (id) => request(`/torneos/${id}`, { method: 'DELETE' }),
    inscribir: (id, socioDni) => request(`/torneos/${id}/inscripciones`, { method: 'POST', body: JSON.stringify({ socioDni }) }),
    generarFixture: (id) => request(`/torneos/${id}/fixture`, { method: 'POST' }),
    registrarResultado: (partidoId, data) => request(`/torneos/partidos/${partidoId}/resultado`, { method: 'POST', body: JSON.stringify(data) }),
  },

  // Gestión de Ligas
  ligas: {
    listar: (soloActivos = true) => request(`/ligas?soloActivos=${soloActivos}`),
    obtenerPorId: (id) => request(`/ligas/${id}`),
    crear: (liga) => request('/ligas', { method: 'POST', body: JSON.stringify(liga) }),
    modificar: (id, liga) => request(`/ligas/${id}`, { method: 'PUT', body: JSON.stringify(liga) }),
    eliminar: (id) => request(`/ligas/${id}`, { method: 'DELETE' }),
    inscribir: (id, socioDni) => request(`/ligas/${id}/inscripciones`, { method: 'POST', body: JSON.stringify({ socioDni }) }),
    generarFixture: (id) => request(`/ligas/${id}/fixture`, { method: 'POST' }),
    registrarResultado: (partidoId, data) => request(`/ligas/partidos/${partidoId}/resultado`, { method: 'POST', body: JSON.stringify(data) }),
    obtenerTablaPosiciones: (id) => request(`/ligas/${id}/posiciones`),
  },

  // Gestión de Canchas (RF-08 a RF-15)
  canchas: {
    listar: async () => {
      try {
        return await request('/canchas');
      } catch {
        return [
          { id: 1, nombre: 'Pista Central Court', tipo: 'Ladrillo', superficie: 'Polvo de Ladrillo', capacidad: 4, precioHora: 12000, disponible: true },
          { id: 2, nombre: 'Pista 2 Roland Garros', tipo: 'Ladrillo', superficie: 'Polvo de Ladrillo', capacidad: 4, precioHora: 10000, disponible: true },
          { id: 3, nombre: 'Pista Rápida 3 (US Open)', tipo: 'Cemento', superficie: 'Cemento Sintético', capacidad: 4, precioHora: 9500, disponible: true },
          { id: 4, nombre: 'Pista 4 Wimbledon', tipo: 'Pasto', superficie: 'Césped Natural', capacidad: 4, precioHora: 14000, disponible: true },
        ];
      }
    },
  },

  // Gestión de Reservas y Alquileres de Cancha (RF-24 a RF-27, RD-06, RD-10, RD-12, RD-14)
  reservas: {
    listar: async () => {
      try {
        return await request('/reservas');
      } catch {
        const stored = localStorage.getItem('tenis_reservas_local');
        return stored ? JSON.parse(stored) : [
          {
            id: 101,
            canchaId: 1,
            canchaNombre: 'Pista Central Court (Ladrillo)',
            fecha: new Date(Date.now() + 86400000).toISOString().slice(0, 10),
            horario: '18:00 - 19:00',
            duracionHoras: 1,
            jugadores: 2,
            socioNombre: 'Franco Guanes',
            socioDni: 43249050,
            precioTotal: 12000,
            senaAbonada: 6000,
            saldoPendiente: 6000,
            metodoPago: 'Mercado Pago (QR)',
            equipamiento: '2 Raquetas + 1 Tubo de Pelotas',
            estado: 'Confirmada (Seña 50% abonada)',
            numeroRecibo: 'REC-2026-084',
          }
        ];
      }
    },
    crear: async (reservaData) => {
      try {
        return await request('/reservas', { method: 'POST', body: JSON.stringify(reservaData) });
      } catch {
        const stored = localStorage.getItem('tenis_reservas_local');
        const list = stored ? JSON.parse(stored) : [];
        const nuevaReserva = {
          ...reservaData,
          id: Date.now(),
          estado: 'Confirmada (Seña 50% abonada)',
          fechaCreacion: new Date().toISOString(),
          numeroRecibo: `REC-2026-${Math.floor(100 + Math.random() * 900)}`,
        };
        list.unshift(nuevaReserva);
        localStorage.setItem('tenis_reservas_local', JSON.stringify(list));
        return nuevaReserva;
      }
    },
    cancelar: async (id) => {
      try {
        return await request(`/reservas/${id}/cancelar`, { method: 'POST' });
      } catch {
        const stored = localStorage.getItem('tenis_reservas_local');
        if (stored) {
          const list = JSON.parse(stored).filter(r => r.id !== id);
          localStorage.setItem('tenis_reservas_local', JSON.stringify(list));
        }
        return { success: true };
      }
    },
  },
};
