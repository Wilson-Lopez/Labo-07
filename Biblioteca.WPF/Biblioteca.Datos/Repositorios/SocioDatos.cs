using System.Collections.Generic;
using System.Data.SqlClient;
using Biblioteca.Datos.Conexion;
using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Repositorios
{
    public class SocioDatos
    {
        public List<Socio> Listar()
        {
            var lista = new List<Socio>();
            string sql = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE Activo = 1";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Socio
                        {
                            SocioId = (int)dr["SocioId"],
                            DNI = dr["DNI"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Email = dr["Email"] == System.DBNull.Value ? "" : dr["Email"].ToString(),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }
            return lista;
        }

        public List<Socio> Buscar(string filtro)
        {
            var lista = new List<Socio>();
            string sql = @"SELECT SocioId, DNI, Nombre, Email, Activo
                           FROM Socios
                           WHERE Activo = 1
                             AND (Nombre LIKE @filtro OR DNI LIKE @filtro)";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@filtro", "%" + filtro + "%"));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Socio
                        {
                            SocioId = (int)dr["SocioId"],
                            DNI = dr["DNI"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Email = dr["Email"] == System.DBNull.Value ? "" : dr["Email"].ToString(),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }
            return lista;
        }

        public Socio ObtenerPorId(int socioId)
        {
            string sql = "SELECT SocioId, DNI, Nombre, Email, Activo FROM Socios WHERE SocioId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", socioId));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new Socio
                        {
                            SocioId = (int)dr["SocioId"],
                            DNI = dr["DNI"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Email = dr["Email"] == System.DBNull.Value ? "" : dr["Email"].ToString(),
                            Activo = (bool)dr["Activo"]
                        };
                    }
                }
            }
            return null;
        }

        public int Insertar(Socio socio)
        {
            string sql = @"INSERT INTO Socios (DNI, Nombre, Email, Activo)
                           VALUES (@dni, @nombre, @email, 1);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@dni", socio.DNI));
                cmd.Parameters.Add(new SqlParameter("@nombre", socio.Nombre));
                cmd.Parameters.Add(new SqlParameter("@email", (object)socio.Email ?? System.DBNull.Value));

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void Actualizar(Socio socio)
        {
            string sql = @"UPDATE Socios
                           SET DNI = @dni, Nombre = @nombre, Email = @email
                           WHERE SocioId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@dni", socio.DNI));
                cmd.Parameters.Add(new SqlParameter("@nombre", socio.Nombre));
                cmd.Parameters.Add(new SqlParameter("@email", (object)socio.Email ?? System.DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@id", socio.SocioId));

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarLogico(int socioId)
        {
            string sql = "UPDATE Socios SET Activo = 0 WHERE SocioId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", socioId));
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteDNI(string dni, int socioIdExcluir)
        {
            string sql = "SELECT COUNT(*) FROM Socios WHERE DNI = @dni AND SocioId <> @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@dni", dni));
                cmd.Parameters.Add(new SqlParameter("@id", socioIdExcluir));
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public bool TienePrestamosPendientes(int socioId)
        {
            string sql = @"SELECT COUNT(*)
                           FROM DetallePrestamo dp
                           INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
                           WHERE p.SocioId = @id AND dp.FechaDevolucion IS NULL";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", socioId));
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}