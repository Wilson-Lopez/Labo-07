using System.Configuration;

namespace Biblioteca.Datos.Conexion
{
    /// <summary>
    /// Devuelve la cadena de conexión leída desde App.config del proyecto de inicio (WPF).
    /// Requisito 15 del laboratorio.
    /// </summary>
    public static class ConexionBD
    {
        public static string CadenaConexion
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["BibliotecaDB"].ConnectionString;
            }
        }
    }
}