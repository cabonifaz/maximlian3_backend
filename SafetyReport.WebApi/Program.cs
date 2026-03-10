using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SafetyReport API",
        Version = "v1"
    });
});

var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

app.MapControllers();

app.Run();