using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Profesores.Commands;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class UpdateProfesorHandler : IUpdateProfesorHandler

    {
        private readonly IProfesorRepository _profesorRepository;

        public UpdateProfesorHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<ProfesorResponseDto?> Handle(
            int id,
            UpdateProfesorCommand command)
        {
            var profesor =
                await _profesorRepository
                    .ObtenerPorIdParaActualizarAsync(id);

            if (profesor is null)
            {
                return null;
            }

            // Actualizar datos personales.
            profesor.Nombre = command.Nombre;
            profesor.Apellido = command.Apellido;
            profesor.Dni = command.Dni;
            profesor.FechaDeNacimiento = command.FechaDeNacimiento;
            profesor.Telefono = command.Telefono;
            profesor.Direccion = command.Direccion;
            profesor.CodigoPostal = command.CodigoPostal;
            profesor.Email = command.Email;

            // Actualizar datos del instructor.
            profesor.especialidad = command.Especialidad;
            profesor.antiguedad = command.Antiguedad;

            // Actualizar datos específicos del profesor.
            profesor.TipoDeEnsenanza = command.TipoDeEnsenanza;

            await _profesorRepository.GuardarCambiosAsync();

            var certificacion = profesor.certificacionDeportiva;

            // Devolver el objeto modificado sin exponer PasswordHash.
            return new ProfesorResponseDto
            {
                Id = profesor.Id,

                Nombre = profesor.Nombre,
                Apellido = profesor.Apellido,
                Dni = profesor.Dni,
                FechaDeNacimiento = profesor.FechaDeNacimiento,
                Telefono = profesor.Telefono,
                Direccion = profesor.Direccion,
                CodigoPostal = profesor.CodigoPostal,
                Email = profesor.Email,

                Especialidad = profesor.especialidad,
                Antiguedad = profesor.antiguedad,
                AdministradorId = profesor.AdministradorId,

                TipoDeEnsenanza = profesor.TipoDeEnsenanza,
                Activo = profesor.Activo,

                CertificacionId = certificacion?.Id,
                NombreCertificacion = certificacion?.NombreCertificacion,
                EnteEmisor = certificacion?.EnteEmisor,
                FechaEmision = certificacion?.FechaEmision,
                CertificacionVerificada =
                    certificacion?.certificacionDeportiva
            };
        }



    }
}
