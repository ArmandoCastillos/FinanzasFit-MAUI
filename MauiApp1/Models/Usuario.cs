using SQLite;

namespace MauiApp1.Models;

public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}