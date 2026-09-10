FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SafetyReport.Dominio/SafetyReport.Domain.csproj    SafetyReport.Dominio/
COPY SafetyReport.Aplicacion/SafetyReport.Application.csproj SafetyReport.Aplicacion/
COPY SafetyReport.Infraestructura/SafetyReport.Infrastructure.csproj SafetyReport.Infraestructura/
COPY SafetyReport.WebApi/SafetyReport.WebApi.csproj    SafetyReport.WebApi/
RUN dotnet restore SafetyReport.WebApi/SafetyReport.WebApi.csproj

COPY . .
RUN dotnet publish SafetyReport.WebApi/SafetyReport.WebApi.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080

# Railway inyecta PORT en runtime; ENV en el Dockerfile no lo expandiria, por eso
# entrypoint en forma shell. Kestrel debe escuchar en 0.0.0.0, no en localhost.
ENTRYPOINT ["sh", "-c", "exec dotnet SafetyReport.WebApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
