using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Biblioteca.Datos.Repositorios;
using Biblioteca.Entidades.Modelos;
using Biblioteca.Negocio.Excepciones;

namespace Biblioteca.Negocio.Negocios
{
    public class SocioNegocio
    {
        private readonly SocioDatos _socioDatos;

        public SocioNegocio()
        {
            _socioDatos = new SocioDatos();
        }

        public async Task<List<Socio>> ListarAsync()
        {
            return await Task.Run(() => _socioDatos.Listar());
        }

        public async Task<List<Socio>> BuscarAsync(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return await ListarAsync();

            return await Task.Run(() => _socioDatos.Buscar(filtro.Trim()));
        }

        public async Task<int> InsertarAsync(Socio socio)
        {
            Validar(socio);

            bool existe = await Task.Run(() => _socioDatos.ExisteDNI(socio.DNI, 0));
            if (existe)
                throw new ReglaNegocioException("Ya existe un socio registrado con el mismo DNI.");

            return await Task.Run(() => _socioDatos.Insertar(socio));
        }

        public async Task ActualizarAsync(Socio socio)
        {
            if (socio.SocioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio válido para actualizar.");

            Validar(socio);

            bool existe = await Task.Run(() => _socioDatos.ExisteDNI(socio.DNI, socio.SocioId));
            if (existe)
                throw new ReglaNegocioException("Ya existe otro socio registrado con el mismo DNI.");

            await Task.Run(() => _socioDatos.Actualizar(socio));
        }

        public async Task EliminarLogicoAsync(int socioId)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio válido para eliminar.");

            bool tienePendientes = await Task.Run(() => _socioDatos.TienePrestamosPendientes(socioId));
            if (tienePendientes)
                throw new ReglaNegocioException(
                    "No se puede dar de baja el socio porque tiene préstamos pendientes de devolución.");

            await Task.Run(() => _socioDatos.EliminarLogico(socioId));
        }

        private void Validar(Socio socio)
        {
            if (socio == null)
                throw new ReglaNegocioException("Los datos del socio no pueden estar vacíos.");

            if (string.IsNullOrWhiteSpace(socio.DNI))
                throw new ReglaNegocioException("El DNI del socio es obligatorio.");

            if (socio.DNI.Trim().Length < 6)
                throw new ReglaNegocioException("El DNI debe tener al menos 6 caracteres.");

            if (string.IsNullOrWhiteSpace(socio.Nombre))
                throw new ReglaNegocioException("El nombre del socio es obligatorio.");

            if (!string.IsNullOrWhiteSpace(socio.Email))
            {
                string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(socio.Email.Trim(), patron))
                    throw new ReglaNegocioException("El email del socio no tiene un formato válido.");
            }
        }
    }
}