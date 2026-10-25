using System;
using System.Collections.Generic;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Domain.Entities
{
    public class TipoDeCancha
    {
        public int Id { get; set; }

        public Superficie superficie { get; set; }

        public int capacidad { get; set; }

        public double precio { get; set; }

        public string? caracteristicas { get; set; }

        public List<Cancha> Canchas { get; set; } = [];

        public TipoDeCancha() { }

        public TipoDeCancha(Superficie superficie, int capacidad, double precio, string? caracteristicas = null)
        {
            Validar(superficie, capacidad, precio);
            this.superficie = superficie;
            this.capacidad = capacidad;
            this.precio = precio;
            this.caracteristicas = caracteristicas?.Trim();
        }

        public void Actualizar(Superficie nuevaSuperficie, int nuevaCapacidad, double nuevoPrecio, string? nuevasCaracteristicas)
        {
            Validar(nuevaSuperficie, nuevaCapacidad, nuevoPrecio);
            superficie = nuevaSuperficie;
            capacidad = nuevaCapacidad;
            precio = nuevoPrecio;
            caracteristicas = nuevasCaracteristicas?.Trim();
        }

        private static void Validar(Superficie superficie, int capacidad, double precio)
        {
            if (!Enum.IsDefined(typeof(Superficie), superficie))
            {
                throw new DomainException("La superficie ingresada no es válida. Debe ser Ladrillo, Cemento o Pasto.");
            }

            if (capacidad <= 0)
            {
                throw new DomainException("La capacidad de la cancha debe ser mayor a 0.");
            }

            if (precio < 0)
            {
                throw new DomainException("El precio del tipo de cancha no puede ser negativo.");
            }
        }
    }
}
