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
  CheckCheck,
  Edit2,
  Trash2,
  ExternalLink,
  DollarSign,
  Wrench
} from 'lucide-react';

export default function ReservasView() {
  const { user } = useAuth();
  const [canchas, setCanchas] = useState([]);
  const [reservas, setReservas] = useState([]);
  const isAdmin = user?.rol === 'Administrador' || user?.rol === 1;
  const reservasVisibles = isAdmin
    ? reservas
    : reservas.filter(r => String(r.socioDni) === String(user?.dni) || (r.jugadoresDetalle && r.jugadoresDetalle.some(j => String(j.dni) === String(user?.dni))));
  const [stockInventario, setStockInventario] = useState({
    pelotas: { id: 1, stock: 0, nombre: 'Tubo de Pelotas' },
    raquetas: { id: 2, stock: 0, nombre: 'Raquetas' },
    redes: { id: 3, stock: 0, nombre: 'Red de Tenis' },
  });
  const [loading, setLoading] = useState(true);

  // Modales
  const [modalReservarOpen, setModalReservarOpen] = useState(false);
  const [modalCanchaOpen, setModalCanchaOpen] = useState(false);
  const [modalFinalizarOpen, setModalFinalizarOpen] = useState(false);
  const [modalReciboOpen, setModalReciboOpen] = useState(false);
  const [reciboActual, setReciboActual] = useState(null);

  // Estado para confirmación QR en recibo (Seña)
  const [inputPaymentId, setInputPaymentId] = useState('');
  const [confirmandoQr, setConfirmandoQr] = useState(false);
  const [qrConfirmError, setQrConfirmError] = useState('');

  // Cancha Form (Crear / Editar)
  const [canchaEditando, setCanchaEditando] = useState(null);
  const [formCancha, setFormCancha] = useState({
    tipoDeCanchaId: 1,
    fotoUrl: '/courts/ladrillo.jpg?v=3'
  });
  const [submittingCancha, setSubmittingCancha] = useState(false);
  const [canchaError, setCanchaError] = useState('');

  // Estado para Bloqueo por Mantenimiento (Solo Administrador)
  const [disponibilidadesPorCancha, setDisponibilidadesPorCancha] = useState({});
  const [modalMantenimientoOpen, setModalMantenimientoOpen] = useState(false);
  const [canchaMantenimiento, setCanchaMantenimiento] = useState(null);
  const [submittingMantenimiento, setSubmittingMantenimiento] = useState(false);
  const [mantenimientoError, setMantenimientoError] = useState('');
  const [formMantenimiento, setFormMantenimiento] = useState({
    fechaInicio: '',
    fechaFin: ''
  });

  const toLocalISOString = (date) => {
    const pad = (n) => String(n).padStart(2, '0');
    const year = date.getFullYear();
    const month = pad(date.getMonth() + 1);
    const day = pad(date.getDate());
    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const handleAbrirMantenimiento = (cancha) => {
    setCanchaMantenimiento(cancha);
    setMantenimientoError('');
    const ahora = new Date();
    const finDefault = new Date(ahora.getTime() + 2 * 60 * 60 * 1000);
    setFormMantenimiento({
      fechaInicio: toLocalISOString(ahora),
      fechaFin: toLocalISOString(finDefault)
    });
    setModalMantenimientoOpen(true);
  };

  const handleSetDuracionMantenimiento = (horas) => {
    const inicio = formMantenimiento.fechaInicio ? new Date(formMantenimiento.fechaInicio) : new Date();
    const fin = new Date(inicio.getTime() + horas * 60 * 60 * 1000);
    setFormMantenimiento(prev => ({
      ...prev,
      fechaFin: toLocalISOString(fin)
    }));
  };

  const handleGuardarMantenimiento = async (e) => {
    e.preventDefault();
    setMantenimientoError('');
    setSubmittingMantenimiento(true);
    try {
      if (!canchaMantenimiento) return;
      if (!formMantenimiento.fechaInicio || !formMantenimiento.fechaFin) {
        throw new Error('Debés indicar fecha y hora de inicio y fin.');
      }
      const ini = new Date(formMantenimiento.fechaInicio);
      const fin = new Date(formMantenimiento.fechaFin);
      if (fin <= ini) {
        throw new Error('La fecha/hora de fin debe ser posterior a la de inicio.');
      }

      await api.canchas.disponibilidad.crear(canchaMantenimiento.id, {
        canchaId: canchaMantenimiento.id,
        fechaHoraInicio: ini.toISOString(),
        fechaHoraFin: fin.toISOString(),
        estado: 1 // Mantenimiento
      });

      alert(`Pista ${canchaMantenimiento.id} bloqueada por mantenimiento con éxito.`);
      setModalMantenimientoOpen(false);
      await cargarDatos();
    } catch (err) {
      setMantenimientoError(err.message || 'Error al registrar mantenimiento');
    } finally {
      setSubmittingMantenimiento(false);
    }
  };

  const handleLevantarMantenimiento = async (canchaId, dispId) => {
    if (!window.confirm('¿Confirmás levantar este mantenimiento y rehabilitar la pista?')) return;
    try {
      await api.canchas.disponibilidad.eliminar(canchaId, dispId);
      alert('Mantenimiento levantado con éxito. La cancha ha sido rehabilitada.');
      setModalMantenimientoOpen(false);
      await cargarDatos();
    } catch (err) {
      alert('Error al levantar mantenimiento: ' + err.message);
    }
  };

  const fotosPorSuperficieId = {
    1: '/courts/ladrillo.jpg?v=3',
    2: '/courts/cemento.jpg?v=3',
    3: '/courts/pasto.jpg?v=5',
  };

  const handleAbrirCrearCancha = () => {
    setCanchaEditando(null);
    setFormCancha({
      tipoDeCanchaId: 1,
      fotoUrl: fotosPorSuperficieId[1]
    });
    setCanchaError('');
    setModalCanchaOpen(true);
  };

  const handleAbrirEditarCancha = (cancha) => {
    setCanchaEditando(cancha);
    setFormCancha({
      tipoDeCanchaId: cancha.tipoDeCanchaId || 1,
      fotoUrl: cancha.fotoUrl || fotosPorSuperficieId[cancha.tipoDeCanchaId || 1]
    });
    setCanchaError('');
    setModalCanchaOpen(true);
  };

  const handleGuardarCancha = async (e) => {
    e.preventDefault();
    setCanchaError('');
    setSubmittingCancha(true);
    try {
      const tipoId = parseInt(formCancha.tipoDeCanchaId, 10);
      if (!tipoId || isNaN(tipoId)) throw new Error('Seleccioná un tipo de cancha válido.');

      if (canchaEditando) {
        await api.canchas.actualizar(canchaEditando.id, {
          id: canchaEditando.id,
          tipoDeCanchaId: tipoId,
          fotoUrl: formCancha.fotoUrl?.trim() || undefined
        });
        alert('Cancha actualizada exitosamente con su fotografía registrada en la base de datos.');
      } else {
        await api.canchas.crear({
          tipoDeCanchaId: tipoId,
          fotoUrl: formCancha.fotoUrl?.trim() || undefined
        });
        alert('Cancha registrada con éxito con la URL de la foto en la base de datos.');
      }
      setModalCanchaOpen(false);
      setCanchaEditando(null);
      cargarDatos();
    } catch (err) {
      setCanchaError(err.message);
    } finally {
      setSubmittingCancha(false);
    }
  };

  const handleEliminarCancha = async (canchaId, nombre) => {
    if (!window.confirm('¿Seguro que deseás eliminar ' + nombre + '? Esta acción eliminará la cancha permanentemente de la base de datos.')) return;
    try {
      await api.canchas.eliminar(canchaId);
      alert('Cancha eliminada correctamente.');
      cargarDatos();
    } catch (err) {
      alert('No se pudo eliminar la cancha: ' + err.message);
    }
  };

  // Form State para la Reserva
  const [canchaSeleccionadaId, setCanchaSeleccionadaId] = useState('');
  const [fecha, setFecha] = useState(new Date(Date.now() + 86400000).toISOString().slice(0, 10));
  const [horaInicio, setHoraInicio] = useState('18:00');
  const [duracionHoras, setDuracionHoras] = useState(1); // 1 o 2 horas
  const [cantidadJugadores, setCantidadJugadores] = useState(2); // 2 o 4 personas
  const [traeEquipamientoPropio, setTraeEquipamientoPropio] = useState(false);

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

  // Pago de Seña (50%)
  const [metodoPago, setMetodoPago] = useState('Mercado Pago (QR)');
  const [descuentoSeleccionado, setDescuentoSeleccionado] = useState(0);
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [accionEnCurso, setAccionEnCurso] = useState(null);

  // Formulario de Tarjeta para Seña
  const [tarjetaForm, setTarjetaForm] = useState({
    numeroTarjeta: '',
    nombreTitular: 'APRO',
    mesVencimiento: '12',
    anioVencimiento: '2028',
    cvv: '123',
    paymentMethodIdMp: 'debvisa',
    cuotas: 1
  });

  // Modal de Cobro Saldo Final (50%) y Finalizar Reserva [RF-65, RD-07, RD-16]
  const [reservaParaFinalizar, setReservaParaFinalizar] = useState(null);
  const [metodoPagoFinal, setMetodoPagoFinal] = useState('Mercado Pago (QR)');
  const [cobroFinalQrData, setCobroFinalQrData] = useState(null);
  const [inputPaymentIdFinal, setInputPaymentIdFinal] = useState('');
  const [submittingFinal, setSubmittingFinal] = useState(false);
  const [finalizarError, setFinalizarError] = useState('');
  const [tarjetaFinalForm, setTarjetaFinalForm] = useState({
    numeroTarjeta: '',
    nombreTitular: 'APRO',
    mesVencimiento: '12',
    anioVencimiento: '2028',
    cvv: '123',
    paymentMethodIdMp: 'debvisa',
    cuotas: 1
  });

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

      // Consultar disponibilidades de canchas (bloqueos por mantenimiento)
      const dispsMap = {};
      await Promise.all(
        (listaCanchas || []).map(async (c) => {
          try {
            const disps = await api.canchas.disponibilidad.listarPorCancha(c.id);
            dispsMap[c.id] = disps || [];
          } catch {
            dispsMap[c.id] = [];
          }
        })
      );
      setDisponibilidadesPorCancha(dispsMap);

      const ahora = new Date();

      // 1. Canchas
      const canchasNormalizadas = (listaCanchas || []).map((c) => {
        const disps = dispsMap[c.id] || [];
        const mantenimientoActivo = disps.find((d) => {
          const esMantenimiento = d.estado === 1 || d.estado === 'Mantenimiento';
          if (!esMantenimiento) return false;
          const ini = new Date(d.fechaHoraInicio);
          const fin = new Date(d.fechaHoraFin);
          return ahora >= ini && ahora <= fin;
        });

        const mantenimientosVigentes = disps.filter((d) => {
          const esMantenimiento = d.estado === 1 || d.estado === 'Mantenimiento';
          if (!esMantenimiento) return false;
          const fin = new Date(d.fechaHoraFin);
          return fin >= ahora;
        });

        return {
          id: c.id,
          nombre: `Pista ${c.id} (${c.superficie || 'Tenis'})`,
          tipo: c.superficie || 'Ladrillo',
          superficie: c.superficie || 'Polvo de Ladrillo',
          capacidad: c.capacidad || 4,
          precioHora: c.precio || 12000,
          tipoDeCanchaId: c.tipoDeCanchaId || (c.superficie === 'Pasto' ? 3 : c.superficie === 'Cemento' ? 2 : 1),
          fotoUrl: c.fotoUrl || (c.superficie === 'Pasto' ? '/courts/pasto.jpg?v=5' : c.superficie === 'Cemento' ? '/courts/cemento.jpg?v=3' : '/courts/ladrillo.jpg?v=3'),
          enMantenimiento: Boolean(mantenimientoActivo),
          mantenimientoActivo: mantenimientoActivo || null,
          mantenimientosVigentes: mantenimientosVigentes || []
        };
      });
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

        const total = r.importeTotal || r.importe || 12000;
        const sena = r.montoSena || (total * 0.5);
        const pendiente = Math.max(0, total - sena);

        return {
          id: r.id,
          numeroRecibo: `REC-2026-00${r.id}`,
          canchaId: r.canchaId,
          canchaNombre: `Pista ${r.canchaId}`,
          fecha: fechaFormateada,
          horario: horarioFormateado,
          duracionHoras: 1,
          jugadores: r.jugadoresDni?.length || r.jugadores?.length || 2,
          jugadoresDetalle: r.jugadores || [],
          socioNombre: r.socioNombre || (r.jugadores && r.jugadores[0] ? `${r.jugadores[0].nombre} ${r.jugadores[0].apellido}` : `Socio DNI ${r.socioDni}`),
          socioDni: r.socioDni,
          precioTotal: total,
          senaAbonada: sena,
          saldoPendiente: pendiente,
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

  // Cálculos de costos y descuentos
  const canchaActual = canchas.find((c) => c.id === parseInt(canchaSeleccionadaId, 10)) || canchas[0] || { precioHora: 12000, id: 1 };
  const precioBase = (canchaActual?.precioHora || 12000) * duracionHoras;
  const montoDescuento = (precioBase * descuentoSeleccionado) / 100;
  const precioTotal = Math.max(0, precioBase - montoDescuento);
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

  // Validación de mantenimiento para el horario seleccionado en el modal
  const turnoInicioDt = new Date(`${fecha}T${horaInicio}:00`);
  const turnoFinDt = new Date(turnoInicioDt.getTime() + duracionHoras * 3600000);
  const dispsCanchaSeleccionada = disponibilidadesPorCancha[canchaSeleccionadaId] || [];
  const tieneMantenimientoEnHorario = dispsCanchaSeleccionada.some((d) => {
    const esMantenimiento = d.estado === 1 || d.estado === 'Mantenimiento';
    if (!esMantenimiento) return false;
    const mIni = new Date(d.fechaHoraInicio);
    const mFin = new Date(d.fechaHoraFin);
    return turnoInicioDt < mFin && turnoFinDt > mIni;
  });

  // Manejo de cambio en los datos de un jugador
  const handleActualizarJugador = (index, campo, valor) => {
    setJugadores(prev => {
      const copia = [...prev];
      copia[index] = { ...copia[index], [campo]: valor };
      return copia;
    });
  };

  // Manejo del cambio en método de pago
  const handleCambiarMetodoPago = (metodo) => {
    setMetodoPago(metodo);
    if (metodo === 'Tarjeta de Débito') {
      setTarjetaForm(prev => ({ ...prev, paymentMethodIdMp: 'debvisa', cuotas: 1 }));
    } else if (metodo === 'Tarjeta de Crédito') {
      setTarjetaForm(prev => ({ ...prev, paymentMethodIdMp: 'visa', cuotas: 1 }));
    }
  };

  // ========================================================
  // PASO 2: CREACIÓN DE RESERVA Y COBRO DE SEÑA 50% REAL
  // [RF-24, RF-65, RD-06, RD-08, RF-69]
  // ========================================================
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

    if (!hayStockGeneral) {
      const faltantes = [];
      if (!hayStockPelotas) faltantes.push(`Tubo de Pelotas (requerido: 1, disponible: ${stockInventario.pelotas.stock})`);
      if (!hayStockRedes) faltantes.push(`Red de Tenis (requerido: 1, disponible: ${stockInventario.redes.stock})`);
      if (!hayStockRaquetas) faltantes.push(`Raquetas (requeridas: ${stockRaquetasRequerido}, disponibles: ${stockInventario.raquetas.stock})`);
      setFormError(`No se puede realizar la reserva: Stock insuficiente de ${faltantes.join('; ')}.`);
      return;
    }

    const jugadoresActivos = jugadores.slice(0, cantidadJugadores);

    for (let i = 0; i < jugadoresActivos.length; i++) {
      const j = jugadoresActivos[i];
      if (!j.nombre || !j.apellido || !j.dni || !j.cobertura) {
        setFormError(`Por favor complete todos los datos del Jugador #${i + 1} (Nombre, Apellido, DNI y Cobertura Médica para el seguro).`);
        return;
      }
    }

    const dnis = jugadoresActivos.map(j => parseInt(j.dni, 10));
    if (new Set(dnis).size !== dnis.length) {
      setFormError('La lista de jugadores contiene números de DNI duplicados.');
      return;
    }

    const esTarjeta = metodoPago === 'Tarjeta de Débito' || metodoPago === 'Tarjeta de Crédito';
    if (esTarjeta) {
      if (!tarjetaForm.numeroTarjeta || tarjetaForm.numeroTarjeta.length < 15) {
        setFormError('Por favor ingrese un número de tarjeta válido (15 o 16 dígitos).');
        return;
      }
      if (!tarjetaForm.nombreTitular.trim()) {
        setFormError('Por favor ingrese el nombre del titular de la tarjeta.');
        return;
      }
      if (!tarjetaForm.cvv || tarjetaForm.cvv.length < 3) {
        setFormError('Por favor ingrese el código de seguridad (CVV) de la tarjeta.');
        return;
      }
    }

    setSubmitting(true);
    try {
      const fechaBase = new Date(`${fecha}T${horaInicio}:00`);
      const fechaFin = new Date(fechaBase.getTime() + duracionHoras * 3600000);
      const titularDni = dnis[0];

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

      // Paso A: Crear la reserva en el backend
      const reservaCreada = await api.reservas.crear(payload);

      let cobro = null;

      if (esTarjeta) {
        // Paso B.1: Tokenizar la tarjeta con Mercado Pago
        const tokenTarjetaMp = await api.mercadopago.crearTokenTarjeta({
          numeroTarjeta: tarjetaForm.numeroTarjeta,
          nombreTitular: tarjetaForm.nombreTitular.trim(),
          mesVencimiento: tarjetaForm.mesVencimiento,
          anioVencimiento: tarjetaForm.anioVencimiento,
          cvv: tarjetaForm.cvv,
          dni: titularDni
        });

        // Paso B.2: Registrar cobro de seña (50%) en el backend
        // TipoDePago: TarjetaCredito = 0, TarjetaDebito = 1
        const tipoPagoEnum = metodoPago === 'Tarjeta de Crédito' ? 0 : 1;
        cobro = await api.cobros.registrar({
          socioDni: titularDni,
          importeBase: precioTotal,
          tipoPago: tipoPagoEnum,
          esSena50Porciento: true,
          esSaldoFinalPartido: false,
          reservaId: reservaCreada.id,
          tokenTarjetaMp,
          paymentMethodIdMp: tarjetaForm.paymentMethodIdMp,
          cuotas: metodoPago === 'Tarjeta de Débito' ? 1 : Number(tarjetaForm.cuotas),
        });
      } else {
        // Paso C.1: Cobro QR en Mercado Pago (TipoDePago.QR = 2)
        cobro = await api.cobros.registrar({
          socioDni: titularDni,
          importeBase: precioTotal,
          tipoPago: 2,
          esSena50Porciento: true,
          esSaldoFinalPartido: false,
          reservaId: reservaCreada.id,
        });
      }

      // Preparar el recibo oficial con los datos reales del cobro
      const nuevoRecibo = {
        id: reservaCreada.id,
        numeroCobro: cobro.numeroCobro,
        reciboId: cobro.reciboId,
        numeroRecibo: cobro.reciboId ? `REC-2026-00${cobro.reciboId}` : `COBRO-#${cobro.numeroCobro}`,
        canchaNombre: canchaActual.nombre,
        fecha,
        horario: `${horaInicio} (${duracionHoras}h)`,
        jugadores: cantidadJugadores,
        jugadoresLista: jugadoresActivos,
        socioNombre: `${jugadoresActivos[0].nombre} ${jugadoresActivos[0].apellido}`,
        socioDni: titularDni,
        precioTotal: reservaCreada.importeTotal || precioTotal,
        senaAbonada: cobro.importe || senaRequerida,
        saldoPendiente: (reservaCreada.importeTotal || precioTotal) - (cobro.importe || senaRequerida),
        descuentoPorcentaje: descuentoSeleccionado,
        montoDescuento,
        metodoPago,
        tipoPago: cobro.tipoPago,
        estadoCobro: cobro.estado, // "Validado" o "Pendiente"
        mercadoPagoPaymentId: cobro.mercadoPagoPaymentId,
        urlPagoQr: cobro.urlPagoQr,
        codigoQrBase64: cobro.codigoQrBase64,
        equipamiento: `1x Red Oficial, 1x Tubo de Pelotas (x3), ${cantidadJugadores}x Raquetas de Tenis`,
      };

      setReciboActual(nuevoRecibo);
      setInputPaymentId('');
      setQrConfirmError('');
      setModalReservarOpen(false);
      setModalReciboOpen(true);
      await cargarDatos();
    } catch (err) {
      setFormError(err.message || 'Error al procesar reserva y cobro de seña en el backend.');
    } finally {
      setSubmitting(false);
    }
  };

  // Confirmar acreditación de pago QR en el Modal de Recibo
  const handleConfirmarAcreditacionQr = async () => {
    if (!reciboActual?.numeroCobro || !inputPaymentId.trim()) return;
    setQrConfirmError('');
    setConfirmandoQr(true);
    try {
      const cobroConfirmado = await api.cobros.confirmarQr(reciboActual.numeroCobro, inputPaymentId.trim());
      setReciboActual(prev => ({
        ...prev,
        estadoCobro: 'Validado',
        reciboId: cobroConfirmado.reciboId,
        numeroRecibo: cobroConfirmado.reciboId ? `REC-2026-00${cobroConfirmado.reciboId}` : prev.numeroRecibo,
        mercadoPagoPaymentId: cobroConfirmado.mercadoPagoPaymentId || inputPaymentId.trim(),
      }));
      setInputPaymentId('');
      await cargarDatos();
    } catch (err) {
      setQrConfirmError(err.message || 'Error al validar el pago en Mercado Pago.');
    } finally {
      setConfirmandoQr(false);
    }
  };


  // Finalizar turno directamente en caso de requerir cierre rápido del club
  const handleFinalizarReserva = async (reserva) => {
    const confirmMsg = '¿Confirmás la finalización del partido para la reserva #' + reserva.id + ' (' + reserva.canchaNombre + ')?\n\n' +
      'Operación oficial del club :\n' +
      '- Se actualizará el estado a Finalizada.\n' +
      '- Se reintegrarán automáticamente las raquetas, pelotas y red al stock disponible del inventario.\n' +
      '- Se cobrará el 50% restante ($ ' + (reserva.saldoPendiente || ((reserva.importe || 0) * 0.5)).toLocaleString('es-AR') + ') completando el 100% en caja.\n        - Se emitirá el registro de pago para el reporte de ingresos.';

    if (!window.confirm(confirmMsg)) return;

    setAccionEnCurso(reserva.id);
    try {
      await api.reservas.finalizar(reserva.id);
      alert('Partido #' + reserva.id + ' finalizado. Cancha liberada, saldo restante cobrado y equipamiento devuelto al stock.');
      await cargarDatos();
    } catch (err) {
      alert('Error al finalizar reserva: ' + err.message);
    } finally {
      setAccionEnCurso(null);
    }
  };

  // ========================================================
  // PASO 3: MODAL DE FINALIZACIÓN Y COBRO DEL 50% RESTANTE
  // [RF-65, RD-07, RD-16]
  // ========================================================
  const handleAbrirFinalizarReserva = (reserva) => {
    setReservaParaFinalizar(reserva);
    setMetodoPagoFinal('Mercado Pago (QR)');
    setCobroFinalQrData(null);
    setInputPaymentIdFinal('');
    setFinalizarError('');
    setTarjetaFinalForm({
      numeroTarjeta: '',
      nombreTitular: 'APRO',
      mesVencimiento: '12',
      anioVencimiento: '2028',
      cvv: '123',
      paymentMethodIdMp: 'debvisa',
      cuotas: 1
    });
    setModalFinalizarOpen(true);
  };

  // Cobro final con Tarjeta
  const handlePagarSaldoFinalTarjeta = async (e) => {
    e.preventDefault();
    if (!reservaParaFinalizar) return;
    setFinalizarError('');

    if (!tarjetaFinalForm.numeroTarjeta || tarjetaFinalForm.numeroTarjeta.length < 15) {
      setFinalizarError('Por favor ingrese un número de tarjeta válido (15 o 16 dígitos).');
      return;
    }

    setSubmittingFinal(true);
    try {
      const tokenTarjetaMp = await api.mercadopago.crearTokenTarjeta({
        numeroTarjeta: tarjetaFinalForm.numeroTarjeta,
        nombreTitular: tarjetaFinalForm.nombreTitular.trim(),
        mesVencimiento: tarjetaFinalForm.mesVencimiento,
        anioVencimiento: tarjetaFinalForm.anioVencimiento,
        cvv: tarjetaFinalForm.cvv,
        dni: reservaParaFinalizar.socioDni,
      });

      const tipoPagoEnum = metodoPagoFinal === 'Tarjeta de Crédito' ? 0 : 1;
      const cobro = await api.cobros.registrar({
        socioDni: reservaParaFinalizar.socioDni,
        importeBase: reservaParaFinalizar.precioTotal,
        tipoPago: tipoPagoEnum,
        esSena50Porciento: false,
        esSaldoFinalPartido: true,
        reservaId: reservaParaFinalizar.id,
        tokenTarjetaMp,
        paymentMethodIdMp: tarjetaFinalForm.paymentMethodIdMp,
        cuotas: metodoPagoFinal === 'Tarjeta de Débito' ? 1 : Number(tarjetaFinalForm.cuotas),
      });

      // Finalizar la reserva y restituir stock en el backend [RD-16]
      await api.reservas.finalizar(reservaParaFinalizar.id);
      setModalFinalizarOpen(false);

      const reciboFinal = {
        id: reservaParaFinalizar.id,
        numeroCobro: cobro.numeroCobro,
        reciboId: cobro.reciboId,
        numeroRecibo: cobro.reciboId ? `REC-2026-00${cobro.reciboId}` : `COBRO-#${cobro.numeroCobro}`,
        canchaNombre: reservaParaFinalizar.canchaNombre,
        fecha: reservaParaFinalizar.fecha,
        horario: reservaParaFinalizar.horario,
        jugadores: reservaParaFinalizar.jugadores,
        jugadoresLista: reservaParaFinalizar.jugadoresDetalle || [],
        socioNombre: reservaParaFinalizar.socioNombre,
        socioDni: reservaParaFinalizar.socioDni,
        precioTotal: reservaParaFinalizar.precioTotal,
        senaAbonada: reservaParaFinalizar.senaAbonada,
        saldoPendiente: 0,
        metodoPago: metodoPagoFinal,
        tipoPago: cobro.tipoPago,
        estadoCobro: 'Validado',
        mercadoPagoPaymentId: cobro.mercadoPagoPaymentId,
        equipamiento: reservaParaFinalizar.equipamiento,
      };

      setReciboActual(reciboFinal);
      setModalReciboOpen(true);
      await cargarDatos();
    } catch (err) {
      setFinalizarError(err.message || 'Error al procesar cobro de saldo final con tarjeta.');
    } finally {
      setSubmittingFinal(false);
    }
  };

  // Generar QR para saldo final
  const handleGenerarQrFinal = async () => {
    if (!reservaParaFinalizar) return;
    setFinalizarError('');
    setSubmittingFinal(true);
    try {
      const cobroQr = await api.cobros.registrar({
        socioDni: reservaParaFinalizar.socioDni,
        importeBase: reservaParaFinalizar.precioTotal,
        tipoPago: 2, // QR = 2
        esSena50Porciento: false,
        esSaldoFinalPartido: true,
        reservaId: reservaParaFinalizar.id,
      });
      setCobroFinalQrData(cobroQr);
    } catch (err) {
      setFinalizarError(err.message || 'Error al generar cobro QR para saldo final.');
    } finally {
      setSubmittingFinal(false);
    }
  };

  // Confirmar acreditación de QR y concluir partido
  const handleConfirmarQrFinal = async () => {
    if (!cobroFinalQrData || !inputPaymentIdFinal.trim() || !reservaParaFinalizar) return;
    setFinalizarError('');
    setSubmittingFinal(true);
    try {
      const cobroConfirmado = await api.cobros.confirmarQr(cobroFinalQrData.numeroCobro, inputPaymentIdFinal.trim());
      await api.reservas.finalizar(reservaParaFinalizar.id);
      setModalFinalizarOpen(false);

      const reciboFinal = {
        id: reservaParaFinalizar.id,
        numeroCobro: cobroConfirmado.numeroCobro,
        reciboId: cobroConfirmado.reciboId,
        numeroRecibo: cobroConfirmado.reciboId ? `REC-2026-00${cobroConfirmado.reciboId}` : `COBRO-#${cobroConfirmado.numeroCobro}`,
        canchaNombre: reservaParaFinalizar.canchaNombre,
        fecha: reservaParaFinalizar.fecha,
        horario: reservaParaFinalizar.horario,
        jugadores: reservaParaFinalizar.jugadores,
        jugadoresLista: reservaParaFinalizar.jugadoresDetalle || [],
        socioNombre: reservaParaFinalizar.socioNombre,
        socioDni: reservaParaFinalizar.socioDni,
        precioTotal: reservaParaFinalizar.precioTotal,
        senaAbonada: reservaParaFinalizar.senaAbonada,
        saldoPendiente: 0,
        metodoPago: 'Mercado Pago (QR)',
        tipoPago: 'QR',
        estadoCobro: 'Validado',
        mercadoPagoPaymentId: cobroConfirmado.mercadoPagoPaymentId || inputPaymentIdFinal.trim(),
        equipamiento: reservaParaFinalizar.equipamiento,
      };

      setReciboActual(reciboFinal);
      setModalReciboOpen(true);
      await cargarDatos();
    } catch (err) {
      setFinalizarError(err.message || 'Error al confirmar acreditación del pago QR.');
    } finally {
      setSubmittingFinal(false);
    }
  };

  // Cancelar reserva
  const handleCancelarReserva = async (reserva) => {
    const confirmMsg = `¿Deseás cancelar la reserva #${reserva.id} en ${reserva.canchaNombre}?\n\n` +
      `Política oficial del club:\n` +
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

  const esTarjeta = metodoPago === 'Tarjeta de Débito' || metodoPago === 'Tarjeta de Crédito';

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
            Control estricto de disponibilidad, nómina con seguro obligatorio y cobranza integrada con Mercado Pago.
          </p>
        </div>

        <button onClick={() => setModalReservarOpen(true)} className="btn btn-primary">
          <Plus size={16} />
          <span>Nueva Reserva de Cancha</span>
        </button>
      </div>

      {/* Monitor de Stock de Equipamiento Deportivo en Vivo */}
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
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 14, flexWrap: 'wrap', gap: 10 }}>
          <h3 style={{ fontSize: '1.1rem', margin: 0, display: 'flex', alignItems: 'center', gap: 8 }}>
            <Sparkles size={16} color="var(--primary)" />
            Infraestructura de Canchas del Complejo
          </h3>

          {user?.rol === 'Administrador' && (
            <button
              onClick={handleAbrirCrearCancha}
              className="btn btn-volt btn-sm"
              style={{ display: 'flex', alignItems: 'center', gap: 6 }}
            >
              <Plus size={16} />
              <span>Nueva Cancha</span>
            </button>
          )}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(290px, 1fr))', gap: 18 }}>
          {canchas.map((c) => (
            <div
              key={c.id}
              className="card"
              style={{
                padding: 0,
                overflow: 'hidden',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
              }}
            >
              <div style={{ position: 'relative', height: 140, overflow: 'hidden', background: 'var(--bg-elevated)' }}>
                <img
                  src={c.fotoUrl || (c.superficie && c.superficie.includes('Pasto') ? '/courts/pasto.jpg?v=5' : c.superficie && c.superficie.includes('Cemento') ? '/courts/cemento.jpg?v=3' : '/courts/ladrillo.jpg?v=3')}
                  alt={c.nombre}
                  style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                  onError={(e) => {
                    e.target.src = '/courts/ladrillo.jpg?v=3';
                  }}
                />
                <div style={{
                  position: 'absolute',
                  inset: 0,
                  background: 'linear-gradient(to top, rgba(0,0,0,0.7) 0%, transparent 60%)'
                }} />
                <span
                  className="badge badge-primary"
                  style={{
                    position: 'absolute',
                    top: 10,
                    right: 10,
                    fontSize: '0.7rem',
                    background: 'rgba(0,0,0,0.65)',
                    backdropFilter: 'blur(4px)',
                    border: '1px solid rgba(255,255,255,0.2)'
                  }}
                >
                  {c.superficie}
                </span>
              </div>

              <div style={{ padding: '16px 18px', display: 'flex', flexDirection: 'column', gap: 10, flex: 1 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div>
                    <h4 style={{ fontSize: '1.1rem', fontWeight: 700, margin: 0 }}>{c.nombre}</h4>
                    <small style={{ color: 'var(--text-dim)', fontSize: '0.75rem' }}>Pista Oficial del Complejo</small>
                  </div>

                  {user?.rol === 'Administrador' && (
                    <div style={{ display: 'flex', gap: 6 }}>
                      <button
                        onClick={() => handleAbrirMantenimiento(c)}
                        className="btn btn-warning btn-sm"
                        title="Gestionar bloqueo por mantenimiento"
                        style={{ padding: '4px 8px' }}
                      >
                        <Wrench size={14} />
                      </button>
                      <button
                        onClick={() => handleAbrirEditarCancha(c)}
                        className="btn btn-secondary btn-sm"
                        title="Modificar cancha y foto"
                        style={{ padding: '4px 8px' }}
                      >
                        <Edit2 size={14} />
                      </button>
                      <button
                        onClick={() => handleEliminarCancha(c.id, c.nombre)}
                        className="btn btn-danger btn-sm"
                        title="Eliminar cancha"
                        style={{ padding: '4px 8px' }}
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  )}
                </div>

                <p style={{ color: 'var(--text-muted)', fontSize: '0.8rem', lineHeight: 1.4, margin: 0 }}>
                  Capacidad para hasta {c.capacidad} jugadores. Con iluminación LED y mantenimiento reglamentario diario.
                </p>
                {c.enMantenimiento && (
                  <div style={{
                    padding: '8px 10px',
                    borderRadius: 6,
                    background: 'rgba(245, 158, 11, 0.12)',
                    border: '1px solid rgba(245, 158, 11, 0.3)',
                    color: 'var(--text-main)',
                    fontSize: '0.75rem',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 6
                  }}>
                    <AlertTriangle size={14} color="#f59e0b" style={{ flexShrink: 0 }} />
                    <span>
                      Bloqueada por mantenimiento hasta las {new Date(c.mantenimientoActivo.fechaHoraFin).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' })} ({new Date(c.mantenimientoActivo.fechaHoraFin).toLocaleDateString('es-AR')}).
                    </span>
                  </div>
                )}

                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderTop: '1px solid var(--border)', paddingTop: 10, marginTop: 'auto' }}>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)' }}>Arancel Oficial:</span>
                  <strong style={{ fontSize: '1.1rem', color: 'var(--primary)' }}>
                    ${(c.precioHora || 12000).toLocaleString('es-AR')}<span style={{ fontSize: '0.75rem', fontWeight: 400, color: 'var(--text-muted)' }}>/hora</span>
                  </strong>
                </div>

                <button
                  disabled={c.enMantenimiento && !isAdmin}
                  onClick={() => {
                    if (c.enMantenimiento && !isAdmin) return;
                    setCanchaSeleccionadaId(c.id.toString());
                    setModalReservarOpen(true);
                  }}
                  className={`btn ${c.enMantenimiento ? (isAdmin ? 'btn-warning' : 'btn-disabled') : 'btn-secondary'} btn-sm`}
                  style={{
                    width: '100%',
                    justifyContent: 'center',
                    marginTop: 4,
                    opacity: c.enMantenimiento && !isAdmin ? 0.6 : 1,
                    cursor: c.enMantenimiento && !isAdmin ? 'not-allowed' : 'pointer'
                  }}
                >
                  {c.enMantenimiento
                    ? (isAdmin ? 'Reservar Pista (Admin Mantenimiento)' : 'Pista en Mantenimiento (No disponible)')
                    : 'Reservar esta Pista'}
                </button>
              </div>
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
              {isAdmin ? 'Auditoría General de Reservas (Todos los Socios)' : 'Mis Reservas de Pista'}
            </h3>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
              Seguimiento de turnos, estado de cobranza desdoblada (50% Seña / 50% Saldo) y liquidación final.
            </span>
          </div>
          <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)' }}>
            Total registros: {reservasVisibles.length}
          </span>
        </div>

        {loading ? (
          <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-muted)' }}>Cargando reservas desde el servidor...</div>
        ) : reservasVisibles.length === 0 ? (
          <div style={{ padding: 32, textAlign: 'center', color: 'var(--text-muted)' }}>
            No hay reservas registradas en este momento en la base de datos.
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', color: 'var(--text-dim)', fontSize: '0.75rem', textTransform: 'uppercase' }}>
                  <th style={{ padding: '12px 14px' }}>Recibo / Cobro #</th>
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
                {reservasVisibles.map((r) => {
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
                      <td style={{ padding: '14px 14px', color: esFinalizada ? '#15803d' : 'var(--text-muted)' }}>
                        {esFinalizada ? '$0 (Abonado)' : `$${(r.saldoPendiente || 0).toLocaleString('es-AR')}`}
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

{(() => {
                            const isAdmin = user?.rol === 'Administrador' || user?.rol === 1;
                            const esProfesorOEntrenador = user?.rol === 'Profesor' || user?.rol === 'Entrenador' || user?.rol === 3 || user?.rol === 4;
                            const esDuenio = user?.dni && r.socioDni === user.dni;
                            const puedeFinalizar = isAdmin && (r.estado === 'Confirmada' || r.estado === 'Pendiente');
                            const puedeCancelar = (isAdmin || esDuenio) && !esProfesorOEntrenador;

                            return (
                              <>
                                {r.estado === 'Pendiente' && (
                                  <button
                                    onClick={async () => {
                                      if (!window.confirm(`¿Confirmás el registro de pago de la seña del 50% ($ ${(r.senaAbonada || 0).toLocaleString('es-AR')}) para confirmar el turno #${r.id}?`)) return;
                                      setAccionEnCurso(r.id);
                                      try {
                                        await api.reservas.confirmar(r.id);
                                        alert(`¡Reserva #${r.id} confirmada con éxito!`);
                                        await cargarDatos();
                                      } catch (err) {
                                        alert(`Error al confirmar: ${err.message}`);
                                      } finally {
                                        setAccionEnCurso(null);
                                      }
                                    }}
                                    disabled={accionEnCurso === r.id}
                                    className="btn btn-volt btn-sm"
                                    title="Validar y abonar seña 50%"
                                    style={{ fontSize: '0.72rem', padding: '3px 8px' }}
                                  >
                                    Abonar Seña 50%
                                  </button>
                                )}
                                {puedeFinalizar && (
                                  <button
                                    onClick={() => handleAbrirFinalizarReserva(r)}
                                    disabled={accionEnCurso === r.id}
                                    className="btn btn-secondary btn-sm"
                                    title="Cobrar saldo restante (50%) y finalizar partido [RF-65, RD-07, RD-16]"
                                    style={{ color: 'var(--primary)', borderColor: 'var(--primary-subtle-border)', display: 'inline-flex', alignItems: 'center', gap: 4, fontSize: '0.72rem', padding: '3px 8px' }}
                                  >
                                    <CheckCheck size={13} />
                                    <span>Cobrar Saldo y Finalizar</span>
                                  </button>
                                )}
                                {!esFinalizada && !esCancelada && puedeCancelar && (
                                  <button
                                    onClick={() => handleCancelarReserva(r)}
                                    disabled={accionEnCurso === r.id}
                                    className="btn btn-danger btn-sm"
                                    title="Cancelar reserva"
                                  >
                                    <X size={13} />
                                  </button>
                                )}
                              </>
                            );
                          })()}
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

      {/* ========================================================
          MODAL 1: ALQUILAR CANCHA Y COBRO DE SEÑA 50%
          ======================================================== */}
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
                <label className="form-label">Modalidad del Partido</label>
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

              {/* Nómina Obligatoria de Jugadores con Seguro Deportivo */}
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
                          <input
                            type="text"
                            inputMode="numeric"
                            maxLength={8}
                            required
                            placeholder="DNI"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.dni}
                            onChange={(e) => handleActualizarJugador(idx, 'dni', e.target.value.replace(/\D/g, '').slice(0, 8))}
                          />
                        </div>
                        <div>
                          <input
                            type="text"
                            required
                            placeholder="Teléfono"
                            className="form-control"
                            style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                            value={j.telefono}
inputMode="numeric" onChange={(e) => handleActualizarJugador(idx, 'telefono', e.target.value.replace(/\D/g, ''))}
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

              {/* Equipamiento Deportivo de Stock Obligatorio */}
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

                <label style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 10,
                  padding: '10px 14px',
                  background: 'var(--bg-elevated)',
                  borderRadius: 'var(--radius-sm)',
                  border: '1px solid var(--border)',
                  cursor: 'pointer'
                }}>
                  <input
                    type="checkbox"
                    checked={traeEquipamientoPropio}
                    onChange={(e) => setTraeEquipamientoPropio(e.target.checked)}
                    style={{ width: 16, height: 16, accentColor: 'var(--primary)', cursor: 'pointer' }}
                  />
                  <span style={{ fontSize: '0.8125rem', fontWeight: 600, color: 'var(--text-main)' }}>
                    Traigo mi propio equipamiento (sin alquiler de raquetas y pelotas del club)
                  </span>
                </label>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 10 }}>
                  <div style={{
                    background: 'var(--bg-card)',
                    border: `1px solid ${traeEquipamientoPropio ? 'var(--border)' : (hayStockPelotas ? 'var(--border)' : 'var(--accent-red-border)')}`,
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 4
                  }}>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', textTransform: 'uppercase', fontWeight: 600 }}>
                      {traeEquipamientoPropio ? 'Pelotas Propias' : '1x Tubo Pelotas (x3)'}
                    </span>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
<span style={{ fontSize: '0.8125rem', fontWeight: 600 }}>Stock Club:</span>
                      <span style={{
                        fontSize: '0.875rem',
                        fontWeight: 700,
                        color: traeEquipamientoPropio ? 'var(--primary)' : (hayStockPelotas ? 'var(--primary)' : 'var(--accent-red)')
                      }}>
                        {stockInventario.pelotas.stock} un.
                      </span>
                    </div>
                    {traeEquipamientoPropio ? (
                      <span style={{ fontSize: '0.6875rem', color: 'var(--primary)', fontWeight: 600 }}>Propio (sin cargo)</span>
                    ) : (
                      !hayStockPelotas && <span style={{ fontSize: '0.6875rem', color: 'var(--accent-red)' }}>¡Sin stock!</span>
                    )}
                  </div>

                  <div style={{
                    background: 'var(--bg-card)',
                    border: `1px solid ${traeEquipamientoPropio ? 'var(--border)' : (hayStockRaquetas ? 'var(--border)' : 'var(--accent-red-border)')}`,
                    borderRadius: 'var(--radius-sm)',
                    padding: '10px 12px',
                    display: 'flex',
                    flexDirection: 'column',
                    gap: 4
                  }}>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-dim)', textTransform: 'uppercase', fontWeight: 600 }}>
                      {traeEquipamientoPropio ? 'Raquetas Propias' : `${cantidadJugadores}x Raquetas (${cantidadJugadores} jug.)`}
                    </span>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
<span style={{ fontSize: '0.8125rem', fontWeight: 600 }}>Stock Club:</span>
                      <span style={{
                        fontSize: '0.875rem',
                        fontWeight: 700,
                        color: traeEquipamientoPropio ? 'var(--primary)' : (hayStockRaquetas ? 'var(--primary)' : 'var(--accent-red)')
                      }}>
                        {stockInventario.raquetas.stock} un.
                      </span>
                    </div>
                    {traeEquipamientoPropio ? (
                      <span style={{ fontSize: '0.6875rem', color: 'var(--primary)', fontWeight: 600 }}>Propio (sin cargo)</span>
                    ) : (
                      !hayStockRaquetas && (
                        <span style={{ fontSize: '0.6875rem', color: 'var(--accent-red)' }}>
                          ¡Faltan {stockRaquetasRequerido - stockInventario.raquetas.stock} raqueta(s)!
                        </span>
                      )
                    )}
                  </div>

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
                      <span style={{ fontSize: '0.875rem', fontWeight: 700, color: hayStockRedes ? 'var(--primary)' : 'var(--accent-red)' }}>
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
<span><strong>Bloqueo por falta de stock:</strong> {traeEquipamientoPropio ? 'No se puede alquilar la pista por falta de red de tenis disponible.' : 'No se puede alquilar la cancha si no hay stock suficiente de redes, pelotas o raquetas del club.'}</span>
                  </div>
                )}
              </div>

              {/* Beneficio o Descuento Arancelario Oficial */}
              <div>
                <label className="form-label">
                  Beneficio / Descuento Arancelario
                </label>
                <select
                  className="form-control"
                  value={descuentoSeleccionado}
                  onChange={(e) => setDescuentoSeleccionado(parseInt(e.target.value, 10))}
                >
                  <option value={0}>Tarifa Estándar Oficial (Sin Descuento)</option>
                  <option value={10}>Descuento por Paquete de Horas (-10%)</option>
                  <option value={15}>Equipo Participante de Ligas Regulares (-15%)</option>
                  <option value={20}>Convenio Escuelas Deportivas / Fútbol Afiliadas (-20%)</option>
                </select>
              </div>

              {/* Desglose Financiero con Seña del 50% Obligatoria */}
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
                  <span style={{ color: 'var(--text-muted)' }}>Subtotal Alquiler ({duracionHoras}h en {canchaActual.nombre}):</span>
                  <span style={{ fontWeight: 600 }}>${precioBase.toLocaleString('es-AR')}</span>
                </div>
                {descuentoSeleccionado > 0 && (
                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8125rem', color: 'var(--accent-volt)' }}>
                    <span>Descuento Oficial ({descuentoSeleccionado}%):</span>
                    <span>-${montoDescuento.toLocaleString('es-AR')}</span>
                  </div>
                )}
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem' }}>
                  <span style={{ color: 'var(--text-main)', fontWeight: 600 }}>Monto Final con Descuento:</span>
                  <span style={{ fontWeight: 700 }}>${precioTotal.toLocaleString('es-AR')}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '1rem', borderTop: '1px solid var(--border)', paddingTop: 8 }}>
                  <span style={{ color: 'var(--primary)', fontWeight: 700 }}>Seña requerida ahora (50% obligatorio):</span>
                  <span style={{ color: 'var(--primary)', fontWeight: 800, fontSize: '1.25rem' }}>
                    ${senaRequerida.toLocaleString('es-AR')}
                  </span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8125rem', color: 'var(--text-dim)' }}>
                  <span>Saldo restante a abonar al finalizar el partido:</span>
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
                  onChange={(e) => handleCambiarMetodoPago(e.target.value)}
                >
                  <option value="Mercado Pago (QR)">Billetera Virtual Mercado Pago (con QR oficial)</option>
                  <option value="Tarjeta de Débito">Tarjeta de Débito</option>
                  <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
                </select>
              </div>

              {/* Campos de Tarjeta si se selecciona Débito o Crédito [Punto 1] */}
              {esTarjeta && (
                <div style={{
                  background: 'var(--bg-elevated)',
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius-md)',
                  padding: '14px 16px',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 12
                }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <span style={{ fontSize: '0.875rem', fontWeight: 700, display: 'flex', alignItems: 'center', gap: 6 }}>
                      <CreditCard size={16} color="var(--primary)" />
                      Datos de la Tarjeta ({metodoPago})
                    </span>
                    <span style={{ fontSize: '0.7rem', color: 'var(--text-dim)' }}>
                      Tokenización segura vía Mercado Pago
                    </span>
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Número de Tarjeta (16 dígitos)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={19}
                        required
                        placeholder="XXXX XXXX XXXX XXXX"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaForm.numeroTarjeta}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, numeroTarjeta: e.target.value.replace(/\D/g, '').slice(0, 16) })}
                      />
                    </div>

                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Franquicia</label>
                      <select
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaForm.paymentMethodIdMp}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, paymentMethodIdMp: e.target.value })}
                      >
                        {metodoPago === 'Tarjeta de Débito' ? (
                          <>
                            <option value="debvisa">Visa Débito (debvisa)</option>
                            <option value="debmaster">Mastercard Débito (debmaster)</option>
                          </>
                        ) : (
                          <>
                            <option value="visa">Visa Crédito (visa)</option>
                            <option value="master">Mastercard Crédito (master)</option>
                          </>
                        )}
                      </select>
                    </div>
                  </div>

                  <div>
                    <label className="form-label" style={{ fontSize: '0.75rem' }}>
                      Nombre del Titular de la Tarjeta <span style={{ color: 'var(--primary)', fontWeight: 400 }}>(usá "APRO" para pago aprobado en pruebas)</span>
                    </label>
                    <input
                      type="text"
                      required
                      placeholder="Ej: APRO o JUAN PEREZ"
                      className="form-control"
                      style={{ fontSize: '0.8125rem' }}
                      value={tarjetaForm.nombreTitular}
                      onChange={(e) => setTarjetaForm({ ...tarjetaForm, nombreTitular: e.target.value })}
                    />
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1.2fr', gap: 10 }}>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Mes (MM)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={2}
                        required
                        placeholder="12"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaForm.mesVencimiento}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, mesVencimiento: e.target.value.replace(/\D/g, '').slice(0, 2) })}
                      />
                    </div>

                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Año (AAAA)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={4}
                        required
                        placeholder="2028"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaForm.anioVencimiento}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, anioVencimiento: e.target.value.replace(/\D/g, '').slice(0, 4) })}
                      />
                    </div>

                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>CVV</label>
                      <input
                        type="password"
                        inputMode="numeric"
                        maxLength={4}
                        required
                        placeholder="123"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaForm.cvv}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, cvv: e.target.value.replace(/\D/g, '').slice(0, 4) })}
                      />
                    </div>

                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Cuotas</label>
                      <select
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        disabled={metodoPago === 'Tarjeta de Débito'}
                        value={metodoPago === 'Tarjeta de Débito' ? 1 : tarjetaForm.cuotas}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, cuotas: Number(e.target.value) })}
                      >
                        <option value={1}>1 Cuota</option>
                        {metodoPago === 'Tarjeta de Crédito' && (
                          <>
                            <option value={3}>3 Cuotas</option>
                            <option value={6}>6 Cuotas</option>
                          </>
                        )}
                      </select>
                    </div>
                  </div>
                </div>
              )}

              {tieneMantenimientoEnHorario && (
                <div style={{
                  background: 'rgba(245, 158, 11, 0.15)',
                  border: '1px solid rgba(245, 158, 11, 0.4)',
                  color: '#f59e0b',
                  padding: '10px 14px',
                  borderRadius: 'var(--radius-sm)',
                  fontSize: '0.85rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8
                }}>
                  <AlertTriangle size={18} style={{ flexShrink: 0 }} />
                  <span>
                    <strong>Pista no disponible:</strong> Esta cancha tiene tareas de mantenimiento técnico programadas que se solapan con este turno (${horaInicio} hs). Por favor seleccioná otra cancha u otro horario.
                  </span>
                </div>
              )}

              {/* Botones de acción */}
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalReservarOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={submitting || (!hayStockGeneral && !traeEquipamientoPropio) || tieneMantenimientoEnHorario}
                  className="btn btn-primary"
                  title={tieneMantenimientoEnHorario ? 'Pista en mantenimiento durante este horario' : (!hayStockGeneral && !traeEquipamientoPropio ? 'Bloqueado por falta de stock suficiente de equipamiento' : '')}
                >
                  {submitting
                    ? (esTarjeta ? 'Procesando pago con Mercado Pago...' : 'Registrando reserva en el servidor...')
                    : tieneMantenimientoEnHorario
                    ? 'Cancha en Mantenimiento en este Turno'
                    : `Abonar Seña ${senaRequerida.toLocaleString('es-AR')} y Confirmar`}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL 2: CREAR / MODIFICAR CANCHA CON FOTO
          ======================================================== */}
      {modalCanchaOpen && (
        <div className="modal-overlay" onClick={() => setModalCanchaOpen(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 480 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 18 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{ background: 'var(--primary-subtle)', padding: 6, borderRadius: 8, color: 'var(--primary)' }}>
                  <Sparkles size={18} />
                </div>
                <h3 style={{ margin: 0, fontSize: '1.15rem' }}>
                  {canchaEditando ? 'Modificar Cancha y Fotografía' : 'Registrar Nueva Cancha'}
                </h3>
              </div>
              <button onClick={() => setModalCanchaOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={18} />
              </button>
            </div>

            {canchaError && (
              <div style={{ background: 'rgba(244, 63, 94, 0.15)', border: '1px solid rgba(244, 63, 94, 0.4)', color: '#f87171', padding: '10px 14px', borderRadius: 8, marginBottom: 14, fontSize: '0.85rem' }}>
                {canchaError}
              </div>
            )}

            <form onSubmit={handleGuardarCancha} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div className="input-group">
                <label className="input-label">Tipo de Superficie / Cancha</label>
                <select
                  className="input-field"
                  value={formCancha.tipoDeCanchaId}
                  onChange={(e) => {
                    const id = parseInt(e.target.value, 10);
                    setFormCancha({
                      tipoDeCanchaId: id,
                      fotoUrl: fotosPorSuperficieId[id] || '/courts/ladrillo.jpg?v=3'
                    });
                  }}
                >
                  <option value={1}>1 - Polvo de Ladrillo (Clay Court)</option>
                  <option value={2}>2 - Cemento Rápido (Hard Court)</option>
                  <option value={3}>3 - Césped Natural (Grass Court)</option>
                </select>
              </div>

              <div className="input-group">
                <label className="input-label">URL de la Fotografía Oficial (guardada en BD)</label>
                <input
                  type="text"
                  required
                  className="input-field"
                  placeholder="Ej. /courts/ladrillo.jpg?v=3 o https://..."
                  value={formCancha.fotoUrl}
                  onChange={(e) => setFormCancha({ ...formCancha, fotoUrl: e.target.value })}
                />
              </div>

              <div>
                <label className="input-label" style={{ display: 'block', marginBottom: 6 }}>Vista Previa de la Cancha</label>
                <div style={{ width: '100%', height: 150, borderRadius: 8, overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--bg-elevated)', position: 'relative' }}>
                  <img
                    src={formCancha.fotoUrl || '/courts/ladrillo.jpg?v=3'}
                    alt="Previsualización"
                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    onError={(e) => {
                      e.target.src = '/courts/ladrillo.jpg?v=3';
                    }}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={submittingCancha}
                className="btn btn-volt"
                style={{ width: '100%', marginTop: 8 }}
              >
                {submittingCancha ? 'Guardando...' : (canchaEditando ? 'Guardar Cambios' : 'Registrar Cancha')}
              </button>
            </form>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL 3: COBRAR SALDO FINAL (50%) Y FINALIZAR PARTIDO
          [RF-65, RD-07, RD-16]
          ======================================================== */}
      {modalFinalizarOpen && reservaParaFinalizar && (
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
          <div className="card" style={{ maxWidth: 560, width: '100%', maxHeight: '90vh', overflowY: 'auto', position: 'relative' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <div>
                <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <DollarSign color="var(--primary)" size={22} />
                  Cobrar Saldo Final (50%) y Finalizar Partido
                </h3>
                <small style={{ color: 'var(--text-muted)' }}>
                  Liquidación final [RF-65, RD-07] y restitución automática de equipamiento al stock [RD-16]
                </small>
              </div>
              <button
                onClick={() => setModalFinalizarOpen(false)}
                style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}
              >
                <X size={20} />
              </button>
            </div>

            {finalizarError && (
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
                <span>{finalizarError}</span>
              </div>
            )}

            {/* Resumen de la reserva a liquidar */}
            <div style={{
              background: 'var(--bg-elevated)',
              border: '1px solid var(--border)',
              borderRadius: 'var(--radius-md)',
              padding: '14px 16px',
              display: 'flex',
              flexDirection: 'column',
              gap: 8,
              marginBottom: 16,
              fontSize: '0.85rem'
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-dim)' }}>Reserva Identificador:</span>
                <strong>#{reservaParaFinalizar.id} ({reservaParaFinalizar.canchaNombre})</strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-dim)' }}>Titular Responsable:</span>
                <span>{reservaParaFinalizar.socioNombre} (DNI {reservaParaFinalizar.socioDni})</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-dim)' }}>Costo Total Alquiler:</span>
                <span>${(reservaParaFinalizar.precioTotal || 12000).toLocaleString('es-AR')}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-dim)' }}>Seña Inicial (50% Abonada):</span>
                <span style={{ color: '#15803d', fontWeight: 600 }}>-${(reservaParaFinalizar.senaAbonada || 6000).toLocaleString('es-AR')}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid var(--border)', paddingTop: 8, fontSize: '1rem' }}>
                <strong style={{ color: 'var(--primary)' }}>Saldo Final a Cobrar (50%):</strong>
                <strong style={{ color: 'var(--primary)', fontSize: '1.2rem' }}>
                  ${(reservaParaFinalizar.saldoPendiente || (reservaParaFinalizar.precioTotal * 0.5)).toLocaleString('es-AR')}
                </strong>
              </div>
            </div>

            {/* Selector de Medio de Pago para el saldo final */}
            <div style={{ marginBottom: 16 }}>
              <label className="form-label">
                Medio de Cobro del Saldo Final
              </label>
              <select
                className="form-control"
                value={metodoPagoFinal}
                onChange={(e) => {
                  const val = e.target.value;
                  setMetodoPagoFinal(val);
                  setCobroFinalQrData(null);
                  setInputPaymentIdFinal('');
                  if (val === 'Tarjeta de Débito') {
                    setTarjetaFinalForm(prev => ({ ...prev, paymentMethodIdMp: 'debvisa', cuotas: 1 }));
                  } else if (val === 'Tarjeta de Crédito') {
                    setTarjetaFinalForm(prev => ({ ...prev, paymentMethodIdMp: 'visa', cuotas: 1 }));
                  }
                }}
              >
                <option value="Mercado Pago (QR)">Billetera Virtual Mercado Pago (con QR oficial)</option>
                <option value="Tarjeta de Débito">Tarjeta de Débito</option>
                <option value="Tarjeta de Crédito">Tarjeta de Crédito</option>
              </select>
            </div>

            {/* Formulario de Tarjeta si se selecciona tarjeta */}
            {(metodoPagoFinal === 'Tarjeta de Débito' || metodoPagoFinal === 'Tarjeta de Crédito') && (
              <form onSubmit={handlePagarSaldoFinalTarjeta} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div style={{
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius-sm)',
                  padding: '12px 14px',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 10
                }}>
                  <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Número de Tarjeta (16 dígitos)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={19}
                        required
                        placeholder="XXXX XXXX XXXX XXXX"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaFinalForm.numeroTarjeta}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, numeroTarjeta: e.target.value.replace(/\D/g, '').slice(0, 16) })}
                      />
                    </div>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Franquicia</label>
                      <select
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaFinalForm.paymentMethodIdMp}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, paymentMethodIdMp: e.target.value })}
                      >
                        {metodoPagoFinal === 'Tarjeta de Débito' ? (
                          <>
                            <option value="debvisa">Visa Débito (debvisa)</option>
                            <option value="debmaster">Mastercard Débito (debmaster)</option>
                          </>
                        ) : (
                          <>
                            <option value="visa">Visa Crédito (visa)</option>
                            <option value="master">Mastercard Crédito (master)</option>
                          </>
                        )}
                      </select>
                    </div>
                  </div>

                  <div>
                    <label className="form-label" style={{ fontSize: '0.75rem' }}>
                      Nombre del Titular <span style={{ color: 'var(--primary)', fontWeight: 400 }}>(usá "APRO" para tests)</span>
                    </label>
                    <input
                      type="text"
                      required
                      placeholder="Ej: APRO"
                      className="form-control"
                      style={{ fontSize: '0.8125rem' }}
                      value={tarjetaFinalForm.nombreTitular}
                      onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, nombreTitular: e.target.value })}
                    />
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1.2fr', gap: 10 }}>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Mes (MM)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={2}
                        required
                        placeholder="12"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaFinalForm.mesVencimiento}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, mesVencimiento: e.target.value.replace(/\D/g, '').slice(0, 2) })}
                      />
                    </div>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Año (AAAA)</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        maxLength={4}
                        required
                        placeholder="2028"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaFinalForm.anioVencimiento}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, anioVencimiento: e.target.value.replace(/\D/g, '').slice(0, 4) })}
                      />
                    </div>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>CVV</label>
                      <input
                        type="password"
                        inputMode="numeric"
                        maxLength={4}
                        required
                        placeholder="123"
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        value={tarjetaFinalForm.cvv}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, cvv: e.target.value.replace(/\D/g, '').slice(0, 4) })}
                      />
                    </div>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>Cuotas</label>
                      <select
                        className="form-control"
                        style={{ fontSize: '0.8125rem' }}
                        disabled={metodoPagoFinal === 'Tarjeta de Débito'}
                        value={metodoPagoFinal === 'Tarjeta de Débito' ? 1 : tarjetaFinalForm.cuotas}
                        onChange={(e) => setTarjetaFinalForm({ ...tarjetaFinalForm, cuotas: Number(e.target.value) })}
                      >
                        <option value={1}>1 Cuota</option>
                        {metodoPagoFinal === 'Tarjeta de Crédito' && (
                          <>
                            <option value={3}>3 Cuotas</option>
                            <option value={6}>6 Cuotas</option>
                          </>
                        )}
                      </select>
                    </div>
                  </div>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 10 }}>
                  <button type="button" onClick={() => setModalFinalizarOpen(false)} className="btn btn-secondary">
                    Cancelar
                  </button>
                  <button type="submit" disabled={submittingFinal} className="btn btn-primary">
                    {submittingFinal ? 'Procesando Cobro...' : `Abonar $${(reservaParaFinalizar.saldoPendiente || (reservaParaFinalizar.precioTotal * 0.5)).toLocaleString('es-AR')} y Finalizar`}
                  </button>
                </div>
              </form>
            )}

            {/* Flujo QR si se selecciona Mercado Pago QR */}
            {metodoPagoFinal === 'Mercado Pago (QR)' && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                {!cobroFinalQrData ? (
                  <div style={{ textAlign: 'center', padding: '16px 0' }}>
                    <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem', marginBottom: 14 }}>
                      Presioná el botón para generar el código QR de cobro en Mercado Pago por el saldo final pendiente.
                    </p>
                    <button
                      type="button"
                      disabled={submittingFinal}
                      onClick={handleGenerarQrFinal}
                      className="btn btn-primary"
                      style={{ margin: '0 auto' }}
                    >
                      {submittingFinal ? 'Generando QR en Mercado Pago...' : `Generar QR por $${(reservaParaFinalizar.saldoPendiente || (reservaParaFinalizar.precioTotal * 0.5)).toLocaleString('es-AR')}`}
                    </button>
                  </div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
                    <img
                      src={`data:image/png;base64,${cobroFinalQrData.codigoQrBase64}`}
                      alt="QR Saldo Final"
                      style={{ width: 190, height: 190, borderRadius: 8, border: '1px solid #cbd5e1' }}
                    />
                    <div style={{ textAlign: 'center', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                      <div>Cobro #{cobroFinalQrData.numeroCobro} generado en Mercado Pago.</div>
                      <a
                        href={cobroFinalQrData.urlPagoQr}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="btn btn-volt btn-sm"
                        style={{ marginTop: 8, display: 'inline-flex', alignItems: 'center', gap: 6, textDecoration: 'none' }}
                      >
                        <ExternalLink size={14} />
                        <span>Abrir Checkout Sandbox Mercado Pago</span>
                      </a>
                    </div>

                    <div style={{ width: '100%', marginTop: 8, padding: '12px', background: 'var(--bg-elevated)', borderRadius: 6, border: '1px solid var(--border)' }}>
                      <label className="form-label" style={{ fontSize: '0.8rem' }}>
                        ID de Pago de Mercado Pago tras abonar en Sandbox:
                      </label>
                      <div style={{ display: 'flex', gap: 8 }}>
                        <input
                          type="number"
                          placeholder="Ej: 1234567890"
                          value={inputPaymentIdFinal}
                          onChange={(e) => setInputPaymentIdFinal(e.target.value)}
                          className="form-control"
                          style={{ fontSize: '0.85rem' }}
                        />
                        <button
                          type="button"
                          disabled={submittingFinal || !inputPaymentIdFinal.trim()}
                          onClick={handleConfirmarQrFinal}
                          className="btn btn-primary btn-sm"
                          style={{ whiteSpace: 'nowrap' }}
                        >
                          {submittingFinal ? 'Confirmando...' : 'Confirmar y Finalizar'}
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 10 }}>
                  <button type="button" onClick={() => setModalFinalizarOpen(false)} className="btn btn-secondary">
                    Cerrar
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL 4: RECIBO OFICIAL CON QR REAL BASE64 O TARJETA
          [RF-69, RF-70, RF-72]
          ======================================================== */}
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
          <div className="card" style={{ maxWidth: 560, width: '100%', padding: 24, position: 'relative', maxHeight: '90vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <CheckCircle2 color="var(--primary)" size={22} />
                Comprobante Oficial de Reserva y Cobro
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
                    Nómina de Jugadores y Póliza de Seguro Deportivo:
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 3, fontSize: '0.75rem', color: '#334155' }}>
                    {reciboActual.jugadoresLista.map((j, i) => (
                      <div key={i} style={{ display: 'flex', justifyContent: 'space-between' }}>
                        <span>#{i + 1} {j.nombre} {j.apellido} (DNI {j.dni})</span>
                        <span style={{ fontWeight: 600, color: '#0284c7' }}>Cobertura: {j.cobertura || j.coberturaMedica || 'Club Oficial'}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Accesorios */}
              <div style={{ borderTop: '1px dashed #cbd5e1', paddingTop: 8, fontSize: '0.75rem', color: '#334155' }}>
                <strong>Equipamiento Oficial Descontado:</strong> {reciboActual.equipamiento}
              </div>

              {/* Bloque QR Base64 real si aplica o bloque de pago con Tarjeta */}
              {reciboActual.codigoQrBase64 ? (
                <div style={{
                  background: '#f8fafc',
                  border: '1px solid #e2e8f0',
                  padding: '16px',
                  borderRadius: 8,
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  gap: 12,
                }}>
                  <img
                    src={`data:image/png;base64,${reciboActual.codigoQrBase64}`}
                    alt="QR Mercado Pago"
                    style={{ width: '200px', height: '200px', borderRadius: '8px', border: '1px solid #cbd5e1' }}
                  />
                  <div style={{ fontSize: '0.8125rem', color: '#334155', textAlign: 'center' }}>
                    <strong>Cobranza Digital QR Mercado Pago</strong>
                    <div>Cobro Oficial #{reciboActual.numeroCobro} · {reciboActual.estadoCobro === 'Validado' ? 'Acreditado y Confirmado' : 'Pendiente de Pago'}</div>
                    {reciboActual.urlPagoQr && (
                      <a
                        href={reciboActual.urlPagoQr}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="btn btn-volt btn-sm"
                        style={{ marginTop: 8, display: 'inline-flex', alignItems: 'center', gap: 6, textDecoration: 'none' }}
                      >
                        <ExternalLink size={14} />
                        <span>Abrir Checkout Sandbox Mercado Pago</span>
                      </a>
                    )}
                  </div>

                  {reciboActual.estadoCobro === 'Pendiente' ? (
                    <div style={{ width: '100%', marginTop: 8, padding: '12px', background: '#ffffff', borderRadius: 6, border: '1px solid #cbd5e1' }}>
                      <label style={{ fontSize: '0.75rem', fontWeight: 700, color: '#334155', display: 'block', marginBottom: 6 }}>
                        Confirmar Acreditación de Pago QR (Ingresá el Payment ID generado en Sandbox):
                      </label>
                      <div style={{ display: 'flex', gap: 8 }}>
                        <input
                          type="number"
                          placeholder="Ej: 1234567890"
                          value={inputPaymentId}
                          onChange={(e) => setInputPaymentId(e.target.value)}
                          className="form-control"
                          style={{ fontSize: '0.8125rem', padding: '6px 10px' }}
                        />
                        <button
                          type="button"
                          disabled={confirmandoQr || !inputPaymentId.trim()}
                          onClick={handleConfirmarAcreditacionQr}
                          className="btn btn-primary btn-sm"
                          style={{ whiteSpace: 'nowrap' }}
                        >
                          {confirmandoQr ? 'Verificando...' : 'Confirmar Acreditación QR'}
                        </button>
                      </div>
                      {qrConfirmError && (
                        <small style={{ color: '#dc2626', fontSize: '0.75rem', marginTop: 4, display: 'block' }}>
                          {qrConfirmError}
                        </small>
                      )}
                    </div>
                  ) : (
                    <div style={{
                      color: '#15803d',
                      fontWeight: 700,
                      fontSize: '0.85rem',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 6,
                      background: 'rgba(21, 128, 61, 0.1)',
                      padding: '8px 14px',
                      borderRadius: 6
                    }}>
                      <CheckCircle2 size={16} color="#15803d" />
                      <span>Pago Acreditado y Validado (MP #{reciboActual.mercadoPagoPaymentId || 'OK'})</span>
                    </div>
                  )}
                </div>
              ) : (
                <div style={{
                  background: '#f8fafc',
                  border: '1px solid #e2e8f0',
                  padding: '12px 16px',
                  borderRadius: 6,
                  display: 'flex',
                  alignItems: 'center',
                  gap: 12,
                }}>
                  <CreditCard size={36} color="#0f172a" />
                  <div style={{ fontSize: '0.8125rem', color: '#334155' }}>
                    <strong>Pago con Tarjeta Procesado Exitosamente</strong>
                    <div>Medio de pago: {reciboActual.metodoPago} {reciboActual.mercadoPagoPaymentId ? `· ID Mercado Pago #${reciboActual.mercadoPagoPaymentId}` : ''}</div>
                    <div style={{ color: '#15803d', fontWeight: 700 }}>
                      {reciboActual.saldoPendiente === 0 ? 'Liquidación Total (100%) Acreditada' : 'Seña 50% Registrada y Validada'}
                    </div>
                  </div>
                </div>
              )}

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
                {reciboActual.descuentoPorcentaje > 0 && (
                  <div style={{ display: 'flex', justifyContent: 'space-between', color: '#0284c7', fontWeight: 600 }}>
                    <span>Descuento Aplicado ({reciboActual.descuentoPorcentaje}%):</span>
                    <span>-${reciboActual.montoDescuento?.toLocaleString('es-AR')}</span>
                  </div>
                )}
                <div style={{ display: 'flex', justifyContent: 'space-between', color: '#15803d', fontWeight: 700 }}>
                  <span>Seña Abonada (50%):</span>
                  <span>-${reciboActual.senaAbonada?.toLocaleString('es-AR')}</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', color: reciboActual.saldoPendiente === 0 ? '#15803d' : '#b91c1c', fontWeight: 700, borderTop: '1px solid #cbd5e1', paddingTop: 4 }}>
                  <span>{reciboActual.saldoPendiente === 0 ? 'Saldo Restante Abonado:' : 'Saldo a pagar al terminar el turno:'}</span>
                  <span>${reciboActual.saldoPendiente?.toLocaleString('es-AR')}</span>
                </div>
              </div>

              <small style={{ fontSize: '0.6875rem', color: '#64748b', textAlign: 'center' }}>
                * Cancelaciones con más de 6 hs de antelación obtienen reembolso del 100% de la seña.
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
