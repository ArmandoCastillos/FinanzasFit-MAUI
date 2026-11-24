using MauiApp1.Data;
using MauiApp1.Helpers;
using MauiApp1.Models;

namespace MauiApp1.Views
{
    public partial class InicioPage : ContentPage
    {
        Presupuesto? presupuesto;
        Resumen? resumen;

        public InicioPage()
        {
            InitializeComponent();
        }

        // Evento del ciclo de vida: se ejecuta cada vez que la página se vuelve visible
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatosAsync();
        }

        async Task CargarDatosAsync()
        {
            presupuesto = await BaseDatos.ObtenerPresupuestoAsync();

            if (presupuesto == null)
            {
                panelVacio.IsVisible = true;
                panelResumen.IsVisible = false;
                return;
            }

            panelVacio.IsVisible = false;
            panelResumen.IsVisible = true;

            var gastos = await BaseDatos.ObtenerGastosAsync(presupuesto.Id);
            resumen = Asesor.Analizar(presupuesto, gastos);

            lblNombre.Text = presupuesto.Nombre;
            lblRestante.Text = $"${resumen.Restante:N2}";
            lblRestante.TextColor = resumen.Color;
            lblGastado.Text = $"Gastado ${resumen.Gastado:N2} de ${presupuesto.MontoTotal:N2}";

            barDinero.Progress = Math.Min(resumen.PorcentajeGastado, 1);
            barDinero.ProgressColor = resumen.Color;

            lblTiempo.Text = $"Día {resumen.DiaActual} de {resumen.DiasTotales} ({presupuesto.Periodo}) " +
                             $"· {presupuesto.FechaInicio:dd/MM/yyyy} al {presupuesto.FechaFin:dd/MM/yyyy}";
            barTiempo.Progress = resumen.PorcentajeTiempo;

            cuadroConsejo.BackgroundColor = resumen.Color;
            lblConsejo.Text = resumen.Mensaje;

            await AvisarSiEsNecesarioAsync();
        }

        // Recordatorio: si vas mal, la app te avisa una vez al día al abrirla
        async Task AvisarSiEsNecesarioAsync()
        {
            if (resumen == null || !resumen.NecesitaAviso)
                return;

            string hoy = DateTime.Today.ToString("yyyy-MM-dd");
            string ultimoAviso = Preferences.Default.Get("ultimoAviso", "");

            if (ultimoAviso != hoy)
            {
                Preferences.Default.Set("ultimoAviso", hoy);
                await DisplayAlertAsync("Recordatorio de tu presupuesto", resumen.Mensaje, "Entendido");
            }
        }

        private async void OnCrearPresupuestoClicked(object? sender, EventArgs e)
        {
            // Navega a la pestaña de Presupuesto usando su Route
            await Shell.Current.GoToAsync("//PresupuestoPage");
        }

        private async void OnRegistrarGastoClicked(object? sender, EventArgs e)
        {
            if (presupuesto == null)
                return;

            // Navegación jerárquica (Push)
            await Navigation.PushAsync(new GastoDetallePage(presupuesto.Id));
        }
        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlertAsync("Cerrar sesión","¿Estás seguro de que deseas salir de tu cuenta?", "Sí, salir","Cancelar");

            if (!confirmar)
                return;

            // Regresa a la pantalla de Login reiniciando la pila de navegación
            if (this.Window != null)
            {
                this.Window.Page = new LoginPage();
            }
        }

        private async void OnVerRecomendacionClicked(object? sender, EventArgs e)
        {
            if (resumen != null)
                await DisplayAlertAsync("Recomendación", resumen.Mensaje, "OK");
        }
    }
}
