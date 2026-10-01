using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Biblioteca.Datos.Conexion;
using Biblioteca.Entidades.Modelos;

namespace Biblioteca.Datos.Repositorios
{
    public class DetallePrestamoDatos
    {
        public List<DetallePrestamo> ListarPorPrestamo(int prestamoId)
        {
            var lista = new List<DetallePrestamo>();
            string sql = @"SELECT PrestamoId, LibroId, FechaDevolucion
                           FROM DetallePrestamo
                           WHERE PrestamoId = @prestamoId";

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
                            FechaDevolucion = dr["FechaDevolucion"] == DBNull.Value
                                                ? (DateTime?)null
                                                : (DateTime)dr["FechaDevolucion"]
                        });
                    }
                }
            }
            return lista;
        }
    }
}