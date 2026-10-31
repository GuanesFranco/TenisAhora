using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.UseCases.Pagos.Commands;
using TenisAhora.Application.UseCases.Pagos.Handlers;
using TenisAhora.Application.UseCases.Pagos.Queries;

namespace TenisAhora.Api.Controllers;

/// <summary>
/// Controlador para la Gestión Integral de Pagos y Cobros.
/// Cumple con las especificaciones [RF-65, RF-66, RF-67, RF-68].
/// </summary>
[ApiController]
[Route("api/cobros")]
public class CobrosController : ControllerBase
{
    private readonly RegistrarCobroHandler _registrarHandler;
    private readonly ConfirmarCobroQrHandler _confirmarQrHandler;
    private readonly ObtenerCobroPorNumeroHandler _obtenerPorNumeroHandler;
    private readonly ListarCobrosHandler _listarHandler;
    private readonly AnularCobroHandler _anularHandler;
    private readonly ImprimirCobroHandler _imprimirHandler;

    public CobrosController(
        RegistrarCobroHandler registrarHandler,
        ConfirmarCobroQrHandler confirmarQrHandler,
        ObtenerCobroPorNumeroHandler obtenerPorNumeroHandler,
        ListarCobrosHandler listarHandler,
        AnularCobroHandler anularHandler,
        ImprimirCobroHandler imprimirHandler)
    {
        _registrarHandler = registrarHandler;
        _confirmarQrHandler = confirmarQrHandler;
        _obtenerPorNumeroHandler = obtenerPorNumeroHandler;
        _listarHandler = listarHandler;
        _anularHandler = anularHandler;
        _imprimirHandler = imprimirHandler;
    }

    /// <summary>
    /// [RF-65] Registra y procesa un nuevo cobro (tarjeta o QR).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CobroResponseDto>> Registrar([FromBody] RegistrarCobroCommand command, CancellationToken ct)
    {
        var creado = await _registrarHandler.Handle(command, ct);
        return CreatedAtAction(nameof(ObtenerPorNumero), new { numeroCobro = creado.NumeroCobro }, creado);
    }

    /// <summary>
    /// [RF-65, RD-08] Confirma un cobro realizado mediante código QR tras verificar su aprobación en Mercado Pago.
    /// </summary>
    [HttpPost("{numeroCobro}/confirmar-qr")]
    public async Task<ActionResult<CobroResponseDto>> ConfirmarQr(int numeroCobro, [FromBody] ConfirmarCobroQrBody body, CancellationToken ct)
    {
        var command = new ConfirmarCobroQrCommand(numeroCobro, body.MercadoPagoPaymentId);
        var cobro = await _confirmarQrHandler.Handle(command, ct);
        return Ok(cobro);
    }

    /// <summary>
    /// [RF-66] Consulta un cobro registrado por su número.
    /// </summary>
    [HttpGet("{numeroCobro}")]
    public async Task<ActionResult<CobroResponseDto>> ObtenerPorNumero(int numeroCobro, CancellationToken ct)
    {
        var cobro = await _obtenerPorNumeroHandler.Handle(new ObtenerCobroPorNumeroQuery(numeroCobro), ct);
        return Ok(cobro);
    }

    /// <summary>
    /// [RF-66] Consulta el listado general de todos los cobros registrados.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CobroResponseDto>>> Listar(CancellationToken ct)
    {
        var lista = await _listarHandler.Handle(new ListarCobrosQuery(), ct);
        return Ok(lista);
    }

    /// <summary>
    /// [RF-67] Anula un cobro registrado y reembolsa/cancela la transacción en Mercado Pago si aplica.
    /// </summary>
    [HttpPost("{numeroCobro}/anular")]
    public async Task<ActionResult<CobroResponseDto>> Anular(int numeroCobro, [FromBody] AnularCobroBody body, CancellationToken ct)
    {
        var command = new AnularCobroCommand(numeroCobro, body.Motivo);
        var cobro = await _anularHandler.Handle(command, ct);
        return Ok(cobro);
    }

    /// <summary>
    /// [RF-68] Genera la información técnica del comprobante de cobro para su impresión física o emisión digital.
    /// </summary>
    [HttpGet("{numeroCobro}/imprimir")]
    public async Task<ActionResult<ImpresionCobroDto>> Imprimir(int numeroCobro, CancellationToken ct)
    {
        var impresion = await _imprimirHandler.Handle(new ImprimirCobroQuery(numeroCobro), ct);
        return Ok(impresion);
    }
}

public record ConfirmarCobroQrBody(long MercadoPagoPaymentId);
public record AnularCobroBody(string Motivo);
