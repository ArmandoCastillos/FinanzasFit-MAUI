using SQLite;
using MauiApp1.Models;

namespace MauiApp1.Data
{
    public static class BaseDatos
    {
        static SQLiteAsyncConnection? conexion;

        // Abre la conexión y crea las tablas solo la primera vez
        static async Task Init()
        {
            if (conexion != null)
                return;

            // AppDataDirectory es una carpeta privada de la app en cada plataforma
            string ruta = Path.Combine(FileSystem.AppDataDirectory, "finanzas.db3");
            conexion = new SQLiteAsyncConnection(ruta);

            await conexion.CreateTableAsync<Presupuesto>();
            await conexion.CreateTableAsync<Gasto>();
            await conexion.CreateTableAsync<Usuario>();
        }

        // - USUARIOS -

        public static async Task RegistrarUsuarioAsync(Usuario usuario)
        {
            await Init();
            await conexion!.InsertAsync(usuario);
        }

        public static async Task<Usuario?> IniciarSesionAsync(string nombre, string contrasena)
        {
            await Init();
            return await conexion!.Table<Usuario>()
                .FirstOrDefaultAsync(u => u.Nombre == nombre && u.Contrasena == contrasena);
        }

        public static async Task<Usuario?> ObtenerUsuarioPorNombreAsync(string nombre)
        {
            await Init();
            return await conexion!.Table<Usuario>()
                .FirstOrDefaultAsync(u => u.Nombre == nombre);
        }

        // - PRESUPUESTO -

        // Devuelve todos los presupuestos / cuentas de ahorro creados
        public static async Task<List<Presupuesto>> ObtenerTodosPresupuestosAsync()
        {
            await Init();
            return await conexion!.Table<Presupuesto>().ToListAsync();
        }

        // READ - devuelve el presupuesto marcado como activo (o el primero si ninguno lo está)
        public static async Task<Presupuesto?> ObtenerPresupuestoAsync()
        {
            await Init();
            var activo = await conexion!.Table<Presupuesto>()
                .FirstOrDefaultAsync(p => p.EsActivo);

            if (activo == null)
            {
                activo = await conexion!.Table<Presupuesto>().FirstOrDefaultAsync();
                if (activo != null)
                {
                    activo.EsActivo = true;
                    await conexion!.UpdateAsync(activo);
                }
            }
            return activo;
        }

        // Marca un presupuesto específico como el activo en toda la app
        public static async Task SeleccionarPresupuestoActivoAsync(int presupuestoId)
        {
            await Init();
            var todos = await conexion!.Table<Presupuesto>().ToListAsync();
            foreach (var p in todos)
            {
                p.EsActivo = (p.Id == presupuestoId);
                await conexion!.UpdateAsync(p);
            }
        }

        // Abona (suma) dinero a un presupuesto existente tipo cuenta de ahorro
        public static async Task AbonarPresupuestoAsync(int presupuestoId, double montoExtra)
        {
            await Init();
            var presupuesto = await conexion!.Table<Presupuesto>()
                .FirstOrDefaultAsync(p => p.Id == presupuestoId);

            if (presupuesto != null)
            {
                presupuesto.MontoTotal += montoExtra;
                await conexion!.UpdateAsync(presupuesto);
            }
        }

        // CREATE / UPDATE - si Id es 0 es nuevo (y pasa a ser el activo), si no, se actualiza
        public static async Task GuardarPresupuestoAsync(Presupuesto presupuesto)
        {
            await Init();
            if (presupuesto.Id == 0)
            {
                var todos = await conexion!.Table<Presupuesto>().ToListAsync();
                foreach (var p in todos)
                {
                    p.EsActivo = false;
                    await conexion!.UpdateAsync(p);
                }
                presupuesto.EsActivo = true;
                await conexion!.InsertAsync(presupuesto);
            }
            else
            {
                await conexion!.UpdateAsync(presupuesto);
            }
        }

        // DELETE - borra el presupuesto y todos sus gastos
        public static async Task EliminarPresupuestoAsync(Presupuesto presupuesto)
        {
            await Init();
            await conexion!.ExecuteAsync("DELETE FROM Gasto WHERE PresupuestoId = ?", presupuesto.Id);
            await conexion!.DeleteAsync(presupuesto);
        }

        // - GASTOS -

        // READ - gastos de un presupuesto, del más reciente al más viejo
        public static async Task<List<Gasto>> ObtenerGastosAsync(int presupuestoId)
        {
            await Init();
            return await conexion!.Table<Gasto>()
                .Where(g => g.PresupuestoId == presupuestoId)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();
        }

        // CREATE / UPDATE - Asesor calcula el restante restando los gastos al MontoTotal)
        public static async Task GuardarGastoAsync(Gasto gasto)
        {
            await Init();
            if (gasto.Id == 0)
                await conexion!.InsertAsync(gasto);
            else
                await conexion!.UpdateAsync(gasto);
        }

      
        public static async Task EliminarGastoAsync(Gasto gasto)
        {
            await Init();
            await conexion!.DeleteAsync(gasto);
        }
    }
}