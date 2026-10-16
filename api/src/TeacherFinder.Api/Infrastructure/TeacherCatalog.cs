using TeacherFinder.Core.Model;

namespace TeacherFinder.Api.Infrastructure;

/// <summary>
/// Carrega os professores do arquivo uma única vez e mantém em memória,
/// como o enunciado permite. O caminho pode ser trocado por configuração
/// (Professores:Arquivo ou PROFESSORES_ARQUIVO), o que ajuda em testes.
/// </summary>
public sealed class TeacherCatalog : ITeacherCatalog
{
    private readonly Lazy<IReadOnlyList<Teacher>> _teachers;

    public TeacherCatalog(IConfiguration configuration)
    {
        var path = ResolvePath(configuration);
        _teachers = new Lazy<IReadOnlyList<Teacher>>(() => TeachersJsonReader.Read(File.ReadAllText(path)));
    }

    public IReadOnlyList<Teacher> GetAll() => _teachers.Value;

    private static string ResolvePath(IConfiguration configuration)
    {
        var configured = configuration["Professores:Arquivo"]
            ?? Environment.GetEnvironmentVariable("PROFESSORES_ARQUIVO");

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        // Padrão: Data/professores.json ao lado do executável (ver o csproj, que
        // copia o arquivo da raiz do repositório para a saída).
        return Path.Combine(AppContext.BaseDirectory, "Data", "professores.json");
    }
}
