using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class GetEntrenadoresHandler : IGetEntrenadoresHandler
    {
        private readonly IEntrenadorRepository _entrenadorRepository;

        public GetEntrenadoresHandler(IEntrenadorRepository entrenadorRepository)
        {
            _entrenadorRepository = entrenadorRepository;
        }

        public async Task<List<EntrenadorResponseDto>> Handle()
        {
            var entrenadores = await _entrenadorRepository.ObtenerTodosAsync();

            return entrenadores.Select(e => new EntrenadorResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Apellido = e.Apellido,
                Dni = e.Dni,
                FechaDeNacimiento = e.FechaDeNacimiento,
                Telefono = e.Telefono,
                Direccion = e.Direccion,
                CodigoPostal = e.CodigoPostal,
                Email = e.Email,
                Especialidad = e.especialidad,
                Antiguedad = e.antiguedad,
                AdministradorId = e.AdministradorId,
                Objetivo = e.Objetivo,
                Activo = e.Activo
            }).ToList();
        }

    }
}
