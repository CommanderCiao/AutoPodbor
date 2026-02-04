using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; 
using System.Text.Json.Serialization;
using VehicleSearchService.Controllers;
using VehicleSearchService.Data;
using VehicleSearchService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Logging.AddConsole();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Vehicles.Any())
    {
        var vehicles = new List<Vehicle>();
        var brands = new[] { "BMW", "Audi", "Mercedes", "Toyota", "Honda", "Ford", "Volkswagen", "Nissan" };
        var models = new[] { "X5", "Q7", "GLE", "Camry", "Civic", "Focus", "Passat", "Pathfinder" };
        var segments = Enum.GetValues<Segment>();
        var sources = Enum.GetValues<SourceOfPurchase>();

        var random = new Random();

        for (int i = 1; i <= 100; i++)
        {
            vehicles.Add(new Vehicle
            {
                Id = i,
                Brand = brands[random.Next(brands.Length)],
                Model = models[random.Next(models.Length)],
                Year = random.Next(2015, 2026),
                Kilometrage = random.Next(10000, 200000),
                Price = Math.Round((decimal)random.Next(500000, 5000000), 2),
                Segment = segments[random.Next(segments.Length)],
                Source = sources[random.Next(sources.Length)],
                VIN = $"VIN{i:D17}" 
            });
        }

        context.Vehicles.AddRange(vehicles);
        context.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
