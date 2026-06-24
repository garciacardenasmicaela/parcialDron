using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Npgsql; // Importante para la creación automática de tablas
using ParcialDron.Datos;
using ParcialDron.Negocio;

namespace ParcialDron
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Cargar configuración desde el appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            IConfiguration configuration = builder.Build();
            string cadenaConexion = configuration.GetConnectionString("PostgresConnection");

            // 2. Inicialización automática de la Base de Datos (Crea las tablas si no existen)
            using (var conn = new NpgsqlConnection(cadenaConexion))
            {
                conn.Open();
                string ddl = @"
                    CREATE TABLE IF NOT EXISTS tb_master_control (
                        id SERIAL PRIMARY KEY,
                        fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        tamano_n INT NOT NULL,
                        despegue_x INT NOT NULL,
                        despegue_y INT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS tb_det_log (
                        id SERIAL PRIMARY KEY,
                        master_id INT NOT NULL,
                        paso_etiqueta INT NOT NULL,
                        posicion_x INT NOT NULL,
                        posicion_y INT NOT NULL,
                        CONSTRAINT fk_master FOREIGN KEY (master_id) REFERENCES tb_master_control(id) ON DELETE CASCADE
                    );";

                using (var cmd = new NpgsqlCommand(ddl, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // 3. Instanciar la Capa de Datos
            DronRepository repo = new DronRepository(cadenaConexion);

            // Interfaz de Consola
            Console.WriteLine("=====================================================");
            Console.WriteLine("    SIMULADOR DE TRAYECTORIA DE UN DRON (POR CAPAS)   ");
            Console.WriteLine("=====================================================");

            Console.Write("Ingrese la dimensión del espacio N (N >= 1): ");
            if (!int.TryParse(Console.ReadLine(), out int n) || n < 1) return;

            Console.Write($"Ingrese coordenada inicial X (0 a {n - 1}): ");
            if (!int.TryParse(Console.ReadLine(), out int inicioX)) return;

            Console.Write($"Ingrese coordenada inicial Y (0 a {n - 1}): ");
            if (!int.TryParse(Console.ReadLine(), out int inicioY)) return;

            // 4. Llamada a la Capa de Negocio (Algoritmo del Dron)
            SimuladorDron simulador = new SimuladorDron(n);
            int alcanzables = simulador.CalcularAlcanzables(inicioX, inicioY);
            bool exito = simulador.IniciarSimulacion(inicioX, inicioY);

            // 5. Gestión del Resultado en Consola
            if (!exito)
            {
                Console.WriteLine("\n[RESULTADO] SIN SOLUCIÓN.");
                simulador.MostrarTablero();
                return;
            }

            Console.WriteLine("\n[RESULTADO] ¡SIMULACIÓN EXITOSA!\n");
            simulador.MostrarTablero();

            // 6. Persistencia y Reporte Inverso (Capa de Datos)
            int masterId = repo.GuardarSimulacion(n, inicioX, inicioY, simulador.SecuenciaMovimientos);

            if (masterId > 0)
            {
                Console.WriteLine($"\n✓ Datos guardados. Master ID: {masterId}");
                repo.GenerarReporteInverso(masterId);
            }
        }
    }
}