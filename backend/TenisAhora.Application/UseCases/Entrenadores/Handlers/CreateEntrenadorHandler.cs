using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenadores.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class CreateEntrenadorHandler : ICreateEntrenadorHandler
    {
        private readonly IEntrenadorRepository _entrenadorRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateEntrenadorHandler(
            IEntrenadorRepository entrenadorRepository,
            IPasswordHasher passwordHasher)
        {
            _entrenadorRepository = entrenadorRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<EntrenadorResponseDto> Handle(
            CreateEntrenadorCommand command)
        {
            var entrenador = new Entrenador
            {
                // Datos personales
                Nombre = command.Nombre,
                Apellido = command.Apellido,
                Dni = command.Dni,
                FechaDeNacimiento = command.FechaDeNacimiento,
                Telefono = command.Telefono,
                Direccion = command.Direccion,
                CodigoPostal = command.CodigoPostal,
                Email = command.Email,

                PasswordHash = _passwordHasher.Hash(command.Password),

                // Datos del instructor
                especialidad = command.Especialidad,
                antiguedad = command.Antiguedad,
                AdministradorId = command.AdministradorId,

                // Datos específicos del entrenador
                Objetivo = command.Objetivo,

                // Estado inicial
                Rol = Rol.Entrenador,
                Activo = false
            };

            var entrenadorCreado =
                await _entrenadorRepository.CrearAsync(entrenador);

            return new EntrenadorResponseDto
            {
                Id = entrenadorCreado.Id,
                Nombre = entrenadorCreado.Nombre,
                Apellido = entrenadorCreado.Apellido,
                Dni = entrenadorCreado.Dni,
                FechaDeNacimiento = entrenadorCreado.FechaDeNacimiento,
                Telefono = entrenadorCreado.Telefono,
                Direccion = entrenadorCreado.Direccion,
                CodigoPostal = entrenadorCreado.CodigoPostal,
                Email = entrenadorCreado.Email,
                Especialidad = entrenadorCreado.especialidad,
                Antiguedad = entrenadorCreado.antiguedad,
                AdministradorId = entrenadorCreado.AdministradorId,
                Objetivo = entrenadorCreado.Objetivo,
                Activo = entrenadorCreado.Activo
            };
        }



    }
}
