using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddConsole();
        
        builder.Services.AddHostedService<ExpensiveHealthMonitor>();

        builder.Services.AddHealthChecks()
            .AddCheck<CustomHealthCheck>("PokeApi", failureStatus: HealthStatus.Unhealthy, tags: new[] { "services" })
            .AddSqlServer("Server=localhost;Database=master;User Id=sa;Password=SuperStr0ngP@ssw0rd;TrustServerCertificate=True;", "SELECT 1")
            .AddRedis("localhost:6379");
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.MapHealthChecks("/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        app.MapGet("/pokemon/{name}", async (string name) =>
            {
               var client = new HttpClient(); 
               var response = await client.GetAsync($"https://pokeapi.co/api/v2/pokemon");
               return response.Content.ReadFromJsonAsync<string>();
            })
            .WithName("GetPokemon")
            .WithOpenApi();

        app.Run();
    }
}