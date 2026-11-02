using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Reportes.Commands;

namespace TenisAhora.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ICreateReporteHandler _handler;

        public ReportesController(ICreateReporteHandler handler)
        {
            _handler = handler;
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CreateReporteCommand command)
        {
            var pdf = await _handler.Handle(command);

            if (pdf == null)
            {
                return BadRequest(
                    "El administrador no existe o el tipo de reporte no es válido. Por ahora solo está implementado el reporte de Canchas.");
            }

            return File(
                pdf,
                "application/pdf",
                $"Reporte_{command.TipoReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }


    }
}
