using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Profesores.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class CreateProfesorHandler : ICreateProfesorHandler
    {
        private readonly IProfesorRepository _profesorRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateProfesorHandler(
            IProfesorRepository profesorRepository,
            IPasswordHasher passwordHasher)
        {
            _profesorRepository = profesorRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<ProfesorResponseDto> Handle(
            CreateProfesorCommand command)
        {
            var profesor = new Profesor
            {
                Nombre = command.Nombre,
                Apellido = command.Apellido,
                Dni = command.Dni,
                FechaDeNacimiento = command.FechaDeNacimiento,
                Telefono = command.Telefono,
                Direccion = command.Direccion,
                CodigoPostal = command.CodigoPostal,
                Email = command.Email,

                PasswordHash = _passwordHasher.Hash(command.Password),

                especialidad = command.Especialidad,
                antiguedad = command.Antiguedad,
                AdministradorId = command.AdministradorId,
                TipoDeEnsenanza = command.TipoDeEnsenanza,

                Rol = Rol.Profesor,
                Activo = false
            };

            var profesorCreado =
                await _profesorRepository.CrearAsync(profesor);

            return new ProfesorResponseDto
            {
                Id = profesorCreado.Id,
                Nombre = profesorCreado.Nombre,
                Apellido = profesorCreado.Apellido,
                Dni = profesorCreado.Dni,
                FechaDeNacimiento = profesorCreado.FechaDeNacimiento,
                Telefono = profesorCreado.Telefono,
                Direccion = profesorCreado.Direccion,
                CodigoPostal = profesorCreado.CodigoPostal,
                Email = profesorCreado.Email,
                Especialidad = profesorCreado.especialidad,
                Antiguedad = profesorCreado.antiguedad,
                AdministradorId = profesorCreado.AdministradorId,
                TipoDeEnsenanza = profesorCreado.TipoDeEnsenanza,
                Activo = profesorCreado.Activo
            };
        }


    }
}
