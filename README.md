# parcialDron
# Examen Parcial: Persistencia y Algoritmia Avanzada en .NET
## Simulador de Trayectoria de un Dron Automatizado (Arquitectura por Capas)

Este proyecto implementa el sistema de navegación de un dron de inspección utilizando una arquitectura por capas en **.NET 8**, persistiendo los datos de las simulaciones de forma síncrona en **PostgreSQL** mediante **ADO.NET (Npgsql)**.

---

### Requisitos

Para poder ejecutar este proyecto, asegúrese de contar con las siguientes herramientas instaladas y configuradas en su entorno:

1. **.NET SDK 8.0** (o superior).
2. **Docker Desktop** con el contenedor de PostgreSQL oficial de la cátedra levantado y corriendo.
3. El contenedor debe estar escuchando en el puerto estándar `5432` y contar con las siguientes credenciales por defecto:
   - **Host:** `localhost`
   - **Puerto:** `5432`
   - **Base de datos:** `postgres`
   - **Usuario:** `postgres`
   - **Contraseña:** `postgres`

---

### Inicialización Automática de la Base de Datos

**no es necesario ejecutar scripts DDL previos en DBeaver.** El programa incluye una rutina de inicialización automática al comenzar la ejecución. Mediante sentencias `CREATE TABLE IF NOT EXISTS`, el propio código de C# se encarga de verificar y fundar las tablas `tb_master_control` y `tb_det_log` con sus respectivas restricciones y claves foráneas en su instancia local de Docker.

---

### Instrucciones para Ejecución

pasos para clonar, compilar y ejecutar el simulador:

1. **Clonar el repositorio:**
   ```bash
   git clone <URL_DE_TU_REPOSITORIO>
   cd ParcialDron
