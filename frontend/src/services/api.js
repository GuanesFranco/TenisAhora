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

    // Si la respuesta es un archivo blob (PDF)
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/pdf')) {
      return await response.blob();
    }

    const data = await response.json().catch(() => null);

    if (!response.ok) {
      const errorMsg = data?.mensaje || data?.error || data?.title || (data?.errors ? Object.values(data.errors).flat().join(', ') : `Error HTTP ${response.status}`);
      throw new Error(errorMsg);
    }

    return data;
  } catch (err) {
    if (err.message === 'Failed to fetch') {
      throw new Error('No se pudo conectar con el servidor backend en http://localhost:5242. Verifique que la API esté iniciada.');
    }
    throw err;
  }
}

export const api = {
  // 1. Autenticación y Sesión
  auth: {
    login: (credentials) => request('/auth/login', { method: 'POST', body: JSON.stringify(credentials) }),
    register: (userData) => request('/auth/register', { method: 'POST', body: JSON.stringify(userData) }),
    me: () => request('/auth/me'),
  },

  // 2. Gestión de Usuarios y Socios (RF-01 a RF-07)
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

  // 3. Canchas (RF-08 a RF-15)
  canchas: {
    listar: () => request('/canchas'),
    obtenerPorId: (id) => request(`/canchas/${id}`),
    crear: (cancha) => request('/canchas', { method: 'POST', body: JSON.stringify(cancha) }),
    actualizar: (id, cancha) => request(`/canchas/${id}`, { method: 'PUT', body: JSON.stringify(cancha) }),
    eliminar: (id) => request(`/canchas/${id}`, { method: 'DELETE' }),
    disponibilidad: {
      listarPorCancha: (canchaId, params = {}) => {
        const q = new URLSearchParams(params).toString();
        return request(`/canchas/${canchaId}/disponibilidad${q ? '?' + q : ''}`);
      },
      obtenerPorId: (canchaId, id) => request(`/canchas/${canchaId}/disponibilidad/${id}`),
      crear: (canchaId, data) => request(`/canchas/${canchaId}/disponibilidad`, { method: 'POST', body: JSON.stringify({ ...data, canchaId }) }),
      modificar: (canchaId, id, data) => request(`/canchas/${canchaId}/disponibilidad/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    },
  },

  // 4. Reservas y Alquileres de Cancha (RF-24 a RF-27)
  reservas: {
    listar: () => request('/reservas'),
    obtenerPorId: (id) => request(`/reservas/${id}`),
    crear: (reservaData) => request('/reservas', { method: 'POST', body: JSON.stringify(reservaData) }),
    confirmar: (id, pagoData) => request(`/reservas/${id}/confirmar`, { method: 'POST', body: JSON.stringify(pagoData) }),
    modificar: (id, reservaData) => request(`/reservas/${id}`, { method: 'PUT', body: JSON.stringify(reservaData) }),
    cancelar: (id) => request(`/reservas/${id}/cancelar`, { method: 'POST' }),
    finalizar: (id) => request(`/reservas/${id}/finalizar`, { method: 'POST' }),
  },

  // 5. Clases de Tenis (RF-42 a RF-48)
  clases: {
    listar: () => request('/clases'),
    obtenerPorId: (id) => request(`/clases/${id}`),
    crear: (clase) => request('/clases', { method: 'POST', body: JSON.stringify(clase) }),
    modificar: (id, clase) => request(`/clases/${id}`, { method: 'PUT', body: JSON.stringify(clase) }),
    darDeBaja: (id) => request(`/clases/${id}`, { method: 'DELETE' }),
    inscribir: (id, dni) => request(`/clases/${id}/inscribir`, { method: 'POST', body: JSON.stringify({ dni }) }),
    registrarAsistencia: (id, data) => request(`/clases/${id}/asistencia`, { method: 'POST', body: JSON.stringify(data) }),
    asignarParticular: (data) => request('/clases/particular', { method: 'POST', body: JSON.stringify(data) }),
  },

  // 6. Entrenamientos Físicos y Técnicos (RF-49 a RF-54)
  entrenamientos: {
    listar: () => request('/entrenamientos'),
    obtenerPorId: (id) => request(`/entrenamientos/${id}`),
    crear: (entrenamiento) => request('/entrenamientos', { method: 'POST', body: JSON.stringify(entrenamiento) }),
    modificar: (id, entrenamiento) => request(`/entrenamientos/${id}`, { method: 'PUT', body: JSON.stringify(entrenamiento) }),
    darDeBaja: (id) => request(`/entrenamientos/${id}`, { method: 'DELETE' }),
    inscribir: (id, dni) => request(`/entrenamientos/${id}/inscribir`, { method: 'POST', body: JSON.stringify({ dni }) }),
    registrarAsistencia: (id, data) => request(`/entrenamientos/${id}/asistencia`, { method: 'POST', body: JSON.stringify(data) }),
    asignarParticular: (data) => request('/entrenamientos/particular', { method: 'POST', body: JSON.stringify(data) }),
  },

  // 7. Productos y Stock (RF-78 a RF-82)
  productos: {
    listar: () => request('/productos'),
    obtenerPorId: (id) => request(`/productos/${id}`),
    crear: (producto) => request('/productos', { method: 'POST', body: JSON.stringify(producto) }),
    modificar: (id, producto) => request(`/productos/${id}`, { method: 'PUT', body: JSON.stringify(producto) }),
    eliminar: (id) => request(`/productos/${id}`, { method: 'DELETE' }),
  },

  // 8. Profesores y Entrenadores
  profesores: {
    listar: () => request('/profesores'),
    obtenerPorId: (id) => request(`/profesores/${id}`),
    crear: (profesor) => request('/profesores', { method: 'POST', body: JSON.stringify(profesor) }),
    actualizar: (id, profesor) => request(`/profesores/${id}`, { method: 'PUT', body: JSON.stringify(profesor) }),
    darDeBaja: (id) => request(`/profesores/${id}`, { method: 'DELETE' }),
    activar: (id) => request(`/profesores/${id}/activar`, { method: 'PUT' }),
    imprimir: (id) => request(`/profesores/${id}/imprimir`),
  },
  entrenadores: {
    listar: () => request('/entrenadores'),
    obtenerPorId: (id) => request(`/entrenadores/${id}`),
    crear: (entrenador) => request('/entrenadores', { method: 'POST', body: JSON.stringify(entrenador) }),
    actualizar: (id, entrenador) => request(`/entrenadores/${id}`, { method: 'PUT', body: JSON.stringify(entrenador) }),
    darDeBaja: (id) => request(`/entrenadores/${id}`, { method: 'DELETE' }),
    activar: (id) => request(`/entrenadores/${id}/activar`, { method: 'PUT' }),
    imprimir: (id) => request(`/entrenadores/${id}/imprimir`),
  },
  certificaciones: {
    crear: (cert) => request('/CertificacionesDeportivas', { method: 'POST', body: JSON.stringify(cert) }),
    resolver: (id, estado) => request(`/CertificacionesDeportivas/${id}/estado`, { method: 'PUT', body: JSON.stringify({ estadoCertificacion: estado }) }),
  },

  // 9. Torneos Oficiales (RF-28 a RF-35)
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

  // 10. Ligas Regulares (RF-36 a RF-41)
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

  // 11. Reportes Oficiales en PDF (QuestPDF)
  reportes: {
    generarPdf: async (tipoReporte, adminId) => {
      const blob = await request('/reportes', {
        method: 'POST',
        body: JSON.stringify({
          tipoReporte,
          administradorId: adminId,
        }),
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Reporte_${tipoReporte}_${new Date().toISOString().slice(0, 10)}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      return true;
    },
    descargarReporte: async (tipoReporte, adminId) => {
      return await request('/reportes', {
        method: 'POST',
        body: JSON.stringify({
          tipoReporte,
          administradorId: adminId,
        }),
      });
    },
  },
};
