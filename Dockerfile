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

# EmailService escribe el token cache de MSAL en una ruta relativa (Email:TokenCachePath
# = App_Data/email_token_cache.bin); sin el directorio, File.WriteAllBytes revienta.
RUN mkdir -p /app/App_Data

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Railway inyecta PORT en runtime; ENV en el Dockerfile no lo expandiria, por eso
# entrypoint en forma shell. Kestrel debe escuchar en 0.0.0.0, no en localhost.
ENTRYPOINT ["sh", "-c", "exec dotnet SafetyReport.WebApi.dll --urls http://0.0.0.0:${PORT:-8080}"]
