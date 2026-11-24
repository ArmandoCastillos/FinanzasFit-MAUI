using MauiApp1.Data;
using MauiApp1.Models;

namespace MauiApp1.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        ActualizarEstiloTabs(esLogin: true);
    }

    private void OnTabLoginClicked(object sender, EventArgs e)
    {
        PanelLogin.IsVisible = true;
        PanelRegistro.IsVisible = false;
        ActualizarEstiloTabs(esLogin: true);
    }

    private void OnTabRegistroClicked(object sender, EventArgs e)
    {
        PanelLogin.IsVisible = false;
        PanelRegistro.IsVisible = true;
        ActualizarEstiloTabs(esLogin: false);
    }

    private void ActualizarEstiloTabs(bool esLogin)
    {
        var colorActivo = Application.Current?.Resources["Primary"] as Color ?? Colors.DodgerBlue;
        var colorInactivo = Colors.LightGray;

        BtnTabLogin.BackgroundColor = esLogin ? colorActivo : colorInactivo;
        BtnTabLogin.TextColor = esLogin ? Colors.White : Colors.Black;

        BtnTabRegistro.BackgroundColor = !esLogin ? colorActivo : colorInactivo;
        BtnTabRegistro.TextColor = !esLogin ? Colors.White : Colors.Black;
    }

    private async void OnIniciarSesionClicked(object sender, EventArgs e)
    {
        string usuario = EntryLoginUsuario.Text?.Trim() ?? string.Empty;
        string contrasena = EntryLoginContrasena.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            await DisplayAlertAsync("Campos vacíos", "Por favor ingresa tu usuario y contraseña.", "OK");
            return;
        }

        var userValido = await BaseDatos.IniciarSesionAsync(usuario, contrasena);
        if (userValido == null)
        {
            await DisplayAlertAsync("Error", "Usuario o contraseña incorrectos.", "OK");
            return;
        }

        // Navegar a la pantalla principal de la app
        if (this.Window != null)
        {
            this.Window.Page = new AppShell();
        }
    }

    private async void OnRegistrarClicked(object sender, EventArgs e)
    {
        string nombre = EntryRegNombre.Text?.Trim() ?? string.Empty;
        string contrasena = EntryRegContrasena.Text ?? string.Empty;
        string confirmar = EntryRegConfirmar.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(confirmar))
        {
            await DisplayAlertAsync("Campos vacíos", "Completa las 3 líneas para registrarte.", "OK");
            return;
        }

        if (contrasena != confirmar)
        {
            await DisplayAlertAsync("Error", "Las contraseñas no coinciden.", "OK");
            return;
        }

        var existente = await BaseDatos.ObtenerUsuarioPorNombreAsync(nombre);
        if (existente != null)
        {
            await DisplayAlertAsync("Aviso", "Ese nombre de usuario ya está registrado.", "OK");
            return;
        }

        await BaseDatos.RegistrarUsuarioAsync(new Usuario
        {
            Nombre = nombre,
            Contrasena = contrasena
        });

        await DisplayAlertAsync("¡Listo!", "Cuenta creada correctamente. Ya puedes iniciar sesión.", "OK");

        // Limpiar campos y cambiar a la pestaña de Iniciar Sesión con el usuario ya escrito
        EntryLoginUsuario.Text = nombre;
        EntryLoginContrasena.Text = string.Empty;
        EntryRegNombre.Text = string.Empty;
        EntryRegContrasena.Text = string.Empty;
        EntryRegConfirmar.Text = string.Empty;

        OnTabLoginClicked(sender, e);
    }
}