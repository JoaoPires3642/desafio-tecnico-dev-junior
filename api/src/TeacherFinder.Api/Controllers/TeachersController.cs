using Microsoft.AspNetCore.Mvc;
using TeacherFinder.Api.Contracts;
using TeacherFinder.Api.Infrastructure;
using TeacherFinder.Core.Search;

namespace TeacherFinder.Api.Controllers;

/// <summary>
/// Camada HTTP: só traduz o pedido, chama o Core e monta a resposta.
/// Nenhuma regra de busca mora aqui.
/// </summary>
[ApiController]
[Route("api/professores")]
public sealed class TeachersController : ControllerBase
{
    private readonly ITeacherCatalog _catalog;
    private readonly SearchRequestValidator _validator;
    private readonly TeacherSearch _search;

    public TeachersController(ITeacherCatalog catalog, SearchRequestValidator validator, TeacherSearch search)
    {
        _catalog = catalog;
        _validator = validator;
        _search = search;
    }

    [HttpPost("busca")]
    [ProducesResponseType(typeof(SearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Search([FromBody] SearchRequestDto request)
    {
        var validation = _validator.Validate(new SearchRequestInput(
            request.Language,
            request.Days,
            request.StartWindow,
            request.EndWindow,
            request.DurationMinutes));

        if (!validation.IsValid)
        {
            return BadRequest(new ValidationErrorResponse("Dados inválidos.", validation.Errors));
        }

        var result = _search.Search(_catalog.GetAll(), validation.Request!);
        return Ok(ToResponse(result));
    }

    private static SearchResponseDto ToResponse(SearchResult result) => new(
        result.Teachers
            .Select(match => new TeacherMatchDto(
                match.Id,
                match.Name,
                match.ClassesPerWeek,
                match.AvailableSlots.Select(slot => slot.ToString()).ToList()))
            .ToList(),
        result.Ineligible
            .Select(ineligible => new IneligibleTeacherDto(ineligible.Id, ineligible.Name, ineligible.Reason))
            .ToList());
}
