using CursosOnline.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CursosOnline.Infrastructure.Persistence.Context;

/// <summary>
/// Fábrica de DbContext para tiempo de diseño (migraciones).
/// Esto permite que "dotnet ef" cree ApplicationDbContext sin depender de la API.
/// </summary>
public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // ATENCIÓN: para producción lee la cadena de conexión desde configuración segura.
        const string connectionString =
            "Host=localhost;Port=5432;Database=gestorcursos;Username=david;Password=david123";

        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
