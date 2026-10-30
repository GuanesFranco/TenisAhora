import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import {
  Calendar,
  Clock,
  Users,
  CreditCard,
  QrCode,
  CheckCircle2,
  Check,
  AlertTriangle,
  X,
  Printer,
  Sparkles,
  ShieldCheck,
  Plus,
  ShoppingBag,
  Layers,
  Activity,
  CheckCheck
} from 'lucide-react';

export default function ReservasView() {
  const { user } = useAuth();
  const [canchas, setCanchas] = useState([]);
  const [reservas, setReservas] = useState([]);
  const [stockInventario, setStockInventario] = useState({
    pelotas: { id: 1, stock: 0, nombre: 'Tubo de Pelotas' },
    raquetas: { id: 2, stock: 0, nombre: 'Raquetas' },
    redes: { id: 3, stock: 0, nombre: 'Red de Tenis' },
  });
  const [loading, setLoading] = useState(true);

  // Modales
  const [modalReservarOpen, setModalReservarOpen] = useState(false);
  const [modalReciboOpen, setModalReciboOpen] = useState(false);
  const [reciboActual, setReciboActual] = useState(null);

  // Form State para la Reserva
  const [canchaSeleccionadaId, setCanchaSeleccionadaId] = useState('');
  const [fecha, setFecha] = useState(new Date(Date.now() + 86400000).toISOString().slice(0, 10));
  const [horaInicio, setHoraInicio] = useState('18:00');
  const [duracionHoras, setDuracionHoras] = useState(1); // 1 o 2 horas
  const [cantidadJugadores, setCantidadJugadores] = useState(2); // 2 o 4 personas

  // Nómina de Jugadores para Seguro Deportivo 
  const [jugadores, setJugadores] = useState([
    {
      dni: user?.dni ? String(user.dni).slice(0, 8) : '',
      nombre: user?.nombre || '',
      apellido: user?.apellido || '',
      telefono: user?.telefono || '',
      cobertura: ''
    },
    { dni: '', nombre: '', apellido: '', telefono: '', cobertura: '' },
    { dni: '', nombre: '', apellido: '', telefono: '', cobertura: '' },
    { dni: '', nombre: '', apellido: '', telefono: '', cobertura: '' }
  ]);

  // Pago
  const [metodoPago, setMetodoPago] = useState('Mercado Pago (QR)');
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [accionEnCurso, setAccionEnCurso] = useState(null);

  useEffect(() => {
    cargarDatos();
  }, []);

  async function cargarDatos() {
    setLoading(true);
    try {
      const [listaCanchas, listaReservas, listaProductos] = await Promise.all([
        api.canchas.listar().catch(() => []),
        api.reservas.listar().catch(() => []),
        api.productos.listar().catch(() => []),
      ]);

      // 1. Canchas
      const canchasNormalizadas = (listaCanchas || []).map((c) => ({
        id: c.id,
        nombre: `Pista ${c.id} (${c.superficie || 'Tenis'})`,
        tipo: c.superficie || 'Ladrillo',
        superficie: c.superficie || 'Polvo de Ladrillo',
        capacidad: c.capacidad || 4,
        precioHora: c.precio || 12000,
      }));
      setCanchas(canchasNormalizadas);
      if (canchasNormalizadas.length > 0 && !canchaSeleccionadaId) {
        setCanchaSeleccionadaId(canchasNormalizadas[0].id.toString());
      }

      // 2. Stock en tiempo real desde productos/stock
      const pelotasProd = (listaProductos || []).find(p => p.stockId === 1 || p.tipoEquipamiento?.toLowerCase().includes('pelota') || p.nombre?.toLowerCase().includes('pelota'));
      const raquetasProd = (listaProductos || []).find(p => p.stockId === 2 || p.tipoEquipamiento?.toLowerCase().includes('raqueta') || p.nombre?.toLowerCase().includes('raqueta'));
      const redesProd = (listaProductos || []).find(p => p.stockId === 3 || p.tipoEquipamiento?.toLowerCase().includes('red') || p.nombre?.toLowerCase().includes('red'));

      setStockInventario({
        pelotas: { id: pelotasProd?.stockId || 1, stock: pelotasProd?.stockActual ?? 0, nombre: pelotasProd?.nombre || 'Tubo de Pelotas Wilson' },
        raquetas: { id: raquetasProd?.stockId || 2, stock: raquetasProd?.stockActual ?? 0, nombre: raquetasProd?.nombre || 'Raquetas Wilson' },
        redes: { id: redesProd?.stockId || 3, stock: redesProd?.stockActual ?? 0, nombre: redesProd?.nombre || 'Red de Tenis Profesional' },
      });

      // 3. Reservas
      const reservasNormalizadas = (listaReservas || []).map((r) => {
        const fechaIso = r.fecha_reserva || r.fecha;
        const fechaFormateada = fechaIso ? new Date(fechaIso).toLocaleDateString('es-AR') : 'Sin fecha';
        const horaIni = r.horaInicio ? new Date(r.horaInicio).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' }) : '';
        const horaFin = r.horaFin ? new Date(r.horaFin).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' }) : '';
        const horarioFormateado = horaIni && horaFin ? `${horaIni} - ${horaFin}` : (r.horario || 'Turno asignado');

        const equipList = (r.equipamientos || []).map(e => `${e.cantidad}x ${e.tipoElemento}`).join(', ') || r.equipamiento || '1x Red, 1x Tubo Pelotas, 2x Raquetas';

        return {
          id: r.id,
          numeroRecibo: `REC-2026-00${r.id}`,
          canchaId: r.canchaId,
          canchaNombre: `Pista ${r.canchaId}`,
          fecha: fechaFormateada,
          horario: horarioFormateado,
          duracionHoras: 1,
          jugadores: r.jugadoresDni?.length || r.jugadores || 2,
          jugadoresDetalle: r.jugadores || [],
          socioNombre: r.socioNombre || (r.jugadores && r.jugadores[0] ? `${r.jugadores[0].nombre} ${r.jugadores[0].apellido}` : `Socio DNI ${r.socioDni}`),
          socioDni: r.socioDni,
          precioTotal: r.importeTotal || 12000,
          senaAbonada: r.montoSena || 6000,
          saldoPendiente: (r.importeTotal || 12000) - (r.montoSena || 6000),
          metodoPago: 'Mercado Pago (QR)',
          equipamiento: equipList,
          estado: r.estado || 'Confirmada (Seña 50% abonada)',
        };
      });

      setReservas(reservasNormalizadas);
    } catch (err) {
      console.error('Error cargando canchas y reservas:', err);
    } finally {
      setLoading(false);
    }
  }

  // Cálculos de costos
  const canchaActual = canchas.find((c) => c.id === parseInt(canchaSeleccionadaId, 10)) || canchas[0] || { precioHora: 12000, id: 1 };
  const precioBase = (canchaActual?.precioHora || 12000) * duracionHoras;
  const precioTotal = precioBase;
  const senaRequerida = precioTotal * 0.50; // Seña 50% obligatoria 
  const saldoRestante = precioTotal - senaRequerida;

  // Validación de Stock Requerido para el Alquiler 
  const stockPelotasRequerido = 1;
  const stockRedesRequerido = 1;
  const stockRaquetasRequerido = cantidadJugadores; // 2 para singles, 4 para dobles

  const hayStockPelotas = stockInventario.pelotas.stock >= stockPelotasRequerido;
  const hayStockRedes = stockInventario.redes.stock >= stockRedesRequerido;
  const hayStockRaquetas = stockInventario.raquetas.stock >= stockRaquetasRequerido;
  const hayStockGeneral = hayStockPelotas && hayStockRedes && hayStockRaquetas;

  const maxFechaAnticipacion = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
  const minFecha = new Date().toISOString().slice(0, 10);

  // Manejo de cambio en los datos de un jugador
  const handleActualizarJugador = (index, campo, valor) => {
    setJugadores(prev => {
      const copia = [...prev];
      copia[index] = { ...copia[index], [campo]: valor };
      return copia;
    });
  };

  const handleCrearReserva = async (e) => {
    e.preventDefault();
    setFormError('');

    if (duracionHoras > 2) {
      setFormError('Cada alquiler de cancha tiene una duración máxima de 2 horas .');
      return;
    }

    if (fecha > maxFechaAnticipacion) {
      setFormError('Las reservas sólo pueden realizarse con un máximo de 30 días de antelación .');
      return;
    }

    // Validación de stock antes de enviar
    if (!hayStockGeneral) {
      const faltantes = [];
      if (!hayStockPelotas) faltantes.push(`Tubo de Pelotas (requerido: 1, disponible: ${stockInventario.pelotas.stock})`);
      if (!hayStockRedes) faltantes.push(`Red de Tenis (requerido: 1, disponible: ${stockInventario.redes.stock})`);
      if (!hayStockRaquetas) faltantes.push(`Raquetas (requeridas: ${stockRaquetasRequerido}, disponibles: ${stockInventario.raquetas.stock})`);
      setFormError(`No se puede realizar la reserva: Stock insuficiente de ${faltantes.join('; ')}.`);
      return;
    }

    // Tomar los jugadores activos según modalidad
    const jugadoresActivos = jugadores.slice(0, cantidadJugadores);

    // Validar que no haya campos vacíos
    for (let i = 0; i < jugadoresActivos.length; i++) {
      const j = jugadoresActivos[i];
      if (!j.nombre || !j.apellido || !j.dni || !j.cobertura) {
        setFormError(`Por favor complete todos los datos del Jugador #${i + 1} (Nombre, Apellido, DNI y Cobertura Médica para el seguro).`);
        return;
      }
    }

    // Validar DNIs duplicados
    const dnis = jugadoresActivos.map(j => parseInt(j.dni, 10));
    if (new Set(dnis).size !== dnis.length) {
      setFormError('La lista de jugadores contiene números de DNI duplicados.');
      return;
    }

    setSubmitting(true);
    try {
      const fechaBase = new Date(`${fecha}T${horaInicio}:00`);
      const fechaFin = new Date(fechaBase.getTime() + duracionHoras * 3600000);

      const titularDni = dnis[0];

      // Equipamiento reglamentario exacto a descontar 
      const equipamientosSolicitados = [
        { equipamientoId: stockInventario.pelotas.id, cantidad: stockPelotasRequerido },
        { equipamientoId: stockInventario.raquetas.id, cantidad: stockRaquetasRequerido },
        { equipamientoId: stockInventario.redes.id, cantidad: stockRedesRequerido },
      ];

      const jugadoresDetallePayload = jugadoresActivos.map(j => ({
        dni: parseInt(j.dni, 10),
        nombre: j.nombre.trim(),
        apellido: j.apellido.trim(),
        telefono: j.telefono ? j.telefono.trim() : '1100000000',
        coberturaMedica: j.cobertura.trim()
      }));

      const payload = {
        canchaId: parseInt(canchaSeleccionadaId, 10),
        fecha_reserva: fechaBase.toISOString(),
        horaInicio: fechaBase.toISOString(),
        horaFin: fechaFin.toISOString(),
        socioDni: titularDni,
        jugadoresDni: dnis,
        equipamientosSolicitados,
        jugadoresDetalle: jugadoresDetallePayload
      };

      const nuevaApi = await api.reservas.crear(payload);

      const nuevoRecibo = {
        id: nuevaApi.id,
        numeroRecibo: `REC-2026-00${nuevaApi.id}`,
        canchaNombre: canchaActual.nombre,
        fecha,
        horario: `${horaInicio} (${duracionHoras}h)`,
        jugadores: cantidadJugadores,
        jugadoresLista: jugadoresActivos,
        socioNombre: `${jugadoresActivos[0].nombre} ${jugadoresActivos[0].apellido}`,
        socioDni: titularDni,
        precioTotal: nuevaApi.importeTotal || precioTotal,
        senaAbonada: nuevaApi.montoSena || senaRequerida,
        saldoPendiente: (nuevaApi.importeTotal || precioTotal) - (nuevaApi.montoSena || senaRequerida),
        metodoPago,
        equipamiento: `1x Red Oficial, 1x Tubo de Pelotas (x3), ${cantidadJugadores}x Raquetas de Tenis`,
      };

      setReciboActual(nuevoRecibo);
      setModalReservarOpen(false);
      setModalReciboOpen(true);
      await cargarDatos();
    } catch (err) {
      setFormError(err.message || 'Error al procesar reserva en el backend.');
    } finally {
      setSubmitting(false);
    }
  };

  // Finalizar turno y restituir stock 
  const handleFinalizarReserva = async (reserva) => {
    const confirmMsg = `¿Confirmás la finalización del partido para la reserva #${reserva.id} (${reserva.canchaNombre})?\n\n` +
      `Operación oficial del club :\n` +
      `- Se actualizará el estado a Finalizada.\n` +
      `- Se reintegrarán automáticamente las raquetas, pelotas y red al stock disponible del inventario.\n` +
      `- Se habilitará el cobro del saldo restante ($ ${(reserva.saldoPendiente || 0).toLocaleString('es-AR')}).`;

    if (!window.confirm(confirmMsg)) return;

    setAccionEnCurso(reserva.id);
    try {
      await api.reservas.finalizar(reserva.id);
      alert(`¡Reserva #${reserva.id} finalizada exitosamente! El equipamiento asignado ha sido reintegrado al stock del club.`);
      await cargarDatos();
    } catch (err) {
      alert(`Error al finalizar reserva: ${err.message}`);
    } finally {
      setAccionEnCurso(null);
    }
  };

  // Cancelar reserva 
  const handleCancelarReserva = async (reserva) => {
    const confirmMsg = `¿Deseás cancelar la reserva #${reserva.id} en ${reserva.canchaNombre}?\n\n` +
      `Política oficial del club :\n` +
      `- Con más de 6 hs de antelación: Reembolso del 100% de la seña abonada y restitución del stock.\n` +
      `- Menos de 6 hs: Retención de la seña en concepto de cargo por cancelación tardía.`;

    if (!window.confirm(confirmMsg)) return;

    setAccionEnCurso(reserva.id);
    try {
      const res = await api.reservas.cancelar(reserva.id);
      alert(res?.mensaje || 'Reserva cancelada correctamente y stock restituido.');
      await cargarDatos();
    } catch (err) {
      alert(`Error al cancelar: ${err.message}`);
    } finally {
      setAccionEnCurso(null);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14 }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', display: 'flex', alignItems: 'center', gap: 10 }}>
            <Calendar color="var(--primary)" size={24} />
            Alquiler de Canchas y Reservas Online
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
            Control estricto de disponibilidad, nómina con cobertura de seguro obligatorio y asignación reglamentaria de stock.
          </p>
        </div>

        <button onClick={() => setModalReservarOpen(true)} className="btn btn-primary">
          <Plus size={16} />
          <span>Nueva Reserva de Cancha</span>
        </button>
      </div>

      {/* Monitor de Stock de Equipamiento Deportivo en Vivo  */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-md)',
        padding: '16px 20px',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12, flexWrap: 'wrap', gap: 8 }}>
          <span style={{ fontSize: '0.8125rem', fontWeight: 700, textTransform: 'uppercase', color: 'var(--text-dim)', letterSpacing: '0.04em', display: 'flex', alignItems: 'center', gap: 6 }}>
            <ShoppingBag size={14} color="var(--primary)" />
            Inventario Deportivo Requerido para Alquileres (Stock en Vivo)
          </span>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
            Se descuenta automáticamente al alquilar y se repone al finalizar el partido 
          </span>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: 12 }}>
          {/* Tubos de Pelotas */}
          <div style={{
            background: 'var(--bg-elevated)',
            border: `1px solid ${stockInventario.pelotas.stock > 0 ? 'var(--border)' : 'var(--accent-red-border)'}`,
            borderRadius: 'var(--radius-sm)',
            padding: '12px 14px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center'
          }}>
            <div>
              <div style={{ fontSize: '0.85rem', fontWeight: 600 }}>Tubos de Pelotas (x3)</div>
              <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>1 tubo por reserva</small>
            </div>
            <div style={{ textAlign: 'right' }}>
              <span style={{
                fontSize: '1.25rem',
                fontWeight: 700,
                color: stockInventario.pelotas.stock > 0 ? 'var(--primary)' : 'var(--accent-red)'
              }}>
                {stockInventario.pelotas.stock}
              </span>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: 4 }}>disp.</span>
            </div>
          </div>

          {/* Raquetas */}
          <div style={{
            background: 'var(--bg-elevated)',
            border: `1px solid ${stockInventario.raquetas.stock >= 2 ? 'var(--border)' : 'var(--accent-red-border)'}`,
            borderRadius: 'var(--radius-sm)',
            padding: '12px 14px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center'
          }}>
            <div>
              <div style={{ fontSize: '0.85rem', fontWeight: 600 }}>Raquetas de Tenis</div>
              <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>2 (singles) o 4 (dobles)</small>
            </div>
            <div style={{ textAlign: 'right' }}>
              <span style={{
                fontSize: '1.25rem',
                fontWeight: 700,
                color: stockInventario.raquetas.stock >= 2 ? 'var(--primary)' : 'var(--accent-red)'
              }}>
                {stockInventario.raquetas.stock}
              </span>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: 4 }}>disp.</span>
            </div>
          </div>

          {/* Redes */}
          <div style={{
            background: 'var(--bg-elevated)',
            border: `1px solid ${stockInventario.redes.stock > 0 ? 'var(--border)' : 'var(--accent-red-border)'}`,
            borderRadius: 'var(--radius-sm)',
            padding: '12px 14px',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center'
          }}>
            <div>
              <div style={{ fontSize: '0.85rem', fontWeight: 600 }}>Redes Reglamentarias</div>
              <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem' }}>1 por pista alquilada</small>
            </div>
            <div style={{ textAlign: 'right' }}>
              <span style={{
                fontSize: '1.25rem',
                fontWeight: 700,
                color: stockInventario.redes.stock > 0 ? 'var(--primary)' : 'var(--accent-red)'
              }}>
                {stockInventario.redes.stock}
              </span>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginLeft: 4 }}>disp.</span>
            </div>
          </div>
        </div>
      </div>

      {/* Tarjetas de Canchas Disponibles */}
      <div>
        <h3 style={{ fontSize: '1.1rem', marginBottom: 12, display: 'flex', alignItems: 'center', gap: 8 }}>
          <Sparkles size={16} color="var(--primary)" />
          Infraestructura de Canchas del Complejo
        </h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
          {canchas.map((c) => (
            <div
              key={c.id}
              className="card"
              style={{
                padding: '20px',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                gap: 12,
              }}
            >
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                  <h4 style={{ fontSize: '1.1rem', fontWeight: 700 }}>{c.nombre}</h4>
                  <span className="badge badge-primary" style={{ fontSize: '0.7rem' }}>
                    {c.superficie}
                  </span>
                </div>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.8rem', lineHeight: 1.4 }}>
                  Capacidad para hasta {c.capacidad} jugadores. Con iluminación LED y mantenimiento reglamentario diario.
                </p>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderTop: '1px solid var(--border)', paddingTop: 10 }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>Arancel Oficial:</span>
                <strong style={{ fontSize: '1.1rem', color: 'var(--primary)' }}>
                  ${(c.precioHora || 12000).toLocaleString('es-AR')}<span style={{ fontSize: '0.75rem', fontWeight: 400, color: 'var(--text-muted)' }}>/hora</span>
                </strong>
              </div>

              <button
                onClick={() => {
                  setCanchaSeleccionadaId(c.id.toString());
                  setModalReservarOpen(true);
                }}
                className="btn btn-secondary btn-sm"
                style={{ width: '100%', justifyContent: 'center' }}
              >
                Reservar esta Pista
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Listado de Reservas Registradas */}
      <div className="card" style={{ padding: '24px' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, flexWrap: 'wrap', gap: 8 }}>
          <div>
            <h3 style={{ fontSize: '1.15rem', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Clock size={18} color="var(--primary)" />
              Reservas Activas y Turnos Registrados
            </h3>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
              Seguimiento de turnos, estado de cobranza desdoblada y devolución de equipamiento asignado.
            </span>
          </div>
          <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>
            Total registros: {reservas.length}
          </span>
        </div>

        {loading ? (
          <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-muted)' }}>Cargando reservas desde el servidor...</div>
        ) : reservas.length === 0 ? (
          <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
            No hay reservas registradas en este momento en la base de datos.
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                  <th style={{ padding: '12px 14px' }}>Recibo #</th>
                  <th style={{ padding: '12px 14px' }}>Pista</th>
                  <th style={{ padding: '12px 14px' }}>Fecha y Turno</th>
                  <th style={{ padding: '12px 14px' }}>Jugadores y Seguro</th>
                  <th style={{ padding: '12px 14px' }}>Equipamiento Asignado</th>
                  <th style={{ padding: '12px 14px' }}>Seña (50%)</th>
                  <th style={{ padding: '12px 14px' }}>Saldo Restante</th>
                  <th style={{ padding: '12px 14px' }}>Estado</th>
                  <th style={{ padding: '12px 14px', textAlign: 'right' }}>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {reservas.map((r) => {
                  const esFinalizada = r.estado?.toLowerCase().includes('finalizada');
                  const esCancelada = r.estado?.toLowerCase().includes('cancelada');

                  return (
                    <tr key={r.id} style={{ borderBottom: '1px solid var(--border)' }}>
                      <td style={{ padding: '14px 14px', fontWeight: 700, color: 'var(--primary)' }}>
                        {r.numeroRecibo || `#${r.id}`}
                      </td>
                      <td style={{ padding: '14px 14px', fontWeight: 600 }}>{r.canchaNombre}</td>
                      <td style={{ padding: '14px 14px' }}>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                          <span>{r.fecha}</span>
                          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{r.horario}</span>
                        </div>
                      </td>
                      <td style={{ padding: '14px 14px' }}>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                          <span style={{ fontWeight: 600 }}>{r.socioNombre}</span>
                          <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', display: 'flex', alignItems: 'center', gap: 4 }}>
                            <ShieldCheck size={12} color="var(--primary)" />
                            {r.jugadores} participantes · Titular DNI {r.socioDni}
                          </span>
                        </div>
                      </td>
                      <td style={{ padding: '14px 14px', fontSize: '0.8rem', color: 'var(--text-muted)', maxWidth: 200 }}>
                        {r.equipamiento}
                      </td>
                      <td style={{ padding: '14px 14px', color: 'var(--primary)', fontWeight: 700 }}>
                        ${(r.senaAbonada || 0).toLocaleString('es-AR')}
                      </td>
                      <td style={{ padding: '14px 14px', color: 'var(--text-muted)' }}>
                        ${(r.saldoPendiente || 0).toLocaleString('es-AR')}
                      </td>
                      <td style={{ padding: '14px 14px' }}>
                        <span className={`badge ${esFinalizada ? 'badge-secondary' : esCancelada ? 'badge-danger' : 'badge-success'}`} style={{ fontSize: '0.7rem' }}>
                          {r.estado}
                        </span>
                      </td>
                      <td style={{ padding: '14px 14px', textAlign: 'right' }}>
                        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 6 }}>
                          <button
                            onClick={() => {
                              setReciboActual(r);
                              setModalReciboOpen(true);
                            }}
                            className="btn btn-secondary btn-sm"
                            title="Ver e imprimir recibo oficial"
                          >
                            <Printer size={13} />
                          </button>

                          {!esFinalizada && !esCancelada && (
                            <>
                              <button
                                onClick={() => handleFinalizarReserva(r)}
                                disabled={accionEnCurso === r.id}
                                className="btn btn-secondary btn-sm"
                                title="Finalizar partido y restituir equipamiento a stock "
                                style={{ color: 'var(--primary)', borderColor: 'var(--primary-subtle-border)' }}
                              >
                                <CheckCheck size={13} />
                              </button>
                              <button
                                onClick={() => handleCancelarReserva(r)}
                                disabled={accionEnCurso === r.id}
                                className="btn btn-danger btn-sm"
                                title="Cancelar reserva "
                              >
                                <X size={13} />
                              </button>
                            </>
                          )}
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

      {/* Modal Alquilar Cancha */}
      {modalReservarOpen && (
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
          <div className="card" style={{ maxWidth: 660, width: '100%', maxHeight: '90vh', overflowY: 'auto', position: 'relative' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <div>
                <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Calendar color="var(--primary)" size={20} />
                  Alquiler de Cancha y Reserva Oficial
                </h3>
                <small style={{ color: 'var(--text-muted)' }}>
                  Complejo Deportivo "El Deporte Saludable" · Cumplimiento Normativa Oficial del Club
                </small>
              </div>
              <button onClick={() => setModalReservarOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && (
              <div style={{
                background: 'var(--accent-red-bg)',
                border: '1px solid var(--accent-red-border)',
                color: 'var(--accent-red)',
                padding: '10px 14px',
                borderRadius: 'var(--radius-sm)',
                marginBottom: 16,
                fontSize: '0.875rem',
                display: 'flex',
                alignItems: 'center',
                gap: 8
              }}>
                <AlertTriangle size={16} />
                <span>{formError}</span>
              </div>
            )}

            <form onSubmit={handleCrearReserva} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              {/* Selección de Cancha */}
              <div>
                <label className="form-label">
                  Seleccionar Cancha (Ladrillo, Cemento o Pasto)
                </label>
                <select
                  className="form-control"
                  value={canchaSeleccionadaId}
                  onChange={(e) => setCanchaSeleccionadaId(e.target.value)}
                >
                  {canchas.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.nombre} · ${(c.precioHora || 12000).toLocaleString('es-AR')}/hora
                    </option>
                  ))}
                </select>
              </div>

              {/* Fecha y Horario */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Fecha (Máx 30 días)</label>
                  <input
                    type="date"
                    required
                    min={minFecha}
                    max={maxFechaAnticipacion}
                    className="form-control"
                    value={fecha}
                    onChange={(e) => setFecha(e.target.value)}
                  />
                </div>

                <div>
                  <label className="form-label">Hora de Inicio</label>
                  <select
                    className="form-control"
                    value={horaInicio}
                    onChange={(e) => setHoraInicio(e.target.value)}
                  >
                    <option value="09:00">09:00 hs</option>
                    <option value="10:00">10:00 hs</option>
                    <option value="11:00">11:00 hs</option>
                    <option value="12:00">12:00 hs</option>
                    <option value="14:00">14:00 hs</option>
                    <option value="15:00">15:00 hs</option>
                    <option value="16:00">16:00 hs</option>
                    <option value="17:00">17:00 hs</option>
                    <option value="18:00">18:00 hs</option>
                    <option value="19:00">19:00 hs</option>
                    <option value="20:00">20:00 hs</option>
                    <option value="21:00">21:00 hs</option>
                  </select>
                </div>

                <div>
                  <label className="form-label">Duración (Máx 2h)</label>
                  <select
                    className="form-control"
                    value={duracionHoras}
                    onChange={(e) => setDuracionHoras(Number(e.target.value))}
                  >
                    <option value={1}>1 Hora</option>
                    <option value={2}>2 Horas (Máximo)</option>
                  </select>
                </div>
              </div>

              {/* Modalidad de Juego (2 o 4 personas) */}
              <div>
                <label className="form-label">Modalidad del Partido </label>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
                  <button
                    type="button"
                    onClick={() => setCantidadJugadores(2)}
                    className={`btn ${cantidadJugadores === 2 ? 'btn-primary' : 'btn-secondary'}`}
                    style={{ justifyContent: 'center' }}
                  >
                    <Users size={16} />
                    <span>2 Jugadores (Singles - 2 Raquetas)</span>
                  </button>
                  <button
                    type="button"
                    onClick={() => setCantidadJugadores(4)}
                    className={`btn ${cantidadJugadores === 4 ? 'btn-primary' : 'btn-secondary'}`}
                    style={{ justifyContent: 'center' }}
                  >
                    <Users size={16} />
                    <span>4 Jugadores (Dobles - 4 Raquetas)</span>
                  </button>
                </div>
              </div>

              {/* Nómina Obligatoria de Jugadores con Seguro Deportivo  */}
              <div style={{
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-md)',
                padding: '16px',
                display: 'flex',
                flexDirection: 'column',
                gap: 12
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.875rem', fontWeight: 700, color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 6 }}>
                    <ShieldCheck size={16} color="var(--primary)" />
                    Nómina de Jugadores y Seguro Deportivo Obligatorio 
                  </span>
                  <span className="badge badge-primary" style={{ fontSize: '0.7rem' }}>
                    {cantidadJugadores} Registros Requeridos
                  </span>
                </div>
                <small style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginTop: -6 }}>
                  Exigencia del complejo: por protocolo de la Asociación de Tenis y póliza de seguro de accidentes personales en cancha, todos los participantes deben estar identificados nominalmente.
                </small>

                <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
                  {jugadores.slice(0, cantidadJugadores).map((j, idx) => (
                    <div key={idx} style={{
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border)',
                      borderRadius: 'var(--radius-sm)',
                      padding: '10px 12px',
                      display: 'flex',
                      flexDirection: 'column',
                      gap: 8
                    }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <span style={{ fontSize: '0.75rem', fontWeight: 700, color: idx === 0 ? 'var(--primary)' : 'var(--text-muted)' }}>
                          {idx === 0 ? 'Jugador #1 (Socio Titular)' : `Jugador #${idx + 1} (Participante Acompañante)`}
                        </span>
                        {idx === 0 && (
                          <span style={{ fontSize: '0.6875rem', color: 'var(--text-dim)' }}>Responsable de la seña</span>
                        )}
                      </div>

                      <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1.2fr 1fr 1fr 1.4fr', gap: 8 }}>
                        <div>
                          <input
                            type="text"
                            required
                            placeholder="Nombre"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.nombre}
                            onChange={(e) => handleActualizarJugador(idx, 'nombre', e.target.value)}
                          />
                        </div>
                        <div>
                          <input
                            type="text"
                            required
                            placeholder="Apellido"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.apellido}
                            onChange={(e) => handleActualizarJugador(idx, 'apellido', e.target.value)}
                          />
                        </div>
                        <div>
                          <input type="text" inputMode="numeric" maxLength={8} required placeholder="DNI (hasta 8 dígitos)" className="form-control" style={{ fontSize: '0.8125rem', padding: '6px 10px' }} value={j.dni} onChange={(e) => handleActualizarJugador(idx, 'dni', e.target.value.replace(/\D/g, '').slice(0, 8))} />
                        </div>
                        <div>
                          <input
                            type="text"
                            required
                            placeholder="Teléfono"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.telefono}
                            inputMode="numeric" placeholder="Solo números" onChange={(e) => handleActualizarJugador(idx, 'telefono', e.target.value.replace(/\D/g, ''))}
                          />
                        </div>
                        <div>
                          <input
                            type="text"
                            required
                            placeholder="Obra Social / Seguro"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.cobertura}
                            onChange={(e) => handleActualizarJugador(idx, 'cobertura', e.target.value)}
                          />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Equipamiento Deportivo de Stock Obligatorio  */}
              <div style={{
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-md)',
                padding: '14px 16px',
                display: 'flex',
                flexDirection: 'column',
                gap: 10,
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: '0.875rem', fontWeight: 700, color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 6 }}>
                    <ShoppingBag size={16} color="var(--primary)" />
                    Equipamiento Reglamentario Asignado (Descuento Automático de Stock)
                  </span>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                    Alquiler incorpora: Pelotas, Raquetas y Red
                  </span>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 10 }}>
                  {/* Tubo de pelotas */}
                  <div style={{
                    background: 'var(--bg-card)',
                    border: `1px solid ${hayStockPelotas ? 'var(--border)' : 'var(--accent-red-border)'}`,
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 4
                  }}>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', textTransform: 'uppercase', fontWeight: 600 }}>
                      1x Tubo Pelotas (x3)
                    </span>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontSize: '0.8125rem', fontWeight: 600 }}>Stock Disponible:</span>
                      <span style={{
                        fontSize: '0.875rem',
                        fontWeight: 700,
                        color: hayStockPelotas ? 'var(--primary)' : 'var(--accent-red)'
                      }}>
                        {stockInventario.pelotas.stock} un.
                      </span>
                    </div>
                    {!hayStockPelotas && (
                      <span style={{ fontSize: '0.6875rem', color: 'var(--accent-red)' }}>¡Sin stock!</span>
                    )}
                  </div>

                  {/* Raquetas */}
                  <div style={{
                    background: 'var(--bg-card)',
                    border: `1px solid ${hayStockRaquetas ? 'var(--border)' : 'var(--accent-red-border)'}`,
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 4
                  }}>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', textTransform: 'uppercase', fontWeight: 600 }}>
                      {cantidadJugadores}x Raquetas ({cantidadJugadores} jug.)
                    </span>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontSize: '0.8125rem', fontWeight: 600 }}>Stock Disponible:</span>
                      <span style={{
                        fontSize: '0.875rem',
                        fontWeight: 700,
                        color: hayStockRaquetas ? 'var(--primary)' : 'var(--accent-red)'
                      }}>
                        {stockInventario.raquetas.stock} un.
                      </span>
                    </div>
                    {!hayStockRaquetas && (
                      <span style={{ fontSize: '0.6875rem', color: 'var(--accent-red)' }}>
                        ¡Faltan {stockRaquetasRequerido - stockInventario.raquetas.stock} raqueta(s)!
                      </span>
                    )}
                  </div>

                  {/* Red */}
                  <div style={{
                    background: 'var(--bg-card)',
                    border: `1px solid ${hayStockRedes ? 'var(--border)' : 'var(--accent-red-border)'}`,
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 4
                  }}>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', textTransform: 'uppercase', fontWeight: 600 }}>
                      1x Red de Pista
                    </span>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontSize: '0.8125rem', fontWeight: 600 }}>Stock Disponible:</span>
                      <span style={{
                        fontSize: '0.875rem',
                        fontWeight: 700,
                        color: hayStockRedes ? 'var(--primary)' : 'var(--accent-red)'
                      }}>
                        {stockInventario.redes.stock} un.
                      </span>
                    </div>
                    {!hayStockRedes && (
                      <span style={{ fontSize: '0.6875rem', color: 'var(--accent-red)' }}>¡Sin stock!</span>
                    )}
                  </div>
                </div>

                {!hayStockGeneral && (
                  <div style={{
                    background: 'var(--accent-red-bg)',
                    color: 'var(--accent-red)',
                    border: '1px solid var(--accent-red-border)',
                    padding: '8px 12px',
                    borderRadius: 'var(--radius-sm)',
                    fontSize: '0.75rem',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 6
                  }}>
                    <AlertTriangle size={14} />
                    <span><strong>Bloqueo por falta de stock :</strong> No se puede alquilar una cancha si no hay stock suficiente de redes, pelotas o raquetas.</span>
                  </div>
                )}
              </div>

              {/* Desglose Financiero con Seña del 50% Obligatoria  */}
              <div style={{
                background: 'var(--primary-subtle)',
                border: '1px solid var(--primary-subtle-border)',
                borderRadius: 'var(--radius-md)',
                padding: '16px',
                display: 'flex',
                flexDirection: 'column',
                gap: 8,
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem' }}>
                  <span style={{ color: 'var(--text-muted)' }}>Costo Total Alquiler ({duracionHoras}h en {canchaActual.nombre}):</span>
                  <span style={{ fontWeight: 600 }}>${precioTotal.toLocaleString('es-AR')}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '1rem', borderTop: '1px solid var(--border)', paddingTop: 8 }}>
                  <span style={{ color: 'var(--primary)', fontWeight: 700 }}>Seña requerida ahora (50% obligatorio ):</span>
                  <span style={{ color: 'var(--primary)', fontWeight: 800, fontSize: '1.25rem' }}>
                    ${senaRequerida.toLocaleString('es-AR')}
                  </span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8125rem', color: 'var(--text-dim)' }}>
                  <span>Saldo restante a abonar al finalizar el partido :</span>
                  <span>${saldoRestante.toLocaleString('es-AR')}</span>
                </div>
              </div>

              {/* Medio de Pago */}
              <div>
                <label className="form-label">
                  Medio de Cobranza de la Seña
                </label>
                <select
                  className="form-control"
                  value={metodoPago}
                  onChange={(e) => setMetodoPago(e.target.value)}
                >
                  <option value="Mercado Pago (QR)">Billetera Virtual Mercado Pago (con QR oficial)</option>
                  <option value="Tarjeta de Débito">Tarjeta de Débito</option>
                  <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
                </select>
              </div>

              {/* Botones de acción */}
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalReservarOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={submitting || !hayStockGeneral}
                  className="btn btn-primary"
                  title={!hayStockGeneral ? 'Bloqueado por falta de stock suficiente de equipamiento' : ''}
                >
                  {submitting ? 'Registrando reserva en el servidor...' : `Abonar Seña $${senaRequerida.toLocaleString('es-AR')} y Confirmar`}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Recibo Oficial */}
      {modalReciboOpen && reciboActual && (
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
          <div className="card" style={{ maxWidth: 540, width: '100%', padding: 24, position: 'relative' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <CheckCircle2 color="var(--primary)" size={22} />
                Comprobante Oficial de Reserva y Seguro
              </h3>
              <button onClick={() => setModalReciboOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Ficha Imprimible del Recibo */}
            <div id="recibo-print" style={{
              background: '#ffffff',
              color: '#0f172a',
              padding: '24px',
              borderRadius: 'var(--radius-sm)',
              display: 'flex',
              flexDirection: 'column',
              gap: 14,
              border: '1px solid #cbd5e1',
            }}>
              <div style={{ borderBottom: '2px solid #0f172a', paddingBottom: 10, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <h2 style={{ fontSize: '1.2rem', margin: 0, fontWeight: 800, color: '#0f172a', letterSpacing: '-0.02em' }}>TENIS AHORA</h2>
                  <small style={{ color: '#475569', fontSize: '0.75rem' }}>Complejo Deportivo "El Deporte Saludable"</small>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <span style={{ fontSize: '0.8125rem', fontWeight: 800, color: '#0f172a' }}>RECIBO OFICIAL</span>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>{reciboActual.numeroRecibo}</div>
                </div>
              </div>

              <div style={{ fontSize: '0.8125rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <div><strong>Socio Titular:</strong> {reciboActual.socioNombre}</div>
                <div><strong>DNI Titular:</strong> {reciboActual.socioDni}</div>
                <div><strong>Pista Asignada:</strong> {reciboActual.canchaNombre}</div>
                <div><strong>Fecha:</strong> {reciboActual.fecha}</div>
                <div><strong>Turno:</strong> {reciboActual.horario}</div>
                <div><strong>Jugadores:</strong> {reciboActual.jugadores} personas</div>
              </div>

              {/* Nómina de Jugadores y Seguro */}
              {reciboActual.jugadoresLista && reciboActual.jugadoresLista.length > 0 && (
                <div style={{ borderTop: '1px dashed #cbd5e1', paddingTop: 8 }}>
                  <div style={{ fontSize: '0.75rem', fontWeight: 700, color: '#1e293b', marginBottom: 4 }}>
                    Nómina de Jugadores y Póliza de Seguro Deportivo :
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 3, fontSize: '0.75rem', color: '#334155' }}>
                    {reciboActual.jugadoresLista.map((j, i) => (
                      <div key={i} style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span>#{i + 1} {j.nombre} {j.apellido} (DNI {j.dni})</span>
                        <span style={{ fontWeight: 600, color: '#0284c7' }}>Cobertura: {j.cobertura}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Accesorios */}
              <div style={{ borderTop: '1px dashed #cbd5e1', paddingTop: 8, fontSize: '0.75rem', color: '#334155' }}>
                <strong>Equipamiento Oficial Descontado:</strong> {reciboActual.equipamiento}
              </div>

              {/* QR */}
              <div style={{
                background: '#f8fafc',
                border: '1px solid #e2e8f0',
                padding: '10px 14px',
                borderRadius: 6,
                display: 'flex',
                alignItems: 'center',
                gap: 12,
              }}>
                <QrCode size={48} color="#0f172a" />
                <div style={{ fontSize: '0.75rem', color: '#334155' }}>
                  <strong>Cobranza Digital QR</strong>
                  <div>Medio de pago: {reciboActual.metodoPago}</div>
                  <div style={{ color: '#15803d', fontWeight: 700 }}>Seña 50% Registrada y Validada</div>
                </div>
              </div>

              {/* Detalle Financiero */}
              <div style={{
                background: '#f1f5f9',
                padding: '10px 14px',
                borderRadius: 6,
                display: 'flex',
                flexDirection: 'column',
                gap: 4,
                fontSize: '0.8125rem'
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <span>Costo Total Alquiler:</span>
                  <strong>${reciboActual.precioTotal?.toLocaleString('es-AR')}</strong>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', color: '#15803d', fontWeight: 700 }}>
                  <span>Seña Abonada (50%):</span>
                  <span>-${reciboActual.senaAbonada?.toLocaleString('es-AR')}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', color: '#b91c1c', fontWeight: 700, borderTop: '1px solid #cbd5e1', paddingTop: 4 }}>
                  <span>Saldo a pagar al terminar el turno :</span>
                  <span>${reciboActual.saldoPendiente?.toLocaleString('es-AR')}</span>
                </div>
              </div>

              <small style={{ fontSize: '0.6875rem', color: '#64748b', textAlign: 'center' }}>
                * Cancelaciones con más de 6 hs de antelación obtienen reembolso del 100% de la seña .
              </small>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginTop: 16 }}>
              <button onClick={() => window.print()} className="btn btn-secondary">
                <Printer size={15} />
                <span>Imprimir Recibo</span>
              </button>
              <button onClick={() => setModalReciboOpen(false)} className="btn btn-primary">
                <span>Finalizar</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
