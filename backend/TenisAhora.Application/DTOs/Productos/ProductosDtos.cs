using System;
using System.ComponentModel.DataAnnotations;

namespace TenisAhora.Application.DTOs.Productos
{
    public class CrearProductoCommand
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0.")]
        public float Precio { get; set; }

        [Range(1, 3, ErrorMessage = "Solo se permiten los equipamientos oficiales del club: 1 (Pelotas de tenis), 2 (Raquetas) o 3 (Redes).")]
        public int StockId { get; set; }
    }

    public class ModificarProductoCommand
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del producto es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0.")]
        public float Precio { get; set; }

        public int? StockId { get; set; }
    }

    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public float Precio { get; set; }
        public int StockId { get; set; }
        public string TipoEquipamiento { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public bool Activo { get; set; }
    }
}
