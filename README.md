# Maximilian backend

ASP.NET Core API targeting .NET 10. The entry point is `SafetyReport.WebApi`.

## Requirements

- WSL with a Linux distribution and the .NET 10 SDK installed inside WSL (`dotnet --list-sdks` should show `10.0.x`).
- Access to the application's MySQL 9.4.0 database, including its existing schema and stored procedures. The backend connects using `MySqlConnector`.
- Development configuration for AWS S3, Cognito, n8n, email, and the electronic invoicing service.

## Initial setup on WSL

Run these commands in your WSL terminal:

```bash
cd /home/gustavo/proyectos/maximilian/maximlian3_backend

# Create the local configuration only if it does not already exist.
if [ ! -f SafetyReport.WebApi/appsettings.json ]; then
  cp SafetyReport.WebApi/appsettings.Model.json SafetyReport.WebApi/appsettings.json
fi

mkdir -p SafetyReport.WebApi/App_Data
dotnet restore SafetyReport.WebApi/SafetyReport.WebApi.csproj
```

Edit `SafetyReport.WebApi/appsettings.json` and replace the template values with your development settings. This file is ignored by Git. Configure `ConnectionStrings`, `AWS`, `Cognito`, `Cors`, and `FacturacionElectronica` from the template.

Use a MySQL connection string for `ConnectionStrings:DefaultConnection`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_MYSQL_HOST;Port=3306;Database=YOUR_DATABASE;User ID=YOUR_USER;Password=YOUR_PASSWORD;"
}
```

Use a database host reachable from WSL (`localhost` if MySQL runs in the same WSL environment). Replace any older SQL Server connection string; `TrustServerCertificate` and `Encrypt` are SQL Server options and should not be included in this MySQL connection string.

The template is missing the following sections; add them as top-level properties, using your development values:

```json
"N8n": {
  "Secret": "YOUR_N8N_SECRET",
  "WebhookObtenerCampos": "https://YOUR_N8N_HOST/webhook/obtenerCampos"
},
"Email": {
  "ClientId": "YOUR_EMAIL_APP_CLIENT_ID",
  "Tenant": "YOUR_EMAIL_TENANT",
  "TokenCachePath": "App_Data/email_token_cache.bin"
}
```

Ensure `Cors:AllowedOrigins` includes your frontend URL (the template uses `http://localhost:3000`). The application reads ASP.NET Core configuration and environment variables; it does not automatically load the root `.env` file. Environment variable names use double underscores for nested settings, for example `ConnectionStrings__DefaultConnection`.

## Start the project on WSL

```bash
cd /home/gustavo/proyectos/maximilian/maximlian3_backend
ASPNETCORE_ENVIRONMENT=Development dotnet run \
  --project SafetyReport.WebApi/SafetyReport.WebApi.csproj \
  --no-launch-profile \
  --urls http://localhost:5080
```

This command builds and starts the API using your local configuration. `--no-launch-profile` prevents the settings in `launchSettings.json` from overriding your local integration settings.

Open **http://localhost:5080/swagger** in your browser to explore the API. Authenticated endpoints require a Cognito access token. Stop the server with `Ctrl+C`.

The invoicing synchronization worker runs at startup and every five minutes, so configure the database and invoicing service for your development environment before starting.
