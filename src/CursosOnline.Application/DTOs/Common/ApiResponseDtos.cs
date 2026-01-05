namespace CursosOnline.Application.DTOs.Common;

/// <summary>
/// Respuesta estándar de la API (no paginada).
/// </summary>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje para el cliente (éxito o error amigable).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Código de error opcional (para que el frontend pueda distinguir casos).
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Datos devueltos por la operación.
    /// Para operaciones sin datos (por ejemplo Delete), puede ser null.
    /// </summary>
    public T? Data { get; set; }
}

/// <summary>
/// Respuesta estándar de la API para listas con paginación.
/// </summary>
public class PagedResponseDto<T> : ApiResponseDto<IEnumerable<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}