using System.Collections.Generic;
using System.Data.SqlClient;
using Biblioteca.Datos.Conexion;
using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Repositorios
{
    public class AutorDatos
    {
        // LISTAR (solo activos)
        public List<Autor> Listar()
        {
            var lista = new List<Autor>();
            string sql = "SELECT AutorId, Nombre, Nacionalidad, Activo FROM Autores WHERE Activo = 1";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Autor
                        {
                            AutorId = (int)dr["AutorId"],
                            Nombre = dr["Nombre"].ToString(),
                            Nacionalidad = dr["Nacionalidad"].ToString(),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }
            return lista;
        }
    }
}