using MauiApp1.Data;
using MauiApp1.Helpers;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    // Página para crear un gasto nuevo, editar o eliminar uno que ya existe
    public partial class GastoDetallePage : ContentPage
    {
        readonly int presupuestoId;
        readonly Gasto? gasto; 

        readonly List<string> categorias = new()
        {
            "Comida", "Transporte", "Entretenimiento", "Servicios", "Escuela", "Salud", "Otros"
        };

        public GastoDetallePage(int presupuestoId, Gasto? gasto = null)
        {
            InitializeComponent();
            this.presupuestoId = presupuestoId;
            this.gasto = gasto;

            pickerCategoria.ItemsSource = categorias;

            if (gasto == null)
            {
                Title = "Nuevo gasto";
                pickerCategoria.SelectedIndex = 0;
                dpFecha.Date = DateTime.Today;
            }
            else
            {
                Title = "Editar gasto";
                txtDescripcion.Text = gasto.Descripcion;
                txtMonto.Text = gasto.Monto.ToString();
                pickerCategoria.SelectedIndex = Math.Max(0, categorias.IndexOf(gasto.Categoria));
                dpFecha.Date = gasto.Fecha;
                btnEliminar.IsVisible = true;
            }
        }

        private void OnMontoTextChanged(object? sender, TextChangedEventArgs e)
        {
            bool esValido = double.TryParse(e.NewTextValue, out double monto) && monto > 0;
            lblErrorMonto.IsVisible = !esValido && !string.IsNullOrEmpty(e.NewTextValue);
            btnGuardar.IsEnabled = esValido;
        }

        // CREATE / UPDATE
        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            if (!double.TryParse(txtMonto.Text, out double monto) || monto <= 0)
                return;

            var registro = gasto ?? new Gasto { PresupuestoId = presupuestoId };
            registro.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? "Gasto" : txtDescripcion.Text.Trim();
            registro.Monto = monto;
            registro.Categoria = pickerCategoria.SelectedItem?.ToString() ?? "Otros";
            registro.Fecha = (dpFecha.Date ?? DateTime.Today).Date;

            await BaseDatos.GuardarGastoAsync(registro);

            // Después de guardar, el asesor revisa si con este gasto ya vas mal
            var presupuesto = await BaseDatos.ObtenerPresupuestoAsync();
            if (presupuesto != null)
            {
                var gastos = await BaseDatos.ObtenerGastosAsync(presupuesto.Id);
                var resumen = Asesor.Analizar(presupuesto, gastos);
                if (resumen.NecesitaAviso)
                    await DisplayAlertAsync("Ojo con tu presupuesto", resumen.Mensaje, "Entendido");
            }

            await Navigation.PopAsync(); // regresa a la página anterior (Pop)
        }

        private async void OnEliminarClicked(object? sender, EventArgs e)
        {
            if (gasto == null)
                return;

            bool confirmar = await DisplayAlertAsync("Eliminar", $"¿Eliminar \"{gasto.Descripcion}\"?", "Sí", "No");
            if (!confirmar)
                return;

            await BaseDatos.EliminarGastoAsync(gasto);
            await Navigation.PopAsync();
        }
    }
}
