import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import {
  ShoppingBag,
  Package,
  Plus,
  Trash2,
  Edit2,
  AlertTriangle,
  CheckCircle2,
  X,
  Search,
  Layers,
  Sparkles
} from 'lucide-react';

export default function ProductosView() {
  const { user } = useAuth();
  const isAdmin = user?.rol === 'Administrador';

  const [productos, setProductos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [search, setSearch] = useState('');
  const [filtroStock, setFiltroStock] = useState('todos');

  // Modal Crear / Editar
  const [modalOpen, setModalOpen] = useState(false);
  const [editandoId, setEditandoId] = useState(null);
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [precio, setPrecio] = useState('');
  const [stockId, setStockId] = useState(1); // 1 = Pelotas, 2 = Raquetas, 3 = Redes

  useEffect(() => {
    cargarProductos();
  }, []);

  async function cargarProductos() {
    setLoading(true);
    setError('');
    try {
      const data = await api.productos.listar();
      setProductos(data || []);
    } catch (err) {
      setError(err.message || 'Error cargando productos.');
    } finally {
      setLoading(false);
    }
  }

  function mostrarMensaje(msg) {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 5000);
  }

  function abrirModalCrear() {
    setEditandoId(null);
    setNombre('');
    setDescripcion('');
    setPrecio('');
    setStockId(1);
    setModalOpen(true);
  }

  function abrirModalEditar(p) {
    setEditandoId(p.id);
    setNombre(p.nombre);
    setDescripcion(p.descripcion);
    setPrecio(p.precio.toString());
    setStockId(p.stockId || 1);
    setModalOpen(true);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    try {
      setError('');
      if (editandoId) {
        await api.productos.modificar(editandoId, {
          nombre,
          descripcion,
          precio: parseFloat(precio),
        });
        mostrarMensaje('Producto actualizado correctamente.');
      } else {
        await api.productos.crear({
          nombre,
          descripcion,
          precio: parseFloat(precio),
          stockId: parseInt(stockId, 10),
        });
        mostrarMensaje('Producto incorporado al catálogo exitosamente.');
      }
      setModalOpen(false);
      cargarProductos();
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleEliminar(id) {
    if (!window.confirm('¿Confirma que desea dar de baja este producto del catálogo?')) return;
    try {
      await api.productos.eliminar(id);
      mostrarMensaje('Producto dado de baja correctamente.');
      cargarProductos();
    } catch (err) {
      setError(err.message);
    }
  }

  // Normalizador para ignorar tildes/acentos y mayúsculas
  const normalizarTexto = (texto) =>
    (texto || '')
      .toString()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase();

  const productosFiltrados = productos.filter((p) => {
    const searchNorm = normalizarTexto(search);
    const matchSearch =
      normalizarTexto(p.nombre).includes(searchNorm) ||
      normalizarTexto(p.descripcion).includes(searchNorm);
    const matchStock = filtroStock === 'todos' ||
      (filtroStock === 'pelotas' && p.stockId === 1) ||
      (filtroStock === 'raquetas' && p.stockId === 2) ||
      (filtroStock === 'redes' && p.stockId === 3);
    return matchSearch && matchStock;
  });

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 16,
        borderBottom: '1px solid var(--border)',
        paddingBottom: 20
      }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 4 }}>
            <ShoppingBag size={26} color="var(--accent-volt)" />
            <h1 style={{ fontSize: '2rem' }}>Catálogo de Productos y Equipamiento</h1>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            {isAdmin
              ? 'Administración de stock disponible, precios e inventario para reservas y venta oficial.'
              : 'Equipamiento reglamentario disponible en el club (raquetas, pelotas de torneo y redes).'}
          </p>
        </div>

        {isAdmin && (
          <button onClick={abrirModalCrear} className="btn btn-volt">
            <Plus size={16} />
            Nuevo Producto
          </button>
        )}
      </div>

      {/* Alertas */}
      {error && (
        <div style={{
          background: 'rgba(239, 68, 68, 0.1)',
          border: '1px solid rgba(239, 68, 68, 0.3)',
          color: '#f87171',
          padding: '12px 16px',
          borderRadius: 'var(--radius-md)',
          display: 'flex',
          alignItems: 'center',
          gap: 10
        }}>
          <AlertTriangle size={18} />
          <span>{error}</span>
        </div>
      )}

      {successMsg && (
        <div style={{
          background: 'rgba(34, 197, 94, 0.1)',
          border: '1px solid rgba(34, 197, 94, 0.3)',
          color: 'var(--accent-volt)',
          padding: '12px 16px',
          borderRadius: 'var(--radius-md)',
          display: 'flex',
          alignItems: 'center',
          gap: 10
        }}>
          <CheckCircle2 size={18} />
          <span>{successMsg}</span>
        </div>
      )}

      {/* Filtros y Búsqueda */}
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
        <div style={{ position: 'relative', flex: 1, minWidth: 260 }}>
          <Search size={16} color="var(--text-dim)" style={{ position: 'absolute', left: 14, top: 13 }} />
          <input
            type="text"
            placeholder="Buscar por marca, modelo o descripción..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="form-control"
            style={{ paddingLeft: 40 }}
          />
        </div>

        <div style={{ display: 'flex', gap: 6 }}>
          {[
            { id: 'todos', label: 'Todo' },
            { id: 'pelotas', label: 'Pelotas' },
            { id: 'raquetas', label: 'Raquetas' },
            { id: 'redes', label: 'Redes' },
          ].map((cat) => (
            <button
              key={cat.id}
              onClick={() => setFiltroStock(cat.id)}
              style={{
                padding: '8px 14px',
                borderRadius: 'var(--radius-sm)',
                background: filtroStock === cat.id ? 'var(--primary)' : 'var(--bg-elevated)',
                color: filtroStock === cat.id ? '#fff' : 'var(--text-muted)',
                border: '1px solid var(--border)',
                fontWeight: 600,
                fontSize: '0.825rem',
                cursor: 'pointer'
              }}
            >
              {cat.label}
            </button>
          ))}
        </div>
      </div>

      {/* Grid de Productos */}
      {loading ? (
        <div style={{ padding: 60, textAlign: 'center', color: 'var(--text-dim)' }}>
          Cargando inventario del predio...
        </div>
      ) : productosFiltrados.length === 0 ? (
        <div className="card" style={{ padding: 48, textAlign: 'center' }}>
          <Package size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
          <h3 style={{ marginBottom: 8 }}>No se encontraron productos</h3>
          <p style={{ color: 'var(--text-muted)' }}>
            Intente con otro término de búsqueda o filtre por otra categoría.
          </p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 20 }}>
          {productosFiltrados.map((prod) => (
            <div key={prod.id} className="card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 12 }}>
                  <span className="badge badge-success">
                    {prod.tipoEquipamiento || 'Equipamiento'}
                  </span>
                  <span style={{
                    fontSize: '0.75rem',
                    fontWeight: 700,
                    color: prod.stockActual > 0 ? 'var(--accent-volt)' : '#f87171',
                    background: 'var(--bg-elevated)',
                    padding: '4px 8px',
                    borderRadius: 6
                  }}>
                    {prod.stockActual > 0 ? `${prod.stockActual} en stock` : 'Sin stock'}
                  </span>
                </div>

                <h3 style={{ fontSize: '1.25rem', marginBottom: 8 }}>{prod.nombre}</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', lineHeight: 1.5, marginBottom: 16 }}>
                  {prod.descripcion || 'Sin descripción detallada.'}
                </p>
              </div>

              <div>
                <div style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'baseline',
                  borderTop: '1px solid var(--border)',
                  paddingTop: 14,
                  marginBottom: 12
                }}>
                  <span style={{ color: 'var(--text-dim)', fontSize: '0.85rem' }}>Precio Oficial</span>
                  <span style={{ fontSize: '1.4rem', fontWeight: 800, color: 'var(--accent-volt)' }}>
                    ${prod.precio?.toLocaleString('es-AR')}
                  </span>
                </div>

                {isAdmin && (
                  <div style={{ display: 'flex', gap: 8 }}>
                    <button
                      onClick={() => abrirModalEditar(prod)}
                      className="btn btn-secondary"
                      style={{ flex: 1 }}
                    >
                      <Edit2 size={15} />
                      Modificar
                    </button>
                    <button
                      onClick={() => handleEliminar(prod.id)}
                      className="btn btn-secondary"
                      style={{ color: '#f87171' }}
                      title="Eliminar producto"
                    >
                      <Trash2 size={15} />
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal Crear / Editar */}
      {modalOpen && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 9999,
          padding: 20
        }}>
          <div className="card" style={{ maxWidth: 480, width: '100%', position: 'relative' }}>
            <button
              onClick={() => setModalOpen(false)}
              style={{ position: 'absolute', top: 16, right: 16, background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ marginBottom: 16, display: 'flex', alignItems: 'center', gap: 10 }}>
              <Package size={22} color="var(--accent-volt)" />
              {editandoId ? 'Modificar Producto' : 'Nuevo Producto al Catálogo'}
            </h3>

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <label className="form-label">Nombre / Modelo del Producto</label>
                <input
                  type="text"
                  required
                  placeholder="Ej: Tubo de Pelotas Head Tour (x3)"
                  value={nombre}
                  onChange={(e) => setNombre(e.target.value)}
                  className="form-control"
                />
              </div>

              <div>
                <label className="form-label">Descripción Técnica</label>
                <textarea
                  rows="3"
                  placeholder="Especificaciones del producto..."
                  value={descripcion}
                  onChange={(e) => setDescripcion(e.target.value)}
                  className="form-control"
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Precio ($ ARS)</label>
                  <input
                    type="number"
                    step="0.01"
                    min="1"
                    required
                    placeholder="Ej: 12500"
                    value={precio}
                    onChange={(e) => setPrecio(e.target.value)}
                    className="form-control"
                  />
                </div>

                {!editandoId && (
                  <div>
                    <label className="form-label">Tipo de Equipamiento</label>
                    <select value={stockId} onChange={(e) => setStockId(e.target.value)} className="form-control">
                      <option value={1}>Pelotas de tenis</option>
                      <option value={2}>Raquetas</option>
                      <option value={3}>Redes</option>
                    </select>
                  </div>
                )}
              </div>

              <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalOpen(false)} className="btn btn-secondary" style={{ flex: 1 }}>
                  Cancelar
                </button>
                <button type="submit" className="btn btn-volt" style={{ flex: 1 }}>
                  {editandoId ? 'Actualizar' : 'Guardar Producto'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

