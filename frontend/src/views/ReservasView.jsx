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
  AlertTriangle,
  X,
  Printer,
  Sparkles,
  ShieldCheck,
  Plus,
  ShoppingBag
} from 'lucide-react';

export default function ReservasView() {
  const { user } = useAuth();
  const [canchas, setCanchas] = useState([]);
  const [reservas, setReservas] = useState([]);
  const [loading, setLoading] = useState(true);

  // Modal Alquilar Cancha
  const [modalReservarOpen, setModalReservarOpen] = useState(false);
  const [modalReciboOpen, setModalReciboOpen] = useState(false);
  const [reciboActual, setReciboActual] = useState(null);

  // Form State
  const [canchaSeleccionadaId, setCanchaSeleccionadaId] = useState('');
  const [fecha, setFecha] = useState(new Date().toISOString().slice(0, 10));
  const [horaInicio, setHoraInicio] = useState('18:00');
  const [duracionHoras, setDuracionHoras] = useState(1); // 1 o 2 horas max [RD-12]
  const [cantidadJugadores, setCantidadJugadores] = useState(2); // 2 o 4 personas [RD-10]
  const [jugadoresNombres, setJugadoresNombres] = useState('');
  const [socioDni, setSocioDni] = useState(user?.dni || 43249050);
  const [socioNombre, setSocioNombre] = useState(user?.nombreCompleto || 'Franco Guanes');

  // Equipamiento deportivo de stock [RD-14]
  const [equipamiento, setEquipamiento] = useState({
    raquetas: 2,
    tubosPelotas: 1,
    redes: true,
  });

  // Pago
  const [metodoPago, setMetodoPago] = useState('Mercado Pago (QR)');
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    cargarDatos();
  }, []);

  async function cargarDatos() {
    setLoading(true);
    try {
      const [listaCanchas, listaReservas] = await Promise.all([
        api.canchas.listar(),
        api.reservas.listar(),
      ]);
      setCanchas(listaCanchas);
      if (listaCanchas.length > 0 && !canchaSeleccionadaId) {
        setCanchaSeleccionadaId(listaCanchas[0].id);
      }
      setReservas(listaReservas);
    } catch (err) {
      console.error('Error cargando datos de canchas y reservas:', err);
    } finally {
      setLoading(false);
    }
  }

  // Cálculos de costos según especificación (Seña 50% [RD-06])
  const canchaActual = canchas.find((c) => c.id === parseInt(canchaSeleccionadaId, 10)) || canchas[0];
  const precioBase = (canchaActual?.precioHora || 10000) * duracionHoras;
  const precioEquipamiento = (equipamiento.raquetas * 1500) + (equipamiento.tubosPelotas * 1200);
  const precioTotal = precioBase + precioEquipamiento;
  const senaRequerida = precioTotal * 0.50; // 50% en concepto de seña [RD-06, RD-08]
  const saldoRestante = precioTotal - senaRequerida; // 50% al finalizar [RD-07]

  // Validar antelación máxima de 30 días [RD-13]
  const maxFechaAnticipacion = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
  const minFecha = new Date().toISOString().slice(0, 10);

  const handleCrearReserva = async (e) => {
    e.preventDefault();
    setFormError('');

    if (duracionHoras > 2) {
      setFormError('Cada alquiler de cancha tiene una duración máxima de 2 horas.');
      return;
    }

    if (fecha > maxFechaAnticipacion) {
      setFormError('Las reservas sólo pueden realizarse con un máximo de 30 días de antelación.');
      return;
    }

    setSubmitting(true);
    try {
      const equipamientoDetalle = `${equipamiento.raquetas} Raqueta(s) + ${equipamiento.tubosPelotas} Tubo(s) de Pelotas${equipamiento.redes ? ' + Red Oficial' : ''}`;

      const horarioString = `${horaInicio} - ${parseInt(horaInicio.slice(0, 2), 10) + parseInt(duracionHoras, 10)}:00`;

      const nueva = await api.reservas.crear({
        canchaId: canchaActual.id,
        canchaNombre: canchaActual.nombre,
        fecha,
        horario: horarioString,
        duracionHoras: parseInt(duracionHoras, 10),
        jugadores: parseInt(cantidadJugadores, 10),
        socioNombre,
        socioDni: parseInt(socioDni, 10),
        precioTotal,
        senaAbonada: senaRequerida,
        saldoPendiente: saldoRestante,
        metodoPago,
        equipamiento: equipamientoDetalle,
      });

      setReciboActual(nueva);
      setModalReservarOpen(false);
      setModalReciboOpen(true);
      await cargarDatos();
    } catch (err) {
      setFormError(err.message || 'Error al procesar reserva');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancelarReserva = async (reserva) => {
    const confirmMsg = `¿Deseás cancelar la reserva #${reserva.id} en ${reserva.canchaNombre}?\n\n` +
      `Política oficial del club:\n` +
      `- Con más de 6 hs de antelación: Reembolso total del 100% de la seña.\n` +
      `- Fuera de término: Aplica cargo por cancelación tardía.`;

    if (!window.confirm(confirmMsg)) return;

    try {
      await api.reservas.cancelar(reserva.id);
      alert('Reserva cancelada correctamente.');
      await cargarDatos();
    } catch (err) {
      alert(`Error al cancelar: ${err.message}`);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2 style={{ fontSize: '1.5rem', display: 'flex', alignItems: 'center', gap: 10 }}>
            <Calendar color="var(--accent-volt)" size={26} />
            Alquiler de Canchas y Reservas Online
          </h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>
            Alquiler de canchas por hora, disponibilidad en vivo y confirmación inmediata con seña del 50%
          </p>
        </div>

        <button onClick={() => setModalReservarOpen(true)} className="btn btn-volt">
          <Plus size={18} />
          <span>Nueva Reserva de Cancha</span>
        </button>
      </div>

      {/* Tarjetas de Canchas Disponibles */}
      <div>
        <h3 style={{ fontSize: '1.15rem', marginBottom: 12, display: 'flex', alignItems: 'center', gap: 8 }}>
          <Sparkles size={18} color="var(--accent-volt)" />
          Infraestructura de Canchas del Complejo
        </h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
          {canchas.map((c) => (
            <div
              key={c.id}
              style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-md)',
                padding: '20px',
                display: 'flex',
                flexDirection: 'column',
                gap: 12,
                position: 'relative',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <span className={`badge ${c.tipo === 'Ladrillo' ? 'badge-warning' : c.tipo === 'Pasto' ? 'badge-success' : 'badge-volt'}`}>
                  {c.tipo}
                </span>
                <span className="badge badge-success" style={{ fontSize: '0.65rem' }}>Habilitada</span>
              </div>

              <div>
                <h4 style={{ fontSize: '1.15rem', color: 'var(--text-main)', marginBottom: 4 }}>{c.nombre}</h4>
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Superficie: {c.superficie || c.tipo}</p>
              </div>

              <div style={{
                background: 'var(--bg-dark)',
                padding: '10px 14px',
                borderRadius: 'var(--radius-sm)',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                border: '1px solid var(--border)'
              }}>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>Arancel por Hora:</span>
                <strong style={{ color: 'var(--accent-volt)', fontSize: '1.05rem' }}>
                  ${(c.precioHora || 10000).toLocaleString('es-AR')}
                </strong>
              </div>

              <button
                onClick={() => {
                  setCanchaSeleccionadaId(c.id);
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
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-lg)',
        padding: '24px',
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: 8 }}>
            <Clock size={20} color="var(--primary)" />
            Reservas Activas y Turnos Asignados
          </h3>
          <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Total registradas: {reservas.length}
          </span>
        </div>

        {loading ? (
          <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-muted)' }}>Cargando reservas...</div>
        ) : reservas.length === 0 ? (
          <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
            No hay reservas registradas en este momento.
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                  <th style={{ padding: '12px 16px' }}>Recibo #</th>
                  <th style={{ padding: '12px 16px' }}>Cancha y Superficie</th>
                  <th style={{ padding: '12px 16px' }}>Fecha y Turno</th>
                  <th style={{ padding: '12px 16px' }}>Socio / Jugadores</th>
                  <th style={{ padding: '12px 16px' }}>Seña 50% Abonada</th>
                  <th style={{ padding: '12px 16px' }}>Saldo Final</th>
                  <th style={{ padding: '12px 16px' }}>Estado</th>
                  <th style={{ padding: '12px 16px', textAlign: 'right' }}>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {reservas.map((r) => (
                  <tr key={r.id} style={{ borderBottom: '1px solid var(--border)' }}>
                    <td style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--accent-volt)' }}>
                      {r.numeroRecibo || `#${r.id}`}
                    </td>
                    <td style={{ padding: '14px 16px', fontWeight: 600 }}>{r.canchaNombre}</td>
                    <td style={{ padding: '14px 16px' }}>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <span>{r.fecha}</span>
                        <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{r.horario} ({r.duracionHoras}h)</span>
                      </div>
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <span>{r.socioNombre}</span>
                        <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>{r.jugadores} jugadores · DNI {r.socioDni}</span>
                      </div>
                    </td>
                    <td style={{ padding: '14px 16px', color: 'var(--primary)', fontWeight: 700 }}>
                      ${(r.senaAbonada || 0).toLocaleString('es-AR')}
                    </td>
                    <td style={{ padding: '14px 16px', color: 'var(--text-muted)' }}>
                      ${(r.saldoPendiente || 0).toLocaleString('es-AR')}
                    </td>
                    <td style={{ padding: '14px 16px' }}>
                      <span className="badge badge-success" style={{ fontSize: '0.7rem' }}>
                        {r.estado || 'Confirmada'}
                      </span>
                    </td>
                    <td style={{ padding: '14px 16px', textAlign: 'right' }}>
                      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                        <button
                          onClick={() => {
                            setReciboActual(r);
                            setModalReciboOpen(true);
                          }}
                          className="btn btn-secondary btn-sm"
                          title="Ver e imprimir recibo oficial"
                        >
                          <Printer size={14} />
                        </button>
                        <button
                          onClick={() => handleCancelarReserva(r)}
                          className="btn btn-danger btn-sm"
                          title="Cancelar reserva"
                        >
                          <X size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* MODAL ALQUILAR CANCHA (RF-24, RD-06, RD-10, RD-12, RD-14, RD-65) */}
      {modalReservarOpen && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 620 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Calendar color="var(--accent-volt)" size={22} />
                Alquiler de Cancha de Tenis
              </h3>
              <button onClick={() => setModalReservarOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {formError && <div className="badge badge-danger" style={{ display: 'block', padding: 10, marginBottom: 16 }}>{formError}</div>}

            <form onSubmit={handleCrearReserva} style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              {/* Selección de Cancha */}
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                  Seleccionar Cancha (Ladrillo, Cemento o Pasto)
                </label>
                <select
                  className="input"
                  value={canchaSeleccionadaId}
                  onChange={(e) => setCanchaSeleccionadaId(e.target.value)}
                >
                  {canchas.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.nombre} · ${c.precioHora?.toLocaleString('es-AR')}/h
                    </option>
                  ))}
                </select>
              </div>

              {/* Fecha y Horario */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                    Fecha (Hasta 30 días)
                  </label>
                  <input
                    type="date"
                    required
                    min={minFecha}
                    max={maxFechaAnticipacion}
                    className="input"
                    value={fecha}
                    onChange={(e) => setFecha(e.target.value)}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                    Hora de Inicio
                  </label>
                  <select
                    className="input"
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
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                    Duración (Máx 2h)
                  </label>
                  <select
                    className="input"
                    value={duracionHoras}
                    onChange={(e) => setDuracionHoras(Number(e.target.value))}
                  >
                    <option value={1}>1 Hora</option>
                    <option value={2}>2 Horas (Máximo)</option>
                  </select>
                </div>
              </div>

              {/* Jugadores [RD-10, RD-11] */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: 12 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                    Modalidad
                  </label>
                  <select
                    className="input"
                    value={cantidadJugadores}
                    onChange={(e) => setCantidadJugadores(Number(e.target.value))}
                  >
                    <option value={2}>2 Jugadores (Singles)</option>
                    <option value={4}>4 Jugadores (Dobles)</option>
                  </select>
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                    Socio Titular y DNI
                  </label>
                  <div style={{ display: 'flex', gap: 8 }}>
                    <input
                      type="text"
                      required
                      placeholder="Nombre y Apellido"
                      className="input"
                      value={socioNombre}
                      onChange={(e) => setSocioNombre(e.target.value)}
                    />
                    <input
                      type="number"
                      required
                      placeholder="DNI"
                      className="input"
                      style={{ maxWidth: 130 }}
                      value={socioDni}
                      onChange={(e) => setSocioDni(e.target.value)}
                    />
                  </div>
                </div>
              </div>

              {/* Equipamiento Deportivo de Stock [RD-14, RD-15] */}
              <div style={{
                background: 'var(--bg-elevated)',
                padding: '14px',
                borderRadius: 'var(--radius-md)',
                display: 'flex',
                flexDirection: 'column',
                gap: 8,
              }}>
                <span style={{ fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 6 }}>
                  <ShoppingBag size={15} color="var(--accent-volt)" />
                  Equipamiento Deportivo (Descontado de Stock en Reserva)
                </span>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 10, fontSize: '0.8rem' }}>
                  <div>
                    <label style={{ display: 'block', marginBottom: 4, color: 'var(--text-muted)' }}>Raquetas ($1.500 c/u)</label>
                    <select
                      className="input"
                      value={equipamiento.raquetas}
                      onChange={(e) => setEquipamiento({ ...equipamiento, raquetas: Number(e.target.value) })}
                    >
                      <option value={0}>Sin raquetas</option>
                      <option value={2}>2 Raquetas</option>
                      <option value={4}>4 Raquetas</option>
                    </select>
                  </div>

                  <div>
                    <label style={{ display: 'block', marginBottom: 4, color: 'var(--text-muted)' }}>Pelotas ($1.200)</label>
                    <select
                      className="input"
                      value={equipamiento.tubosPelotas}
                      onChange={(e) => setEquipamiento({ ...equipamiento, tubosPelotas: Number(e.target.value) })}
                    >
                      <option value={0}>Sin pelotas</option>
                      <option value={1}>1 Tubo x3</option>
                      <option value={2}>2 Tubos x6</option>
                    </select>
                  </div>

                  <div>
                    <label style={{ display: 'block', marginBottom: 4, color: 'var(--text-muted)' }}>Red de Pista</label>
                    <div style={{ height: 42, display: 'flex', alignItems: 'center', color: 'var(--accent-volt)', fontWeight: 600 }}>
                      ✓ Instalada Oficial
                    </div>
                  </div>
                </div>
              </div>

              {/* Desglose Financiero con Seña del 50% [RD-06, RD-07, RD-08] */}
              <div style={{
                background: 'rgba(34, 197, 94, 0.08)',
                border: '1px solid rgba(34, 197, 94, 0.3)',
                borderRadius: 'var(--radius-md)',
                padding: '16px',
                display: 'flex',
                flexDirection: 'column',
                gap: 8,
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem' }}>
                  <span style={{ color: 'var(--text-muted)' }}>Total del Alquiler ({duracionHoras}h + Accesorios):</span>
                  <span style={{ fontWeight: 600 }}>${precioTotal.toLocaleString('es-AR')}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '1rem', borderTop: '1px solid var(--border)', paddingTop: 8 }}>
                  <span style={{ color: 'var(--accent-volt)', fontWeight: 700 }}>Seña requerida ahora (50% obligatorio):</span>
                  <span style={{ color: 'var(--accent-volt)', fontWeight: 800, fontSize: '1.2rem' }}>
                    ${senaRequerida.toLocaleString('es-AR')}
                  </span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                  <span>Saldo a abonar al finalizar el partido:</span>
                  <span>${saldoRestante.toLocaleString('es-AR')}</span>
                </div>
              </div>

              {/* Medio de Pago */}
              <div>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: 6, color: 'var(--text-muted)' }}>
                  Medio de Cobranza de la Seña
                </label>
                <select
                  className="input"
                  value={metodoPago}
                  onChange={(e) => setMetodoPago(e.target.value)}
                >
                  <option value="Mercado Pago (QR)">Billetera Virtual Mercado Pago (con QR)</option>
                  <option value="Tarjeta de Débito">Tarjeta de Débito</option>
                  <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
                </select>
              </div>

              {/* Botones de acción */}
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 12 }}>
                <button type="button" onClick={() => setModalReservarOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submitting} className="btn btn-volt">
                  {submitting ? 'Confirmando...' : `Abonar Seña $${senaRequerida.toLocaleString('es-AR')} y Confirmar`}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL RECIBO OFICIAL DE PAGO Y RESERVA */}
      {modalReciboOpen && reciboActual && (
        <div className="modal-backdrop">
          <div className="modal-content" style={{ maxWidth: 500, padding: 32 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <CheckCircle2 color="var(--primary)" size={24} />
                Comprobante Oficial de Reserva
              </h3>
              <button onClick={() => setModalReciboOpen(false)} style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Ficha Imprimible del Recibo */}
            <div id="recibo-print" style={{
              background: '#fff',
              color: '#000',
              padding: '24px',
              borderRadius: 'var(--radius-md)',
              display: 'flex',
              flexDirection: 'column',
              gap: 16,
              boxShadow: '0 4px 20px rgba(0,0,0,0.3)',
            }}>
              <div style={{ borderBottom: '2px solid #000', paddingBottom: 12, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <h2 style={{ fontSize: '1.25rem', margin: 0, fontWeight: 900, color: '#0f1d18' }}>TENIS AHORA</h2>
                  <small style={{ color: '#555' }}>Complejo Deportivo "El Deporte Saludable"</small>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <span style={{ fontSize: '0.85rem', fontWeight: 800 }}>RECIBO OFICIAL</span>
                  <div style={{ fontSize: '0.8rem', color: '#666' }}>{reciboActual.numeroRecibo}</div>
                </div>
              </div>

              <div style={{ fontSize: '0.85rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <div><strong>Socio:</strong> {reciboActual.socioNombre}</div>
                <div><strong>DNI:</strong> {reciboActual.socioDni}</div>
                <div><strong>Pista:</strong> {reciboActual.canchaNombre}</div>
                <div><strong>Fecha:</strong> {reciboActual.fecha}</div>
                <div><strong>Turno:</strong> {reciboActual.horario}</div>
                <div><strong>Jugadores:</strong> {reciboActual.jugadores} pers.</div>
              </div>

              <div style={{ borderTop: '1px dashed #999', paddingTop: 8, fontSize: '0.8rem', color: '#444' }}>
                <strong>Accesorios asignados:</strong> {reciboActual.equipamiento}
              </div>

              {/* Simulación QR de Mercado Pago */}
              <div style={{
                background: '#f8fafc',
                border: '1px solid #cbd5e1',
                padding: '12px',
                borderRadius: 8,
                display: 'flex',
                alignItems: 'center',
                gap: 14,
              }}>
                <QrCode size={56} color="#0284c7" />
                <div style={{ fontSize: '0.75rem', color: '#334155' }}>
                  <strong>Cobranza Digital QR</strong>
                  <div>Medio: {reciboActual.metodoPago}</div>
                  <div style={{ color: '#16a34a', fontWeight: 700 }}>Seña 50% VALIDADA</div>
                </div>
              </div>

              <div style={{
                background: '#f1f5f9',
                padding: '12px',
                borderRadius: 6,
                display: 'flex',
                flexDirection: 'column',
                gap: 4,
                fontSize: '0.85rem'
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
                  <span>Saldo a pagar al terminar:</span>
                  <span>${reciboActual.saldoPendiente?.toLocaleString('es-AR')}</span>
                </div>
              </div>

              <small style={{ fontSize: '0.65rem', color: '#64748b', textAlign: 'center' }}>
                * Cancelaciones con más de 6 hs de antelación obtienen reembolso del 100% de la seña abonada.
              </small>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginTop: 20 }}>
              <button onClick={() => window.print()} className="btn btn-secondary">
                <Printer size={16} />
                <span>Imprimir Recibo</span>
              </button>
              <button onClick={() => setModalReciboOpen(false)} className="btn btn-volt">
                <span>Finalizar</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
