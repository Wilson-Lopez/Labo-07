using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Biblioteca.Datos.Conexion;
using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Repositorios
{
    public class PrestamoDatos
    {
        /// <summary>
        /// Registra un préstamo con sus detalles en UNA SOLA TRANSACCIÓN.
        /// Retorna el PrestamoId generado.
        /// Requisito 10 del laboratorio.
        /// </summary>
        public int RegistrarPrestamo(Prestamo prestamo)
        {
            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar cabecera
                        string sqlCab = @"INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
                                          VALUES (@socioId, @fechaPrestamo, @fechaLimite, 'Pendiente');
                                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int prestamoId;
                        using (var cmd = new SqlCommand(sqlCab, cn, tx))
                        {
                            cmd.Parameters.Add(new SqlParameter("@socioId", prestamo.SocioId));
                            cmd.Parameters.Add(new SqlParameter("@fechaPrestamo", prestamo.FechaPrestamo));
                            cmd.Parameters.Add(new SqlParameter("@fechaLimite", prestamo.FechaLimite));
                            prestamoId = (int)cmd.ExecuteScalar();
                        }

                        // 2. Insertar detalles y descontar stock
                        foreach (var det in prestamo.Detalles)
                        {
                            string sqlDet = @"INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
                                              VALUES (@prestamoId, @libroId, NULL);";
                            using (var cmd = new SqlCommand(sqlDet, cn, tx))
                            {
                                cmd.Parameters.Add(new SqlParameter("@prestamoId", prestamoId));
                                cmd.Parameters.Add(new SqlParameter("@libroId", det.LibroId));
                                cmd.ExecuteNonQuery();
                            }

                            string sqlStock = "UPDATE Libros SET Ejemplares = Ejemplares - 1 WHERE LibroId = @libroId";
                            using (var cmd = new SqlCommand(sqlStock, cn, tx))
                            {
                                cmd.Parameters.Add(new SqlParameter("@libroId", det.LibroId));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return prestamoId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Registra la devolución de UN libro en un préstamo:
        /// - Actualiza FechaDevolucion
        /// - Devuelve el ejemplar al stock
        /// - Si ya no quedan pendientes en el préstamo, marca la cabecera como 'Devuelto'
        /// </summary>
        public void RegistrarDevolucion(int prestamoId, int libroId, DateTime fechaDevolucion)
        {
            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar fecha de devolución del detalle
                        string sqlDet = @"UPDATE DetallePrestamo
                                          SET FechaDevolucion = @fecha
                                          WHERE PrestamoId = @prestamoId AND LibroId = @libroId
                                            AND FechaDevolucion IS NULL";
                        using (var cmd = new SqlCommand(sqlDet, cn, tx))
                        {
                            cmd.Parameters.Add(new SqlParameter("@fecha", fechaDevolucion));
                            cmd.Parameters.Add(new SqlParameter("@prestamoId", prestamoId));
                            cmd.Parameters.Add(new SqlParameter("@libroId", libroId));
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Devolver ejemplar al stock
                        string sqlStock = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @libroId";
                        using (var cmd = new SqlCommand(sqlStock, cn, tx))
                        {
                            cmd.Parameters.Add(new SqlParameter("@libroId", libroId));
                            cmd.ExecuteNonQuery();
                        }

                        // 3. ¿Quedan pendientes?
                        string sqlPend = @"SELECT COUNT(*) FROM DetallePrestamo
                                           WHERE PrestamoId = @prestamoId AND FechaDevolucion IS NULL";
                        int pendientes;
                        using (var cmd = new SqlCommand(sqlPend, cn, tx))
                        {
                            cmd.Parameters.Add(new SqlParameter("@prestamoId", prestamoId));
                            pendientes = (int)cmd.ExecuteScalar();
                        }

                        if (pendientes == 0)
                        {
                            string sqlCab = "UPDATE Prestamos SET Estado = 'Devuelto' WHERE PrestamoId = @prestamoId";
                            using (var cmd = new SqlCommand(sqlCab, cn, tx))
                            {
                                cmd.Parameters.Add(new SqlParameter("@prestamoId", prestamoId));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Cuenta libros pendientes de un socio (para la regla del máximo de 3).
        /// </summary>
        public int ContarPendientesPorSocio(int socioId)
        {
            string sql = @"SELECT COUNT(*)
                           FROM DetallePrestamo dp
                           INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
                           WHERE p.SocioId = @socioId AND dp.FechaDevolucion IS NULL";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@socioId", socioId));
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Reporte por intervalo de fechas con INNER JOIN.
        /// Requisito 14 del laboratorio.
        /// </summary>
        public List<DetallePrestamo> ReportePorFechas(DateTime desde, DateTime hasta)
        {
            var lista = new List<DetallePrestamo>();
            string sql = @"SELECT p.PrestamoId, s.Nombre AS NombreSocio,
                                  l.Titulo AS TituloLibro, a.Nombre AS NombreAutor,
                                  p.FechaLimite, p.Estado, dp.LibroId, dp.FechaDevolucion
                           FROM Prestamos p
                           INNER JOIN Socios s          ON s.SocioId  = p.SocioId
                           INNER JOIN DetallePrestamo dp ON dp.PrestamoId = p.PrestamoId
                           INNER JOIN Libros l           ON l.LibroId   = dp.LibroId
                           INNER JOIN Autores a          ON a.AutorId   = l.AutorId
                           WHERE p.FechaPrestamo BETWEEN @desde AND @hasta
                           ORDER BY p.PrestamoId, l.Titulo";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@desde", desde));
                cmd.Parameters.Add(new SqlParameter("@hasta", hasta));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new DetallePrestamo
                        {
                            PrestamoId = (int)dr["PrestamoId"],
                            LibroId = (int)dr["LibroId"],
                            TituloLibro = dr["TituloLibro"].ToString(),
                            NombreAutor = dr["NombreAutor"].ToString(),
                            FechaLimite = (DateTime)dr["FechaLimite"],
                            Estado = dr["Estado"].ToString(),
                            FechaDevolucion = dr["FechaDevolucion"] == DBNull.Value
                                                ? (DateTime?)null
                                                : (DateTime)dr["FechaDevolucion"],
                            NombreSocio = dr["NombreSocio"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Obtiene los detalles (libros) pendientes de un préstamo.
        /// </summary>
        public List<DetallePrestamo> DetallesPendientes(int prestamoId)
        {
            var lista = new List<DetallePrestamo>();
            string sql = @"SELECT dp.PrestamoId, dp.LibroId, l.Titulo AS TituloLibro,
                                  p.FechaLimite
                           FROM DetallePrestamo dp
                           INNER JOIN Libros l    ON l.LibroId = dp.LibroId
                           INNER JOIN Prestamos p ON p.PrestamoId = dp.PrestamoId
                           WHERE dp.PrestamoId = @prestamoId AND dp.FechaDevolucion IS NULL";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@prestamoId", prestamoId));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new DetallePrestamo
                        {
                            PrestamoId = (int)dr["PrestamoId"],
                            LibroId = (int)dr["LibroId"],
                            TituloLibro = dr["TituloLibro"].ToString(),
                            FechaLimite = (DateTime)dr["FechaLimite"]
                        });
                    }
                }
            }
            return lista;
        }

        /// <summary>
        /// Lista todos los préstamos de un socio con su estado.
        /// </summary>
        public List<Prestamo> ListarPorSocio(int socioId)
        {
            var lista = new List<Prestamo>();
            string sql = @"SELECT PrestamoId, SocioId, FechaPrestamo, FechaLimite, Estado
                           FROM Prestamos
                           WHERE SocioId = @socioId
                           ORDER BY PrestamoId DESC";

            using (var cn = new SqlConnection(ConexionBD.CadenaConexion))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@socioId", socioId));
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Prestamo
                        {
                            PrestamoId = (int)dr["PrestamoId"],
                            SocioId = (int)dr["SocioId"],
                            FechaPrestamo = (DateTime)dr["FechaPrestamo"],
                            FechaLimite = (DateTime)dr["FechaLimite"],
                            Estado = dr["Estado"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}