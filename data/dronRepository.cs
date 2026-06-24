using System;
using System.Collections.Generic;
using Npgsql;

namespace ParcialDron.Datos
{
    public class DronRepository
    {
        private readonly string cadenaConexion;

        public DronRepository(string conexion)
        {
            cadenaConexion = conexion;
        }

        public int GuardarSimulacion(int n, int x, int y, List<(int X, int Y)> movimientos)
        {
            using (var conn = new NpgsqlConnection(cadenaConexion))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int idCabecera = 0;

                        string sqlMaster = @"INSERT INTO tb_master_control (tamano_n, despegue_x, despegue_y) 
                                             VALUES (@n, @x, @y) RETURNING id;";
                        
                        using (var cmd = new NpgsqlCommand(sqlMaster, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("n", n);
                            cmd.Parameters.AddWithValue("x", x);
                            cmd.Parameters.AddWithValue("y", y);
                            idCabecera = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string sqlDetalle = @"INSERT INTO tb_det_log (master_id, paso_etiqueta, posicion_x, posicion_y) 
                                              VALUES (@masterId, @etiqueta, @px, @py);";

                        int i = 0;
                        int cantidadMovimientos = movimientos.Count;

                        while (i < cantidadMovimientos)
                        {
                            int etiquetaOfuscada = (i % 2 == 0) ? (i * 2) : (i * -1);

                            using (var cmdDet = new NpgsqlCommand(sqlDetalle, conn, trans))
                            {
                                cmdDet.Parameters.AddWithValue("masterId", idCabecera);
                                cmdDet.Parameters.AddWithValue("etiqueta", etiquetaOfuscada);
                                cmdDet.Parameters.AddWithValue("px", movimientos[i].X);
                                cmdDet.Parameters.AddWithValue("py", movimientos[i].Y);
                                
                                cmdDet.ExecuteNonQuery();
                            }
                            i++;
                        }

                        string sqlContar = "SELECT COUNT(*) FROM tb_master_control;";
                        long totalSimulaciones = 0;
                        
                        using (var cmdCount = new NpgsqlCommand(sqlContar, conn, trans))
                        {
                            totalSimulaciones = Convert.ToInt64(cmdCount.ExecuteScalar());
                        }

                        Console.WriteLine("\n=====================================================");
                        Console.WriteLine("¡Simulación guardada con éxito en PostgreSQL!");
                        Console.WriteLine($"Simulaciones previas ya existentes: {totalSimulaciones - 1}");
                        Console.WriteLine($"Esta simulación es la N°: {totalSimulaciones} en tu historial.");
                        Console.WriteLine("=====================================================");

                        trans.Commit();
                        return idCabecera;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        Console.WriteLine("Error crítico en la capa de datos: " + ex.Message);
                        return 0;
                    }
                }
            }
        }

        public void GenerarReporteInverso(int masterId)
        {
            Console.WriteLine("\n=====================================================");
            Console.WriteLine("   REPORTE INVERSO: ÚLTIMOS 5 PASOS RECONSTRUIDOS    ");
            Console.WriteLine("=====================================================");

            string sqlReporte = @"SELECT paso_etiqueta, posicion_x, posicion_y 
                                  FROM tb_det_log 
                                  WHERE master_id = @masterId 
                                  ORDER BY id DESC LIMIT 5;";

            using (var conn = new NpgsqlConnection(cadenaConexion))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sqlReporte, conn))
                {
                    cmd.Parameters.AddWithValue("masterId", masterId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int etiquetaOfuscada = reader.GetInt32(0);
                            int posX = reader.GetInt32(1);
                            int posY = reader.GetInt32(2);
                            
                            int pasoReal = (etiquetaOfuscada < 0) ? (etiquetaOfuscada * -1) : (etiquetaOfuscada / 2);

                            Console.WriteLine($"Registro en BD Ofuscado: {etiquetaOfuscada, 4}  -->  [PASO REAL RECONSTRUIDO: {pasoReal, 2}] en Coordenadas: ({posX}, {posY})");
                        }
                    }
                }
            }
            Console.WriteLine("=====================================================");
        }
    }
}