using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Biblioteca.Datos.Conexion;
using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Repositorios
{
    public class LibroDatos
    {
        // LISTAR (con nombre del autor, requisito 12)
        public List<Libro> Listar()
        {
            var lista = new List<Libro>();
            string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
                                  a.Nombre AS NombreAutor
                           FROM Libros l
                           INNER JOIN Autores a ON a.AutorId = l.AutorId
                           WHERE l.Activo = 1";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Libro
                        {
                            LibroId = (int)dr["LibroId"],
                            Titulo = dr["Titulo"].ToString(),
                            ISBN = dr["ISBN"].ToString(),
                            AutorId = (int)dr["AutorId"],
                            Ejemplares = (int)dr["Ejemplares"],
                            Activo = (bool)dr["Activo"],
                            NombreAutor = dr["NombreAutor"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        // BUSCAR por título o autor
        public List<Libro> Buscar(string filtro)
        {
            var lista = new List<Libro>();
            string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
                                  a.Nombre AS NombreAutor
                           FROM Libros l
                           INNER JOIN Autores a ON a.AutorId = l.AutorId
                           WHERE l.Activo = 1
                             AND (l.Titulo LIKE @filtro OR a.Nombre LIKE @filtro)";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@filtro", "%" + filtro + "%"));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Libro
                        {
                            LibroId = (int)dr["LibroId"],
                            Titulo = dr["Titulo"].ToString(),
                            ISBN = dr["ISBN"].ToString(),
                            AutorId = (int)dr["AutorId"],
                            Ejemplares = (int)dr["Ejemplares"],
                            Activo = (bool)dr["Activo"],
                            NombreAutor = dr["NombreAutor"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        // OBTENER por Id
        public Libro ObtenerPorId(int libroId)
        {
            string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo,
                                  a.Nombre AS NombreAutor
                           FROM Libros l
                           INNER JOIN Autores a ON a.AutorId = l.AutorId
                           WHERE l.LibroId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", libroId));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new Libro
                        {
                            LibroId = (int)dr["LibroId"],
                            Titulo = dr["Titulo"].ToString(),
                            ISBN = dr["ISBN"].ToString(),
                            AutorId = (int)dr["AutorId"],
                            Ejemplares = (int)dr["Ejemplares"],
                            Activo = (bool)dr["Activo"],
                            NombreAutor = dr["NombreAutor"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        // INSERTAR
        public int Insertar(Libro libro)
        {
            string sql = @"INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
                           VALUES (@titulo, @isbn, @autorId, @ejemplares, 1);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@titulo", libro.Titulo));
                cmd.Parameters.Add(new SqlParameter("@isbn", libro.ISBN));
                cmd.Parameters.Add(new SqlParameter("@autorId", libro.AutorId));
                cmd.Parameters.Add(new SqlParameter("@ejemplares", libro.Ejemplares));

                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // ACTUALIZAR
        public void Actualizar(Libro libro)
        {
            string sql = @"UPDATE Libros
                           SET Titulo = @titulo, ISBN = @isbn, AutorId = @autorId, Ejemplares = @ejemplares
                           WHERE LibroId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@titulo", libro.Titulo));
                cmd.Parameters.Add(new SqlParameter("@isbn", libro.ISBN));
                cmd.Parameters.Add(new SqlParameter("@autorId", libro.AutorId));
                cmd.Parameters.Add(new SqlParameter("@ejemplares", libro.Ejemplares));
                cmd.Parameters.Add(new SqlParameter("@id", libro.LibroId));

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // BAJA LÓGICA
        public void EliminarLogico(int libroId)
        {
            string sql = "UPDATE Libros SET Activo = 0 WHERE LibroId = @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", libroId));
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ¿Existe ISBN en otro libro?
        public bool ExisteISBN(string isbn, int libroIdExcluir)
        {
            string sql = "SELECT COUNT(*) FROM Libros WHERE ISBN = @isbn AND LibroId <> @id";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@isbn", isbn));
                cmd.Parameters.Add(new SqlParameter("@id", libroIdExcluir));
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // ¿Tiene préstamos pendientes?
        public bool TienePrestamosPendientes(int libroId)
        {
            string sql = @"SELECT COUNT(*)
                           FROM DetallePrestamo dp
                           INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
                           WHERE dp.LibroId = @id AND dp.FechaDevolucion IS NULL";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@id", libroId));
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}