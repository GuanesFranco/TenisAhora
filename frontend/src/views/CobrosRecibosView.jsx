import React, { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../context/AuthContext';
import {
  Receipt,
  CreditCard,
  QrCode,
  Search,
  Plus,
  Printer,
  XCircle,
  CheckCircle2,
  AlertTriangle,
  X,
  ExternalLink,
  DollarSign,
  FileText,
  ShieldCheck,
  RefreshCw,
  Eye
} from 'lucide-react';

export default function CobrosRecibosView() {
  const { user } = useAuth();
  const isAdmin = user?.rol === 'Administrador';

  // Control de Pestañas: 'cobros' | 'recibos'
  const [activeTab, setActiveTab] = useState('cobros');

  // Mensajes globales
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // ==========================================
  // ESTADO: PESTAÑA COBROS [RF-65 a RF-68]
  // ==========================================
  const [cobros, setCobros] = useState([]);
  const [loadingCobros, setLoadingCobros] = useState(false);
  const [filtroNumeroCobro, setFiltroNumeroCobro] = useState('');

  // Modal Registrar Nuevo Cobro
  const [modalCobroOpen, setModalCobroOpen] = useState(false);
  const [submittingCobro, setSubmittingCobro] = useState(false);
  const [cobroForm, setCobroForm] = useState({
    socioDni: user?.dni ? String(user.dni) : '',
    reservaId: '',
    inscripcionId: '',
    importeBase: '',
    modalidadPago: 'total', // 'total' (100%), 'sena' (50%), 'saldo' (50%)
    descuentoId: '',
    recargoCancelacion: 0,
    tipoPago: 'QR', // 'QR', 'TarjetaDebito', 'TarjetaCredito'
  });

  // Datos Tarjeta para Nuevo Cobro
  const [tarjetaForm, setTarjetaForm] = useState({
    numeroTarjeta: '',
    nombreTitular: 'APRO',
    mesVencimiento: '12',
    anioVencimiento: '2028',
    cvv: '123',
    paymentMethodIdMp: 'debvisa',
    cuotas: 1
  });

  // Modal QR Cobro (Tras registrar QR)
  const [modalQrCobroOpen, setModalQrCobroOpen] = useState(false);
  const [cobroQrActual, setCobroQrActual] = useState(null);
  const [inputPaymentIdQr, setInputPaymentIdQr] = useState('');
  const [confirmandoQr, setConfirmandoQr] = useState(false);

  // Modal Anular Cobro [RF-67]
  const [modalAnularCobroOpen, setModalAnularCobroOpen] = useState(false);
  const [cobroParaAnular, setCobroParaAnular] = useState(null);
  const [motivoAnulacion, setMotivoAnulacion] = useState('');
  const [submittingAnulacion, setSubmittingAnulacion] = useState(false);

  // Modal Imprimir Cobro [RF-68]
  const [modalImprimirCobroOpen, setModalImprimirCobroOpen] = useState(false);
  const [impresionCobroData, setImpresionCobroData] = useState(null);
  const [loadingImpresionCobro, setLoadingImpresionCobro] = useState(false);

  // ==========================================
  // ESTADO: PESTAÑA RECIBOS [RF-69 a RF-72]
  // ==========================================
  const [recibos, setRecibos] = useState([]);
  const [loadingRecibos, setLoadingRecibos] = useState(false);
  const [filtroIdRecibo, setFiltroIdRecibo] = useState('');

  // Generación Manual de Recibo por N° Cobro [RF-69]
  const [numeroCobroParaGenerar, setNumeroCobroParaGenerar] = useState('');
  const [generandoRecibo, setGenerandoRecibo] = useState(false);

  // Modal Imprimir Recibo [RF-72]
  const [modalImprimirReciboOpen, setModalImprimirReciboOpen] = useState(false);
  const [impresionReciboData, setImpresionReciboData] = useState(null);
  const [loadingImpresionRecibo, setLoadingImpresionRecibo] = useState(false);

  useEffect(() => {
    cargarCobros();
    cargarRecibos();
  }, []);

  function mostrarNotificacion(msg, esError = false) {
    if (esError) {
      setError(msg);
      setTimeout(() => setError(''), 6000);
    } else {
      setSuccessMsg(msg);
      setTimeout(() => setSuccessMsg(''), 5000);
    }
  }

  // ------------------------------------------
  // CARGAS Y CONSULTAS DE COBROS
  // ------------------------------------------
  async function cargarCobros() {
    setLoadingCobros(true);
    try {
      const data = await api.cobros.listar();
      setCobros(Array.isArray(data) ? data : []);
    } catch (err) {
      mostrarNotificacion(err.message || 'Error al listar cobros.', true);
    } finally {
      setLoadingCobros(false);
    }
  }

  async function handleBuscarCobroPorNumero() {
    const num = parseInt(filtroNumeroCobro, 10);
    if (!num || isNaN(num)) {
      mostrarNotificacion('Por favor ingrese un número de cobro válido.', true);
      return;
    }
    setLoadingCobros(true);
    try {
      const cobro = await api.cobros.obtenerPorNumero(num);
      setCobros(cobro ? [cobro] : []);
    } catch (err) {
      mostrarNotificacion(`No se encontró el cobro #${num}: ${err.message}`, true);
    } finally {
      setLoadingCobros(false);
    }
  }

  // ------------------------------------------
  // REGISTRAR NUEVO COBRO [RF-65]
  // ------------------------------------------
  const handleAbrirModalCobro = () => {
    setCobroForm({
      socioDni: user?.dni ? String(user.dni) : '',
      reservaId: '',
      inscripcionId: '',
      importeBase: '',
      modalidadPago: 'total',
      descuentoId: '',
      recargoCancelacion: 0,
      tipoPago: 'QR',
    });
    setTarjetaForm({
      numeroTarjeta: '',
      nombreTitular: 'APRO',
      mesVencimiento: '12',
      anioVencimiento: '2028',
      cvv: '123',
      paymentMethodIdMp: 'debvisa',
      cuotas: 1
    });
    setModalCobroOpen(true);
  };

  const handleRegistrarCobro = async (e) => {
    e.preventDefault();
    const dni = parseInt(cobroForm.socioDni, 10);
    const monto = parseFloat(cobroForm.importeBase);

    if (!dni || isNaN(dni)) {
      mostrarNotificacion('El DNI del socio es obligatorio y debe ser numérico.', true);
      return;
    }
    if (!monto || isNaN(monto) || monto <= 0) {
      mostrarNotificacion('El importe base debe ser un monto mayor a cero.', true);
      return;
    }

    setSubmittingCobro(true);
    try {
      const esSena = cobroForm.modalidadPago === 'sena';
      const esSaldo = cobroForm.modalidadPago === 'saldo';

      let tipoPagoEnum = 2; // QR = 2
      if (cobroForm.tipoPago === 'TarjetaCredito') tipoPagoEnum = 0;
      if (cobroForm.tipoPago === 'TarjetaDebito') tipoPagoEnum = 1;

      let tokenTarjetaMp = null;
      if (tipoPagoEnum === 0 || tipoPagoEnum === 1) {
        if (!tarjetaForm.numeroTarjeta || tarjetaForm.numeroTarjeta.length < 15) {
          throw new Error('Ingrese un número de tarjeta válido (15 o 16 dígitos).');
        }
        tokenTarjetaMp = await api.mercadopago.crearTokenTarjeta({
          numeroTarjeta: tarjetaForm.numeroTarjeta,
          nombreTitular: tarjetaForm.nombreTitular.trim(),
          mesVencimiento: tarjetaForm.mesVencimiento,
          anioVencimiento: tarjetaForm.anioVencimiento,
          cvv: tarjetaForm.cvv,
          dni
        });
      }

      const payload = {
        socioDni: dni,
        importeBase: monto,
        tipoPago: tipoPagoEnum,
        esSena50Porciento: esSena,
        esSaldoFinalPartido: esSaldo,
        reservaId: cobroForm.reservaId ? parseInt(cobroForm.reservaId, 10) : null,
        inscripcionId: cobroForm.inscripcionId ? parseInt(cobroForm.inscripcionId, 10) : null,
        descuentoId: cobroForm.descuentoId ? parseInt(cobroForm.descuentoId, 10) : null,
        recargoCancelacion: parseFloat(cobroForm.recargoCancelacion) || 0,
        tokenTarjetaMp: tokenTarjetaMp || undefined,
        paymentMethodIdMp: (tipoPagoEnum === 0 || tipoPagoEnum === 1) ? tarjetaForm.paymentMethodIdMp : undefined,
        cuotas: tipoPagoEnum === 1 ? 1 : Number(tarjetaForm.cuotas)
      };

      const cobro = await api.cobros.registrar(payload);
      setModalCobroOpen(false);

      if (tipoPagoEnum === 2) {
        // Abrir modal QR con la imagen Base64 devuelta
        setCobroQrActual(cobro);
        setInputPaymentIdQr('');
        setModalQrCobroOpen(true);
      } else {
        mostrarNotificacion(`¡Cobro #${cobro.numeroCobro} registrado y validado con éxito! Recibo emitido: #${cobro.reciboId || 'N/A'}`);
      }

      await Promise.all([cargarCobros(), cargarRecibos()]);
    } catch (err) {
      mostrarNotificacion(err.message || 'Error al procesar el cobro en el backend.', true);
    } finally {
      setSubmittingCobro(false);
    }
  };

  // Confirmar Acreditación QR
  const handleConfirmarQr = async () => {
    if (!cobroQrActual || !inputPaymentIdQr.trim()) return;
    setConfirmandoQr(true);
    try {
      const cobroConfirmado = await api.cobros.confirmarQr(cobroQrActual.numeroCobro, inputPaymentIdQr.trim());
      setCobroQrActual(cobroConfirmado);
      mostrarNotificacion(`¡Cobro #${cobroConfirmado.numeroCobro} acreditado correctamente! Recibo generado: #${cobroConfirmado.reciboId}`);
      await Promise.all([cargarCobros(), cargarRecibos()]);
    } catch (err) {
      mostrarNotificacion(err.message || 'Error al validar el pago en Mercado Pago.', true);
    } finally {
      setConfirmandoQr(false);
    }
  };

  // Abrir modal para anular cobro [RF-67]
  const handleAbrirAnularCobro = (c) => {
    setCobroParaAnular(c);
    setMotivoAnulacion('');
    setModalAnularCobroOpen(true);
  };

  const handleConfirmarAnularCobro = async (e) => {
    e.preventDefault();
    if (!cobroParaAnular || !motivoAnulacion.trim()) return;
    setSubmittingAnulacion(true);
    try {
      await api.cobros.anular(cobroParaAnular.numeroCobro, motivoAnulacion.trim());
      mostrarNotificacion(`Cobro #${cobroParaAnular.numeroCobro} anulado exitosamente.`);
      setModalAnularCobroOpen(false);
      await Promise.all([cargarCobros(), cargarRecibos()]);
    } catch (err) {
      mostrarNotificacion(`Error al anular cobro: ${err.message}`, true);
    } finally {
      setSubmittingAnulacion(false);
    }
  };

  // Imprimir comprobante de cobro [RF-68]
  const handleImprimirCobro = async (numeroCobro) => {
    setLoadingImpresionCobro(true);
    try {
      const data = await api.cobros.imprimir(numeroCobro);
      setImpresionCobroData(data);
      setModalImprimirCobroOpen(true);
    } catch (err) {
      mostrarNotificacion(`Error al obtener información de impresión del cobro: ${err.message}`, true);
    } finally {
      setLoadingImpresionCobro(false);
    }
  };

  // ------------------------------------------
  // CARGAS Y CONSULTAS DE RECIBOS [RF-69 a RF-72]
  // ------------------------------------------
  async function cargarRecibos() {
    setLoadingRecibos(true);
    try {
      const data = await api.recibos.listar();
      setRecibos(Array.isArray(data) ? data : []);
    } catch (err) {
      mostrarNotificacion(err.message || 'Error al listar recibos.', true);
    } finally {
      setLoadingRecibos(false);
    }
  }

  async function handleBuscarReciboPorId() {
    const id = parseInt(filtroIdRecibo, 10);
    if (!id || isNaN(id)) {
      mostrarNotificacion('Por favor ingrese un ID de recibo válido.', true);
      return;
    }
    setLoadingRecibos(true);
    try {
      const r = await api.recibos.obtenerPorId(id);
      setRecibos(r ? [r] : []);
    } catch (err) {
      mostrarNotificacion(`No se encontró el recibo #${id}: ${err.message}`, true);
    } finally {
      setLoadingRecibos(false);
    }
  }

  // Generación Manual de Recibo por N° Cobro [RF-69]
  const handleGenerarReciboManual = async (e) => {
    e.preventDefault();
    const num = parseInt(numeroCobroParaGenerar, 10);
    if (!num || isNaN(num)) {
      mostrarNotificacion('Por favor ingrese un número de cobro válido.', true);
      return;
    }
    setGenerandoRecibo(true);
    try {
      const nuevo = await api.recibos.generar(num);
      mostrarNotificacion(`¡Recibo #${nuevo.id} generado exitosamente para el Cobro #${num}!`);
      setNumeroCobroParaGenerar('');
      await Promise.all([cargarCobros(), cargarRecibos()]);
    } catch (err) {
      mostrarNotificacion(`Error al emitir recibo: ${err.message}`, true);
    } finally {
      setGenerandoRecibo(false);
    }
  };

  // Anular Recibo [RF-71]
  const handleAnularRecibo = async (id) => {
    if (!window.confirm(`¿Seguro que desea anular el Recibo #${id}? Esta acción cancelará su validez fiscal.`)) return;
    try {
      await api.recibos.anular(id);
      mostrarNotificacion(`Recibo #${id} anulado correctamente.`);
      await Promise.all([cargarCobros(), cargarRecibos()]);
    } catch (err) {
      mostrarNotificacion(`Error al anular recibo: ${err.message}`, true);
    }
  };

  // Imprimir Recibo Oficial [RF-72]
  const handleImprimirRecibo = async (id) => {
    setLoadingImpresionRecibo(true);
    try {
      const data = await api.recibos.imprimir(id);
      setImpresionReciboData(data);
      setModalImprimirReciboOpen(true);
    } catch (err) {
      mostrarNotificacion(`Error al obtener información de impresión del recibo: ${err.message}`, true);
    } finally {
      setLoadingImpresionRecibo(false);
    }
  };

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
            <Receipt size={26} color="var(--primary)" />
            <h1 style={{ fontSize: '2rem' }}>Centro de Cobranzas y Recibos Oficiales</h1>
          </div>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
            Control financiero integral: transacciones con Mercado Pago (QR y Tarjetas), liquidación de alquileres y emisión de comprobantes [RF-65 a RF-72].
          </p>
        </div>

        {isAdmin && (
          <button onClick={handleAbrirModalCobro} className="btn btn-primary">
            <Plus size={16} />
            <span>Registrar Nuevo Cobro</span>
          </button>
        )}
      </div>

      {/* Alertas Globales */}
      {error && (
        <div style={{
          background: 'var(--accent-red-bg)',
          border: '1px solid var(--accent-red-border)',
          color: 'var(--accent-red)',
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

      {/* Selector de Pestañas Internas */}
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
          onClick={() => setActiveTab('cobros')}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            padding: '9px 18px',
            borderRadius: 'var(--radius-sm)',
            border: 'none',
            background: activeTab === 'cobros' ? 'var(--primary)' : 'transparent',
            color: activeTab === 'cobros' ? '#fff' : 'var(--text-muted)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            transition: 'all 0.15s ease'
          }}
        >
          <CreditCard size={16} />
          <span>Gestión de Cobros</span>
          <span style={{
            fontSize: '0.65rem',
            background: activeTab === 'cobros' ? 'rgba(255,255,255,0.25)' : 'var(--bg-elevated)',
            padding: '2px 6px',
            borderRadius: 4
          }}>
            RF-65/68
          </span>
        </button>

        <button
          onClick={() => setActiveTab('recibos')}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            padding: '9px 18px',
            borderRadius: 'var(--radius-sm)',
            border: 'none',
            background: activeTab === 'recibos' ? 'var(--primary)' : 'transparent',
            color: activeTab === 'recibos' ? '#fff' : 'var(--text-muted)',
            fontWeight: 600,
            fontSize: '0.875rem',
            cursor: 'pointer',
            transition: 'all 0.15s ease'
          }}
        >
          <Receipt size={16} />
          <span>Gestión de Recibos</span>
          <span style={{
            fontSize: '0.65rem',
            background: activeTab === 'recibos' ? 'rgba(255,255,255,0.25)' : 'var(--bg-elevated)',
            padding: '2px 6px',
            borderRadius: 4
          }}>
            RF-69/72
          </span>
        </button>
      </div>

      {/* ========================================================
          PESTAÑA 1: GESTIÓN DE COBROS [RF-65, RF-66, RF-67, RF-68]
          ======================================================== */}
      {activeTab === 'cobros' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
          {/* Barra de Búsqueda y Filtros de Cobros */}
          <div className="card" style={{ padding: '14px 18px', display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
            <div style={{ position: 'relative', flex: 1, minWidth: 260 }}>
              <Search size={16} color="var(--text-dim)" style={{ position: 'absolute', left: 14, top: 13 }} />
              <input
                type="number"
                placeholder="Buscar por N° de Cobro exacto..."
                value={filtroNumeroCobro}
                onChange={(e) => setFiltroNumeroCobro(e.target.value)}
                className="form-control"
                style={{ paddingLeft: 40 }}
              />
            </div>
            <button onClick={handleBuscarCobroPorNumero} className="btn btn-secondary">
              <Search size={15} />
              <span>Buscar</span>
            </button>
            <button
              onClick={() => {
                setFiltroNumeroCobro('');
                cargarCobros();
              }}
              className="btn btn-secondary"
            >
              <RefreshCw size={15} />
              <span>Ver Todos</span>
            </button>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-dim)', marginLeft: 'auto' }}>
              Registros cargados: {cobros.length}
            </span>
          </div>

          {/* Tabla de Cobros */}
          <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
            {loadingCobros ? (
              <div style={{ padding: 48, textAlign: 'center', color: 'var(--text-dim)' }}>
                Consultando transacciones de cobro en el backend...
              </div>
            ) : cobros.length === 0 ? (
              <div style={{ padding: 48, textAlign: 'center' }}>
                <CreditCard size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
                <h3 style={{ marginBottom: 6 }}>No hay cobros registrados</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                  No se encontraron transacciones con el criterio ingresado.
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
                      <th style={{ padding: '14px 16px' }}>N° Cobro</th>
                      <th style={{ padding: '14px 16px' }}>Fecha</th>
                      <th style={{ padding: '14px 16px' }}>Socio DNI</th>
                      <th style={{ padding: '14px 16px' }}>Medio de Pago</th>
                      <th style={{ padding: '14px 16px' }}>Importe</th>
                      <th style={{ padding: '14px 16px', textAlign: 'center' }}>% Pago</th>
                      <th style={{ padding: '14px 16px', textAlign: 'center' }}>Estado</th>
                      <th style={{ padding: '14px 16px' }}>ID Mercado Pago</th>
                      <th style={{ padding: '14px 16px', textAlign: 'center' }}>Recibo #</th>
                      <th style={{ padding: '14px 16px', textAlign: 'right' }}>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {cobros.map((c) => {
                      const fechaStr = c.fechaPago ? new Date(c.fechaPago).toLocaleDateString('es-AR', { hour: '2-digit', minute: '2-digit' }) : 'N/A';
                      const estadoLower = (c.estado || '').toLowerCase();
                      const badgeClass =
                        estadoLower === 'validado' ? 'badge-success' :
                        estadoLower === 'pendiente' ? 'badge-primary' :
                        estadoLower === 'anulado' ? 'badge-secondary' : 'badge-danger';

                      return (
                        <tr key={c.numeroCobro} style={{ borderBottom: '1px solid var(--border)' }}>
                          <td style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--primary)' }}>
                            #{c.numeroCobro}
                          </td>
                          <td style={{ padding: '14px 16px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                            {fechaStr}
                          </td>
                          <td style={{ padding: '14px 16px', fontWeight: 600 }}>
                            {c.socioDni}
                          </td>
                          <td style={{ padding: '14px 16px' }}>
                            <span style={{ fontSize: '0.8rem', fontWeight: 500 }}>
                              {c.tipoPago}
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--accent-volt)' }}>
                            ${c.importe?.toLocaleString('es-AR')}
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'center' }}>
                            <span className="badge badge-primary" style={{ fontSize: '0.7rem' }}>
                              {c.porcentajePago}%
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'center' }}>
                            <span className={`badge ${badgeClass}`} style={{ fontSize: '0.7rem' }}>
                              {c.estado}
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', fontSize: '0.8rem', color: 'var(--text-dim)' }}>
                            {c.mercadoPagoPaymentId ? `#${c.mercadoPagoPaymentId}` : '---'}
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'center', fontWeight: 600, color: 'var(--primary)' }}>
                            {c.reciboId ? `#${c.reciboId}` : '---'}
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'right' }}>
                            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 6 }}>
                              {/* Confirmar QR si está pendiente */}
                              {c.estado === 'Pendiente' && c.tipoPago === 'QR' && (
                                <button
                                  onClick={() => {
                                    setCobroQrActual(c);
                                    setInputPaymentIdQr('');
                                    setModalQrCobroOpen(true);
                                  }}
                                  className="btn btn-primary btn-sm"
                                  title="Ver QR y confirmar pago"
                                >
                                  <QrCode size={13} />
                                  <span>QR</span>
                                </button>
                              )}

                              {/* Imprimir comprobante [RF-68] */}
                              <button
                                onClick={() => handleImprimirCobro(c.numeroCobro)}
                                className="btn btn-secondary btn-sm"
                                title="Ver e imprimir comprobante de cobro"
                              >
                                <Printer size={13} />
                              </button>

                              {/* Anular cobro [RF-67] */}
                              {c.estado !== 'Anulado' && isAdmin && (
                                <button
                                  onClick={() => handleAbrirAnularCobro(c)}
                                  className="btn btn-danger btn-sm"
                                  title="Anular cobro y procesar reembolso"
                                >
                                  <XCircle size={13} />
                                </button>
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
        </div>
      )}

      {/* ========================================================
          PESTAÑA 2: GESTIÓN DE RECIBOS [RF-69, RF-70, RF-71, RF-72]
          ======================================================== */}
      {activeTab === 'recibos' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
          {/* Barra Superior y Formulario de Emisión Manual */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            {/* Buscador de Recibo por ID [RF-70] */}
            <div className="card" style={{ padding: '14px 18px', display: 'flex', gap: 10, alignItems: 'center' }}>
              <div style={{ position: 'relative', flex: 1 }}>
                <Search size={16} color="var(--text-dim)" style={{ position: 'absolute', left: 14, top: 13 }} />
                <input
                  type="number"
                  placeholder="Buscar por ID de Recibo (ej: 1)..."
                  value={filtroIdRecibo}
                  onChange={(e) => setFiltroIdRecibo(e.target.value)}
                  className="form-control"
                  style={{ paddingLeft: 40 }}
                />
              </div>
              <button onClick={handleBuscarReciboPorId} className="btn btn-secondary">
                <Search size={15} />
                <span>Buscar</span>
              </button>
              <button
                onClick={() => {
                  setFiltroIdRecibo('');
                  cargarRecibos();
                }}
                className="btn btn-secondary"
              >
                <RefreshCw size={15} />
              </button>
            </div>

            {/* Generar Recibo Manual por N° de Cobro [RF-69] */}
            <form onSubmit={handleGenerarReciboManual} className="card" style={{ padding: '14px 18px', display: 'flex', gap: 10, alignItems: 'center' }}>
              <div style={{ flex: 1 }}>
                <input
                  type="number"
                  required
                  placeholder="N° de Cobro validado para emitir recibo..."
                  value={numeroCobroParaGenerar}
                  onChange={(e) => setNumeroCobroParaGenerar(e.target.value)}
                  className="form-control"
                />
              </div>
              <button type="submit" disabled={generandoRecibo} className="btn btn-primary" style={{ whiteSpace: 'nowrap' }}>
                <Plus size={15} />
                <span>{generandoRecibo ? 'Emitiendo...' : 'Generar Recibo [RF-69]'}</span>
              </button>
            </form>
          </div>

          {/* Tabla de Recibos Emitidos */}
          <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
            {loadingRecibos ? (
              <div style={{ padding: 48, textAlign: 'center', color: 'var(--text-dim)' }}>
                Consultando libro de recibos emitidos en el backend...
              </div>
            ) : recibos.length === 0 ? (
              <div style={{ padding: 48, textAlign: 'center' }}>
                <Receipt size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
                <h3 style={{ marginBottom: 6 }}>No hay recibos registrados</h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                  Los recibos se emiten automáticamente al validar un cobro o de forma manual con su número de cobro.
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
                      <th style={{ padding: '14px 16px', width: 90 }}>ID Recibo</th>
                      <th style={{ padding: '14px 16px' }}>N° Cobro Asociado</th>
                      <th style={{ padding: '14px 16px' }}>Fecha de Emisión</th>
                      <th style={{ padding: '14px 16px' }}>Medio de Pago</th>
                      <th style={{ padding: '14px 16px', textAlign: 'center' }}>% Abonado</th>
                      <th style={{ padding: '14px 16px' }}>Importe Total</th>
                      <th style={{ padding: '14px 16px', textAlign: 'center' }}>Estado</th>
                      <th style={{ padding: '14px 16px', textAlign: 'right' }}>Acciones</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recibos.map((r) => {
                      const fechaStr = r.fechaEmision ? new Date(r.fechaEmision).toLocaleDateString('es-AR', { hour: '2-digit', minute: '2-digit' }) : 'N/A';

                      return (
                        <tr key={r.id} style={{ borderBottom: '1px solid var(--border)' }}>
                          <td style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--primary)' }}>
                            REC-2026-00{r.id}
                          </td>
                          <td style={{ padding: '14px 16px', fontWeight: 600 }}>
                            Cobro #{r.numeroCobro}
                          </td>
                          <td style={{ padding: '14px 16px', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                            {fechaStr}
                          </td>
                          <td style={{ padding: '14px 16px' }}>
                            <span style={{ fontSize: '0.8rem', fontWeight: 500 }}>
                              {r.medioDePago}
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'center' }}>
                            <span className="badge badge-primary" style={{ fontSize: '0.7rem' }}>
                              {r.porcentajePago}%
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--accent-volt)' }}>
                            ${r.importeTotal?.toLocaleString('es-AR')}
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'center' }}>
                            <span className={`badge ${r.anulado ? 'badge-danger' : 'badge-success'}`} style={{ fontSize: '0.7rem' }}>
                              {r.anulado ? 'Anulado' : 'Válido'}
                            </span>
                          </td>
                          <td style={{ padding: '14px 16px', textAlign: 'right' }}>
                            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 6 }}>
                              <button
                                onClick={() => handleImprimirRecibo(r.id)}
                                className="btn btn-secondary btn-sm"
                                title="Ver e imprimir recibo oficial [RF-72]"
                              >
                                <Printer size={13} />
                                <span>Imprimir</span>
                              </button>

                              {!r.anulado && isAdmin && (
                                <button
                                  onClick={() => handleAnularRecibo(r.id)}
                                  className="btn btn-danger btn-sm"
                                  title="Anular recibo [RF-71]"
                                >
                                  <X size={13} />
                                </button>
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
        </div>
      )}

      {/* ========================================================
          MODAL A: REGISTRAR NUEVO COBRO [RF-65]
          ======================================================== */}
      {modalCobroOpen && (
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
          <div className="card" style={{ maxWidth: 580, width: '100%', maxHeight: '90vh', overflowY: 'auto', position: 'relative' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <div>
                <h3 style={{ fontSize: '1.25rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <DollarSign color="var(--primary)" size={22} />
                  Registrar Cobro Oficial [RF-65]
                </h3>
                <small style={{ color: 'var(--text-muted)' }}>
                  Alquileres de cancha, clases, entrenamientos o cargos por cancelación
                </small>
              </div>
              <button onClick={() => setModalCobroOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleRegistrarCobro} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">DNI del Socio Titular *</label>
                  <input
                    type="number"
                    required
                    placeholder="Ej: 40123456"
                    className="form-control"
                    value={cobroForm.socioDni}
                    onChange={(e) => setCobroForm({ ...cobroForm, socioDni: e.target.value })}
                  />
                </div>

                <div>
                  <label className="form-label">Importe Base ($ ARS) *</label>
                  <input
                    type="number"
                    step="0.01"
                    min="1"
                    required
                    placeholder="Ej: 12000"
                    className="form-control"
                    value={cobroForm.importeBase}
                    onChange={(e) => setCobroForm({ ...cobroForm, importeBase: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Modalidad de Cobro</label>
                  <select
                    className="form-control"
                    value={cobroForm.modalidadPago}
                    onChange={(e) => setCobroForm({ ...cobroForm, modalidadPago: e.target.value })}
                  >
                    <option value="total">Pago Total (100%)</option>
                    <option value="sena">Seña Inicial (50% obligatorio)</option>
                    <option value="saldo">Saldo Final (50% restante)</option>
                  </select>
                </div>

                <div>
                  <label className="form-label">Medio de Pago</label>
                  <select
                    className="form-control"
                    value={cobroForm.tipoPago}
                    onChange={(e) => {
                      const val = e.target.value;
                      setCobroForm({ ...cobroForm, tipoPago: val });
                      if (val === 'TarjetaDebito') {
                        setTarjetaForm(prev => ({ ...prev, paymentMethodIdMp: 'debvisa', cuotas: 1 }));
                      } else if (val === 'TarjetaCredito') {
                        setTarjetaForm(prev => ({ ...prev, paymentMethodIdMp: 'visa', cuotas: 1 }));
                      }
                    }}
                  >
                    <option value="QR">Billetera Virtual Mercado Pago (QR)</option>
                    <option value="TarjetaDebito">Tarjeta de Débito</option>
                    <option value="TarjetaCredito">Tarjeta de Crédito</option>
                  </select>
                </div>
              </div>

              {/* Vinculaciones Opcionales */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">ID de Reserva (Opcional)</label>
                  <input
                    type="number"
                    placeholder="Ej: 4"
                    className="form-control"
                    value={cobroForm.reservaId}
                    onChange={(e) => setCobroForm({ ...cobroForm, reservaId: e.target.value })}
                  />
                </div>

                <div>
                  <label className="form-label">ID de Inscripción (Opcional)</label>
                  <input
                    type="number"
                    placeholder="Ej: 1"
                    className="form-control"
                    value={cobroForm.inscripcionId}
                    onChange={(e) => setCobroForm({ ...cobroForm, inscripcionId: e.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div>
                  <label className="form-label">Recargo Cancelación Tardía</label>
                  <input
                    type="number"
                    min="0"
                    placeholder="0"
                    className="form-control"
                    value={cobroForm.recargoCancelacion}
                    onChange={(e) => setCobroForm({ ...cobroForm, recargoCancelacion: e.target.value })}
                  />
                </div>

                <div>
                  <label className="form-label">ID Descuento (Opcional)</label>
                  <input
                    type="number"
                    placeholder="Ej: 1"
                    className="form-control"
                    value={cobroForm.descuentoId}
                    onChange={(e) => setCobroForm({ ...cobroForm, descuentoId: e.target.value })}
                  />
                </div>
              </div>

              {/* Campos de Tarjeta si corresponde */}
              {(cobroForm.tipoPago === 'TarjetaDebito' || cobroForm.tipoPago === 'TarjetaCredito') && (
                <div style={{
                  background: 'var(--bg-elevated)',
                  border: '1px solid var(--border)',
                  borderRadius: 'var(--radius-sm)',
                  padding: '12px 14px',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 10
                }}>
                  <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 10 }}>
                    <div>
                      <label className="form-label" style={{ fontSize: '0.75rem' }}>N° Tarjeta (16 dígitos)</label>
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
                        {cobroForm.tipoPago === 'TarjetaDebito' ? (
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
                        disabled={cobroForm.tipoPago === 'TarjetaDebito'}
                        value={cobroForm.tipoPago === 'TarjetaDebito' ? 1 : tarjetaForm.cuotas}
                        onChange={(e) => setTarjetaForm({ ...tarjetaForm, cuotas: Number(e.target.value) })}
                      >
                        <option value={1}>1 Cuota</option>
                        {cobroForm.tipoPago === 'TarjetaCredito' && (
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

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalCobroOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submittingCobro} className="btn btn-primary">
                  {submittingCobro ? 'Procesando en Mercado Pago...' : 'Registrar y Cobrar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL B: VISUALIZACIÓN QR Y CONFIRMACIÓN [RD-08]
          ======================================================== */}
      {modalQrCobroOpen && cobroQrActual && (
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
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <QrCode color="var(--primary)" size={20} />
                Orden de Pago QR #{cobroQrActual.numeroCobro}
              </h3>
              <button onClick={() => setModalQrCobroOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 14 }}>
              {cobroQrActual.codigoQrBase64 ? (
                <img
                  src={`data:image/png;base64,${cobroQrActual.codigoQrBase64}`}
                  alt="QR Mercado Pago"
                  style={{ width: 210, height: 210, borderRadius: 8, border: '1px solid #cbd5e1' }}
                />
              ) : (
                <div style={{ padding: 30, background: 'var(--bg-elevated)', borderRadius: 8 }}>
                  <QrCode size={80} color="var(--primary)" />
                </div>
              )}

              <div style={{ textAlign: 'center', fontSize: '0.85rem' }}>
                <div>Monto a cobrar: <strong>${cobroQrActual.importe?.toLocaleString('es-AR')}</strong> ({cobroQrActual.porcentajePago}%)</div>
                <div style={{ color: 'var(--text-muted)', marginTop: 4 }}>
                  Estado: <span className="badge badge-primary">{cobroQrActual.estado}</span>
                </div>
                {cobroQrActual.urlPagoQr && (
                  <a
                    href={cobroQrActual.urlPagoQr}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn btn-volt btn-sm"
                    style={{ marginTop: 10, display: 'inline-flex', alignItems: 'center', gap: 6, textDecoration: 'none' }}
                  >
                    <ExternalLink size={14} />
                    <span>Abrir Checkout Sandbox Mercado Pago</span>
                  </a>
                )}
              </div>

              {cobroQrActual.estado === 'Pendiente' ? (
                <div style={{ width: '100%', padding: '12px', background: 'var(--bg-elevated)', borderRadius: 6, border: '1px solid var(--border)' }}>
                  <label className="form-label" style={{ fontSize: '0.75rem', fontWeight: 700 }}>
                    Confirmar Acreditación QR (Payment ID generado en Sandbox):
                  </label>
                  <div style={{ display: 'flex', gap: 8 }}>
                    <input
                      type="number"
                      placeholder="Ej: 1234567890"
                      value={inputPaymentIdQr}
                      onChange={(e) => setInputPaymentIdQr(e.target.value)}
                      className="form-control"
                      style={{ fontSize: '0.85rem' }}
                    />
                    <button
                      type="button"
                      disabled={confirmandoQr || !inputPaymentIdQr.trim()}
                      onClick={handleConfirmarQr}
                      className="btn btn-primary btn-sm"
                      style={{ whiteSpace: 'nowrap' }}
                    >
                      {confirmandoQr ? 'Validando...' : 'Confirmar QR'}
                    </button>
                  </div>
                </div>
              ) : (
                <div style={{ color: '#15803d', fontWeight: 700, fontSize: '0.85rem', display: 'flex', alignItems: 'center', gap: 6 }}>
                  <CheckCircle2 size={16} color="#15803d" />
                  <span>Cobro validado con éxito (Recibo #{cobroQrActual.reciboId})</span>
                </div>
              )}
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
              <button onClick={() => setModalQrCobroOpen(false)} className="btn btn-secondary">
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL C: ANULAR COBRO Y PROCESAR REEMBOLSO [RF-67]
          ======================================================== */}
      {modalAnularCobroOpen && cobroParaAnular && (
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
          <div className="card" style={{ maxWidth: 440, width: '100%', position: 'relative' }}>
            <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: 8, marginBottom: 12, color: 'var(--accent-red)' }}>
              <XCircle size={20} />
              Anular Cobro #{cobroParaAnular.numeroCobro} [RF-67]
            </h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', lineHeight: 1.5, marginBottom: 14 }}>
              Esta acción anulará el cobro de <strong>${cobroParaAnular.importe?.toLocaleString('es-AR')}</strong>, reembolsará o cancelará la transacción en Mercado Pago y anulará el recibo vinculado.
            </p>

            <form onSubmit={handleConfirmarAnularCobro} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              <div>
                <label className="form-label">Motivo de Anulación (Obligatorio) *</label>
                <textarea
                  rows="3"
                  required
                  placeholder="Indique el motivo técnico o administrativo de la anulación..."
                  className="form-control"
                  value={motivoAnulacion}
                  onChange={(e) => setMotivoAnulacion(e.target.value)}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 8 }}>
                <button type="button" onClick={() => setModalAnularCobroOpen(false)} className="btn btn-secondary">
                  Cancelar
                </button>
                <button type="submit" disabled={submittingAnulacion} className="btn btn-danger">
                  {submittingAnulacion ? 'Procesando Reembolso...' : 'Confirmar Anulación'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL D: IMPRESIÓN COMPROBANTE DE COBRO [RF-68]
          ======================================================== */}
      {modalImprimirCobroOpen && impresionCobroData && (
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
          <div className="card" style={{ maxWidth: 520, width: '100%', position: 'relative', maxHeight: '90vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <FileText color="var(--primary)" size={20} />
                Comprobante Oficial de Cobro [RF-68]
              </h3>
              <button onClick={() => setModalImprimirCobroOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Ficha Imprimible */}
            <div id="print-cobro" style={{
              background: '#ffffff',
              color: '#0f172a',
              padding: '24px',
              borderRadius: 'var(--radius-sm)',
              display: 'flex',
              flexDirection: 'column',
              gap: 12,
              border: '1px solid #cbd5e1'
            }}>
              <div style={{ borderBottom: '2px solid #0f172a', paddingBottom: 8, display: 'flex', justifyContent: 'space-between' }}>
                <div>
                  <h2 style={{ fontSize: '1.15rem', margin: 0, fontWeight: 800 }}>{impresionCobroData.establecimiento}</h2>
                  <small style={{ color: '#64748b', fontSize: '0.75rem' }}>Control Fiscal y Tesorería</small>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <span style={{ fontSize: '0.85rem', fontWeight: 800 }}>COMPROBANTE DE COBRO</span>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>N° {impresionCobroData.numeroCobro}</div>
                </div>
              </div>

              <div style={{ fontSize: '0.8125rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <div><strong>Fecha:</strong> {new Date(impresionCobroData.fechaPago).toLocaleString('es-AR')}</div>
                <div><strong>Medio:</strong> {impresionCobroData.medioDePago}</div>
                <div><strong>Estado:</strong> {impresionCobroData.estado}</div>
                <div><strong>% Liquidado:</strong> {impresionCobroData.porcentajePago}%</div>
                {impresionCobroData.mercadoPagoPaymentId && (
                  <div><strong>ID MP:</strong> #{impresionCobroData.mercadoPagoPaymentId}</div>
                )}
                {impresionCobroData.reciboId && (
                  <div><strong>Recibo Fiscal:</strong> #{impresionCobroData.reciboId}</div>
                )}
              </div>

              <div style={{ borderTop: '1px dashed #cbd5e1', paddingTop: 8, fontSize: '0.8125rem' }}>
                <strong>Concepto:</strong> {impresionCobroData.concepto}
              </div>

              <div style={{
                background: '#f1f5f9',
                padding: '12px',
                borderRadius: 6,
                display: 'flex',
                justifyContent: 'space-between',
                fontSize: '1.05rem',
                fontWeight: 800,
                color: '#0f172a'
              }}>
                <span>Total Cobrado:</span>
                <span>${impresionCobroData.importeTotal?.toLocaleString('es-AR')}</span>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginTop: 16 }}>
              <button onClick={() => window.print()} className="btn btn-secondary">
                <Printer size={15} />
                <span>Imprimir</span>
              </button>
              <button onClick={() => setModalImprimirCobroOpen(false)} className="btn btn-primary">
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ========================================================
          MODAL E: IMPRESIÓN RECIBO OFICIAL [RF-72]
          ======================================================== */}
      {modalImprimirReciboOpen && impresionReciboData && (
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
          <div className="card" style={{ maxWidth: 540, width: '100%', position: 'relative', maxHeight: '90vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <h3 style={{ fontSize: '1.2rem', display: 'flex', alignItems: 'center', gap: 8 }}>
                <Receipt color="var(--primary)" size={20} />
                Recibo Oficial de Pago [RF-72]
              </h3>
              <button onClick={() => setModalImprimirReciboOpen(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            {/* Ficha Imprimible del Recibo */}
            <div id="print-recibo" style={{
              background: '#ffffff',
              color: '#0f172a',
              padding: '24px',
              borderRadius: 'var(--radius-sm)',
              display: 'flex',
              flexDirection: 'column',
              gap: 12,
              border: '1px solid #cbd5e1'
            }}>
              <div style={{ borderBottom: '2px solid #0f172a', paddingBottom: 8, display: 'flex', justifyContent: 'space-between' }}>
                <div>
                  <h2 style={{ fontSize: '1.2rem', margin: 0, fontWeight: 800 }}>TENIS AHORA</h2>
                  <small style={{ color: '#64748b', fontSize: '0.75rem' }}>{impresionReciboData.establecimiento}</small>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <span style={{ fontSize: '0.85rem', fontWeight: 800 }}>RECIBO OFICIAL</span>
                  <div style={{ fontSize: '0.75rem', color: '#64748b' }}>REC-2026-00{impresionReciboData.reciboId}</div>
                </div>
              </div>

              <div style={{ fontSize: '0.8125rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
                <div><strong>Socio:</strong> {impresionReciboData.nombreSocio || `Socio DNI ${impresionReciboData.socioDni}`}</div>
                <div><strong>DNI:</strong> {impresionReciboData.socioDni || '---'}</div>
                <div><strong>Fecha Emisión:</strong> {new Date(impresionReciboData.fechaEmision).toLocaleString('es-AR')}</div>
                <div><strong>N° Cobro Asociado:</strong> #{impresionReciboData.numeroCobro}</div>
                <div><strong>Medio de Pago:</strong> {impresionReciboData.medioDePago}</div>
                <div><strong>Estado:</strong> {impresionReciboData.estadoRecibo}</div>
              </div>

              <div style={{ borderTop: '1px dashed #cbd5e1', paddingTop: 8, fontSize: '0.8125rem' }}>
                <strong>Concepto Liquidado:</strong> {impresionReciboData.concepto}
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
                  <span>Porcentaje Abonado:</span>
                  <strong>{impresionReciboData.porcentajeAbonado}%</strong>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid #cbd5e1', paddingTop: 4, fontSize: '1.05rem', fontWeight: 800 }}>
                  <span>Total Liquidado:</span>
                  <span style={{ color: '#15803d' }}>${impresionReciboData.importeTotal?.toLocaleString('es-AR')}</span>
                </div>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12, marginTop: 16 }}>
              <button onClick={() => window.print()} className="btn btn-secondary">
                <Printer size={15} />
                <span>Imprimir Recibo</span>
              </button>
              <button onClick={() => setModalImprimirReciboOpen(false)} className="btn btn-primary">
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
