using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Pagos.Commands;

public record RegistrarCobroCommand(
    int SocioDni,
    float ImporteBase,
    TipoDePago TipoPago,
    bool EsSena50Porciento = false,
    bool EsSaldoFinalPartido = false,
    int? ReservaId = null,
    int? InscripcionId = null,
    int? DescuentoId = null,
    float RecargoCancelacion = 0f,
    string? TokenTarjetaMp = null,
    string? PaymentMethodIdMp = null,
    int Cuotas = 1
);
