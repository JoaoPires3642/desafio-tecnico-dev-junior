using TeacherFinder.Core.Model;

namespace TeacherFinder.Api.Infrastructure;

/// <summary>Fornece os professores para a busca.</summary>
public interface ITeacherCatalog
{
    IReadOnlyList<Teacher> GetAll();
}
