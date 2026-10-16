using Microsoft.AspNetCore.Mvc;
using TeacherFinder.Api.Contracts;
using TeacherFinder.Api.Infrastructure;
using TeacherFinder.Core.Search;

const string WebCorsPolicy = "web";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ITeacherCatalog, TeacherCatalog>();
builder.Services.AddSingleton<SearchRequestValidator>();
builder.Services.AddSingleton<TeacherSearch>();

// Libera a tela (por padrão em http://localhost:5173). Dá para ajustar por CORS_ORIGEM,
// aceitando várias origens separadas por vírgula.
var allowedOrigins = builder.Configuration["CORS_ORIGEM"] is { Length: > 0 } configuredOrigins
    ? configuredOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : new[] { "http://localhost:5173" };

builder.Services.AddCors(options => options.AddPolicy(
    WebCorsPolicy,
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// Garante que qualquer 400 (inclusive JSON malformado) saia no mesmo formato { mensagem, erros }.
// Os erros de campo são tratados pelo validador do Core, com mensagens em português;
// aqui só caem problemas de corpo (JSON quebrado, tipo errado), então vale uma mensagem pt-BR.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(new ValidationErrorResponse(
            "Dados inválidos.",
            new[] { "Corpo da requisição inválido. Envie um JSON no formato esperado." }));
});

var app = builder.Build();

app.UseCors(WebCorsPolicy);
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

/// <summary>Exposto para os testes de integração usarem WebApplicationFactory.</summary>
public partial class Program;
