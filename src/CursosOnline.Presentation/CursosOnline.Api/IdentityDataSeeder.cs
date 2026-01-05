using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CursosOnline.Api;

public static class IdentityDataSeeder
{
    private const string DefaultEmail = "david@gmail.com";
    private const string DefaultPassword = "David123*";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existingUser = await userManager.FindByEmailAsync(DefaultEmail);
        if (existingUser != null)
        {
            return;
        }

        var user = new IdentityUser
        {
            UserName = DefaultEmail,
            Email = DefaultEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (!result.Succeeded)
        {
            // En un entorno real, deberías registrar estos errores.
            throw new Exception("No se pudo crear el usuario por defecto: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
