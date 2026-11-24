using SQLite;

namespace MauiApp1.Models
{
    // Tabla "Presupuesto": el objetivo de dinero que el usuario quiere cuidar
    public class Presupuesto
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        // Dinero total con el que empieza el periodo
        public double MontoTotal { get; set; }

        // Día, Semana, Mes, Año o Personalizado
        public string Periodo { get; set; } = "Mes";

        public DateTime FechaInicio { get; set; }
        public string TipoGasto { get; set; } = "General";
        public DateTime FechaFin { get; set; }

        // Indica cuál presupuesto está seleccionado actualmente
        public bool EsActivo { get; set; }
    }
}