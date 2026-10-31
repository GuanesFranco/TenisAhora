namespace TenisAhora.Application.UseCases.Pagos.Commands;

public record ConfirmarCobroQrCommand(int NumeroCobro, long MercadoPagoPaymentId);
