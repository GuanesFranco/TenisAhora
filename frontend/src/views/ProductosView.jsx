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
  Sparkles,
  Boxes,
  RefreshCw
} from 'lucide-react';

export default function ProductosView() {
  const { user } = useAuth();
  const isAdmin = user?.rol === 'Administrador';

  // Control de Pestañas: 'productos' (Catálogo Comercial) | 'stock' (Pañol / RF-19 a RF-23)
  const [tabActiva, setTabActiva] = useState('productos');

  // ==========================================
  // ESTADO: CATÁLOGO COMERCIAL DE PRODUCTOS
  // ==========================================
  const [productos, setProductos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [search, setSearch] = useState('');
  const [filtroStock, setFiltroStock] = useState('todos');

  // Modal Productos (Crear / Editar)
  const [modalOpen, setModalOpen] = useState(false);
  const [editandoId, setEditandoId] = useState(null);
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [precio, setPrecio] = useState('');
  const [stockId, setStockId] = useState(1);
  const [cantidadInicial, setCantidadInicial] = useState('0'); // 1 = Pelotas, 2 = Raquetas, 3 = Redes

  // ==========================================
  // ESTADO: STOCK DE EQUIPAMIENTO / PAÑOL
  // ==========================================
  const [itemsStock, setItemsStock] = useState([]);
  const [loadingStock, setLoadingStock] = useState(false);
  const [filtroStockId, setFiltroStockId] = useState('');
  const [stockError, setStockError] = useState('');
  const [stockSuccessMsg, setStockSuccessMsg] = useState('');

  // Modal Stock (Alta / Modificación)
  const [modalStockOpen, setModalStockOpen] = useState(false);
  const [itemStockEditando, setItemStockEditando] = useState(null);
  const [formStock, setFormStock] = useState({
    tipoElemento: '',
    cantidadDisponible: 0,
  });
  const [submittingStock, setSubmittingStock] = useState(false);

  useEffect(() => {
    cargarProductos();
    if (isAdmin) {
      cargarStock();
    }
  }, [isAdmin]);

  // ------------------------------------------
  // CARGAS DE DATOS
  // ------------------------------------------
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

  async function cargarStock() {
    setLoadingStock(true);
    setStockError('');
    try {
      const data = await api.stock.listar();
      setItemsStock(data || []);
    } catch (err) {
      setStockError(err.message || 'Error cargando inventario de stock.');
    } finally {
      setLoadingStock(false);
    }
  }

  function mostrarMensaje(msg) {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 5000);
  }

  function mostrarMensajeStock(msg) {
    setStockSuccessMsg(msg);
    setTimeout(() => setStockSuccessMsg(''), 5000);
  }

  // ------------------------------------------
  // ACCIONES CATÁLOGO COMERCIAL
  // ------------------------------------------
  function abrirModalCrear() {
    setEditandoId(null);
    setNombre('');
    setDescripcion('');
    setPrecio('');
    setStockId(1);
    setCantidadInicial('0');
    setModalOpen(true);
  }

  function abrirModalEditar(p) {
    setEditandoId(p.id);
    setNombre(p.nombre);
    setDescripcion(p.descripcion);
    setPrecio(p.precio.toString());
    setStockId(p.stockId || 1);
    setCantidadInicial(String(p.stockActual ?? 0));
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
          cantidadDisponible: parseInt(cantidadInicial, 10) || 0,
        });
        mostrarMensaje('Producto actualizado correctamente.');
      } else {
        await api.productos.crear({
          nombre,
          descripcion,
          precio: parseFloat(precio),
          cantidadDisponible: parseInt(cantidadInicial, 10) || 0,
          stockId: parseInt(stockId, 10),
          cantidadInicial: parseInt(cantidadInicial, 10) || 0,
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

  // ------------------------------------------
  // ACCIONES STOCK / PAÑOL (RF-19 a RF-23)
  // ------------------------------------------
  function abrirModalCrearStock() {
    setItemStockEditando(null);
    setFormStock({
      tipoElemento: '',
      cantidadDisponible: 1,
    });
    setStockError('');
    setModalStockOpen(true);
  }

  function abrirModalEditarStock(item) {
    setItemStockEditando(item);
    setFormStock({
      tipoElemento: item.tipoElemento || '',
      cantidadDisponible: item.cantidadDisponible ?? 0,
    });
    setStockError('');
    setModalStockOpen(true);
  }

  // [RF-19] Alta y [RF-22] Modificación de Stock
  async function handleSubmitStock(e) {
    e.preventDefault();
    setStockError('');
    setSubmittingStock(true);
    try {
      const cantidad = parseInt(formStock.cantidadDisponible, 10);
      if (isNaN(cantidad) || cantidad < 0) {
        throw new Error('La cantidad disponible debe ser un número entero mayor o igual a cero.');
      }

      if (itemStockEditando) {
        // [RF-22] Modificar cantidad disponible
        await api.stock.modificar(itemStockEditando.id, {
          id: itemStockEditando.id,
          cantidadDisponible: cantidad,
        });
        mostrarMensajeStock(`Cantidad de stock para "${itemStockEditando.tipoElemento}" actualizada a ${cantidad} unidades.`);
      } else {
        // [RF-19] Registrar nuevo equipamiento
        if (!formStock.tipoElemento.trim()) {
          throw new Error('El tipo de equipamiento deportivo es obligatorio.');
        }
        await api.stock.registrar({
          tipoElemento: formStock.tipoElemento.trim(),
          cantidadDisponible: cantidad,
        });
        mostrarMensajeStock(`Nuevo equipamiento "${formStock.tipoElemento}" registrado con éxito.`);
      }

      setModalStockOpen(false);
      await Promise.all([cargarStock(), cargarProductos()]);
    } catch (err) {
      setStockError(err.message || 'Error al procesar la operación de stock.');
    } finally {
      setSubmittingStock(false);
    }
  }

  // [RF-23] Eliminar equipamiento deportivo
  async function handleEliminarStock(id, tipoElemento) {
    setStockError('');
    const confirmacion = window.confirm(
      `¿Desea eliminar del inventario el equipamiento "${tipoElemento}" (ID: ${id})?\n\n` +
      `Nota: Solo se podrá eliminar si no tiene reservas activas o históricas vinculadas.`
    );
    if (!confirmacion) return;

    try {
      await api.stock.eliminar(id);
      mostrarMensajeStock(`Equipamiento "${tipoElemento}" eliminado del stock exitosamente.`);
      await Promise.all([cargarStock(), cargarProductos()]);
    } catch (err) {
      setStockError(`No se pudo eliminar el equipamiento #${id}: ${err.message}`);
    }
  }

  // Normalizador para ignorar tildes/acentos y mayúsculas
  const normalizarTexto = (texto) =>
    (texto || '')
      .toString()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase();

  // Filtrado catálogo comercial
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

  // [RF-20] Filtrado de Stock de Pañol por ID o nombre
  const itemsStockFiltrados = itemsStock.filter((s) => {
    if (!filtroStockId.trim()) return true;
    const busqueda = filtroStockId.trim().toLowerCase();
    return (
      s.id.toString() === busqueda ||
      normalizarTexto(s.tipoElemento).includes(normalizarTexto(busqueda))
    );
  });

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header General */}
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
            {tabActiva === 'stock' ? (
              <Boxes size={26} color="var(--primary)" />
            ) : (
              <ShoppingBag size={26} color="var(--accent-volt)" />
            )}
            <h1 style={{ fontSize: '2rem' }}>
              {tabActiva === 'stock'
                ? 'Stock de Equipamiento Deportivo (Pañol)'
                : 'Catálogo de Productos y Tienda Oficial'}
            </h1>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            {tabActiva === 'stock'
              ? 'Control estricto de existencias físicas para canchas y alquileres [RF-19 a RF-23]: tubos de pelotas, raquetas y redes.'
              : isAdmin
                ? 'Administración de artículos comerciales, precios e inventario para reservas y venta oficial.'
                : 'Equipamiento reglamentario disponible en el club (raquetas, pelotas de torneo y redes).'}
          </p>
        </div>

        {/* Acciones principales según pestaña */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          {isAdmin && tabActiva === 'stock' && (
            <>
              <button onClick={cargarStock} className="btn btn-secondary" title="Refrescar existencias">
                <RefreshCw size={15} />
                <span>Actualizar</span>
              </button>
              <button onClick={abrirModalCrearStock} className="btn btn-primary">
                <Plus size={16} />
                <span>Nuevo Equipamiento</span>
              </button>
            </>
          )}

          {isAdmin && tabActiva === 'productos' && (
            <button onClick={abrirModalCrear} className="btn btn-volt">
              <Plus size={16} />
              <span>Nuevo Producto</span>
            </button>
          )}
        </div>
      </div>

      {/* Selector de Pestañas (Solo Administrador) */}
      {isAdmin && (
        <div style={{
          display: 'flex',
          gap: 10,
          background: 'var(--bg-card)',
          padding: '6px',
          borderRadius: 'var(--radius-md)',
          border: '1px solid var(--border)',
          width: 'fit-content'
        }}>
          <button
            onClick={() => setTabActiva('productos')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              padding: '9px 18px',
              borderRadius: 'var(--radius-sm)',
              border: 'none',
              background: tabActiva === 'productos' ? 'var(--primary)' : 'transparent',
              color: tabActiva === 'productos' ? '#fff' : 'var(--text-muted)',
              fontWeight: 600,
              fontSize: '0.875rem',
              cursor: 'pointer',
              transition: 'all 0.15s ease'
            }}
          >
            <ShoppingBag size={16} />
            <span>Catálogo Comercial</span>
          </button>

          <button
            onClick={() => setTabActiva('stock')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              padding: '9px 18px',
              borderRadius: 'var(--radius-sm)',
              border: 'none',
              background: tabActiva === 'stock' ? 'var(--primary)' : 'transparent',
              color: tabActiva === 'stock' ? '#fff' : 'var(--text-muted)',
              fontWeight: 600,
              fontSize: '0.875rem',
              cursor: 'pointer',
              transition: 'all 0.15s ease'
            }}
          >
            <Boxes size={16} />
            <span>Stock de Equipamiento (Pañol)</span>
            <span style={{
              fontSize: '0.65rem',
              background: tabActiva === 'stock' ? 'rgba(255,255,255,0.25)' : 'var(--bg-elevated)',
              padding: '2px 6px',
              borderRadius: 4
            }}>
              RF-19/23
            </span>
          </button>
        </div>
      )}

      {/* ========================================================
          PESTAÑA 1: STOCK DE EQUIPAMIENTO / PAÑOL (RF-19 a RF-23)
          ======================================================== */}
      {isAdmin && tabActiva === 'stock' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
          {/* Alertas Stock */}
          {stockError && (
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
              <span>{stockError}</span>
            </div>
          )}

          {stockSuccessMsg && (
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
              <span>{stockSuccessMsg}</span>
            </div>
          )}

          {/* Filtro por ID o Nombre [RF-20] */}
          <div className="card" style={{ padding: '14px 18px', display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
            <div style={{ position: 'relative', flex: 1, minWidth: 260 }}>
              <Search size={16} color="var(--text-dim)" style={{ position: 'absolute', left: 14, top: 13 }} />
              <input
                type="text"
                placeholder="Buscar por ID de equipamiento (ej: 1, 2) o nombre del elemento..."
                value={filtroStockId}
                onChange={(e) => setFiltroStockId(e.target.value)}
                className="form-control"
                style={{ paddingLeft: 40 }}
              />
            </div>
            {filtroStockId && (
              <button
                onClick={() => setFiltroStockId('')}
                className="btn btn-secondary btn-sm"
              >
                Limpiar filtro
              </button>
            )}
            <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)', marginLeft: 'auto' }}>
              Mostrando {itemsStockFiltrados.length} de {itemsStock.length} ítems en el pañol
            </span>
          </div>

          {/* Tabla de Stock [RF-20] */}
          <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
            {loadingStock ? (
              <div style={{ padding: 48, textAlign: 'center', color: 'var(--text-dim)' }}>
                Consultando inventario de stock en tiempo real desde la API...
              </div>
            ) : itemsStockFiltrados.length === 0 ? (
              <div style={{ padding: 48, textAlign: 'center' }}>
                <Boxes size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
                <h3 style={{ marginBottom: 6 }}>No se encontraron elementos de stock</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                  {filtroStockId ? 'No hay ningún ítem que coincida con el criterio de búsqueda.' : 'No hay equipamientos dados de alta en el pañol.'}
                </p>
              </div>
            ) : (
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                  <thead>
                    <tr style={{
                      background: 'var(--bg-elevated)',
                      borderBottom: '1px solid var(--border)',
                      color: 'var(--text-dim)',
                      fontSize: '0.75rem',
                      textTransform: 'uppercase'
                    }}>
                      <th style={{ padding: '14px 18px', width: 80 }}>ID</th>
                      <th style={{ padding: '14px 18px' }}>Tipo de Equipamiento Deportivo</th>
                      <th style={{ padding: '14px 18px', textAlign: 'center' }}>Cantidad Disponible (Física)</th>
                      <th style={{ padding: '14px 18px', textAlign: 'center' }}>Estado de Disponibilidad</th>
                      <th style={{ padding: '14px 18px', textAlign: 'right' }}>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {itemsStockFiltrados.map((item) => {
                      const disponible = item.cantidadDisponible > 0;
                      return (
                        <tr key={item.id} style={{ borderBottom: '1px solid var(--border)' }}>
                          <td style={{ padding: '14px 18px', fontWeight: 700, color: 'var(--primary)' }}>
                            #{item.id}
                          </td>
                          <td style={{ padding: '14px 18px' }}>
                            <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                              <strong style={{ fontSize: '0.925rem' }}>{item.tipoElemento}</strong>
                              <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>
                                {item.id === 1 ? '1 unidad asignada por reserva de cancha' :
                                 item.id === 2 ? '2 (singles) o 4 (dobles) por partido' :
                                 item.id === 3 ? '1 red por pista activa' : 'Equipamiento reglamentario'}
                              </span>
                            </div>
                          </td>
                          <td style={{ padding: '14px 18px', textAlign: 'center' }}>
                            <span style={{
                              fontSize: '1.25rem',
                              fontWeight: 800,
                              color: disponible ? 'var(--primary)' : 'var(--accent-red)'
                            }}>
                              {item.cantidadDisponible}
                            </span>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: 4 }}>
                              unidades
                            </span>
                          </td>
                          <td style={{ padding: '14px 18px', textAlign: 'center' }}>
                            <span className={`badge ${disponible ? 'badge-success' : 'badge-danger'}`} style={{ fontSize: '0.725rem' }}>
                              {disponible ? 'Disponible para Reserva' : 'Agotado (Bloquea Canchas)'}
                            </span>
                          </td>
                          <td style={{ padding: '14px 18px', textAlign: 'right' }}>
                            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                              <button
                                onClick={() => abrirModalEditarStock(item)}
                                className="btn btn-secondary btn-sm"
                                title="Modificar cantidad disponible [RF-22]"
                              >
                                <Edit2 size={14} />
                                <span>Ajustar Stock</span>
                              </button>
                              <button
                                onClick={() => handleEliminarStock(item.id, item.tipoElemento)}
                                className="btn btn-danger btn-sm"
                                title="Eliminar del inventario [RF-23]"
                              >
                                <Trash2 size={14} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ========================================================
          PESTAÑA 2: CATÁLOGO COMERCIAL DE PRODUCTOS
          ======================================================== */}
      {tabActiva === 'productos' && (
        <>
          {/* Alertas Catálogo */}
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
              Cargando catálogo comercial del club...
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
                        {prod.stockActual > 0 ? (isAdmin ? `${prod.stockActual} en stock` : 'Disponible') : 'Sin stock'}
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
        </>
      )}

      {/* ========================================================
          MODAL: CREAR / EDITAR PRODUCTO COMERCIAL
          ======================================================== */}
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

              <div style={{ display: 'grid', gridTemplateColumns: !editandoId ? '1fr 1fr 1fr' : '1fr', gap: 12 }}>
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

                <div>
                  <label className="form-label">{editandoId ? 'Stock Actual Disponible (Unidades)' : 'Stock Inicial (Unidades)'}</label>
                  <input
                    type="number"
                    min="0"
                    placeholder="Ej: 10"
                    value={cantidadInicial}
                    onChange={(e) => setCantidadInicial(e.target.value)}
                    className="form-control"
                  />
                </div>
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

      {/* ========================================================
          MODAL: ALTA Y MODIFICACIÓN DE STOCK / PAÑOL (RF-19 y RF-22)
          ======================================================== */}
      {modalStockOpen && (
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
              onClick={() => setModalStockOpen(false)}
              style={{ position: 'absolute', top: 16, right: 16, background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer' }}
            >
              <X size={20} />
            </button>

            <h3 style={{ marginBottom: 16, display: 'flex', alignItems: 'center', gap: 10 }}>
              <Boxes size={22} color="var(--primary)" />
              {itemStockEditando ? `Ajustar Existencias: #${itemStockEditando.id}` : 'Registrar Nuevo Equipamiento'}
            </h3>

            {stockError && (
              <div style={{
                background: 'rgba(239, 68, 68, 0.1)',
                border: '1px solid rgba(239, 68, 68, 0.3)',
                color: '#f87171',
                padding: '10px 12px',
                borderRadius: 'var(--radius-sm)',
                marginBottom: 12,
                fontSize: '0.85rem',
                display: 'flex',
                alignItems: 'center',
                gap: 8
              }}>
                <AlertTriangle size={16} />
                <span>{stockError}</span>
              </div>
            )}

            <form onSubmit={handleSubmitStock} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <label className="form-label">Tipo de Equipamiento Deportivo [RF-19]</label>
                <input
                  type="text"
                  required
                  disabled={Boolean(itemStockEditando)}
                  placeholder="Ej: Tubo de Pelotas Wilson Tour (x3), Raqueta Babolat..."
                  value={formStock.tipoElemento}
                  onChange={(e) => setFormStock({ ...formStock, tipoElemento: e.target.value })}
                  className="form-control"
                  style={itemStockEditando ? { opacity: 0.7, background: 'var(--bg-elevated)' } : {}}
                />
                {itemStockEditando && (
                  <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                    Por normativa del club [RF-22], solo se actualiza la cantidad disponible del inventario.
                  </small>
                )}
              </div>

              <div>
                <label className="form-label">Cantidad Disponible (Stock Actual) [RF-22]</label>
                <input
                  type="number"
                  min="0"
                  step="1"
                  required
                  placeholder="Ej: 24"
                  value={formStock.cantidadDisponible}
                  onChange={(e) => setFormStock({ ...formStock, cantidadDisponible: e.target.value })}
                  className="form-control"
                />
                <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                  Indique el número real de unidades físicas disponibles en el pañol.
                </small>
              </div>

              <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
                <button
                  type="button"
                  onClick={() => setModalStockOpen(false)}
                  className="btn btn-secondary"
                  style={{ flex: 1 }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={submittingStock}
                  className="btn btn-primary"
                  style={{ flex: 1 }}
                >
                  {submittingStock
                    ? 'Guardando...'
                    : itemStockEditando
                      ? 'Actualizar Stock'
                      : 'Registrar Equipamiento'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
