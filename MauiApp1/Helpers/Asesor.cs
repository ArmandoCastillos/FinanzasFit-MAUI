using MauiApp1.Models;

namespace MauiApp1.Helpers
{
    // Resultado del análisis que se muestra en la pantalla de Inicio
    public class Resumen
    {
        public double Gastado { get; set; }
        public double Restante { get; set; }
        public double PorcentajeGastado { get; set; }   
        public double PorcentajeTiempo { get; set; }    
        public int DiasTotales { get; set; }
        public int DiaActual { get; set; }
        public int DiasRestantes { get; set; }
        public double GastoDiarioRecomendado { get; set; }
        public string Mensaje { get; set; } = "";
        public Color Color { get; set; } = Colors.Gray;
        public bool NecesitaAviso { get; set; }
    }

    // El "entrenador" de la app: compara el % de dinero gastado contra el % de tiempo transcurrido.
    // Ejemplo: si ya pasó el 50% del periodo, lo sano es haber gastado como máximo el 50% del dinero.
    public static class Asesor
    {
        static readonly Color Verde = Color.FromArgb("#2E7D32");
        static readonly Color Naranja = Color.FromArgb("#EF6C00");
        static readonly Color Rojo = Color.FromArgb("#C62828");
        static readonly Color Gris = Color.FromArgb("#616161");

        public static Resumen Analizar(Presupuesto p, List<Gasto> gastos)
        {
            var r = new Resumen();

            // --- Dinero ---
            r.Gastado = gastos.Sum(g => g.Monto);
            r.Restante = p.MontoTotal - r.Gastado;
            r.PorcentajeGastado = p.MontoTotal > 0 ? r.Gastado / p.MontoTotal : 0;

            // --- Tiempo ---
            DateTime hoy = DateTime.Today;
            r.DiasTotales = Math.Max(1, (p.FechaFin.Date - p.FechaInicio.Date).Days + 1);
            r.DiaActual = Math.Clamp((hoy - p.FechaInicio.Date).Days + 1, 1, r.DiasTotales);
            r.DiasRestantes = r.DiasTotales - r.DiaActual + 1; // incluye el día de hoy
            r.PorcentajeTiempo = (double)r.DiaActual / r.DiasTotales;

            // Cuánto se puede gastar por día para llegar al final del periodo
            r.GastoDiarioRecomendado = r.Restante > 0 ? r.Restante / r.DiasRestantes : 0;

            // Categoría donde más se ha gastado (para dar un consejo más concreto)
            string consejoCategoria = "";
            if (gastos.Count > 0)
            {
                var mayor = gastos
                    .GroupBy(g => g.Categoria)
                    .Select(grupo => new { Categoria = grupo.Key, Total = grupo.Sum(g => g.Monto) })
                    .OrderByDescending(x => x.Total)
                    .First();
                consejoCategoria = $"\nTu mayor gasto es en {mayor.Categoria} (${mayor.Total:N2}).";
            }

            // --- Reglas de recomendación ---
            if (hoy > p.FechaFin.Date)
            {
                r.Color = Gris;
                r.Mensaje = r.Restante >= 0
                    ? $"El periodo terminó. ¡Lo lograste! Te sobraron ${r.Restante:N2}. Considera ahorrarlos."
                    : $"El periodo terminó y te pasaste por ${-r.Restante:N2}. Crea un nuevo presupuesto más realista.";
            }
            else if (r.Restante <= 0)
            {
                r.Color = Rojo;
                r.NecesitaAviso = true;
                r.Mensaje = $"Ya te gastaste todo el presupuesto y faltan {r.DiasRestantes} días. " +
                            "Evita cualquier gasto que no sea indispensable." + consejoCategoria;
            }
            else if (r.PorcentajeGastado > r.PorcentajeTiempo + 0.15)
            {
                r.Color = Rojo;
                r.NecesitaAviso = true;
                r.Mensaje = $"¡Cuidado! Llevas {r.PorcentajeGastado:P0} del dinero gastado y solo ha pasado " +
                            $"{r.PorcentajeTiempo:P0} del tiempo. Para llegar al final gasta máximo " +
                            $"${r.GastoDiarioRecomendado:N2} por día." + consejoCategoria;
            }
            else if (r.PorcentajeGastado > r.PorcentajeTiempo)
            {
                r.Color = Naranja;
                r.NecesitaAviso = true;
                r.Mensaje = $"Vas un poco por encima del ritmo ideal. Intenta no pasar de " +
                            $"${r.GastoDiarioRecomendado:N2} por día." + consejoCategoria;
            }
            else
            {
                r.Color = Verde;
                r.Mensaje = $"¡Vas bien! Puedes gastar hasta ${r.GastoDiarioRecomendado:N2} por día " +
                            $"durante los {r.DiasRestantes} días que quedan.";
            }

            return r;
        }
    }
}
