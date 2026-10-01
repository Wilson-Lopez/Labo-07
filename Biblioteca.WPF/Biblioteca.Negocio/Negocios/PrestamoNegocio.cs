using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biblioteca.Datos.Repositorios;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.Negocio.Negocios
{
    public class PrestamoNegocio
    {
        private const decimal MULTA_POR_DIA = 1.50m;
        private const int MAX_LIBROS_PENDIENTES = 3;

        private readonly PrestamoDatos _prestamoDatos;
        private readonly SocioDatos _socioDatos;
        private readonly LibroDatos _libroDatos;

        public PrestamoNegocio()
        {
            _prestamoDatos = new PrestamoDatos();
            _socioDatos = new SocioDatos();
            _libroDatos = new LibroDatos();
        }

        // ============ PRÉSTAMO ============
        public async Task<int> RegistrarPrestamoAsync(int socioId, List<int> libroIds,
                                                      DateTime fechaPrestamo, DateTime fechaLimite)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio.");

            if (libroIds == null || libroIds.Count == 0)
                throw new ReglaNegocioException("Debe agregar al menos un libro al préstamo.");

            if (libroIds.Count != libroIds.Distinct().Count())
                throw new ReglaNegocioException("No se puede prestar el mismo libro dos veces en un mismo préstamo.");

            if (fechaLimite.Date <= fechaPrestamo.Date)
                throw new ReglaNegocioException("La fecha límite debe ser posterior a la fecha del préstamo.");

            var socio = await Task.Run(() => _socioDatos.ObtenerPorId(socioId));
            if (socio == null || !socio.Activo)
                throw new ReglaNegocioException("El socio no existe o está inactivo.");

            int pendientesActuales = await Task.Run(() => _prestamoDatos.ContarPendientesPorSocio(socioId));
            if (pendientesActuales + libroIds.Count > MAX_LIBROS_PENDIENTES)
                throw new ReglaNegocioException(
                    string.Format("El socio '{0}' ya tiene {1} libro(s) pendiente(s). " +
                                  "No puede superar los {2} libros pendientes.",
                                  socio.Nombre, pendientesActuales, MAX_LIBROS_PENDIENTES));

            foreach (int libroId in libroIds)
            {
                var libro = await Task.Run(() => _libroDatos.ObtenerPorId(libroId));
                if (libro == null || !libro.Activo)
                    throw new ReglaNegocioException(
                        string.Format("El libro con Id {0} no existe o está inactivo.", libroId));

                if (libro.Ejemplares <= 0)
                    throw new ReglaNegocioException(
                        string.Format("El libro '{0}' no tiene ejemplares disponibles.", libro.Titulo));
            }

            var prestamo = new Prestamo
            {
                SocioId = socioId,
                FechaPrestamo = fechaPrestamo,
                FechaLimite = fechaLimite,
                Estado = "Pendiente"
            };

            foreach (int libroId in libroIds)
            {
                prestamo.Detalles.Add(new DetallePrestamo
                {
                    LibroId = libroId,
                    FechaDevolucion = null
                });
            }

            return await Task.Run(() => _prestamoDatos.RegistrarPrestamo(prestamo));
        }

        public async Task<List<Prestamo>> ListarPorSocioAsync(int socioId)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio válido.");

            return await Task.Run(() => _prestamoDatos.ListarPorSocio(socioId));
        }

        public async Task<List<DetallePrestamo>> ListarPendientesAsync(int prestamoId)
        {
            if (prestamoId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un préstamo válido.");

            return await Task.Run(() => _prestamoDatos.DetallesPendientes(prestamoId));
        }

        // ============ DEVOLUCIÓN ============
        public decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion)
        {
            int diasRetraso = (fechaDevolucion.Date - fechaLimite.Date).Days;
            if (diasRetraso <= 0) return 0m;
            return diasRetraso * MULTA_POR_DIA;
        }

        public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId)
        {
            if (prestamoId <= 0 || libroId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un préstamo y un libro válidos.");

            var pendientes = await Task.Run(() => _prestamoDatos.DetallesPendientes(prestamoId));

            DetallePrestamo detalleEncontrado = null;
            foreach (var d in pendientes)
            {
                if (d.LibroId == libroId) { detalleEncontrado = d; break; }
            }

            if (detalleEncontrado == null)
                throw new ReglaNegocioException(
                    "El libro seleccionado no pertenece a los pendientes de este préstamo.");

            DateTime fechaDevolucion = DateTime.Now;
            decimal multa = CalcularMulta(detalleEncontrado.FechaLimite, fechaDevolucion);

            await Task.Run(() => _prestamoDatos.RegistrarDevolucion(prestamoId, libroId, fechaDevolucion));

            return multa;
        }

        // ============ REPORTE ============
        public async Task<List<DetallePrestamo>> ReportePorFechasAsync(DateTime desde, DateTime hasta)
        {
            if (hasta.Date < desde.Date)
                throw new ReglaNegocioException(
                    "La fecha 'Hasta' debe ser mayor o igual a la fecha 'Desde'.");

            return await Task.Run(() => _prestamoDatos.ReportePorFechas(desde, hasta));
        }
    }
}