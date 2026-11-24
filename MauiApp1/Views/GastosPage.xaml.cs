using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class GastosPage : ContentPage
    {
        Presupuesto? presupuesto;
        List<Gasto> todosLosGastos = new();

        public GastosPage()
        {
            InitializeComponent();
        }

        // READ - recarga la lista cada vez que se entra a la pestaña
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            presupuesto = await BaseDatos.ObtenerPresupuestoAsync();

            if (presupuesto == null)
            {
                lblTotal.Text = "Primero crea un presupuesto";
                btnAgregar.IsEnabled = false;
                todosLosGastos = new();
                listaGastos.ItemsSource = todosLosGastos;
                return;
            }

            btnAgregar.IsEnabled = true;
            todosLosGastos = await BaseDatos.ObtenerGastosAsync(presupuesto.Id);
            lblTotal.Text = $"Total gastado: ${todosLosGastos.Sum(g => g.Monto):N2}";
            Filtrar(buscador.Text);
        }

        // filtra la lista en tiempo real
        private void OnBuscarTextChanged(object? sender, TextChangedEventArgs e)
        {
            Filtrar(e.NewTextValue);
        }

        void Filtrar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                listaGastos.ItemsSource = todosLosGastos;
                return;
            }

            listaGastos.ItemsSource = todosLosGastos
                .Where(g => g.Descripcion.Contains(texto, StringComparison.OrdinalIgnoreCase)
                         || g.Categoria.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // abre el detalle para editar o eliminar
        private async void OnGastoSeleccionado(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Gasto gasto)
                return;

            listaGastos.SelectedItem = null; // permite volver a seleccionar el mismo
            await Navigation.PushAsync(new GastoDetallePage(gasto.PresupuestoId, gasto));
        }

        private async void OnAgregarClicked(object? sender, EventArgs e)
        {
            if (presupuesto != null)
                await Navigation.PushAsync(new GastoDetallePage(presupuesto.Id));
        }
    }
}
