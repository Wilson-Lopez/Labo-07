using System;
using System.Collections.Generic;

namespace Biblioteca.Entidades.Modelos
{
    public class Prestamo
    {
        public int PrestamoId { get; set; }
        public int SocioId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; }

        // Propiedades auxiliares para el reporte y la UI
        public string NombreSocio { get; set; }
        public List<DetallePrestamo> Detalles { get; set; }

        public Prestamo()
        {
            Detalles = new List<DetallePrestamo>();
        }
    }
}