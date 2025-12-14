using Microsoft.AspNetCore.Builder;
using Simpchat.Application;

using Simpchat.Infrastructure;
using Simpchat.Infrastructure.Persistence.Extentions;
using Simpchat.Infrastructure.Persistence.Interfaces;
using Simpchat.Shared;
using Simpchat.Web;
using Simpchat.Web.Middlewares;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serializer to handle enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services
    .AddWeb(builder.Configuration)
    .AddShared(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://10.30.1.77:5173", "http://192.168.56.1:5173") // Add your frontend URLs
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // REQUIRED for SignalR
            .WithExposedHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

using (var scope = app.Services.CreateAsyncScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync();
}

app.UseRouting();

app.UseCors("CorsPolicy");

// Disable HTTPS redirection for development with local IP
// app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<Simpchat.Web.Hubs.ChatHub>("/hubs/chat");

app.Run();