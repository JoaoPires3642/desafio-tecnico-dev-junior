# Dockerfile da API. O contexto de build é a RAIZ do repositório
# (ver docker-compose.yml), porque precisamos de professores.json e de todo o api/.

# ---- build: restaura e publica a API em Release ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projetos + solução
COPY api/ api/
COPY professores.json professores.json

RUN dotnet restore api/TeacherFinder.sln
RUN dotnet publish api/src/TeacherFinder.Api/TeacherFinder.Api.csproj \
        -c Release -o /app/publish --no-restore

# ---- testes: alvo usado por `docker compose --profile testes run --rm testes` ----
# Os testes rodam quando o serviço sobe (e o container encerra ao terminar),
# em vez de rodar em tempo de build — senão o `run` ficaria pendurado sem comando.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS testes
WORKDIR /src
COPY api/ api/
COPY professores.json professores.json
CMD ["dotnet", "test", "api/TeacherFinder.sln", "-c", "Release"]

# ---- runtime: imagem final enxuta ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeacherFinder.Api.dll"]
