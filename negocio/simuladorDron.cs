using System;
using System.Collections.Generic;

namespace ParcialDron.Negocio
{
    public class SimuladorDron
    {
        private int N;
        private int[,] tablero;
        private int cantidadAlcanzables;
        public List<(int X, int Y)> SecuenciaMovimientos { get; private set; }

        // Vectores de movimiento en "L" (2 casilleros en un eje y 1 en el otro)
        private readonly int[] movX = { -2, -2, -1, -1, 1, 1, 2, 2 };
        private readonly int[] movY = { -1, 1, -2, 2, -2, 2, -1, 1 };

        public SimuladorDron(int tamanoN)
        {
            N = tamanoN;
            tablero = new int[N, N];
            SecuenciaMovimientos = new List<(int X, int Y)>();

            // Inicializamos la matriz con -1 (casilleros vacíos)
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    tablero[i, j] = -1;
        }

        private bool EsValido(int x, int y)
        {
            return (x >= 0 && x < N && y >= 0 && y < N && tablero[x, y] == -1);
        }

        // PARTE B: Pre-calcular cuántas parcelas son alcanzables con BFS (Grafo)
        public int CalcularAlcanzables(int inicioX, int inicioY)
        {
            HashSet<(int, int)> visitados = new HashSet<(int, int)>();
            Queue<(int, int)> cola = new Queue<(int, int)>();

            cola.Enqueue((inicioX, inicioY));
            visitados.Add((inicioX, inicioY));

            while (cola.Count > 0)
            {
                var (actualX, actualY) = cola.Dequeue();

                for (int i = 0; i < 8; i++)
                {
                    int nx = actualX + movX[i];
                    int ny = actualY + movY[i];

                    if (nx >= 0 && nx < N && ny >= 0 && ny < N)
                    {
                        if (!visitados.Contains((nx, ny)))
                        {
                            visitados.Add((nx, ny));
                            cola.Enqueue((nx, ny));
                        }
                    }
                }
            }
            cantidadAlcanzables = visitados.Count;
            return cantidadAlcanzables;
        }

        // Contar salidas disponibles (Heurística de Warnsdorff / Grado)
        private int ObtenerGrado(int x, int y)
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if (EsValido(x + movX[i], y + movY[i]))
                    count++;
            }
            return count;
        }

        public bool IniciarSimulacion(int inicioX, int inicioY)
        {
            tablero[inicioX, inicioY] = 0;
            SecuenciaMovimientos.Add((inicioX, inicioY));

            return ResolverRecursivo(inicioX, inicioY, 1);
        }

        private bool ResolverRecursivo(int x, int y, int pasoActual)
        {
            if (pasoActual == cantidadAlcanzables)
                return true;

            List<Candidato> candidatos = new List<Candidato>();

            for (int i = 0; i < 8; i++)
            {
                int proximoX = x + movX[i];
                int proximoY = y + movY[i];

                if (EsValido(proximoX, proximoY))
                {
                    int grado = ObtenerGrado(proximoX, proximoY);
                    candidatos.Add(new Candidato(proximoX, proximoY, grado));
                }
            }

            // REQUISITO EXIGIDO: Ordenar de MENOR a MAYOR grado
            candidatos.Sort((a, b) => a.Grado.CompareTo(b.Grado));

            int idx = 0;
            while (idx < candidatos.Count)
            {
                Candidato c = candidatos[idx];
                tablero[c.X, c.Y] = pasoActual;
                SecuenciaMovimientos.Add((c.X, c.Y));

                if (ResolverRecursivo(c.X, c.Y, pasoActual + 1))
                    return true;

                // BACKTRACKING: Si no funcionó, borramos el rastro
                tablero[c.X, c.Y] = -1;
                SecuenciaMovimientos.RemoveAt(SecuenciaMovimientos.Count - 1);
                idx++;
            }

            return false;
        }

        public void MostrarTablero()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (tablero[i, j] == -1)
                        Console.Write(". \t");
                    else
                        Console.Write(tablero[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }

    public class Candidato
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Grado { get; set; }
        public Candidato(int x, int y, int grado) { X = x; Y = y; Grado = grado; }
    }
}