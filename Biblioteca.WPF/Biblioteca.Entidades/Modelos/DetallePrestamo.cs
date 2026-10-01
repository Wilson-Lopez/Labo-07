using System;

namespace Biblioteca.Entidades.Modelos
{
    public class DetallePrestamo
    {
        public int PrestamoId { get; set; }
        public int LibroId { get; set; }
        public DateTime? FechaDevolucion { get; set; }

        // Propiedades auxiliares para el reporte (requisito 14)
        public string TituloLibro { get; set; }
        public string NombreAutor { get; set; }
        public string NombreSocio { get; set; }
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; }
    }
}