namespace TenisAhora.Application.DTOs.Pagos;

public record ProcesarTarjetaMpRequest(
    decimal Monto,
    string TokenTarjeta,
    string PaymentMethodId,
    int Cuotas,
    string EmailPagador,
    int DniPagador,
    string Descripcion);

public record ResultadoPagoMpDto(
    long PaymentId,
    string Status,
    string StatusDetail,
    bool Aprobado);

public record GenerarQrMpRequest(
    decimal Monto,
    string TituloConcepto,
    string ReferenciaExterna,
    string EmailPagador);

public record ResultadoQrMpDto(
    string PreferenceId,
    string UrlPagoQr,
    string CodigoQrBase64);
