using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Views;

public partial class PresupuestoPage : ContentPage
{
    private List<Presupuesto> _listaPresupuestos = new();
    private Presupuesto? _presupuestoSeleccionado;
    private bool _cargandoPicker = false;

    public PresupuestoPage()
    {
        InitializeComponent();
        PickerPeriodo.SelectedIndex = 2;   // "Mes" por defecto
        PickerTipoGasto.SelectedIndex = 0; // "General" por defecto
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPresupuestosAsync();
    }

    private async Task CargarPresupuestosAsync()
    {
        _cargandoPicker = true;

        _listaPresupuestos = await BaseDatos.ObtenerTodosPresupuestosAsync();

        // Cargamos los textos directamente para que el Picker siempre responda al cambio
        PickerPresupuestos.ItemsSource = _listaPresupuestos
            .Select(p => $"{p.Nombre} ({p.TipoGasto})")
            .ToList();

        var activo = await BaseDatos.ObtenerPresupuestoAsync();
        if (activo != null && _listaPresupuestos.Count > 0)
        {
            int indice = _listaPresupuestos.FindIndex(p => p.Id == activo.Id);
            if (indice >= 0)
            {
                _presupuestoSeleccionado = _listaPresupuestos[indice];
                PickerPresupuestos.SelectedIndex = indice;
            }
        }
        else
        {
            _presupuestoSeleccionado = null;
            PickerPresupuestos.SelectedIndex = -1;
        }

        await ActualizarUIAsync();
        _cargandoPicker = false;
    }

    private async Task ActualizarUIAsync()
    {
        if (_presupuestoSeleccionado != null)
        {
            var gastos = await BaseDatos.ObtenerGastosAsync(_presupuestoSeleccionado.Id);
            double gastado = gastos.Sum(g => g.Monto);
            double disponible = _presupuestoSeleccionado.MontoTotal - gastado;

            LblSaldoActual.Text = $"Disponible: {disponible:C2} (de {_presupuestoSeleccionado.MontoTotal:C2})";
            LblPeriodoActual.Text = $"Tipo: {_presupuestoSeleccionado.TipoGasto} • Periodo: {_presupuestoSeleccionado.Periodo} ({_presupuestoSeleccionado.FechaInicio:dd/MM/yyyy} - {_presupuestoSeleccionado.FechaFin:dd/MM/yyyy})";
            LblPeriodoActual.IsVisible = true;
            FrameAbonar.IsVisible = true;
            BtnEliminarActual.IsVisible = true;
        }
        else
        {
            LblSaldoActual.Text = "Aún no tienes presupuestos creados";
            LblPeriodoActual.IsVisible = false;
            FrameAbonar.IsVisible = false;
            BtnEliminarActual.IsVisible = false;
        }
    }

    private async void OnPresupuestoSeleccionado(object sender, EventArgs e)
    {
        if (_cargandoPicker)
            return;

        int index = PickerPresupuestos.SelectedIndex;
        if (index >= 0 && index < _listaPresupuestos.Count)
        {
            var elegido = _listaPresupuestos[index];
            _presupuestoSeleccionado = elegido;
            await BaseDatos.SeleccionarPresupuestoActivoAsync(elegido.Id);
            await ActualizarUIAsync();
        }
    }

    private async void OnAbonarClicked(object sender, EventArgs e)
    {
        if (_presupuestoSeleccionado == null)
            return;

        if (!double.TryParse(EntryMontoAbono.Text, out double montoExtra) || montoExtra <= 0)
        {
            await DisplayAlertAsync("Monto inválido", "Ingresa una cantidad mayor a 0 para abonar.", "OK");
            return;
        }

        await BaseDatos.AbonarPresupuestoAsync(_presupuestoSeleccionado.Id, montoExtra);
        EntryMontoAbono.Text = string.Empty;

        await DisplayAlertAsync("¡Abono exitoso!", $"Se agregaron {montoExtra:C2} a '{_presupuestoSeleccionado.Nombre}'.", "OK");
        await CargarPresupuestosAsync();
    }

    private async void OnCrearNuevoClicked(object sender, EventArgs e)
    {
        string nombre = EntryNuevoNombre.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlertAsync("Falta nombre", "Escribe un nombre para identificar este presupuesto.", "OK");
            return;
        }

        if (!double.TryParse(EntryNuevoMonto.Text, out double montoInicial) || montoInicial < 0)
        {
            await DisplayAlertAsync("Monto inválido", "Ingresa un monto inicial válido (0 o mayor).", "OK");
            return;
        }

        string periodo = PickerPeriodo.SelectedItem?.ToString() ?? "Mes";
        string tipoGasto = PickerTipoGasto.SelectedItem?.ToString() ?? "General";

        DateTime inicio = DateTime.Now;
        DateTime fin = periodo switch
        {
            "Día" => inicio.AddDays(1),
            "Semana" => inicio.AddDays(7),
            "Año" => inicio.AddYears(1),
            _ => inicio.AddMonths(1)
        };

        var nuevo = new Presupuesto
        {
            Nombre = nombre,
            MontoTotal = montoInicial,
            Periodo = periodo,
            TipoGasto = tipoGasto,
            FechaInicio = inicio,
            FechaFin = fin
        };

        await BaseDatos.GuardarPresupuestoAsync(nuevo);

        EntryNuevoNombre.Text = string.Empty;
        EntryNuevoMonto.Text = string.Empty;
        PickerPeriodo.SelectedIndex = 2;
        PickerTipoGasto.SelectedIndex = 0;

        await DisplayAlertAsync("Creado", $"El presupuesto '{nombre}' ({tipoGasto} - {periodo}) ahora es tu cuenta activa.", "OK");
        await CargarPresupuestosAsync();
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (_presupuestoSeleccionado == null)
            return;

        bool confirmar = await DisplayAlertAsync("Eliminar presupuesto", $"¿Seguro que quieres borrar '{_presupuestoSeleccionado.Nombre}' y todos sus gastos asociados?", "Sí, eliminar","Cancelar");

        if (!confirmar)
            return;

        await BaseDatos.EliminarPresupuestoAsync(_presupuestoSeleccionado);
        await CargarPresupuestosAsync();
    }
}