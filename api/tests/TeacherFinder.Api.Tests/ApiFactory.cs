using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TeacherFinder.Api.Tests;

/// <summary>
/// Sobe a API em memória (sem portas) para os testes de integração e aponta a
/// carga dos professores para o professores.json real, copiado para a saída do
/// projeto de teste pelo csproj.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<global::Program>
{
    public static string ProfessoresPath => Path.Combine(AppContext.BaseDirectory, "Data", "professores.json");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Professores:Arquivo"] = ProfessoresPath,
            }));
    }
}
