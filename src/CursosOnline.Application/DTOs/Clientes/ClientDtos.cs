using System.ComponentModel.DataAnnotations;

namespace CursosOnline.Application.DTOs.Clientes;

/// <summary>
/// DTO para registrar un nuevo cliente.
/// La contraseña solo viaja de entrada y nunca se devuelve en respuestas.
/// </summary>
public class ClientRegisterDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO para login de cliente.
/// Solo se usa para autenticar, no se devuelve nunca al cliente.
/// </summary>
public class ClientLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar datos de perfil de cliente.
/// No incluye contraseña ni campos sensibles.
/// </summary>
public class ClientUpdateDto
{
    [MaxLength(150)]
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO de respuesta para exponer datos de cliente al frontend.
/// Nunca incluye contraseña, hashes, tokens ni flags internos.
/// </summary>
public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
