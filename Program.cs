using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthCheck;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<ExpensiveHealthMonitor>();
        builder.Services.AddHostedService(p => p.GetRequiredService<ExpensiveHealthMonitor>());

        builder.Services.AddHealthChecks()
            .AddCheck<CustomHealthCheck>("PokeApi")
            .AddCheck<ExpensiveHealthMonitor>("ExpensiveHealthCheck");

        if (builder.Configuration.GetValue<bool>("Redis-Sql-HealthCheck-Enabled"))
        {
            builder.Services.AddHealthChecks()
                .AddSqlServer("Server=localhost;Database=master;User Id=sa;Password=SuperStr0ngP@ssw0rd;TrustServerCertificate=True;", "SELECT 1")
                .AddRedis("localhost:6379");
        }
        
        
        builder.Services.AddHttpClient();
        
        var app = builder.Build();

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
            });

        app.Run();
    }
}