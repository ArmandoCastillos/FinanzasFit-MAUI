using SQLite;

namespace MauiApp1.Models
{
    // Tabla "Gasto": cada salida de dinero ligada a un presupuesto
    public class Gasto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Llave foránea hacia Presupuesto.Id
        [Indexed]
        public int PresupuestoId { get; set; }

        public string Descripcion { get; set; } = "";

        public double Monto { get; set; }

        public string Categoria { get; set; } = "Otros";

        public DateTime Fecha { get; set; }
    }
}
