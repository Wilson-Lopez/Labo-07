using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos.Repositorios;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.Negocio.Negocios
{
    public class LibroNegocio
    {
        private readonly LibroDatos _libroDatos;
        private readonly AutorDatos _autorDatos;

        public LibroNegocio()
        {
            _libroDatos = new LibroDatos();
            _autorDatos = new AutorDatos();
        }

        public async Task<List<Libro>> ListarAsync()
        {
            return await Task.Run(() => _libroDatos.Listar());
        }

        public async Task<List<Autor>> ListarAutoresAsync()
        {
            return await Task.Run(() => _autorDatos.Listar());
        }

        public async Task<List<Libro>> BuscarAsync(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return await ListarAsync();

            return await Task.Run(() => _libroDatos.Buscar(filtro.Trim()));
        }

        public async Task<int> InsertarAsync(Libro libro)
        {
            Validar(libro, esNuevo: true);

            bool existe = await Task.Run(() => _libroDatos.ExisteISBN(libro.ISBN, 0));
            if (existe)
                throw new ReglaNegocioException("Ya existe un libro registrado con el mismo ISBN.");

            return await Task.Run(() => _libroDatos.Insertar(libro));
        }

        public async Task ActualizarAsync(Libro libro)
        {
            if (libro.LibroId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un libro válido para actualizar.");

            Validar(libro, esNuevo: false);

            bool existe = await Task.Run(() => _libroDatos.ExisteISBN(libro.ISBN, libro.LibroId));
            if (existe)
                throw new ReglaNegocioException("Ya existe otro libro registrado con el mismo ISBN.");

            await Task.Run(() => _libroDatos.Actualizar(libro));
        }

        public async Task EliminarLogicoAsync(int libroId)
        {
            if (libroId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un libro válido para eliminar.");

            bool tienePendientes = await Task.Run(() => _libroDatos.TienePrestamosPendientes(libroId));
            if (tienePendientes)
                throw new ReglaNegocioException(
                    "No se puede dar de baja el libro porque tiene préstamos pendientes de devolución.");

            await Task.Run(() => _libroDatos.EliminarLogico(libroId));
        }

        private void Validar(Libro libro, bool esNuevo)
        {
            if (libro == null)
                throw new ReglaNegocioException("Los datos del libro no pueden estar vacíos.");

            if (string.IsNullOrWhiteSpace(libro.Titulo))
                throw new ReglaNegocioException("El título del libro es obligatorio.");

            if (string.IsNullOrWhiteSpace(libro.ISBN))
                throw new ReglaNegocioException("El ISBN del libro es obligatorio.");

            if (libro.AutorId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un autor para el libro.");

            if (libro.Ejemplares < 0)
                throw new ReglaNegocioException("El número de ejemplares no puede ser negativo.");

            if (esNuevo && libro.Ejemplares == 0)
                throw new ReglaNegocioException("El libro debe tener al menos un ejemplar al ser registrado.");
        }
    }
}