using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.CertificacionesDeportivas.Commands;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CertificacionesDeportivasController : ControllerBase
    {
        private readonly ICreateCertificacionDeportivaHandler _handler;
        private readonly IResolverCertificacionHandler _resolverHandler;


        public CertificacionesDeportivasController( ICreateCertificacionDeportivaHandler handler, IResolverCertificacionHandler resolverHandler)
        {
            _handler = handler;
            _resolverHandler = resolverHandler;
        }


        [HttpPost]
        public async Task<IActionResult> Create( [FromBody] CreateCertificacionDeportivaCommand command)
        {
            try
            {
                var id = await _handler.Handle(command);

                return Created(
                    $"/api/CertificacionesDeportivas/{id}",
                    new
                    {
                        id,
                        mensaje = "Certificación deportiva registrada correctamente."
                    });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }


        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> Resolver(
    int id,
    [FromBody] ResolverCertificacionCommand command)
        {
            if (command.EstadoCertificacion != EstadoCertificacion.Aprobada &&
                command.EstadoCertificacion != EstadoCertificacion.Rechazada)
            {
                return BadRequest(new
                {
                    mensaje = "El estado debe ser Aprobada o Rechazada."
                });
            }

            var resultado = await _resolverHandler.Handle(id, command);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró la certificación o no se pudo resolver."
                });
            }

            return Ok(new
            {
                mensaje = "Certificación resuelta correctamente."
            });
        }


    }
}
