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
        var vehicles = new List<Vehicle>
        {
            // === Economy сегмент (8 авто) ===
            new Vehicle
            {
                Id = 1,
                Brand = "Lada",
                Model = "Vesta",
                Year = 2022,
                Kilometrage = 35000,
                Price = 1_450_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.PrivateSeller,
                VIN = "XTA219000N0123456",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 2,
                Brand = "Lada",
                Model = "Granta",
                Year = 2021,
                Kilometrage = 52000,
                Price = 980_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.Dealer,
                VIN = "XTA219000M0234567",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 3,
                Brand = "Renault",
                Model = "Duster",
                Year = 2020,
                Kilometrage = 78000,
                Price = 1_890_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.PrivateSeller,
                VIN = "XUF553000L0345678",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 4,
                Brand = "Kia",
                Model = "Rio",
                Year = 2023,
                Kilometrage = 18000,
                Price = 2_150_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.Dealer,
                VIN = "KNAFB811CN0456789",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 5,
                Brand = "Hyundai",
                Model = "Solaris",
                Year = 2022,
                Kilometrage = 41000,
                Price = 1_980_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.PrivateSeller,
                VIN = "KMJB8111CN0567890",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 6,
                Brand = "Lada",
                Model = "Niva Travel",
                Year = 2021,
                Kilometrage = 29000,
                Price = 1_650_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.Dealer,
                VIN = "XTA212300M0678901",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 7,
                Brand = "Volkswagen",
                Model = "Polo",
                Year = 2020,
                Kilometrage = 65000,
                Price = 1_720_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.PrivateSeller,
                VIN = "WVWZZZ6RZLP0789012",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 8,
                Brand = "Skoda",
                Model = "Rapid",
                Year = 2021,
                Kilometrage = 48000,
                Price = 1_850_000m,
                Segment = Segment.Working,
                Source = SourceOfPurchase.Dealer,
                VIN = "TMBZZZNH5M00890123",
                Status = VehicleStatus.Available
            },

            // === Middle сегмент (10 авто) ===
            new Vehicle
            {
                Id = 9,
                Brand = "Kia",
                Model = "Sportage",
                Year = 2022,
                Kilometrage = 32000,
                Price = 3_450_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "KNAK6811CN0901234",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 10,
                Brand = "Hyundai",
                Model = "Creta",
                Year = 2023,
                Kilometrage = 12000,
                Price = 2_890_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "KMJS8811CN1012345",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 11,
                Brand = "Toyota",
                Model = "Camry",
                Year = 2021,
                Kilometrage = 55000,
                Price = 4_200_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Import,
                VIN = "JTMBF33V1M0112345",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 12,
                Brand = "Toyota",
                Model = "RAV4",
                Year = 2022,
                Kilometrage = 28000,
                Price = 4_950_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Import,
                VIN = "JTMBF33V2N0123456",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 13,
                Brand = "Volkswagen",
                Model = "Tiguan",
                Year = 2021,
                Kilometrage = 44000,
                Price = 3_850_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "WVWZZZ5NZMW134567",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 14,
                Brand = "Skoda",
                Model = "Octavia",
                Year = 2022,
                Kilometrage = 36000,
                Price = 3_150_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.PrivateSeller,
                VIN = "TMBZZZNE5N0145678",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 15,
                Brand = "Geely",
                Model = "Monjaro",
                Year = 2024,
                Kilometrage = 5000,
                Price = 4_350_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "L6T889CE4RN156789",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 16,
                Brand = "Haval",
                Model = "F7",
                Year = 2023,
                Kilometrage = 21000,
                Price = 3_290_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "LGWFF5A59PH167890",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 17,
                Brand = "Chery",
                Model = "Tiggo 7 Pro",
                Year = 2023,
                Kilometrage = 15000,
                Price = 2_950_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Dealer,
                VIN = "LVVDB11B9PD178901",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 18,
                Brand = "Mazda",
                Model = "CX-5",
                Year = 2021,
                Kilometrage = 47000,
                Price = 3_650_000m,
                Segment = Segment.Middle,
                Source = SourceOfPurchase.Import,
                VIN = "JM3KFBDM5M0189012",
                Status = VehicleStatus.Available
            },

            // === Premium сегмент (7 авто) ===
            new Vehicle
            {
                Id = 19,
                Brand = "BMW",
                Model = "X5",
                Year = 2022,
                Kilometrage = 25000,
                Price = 9_500_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Import,
                VIN = "WBAKR4105N0190123",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 20,
                Brand = "BMW",
                Model = "X3",
                Year = 2021,
                Kilometrage = 38000,
                Price = 7_200_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Dealer,
                VIN = "WBA8E9105M0201234",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 21,
                Brand = "Mercedes",
                Model = "GLE",
                Year = 2023,
                Kilometrage = 14000,
                Price = 11_800_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Import,
                VIN = "WDD1670151A212345",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 22,
                Brand = "Mercedes",
                Model = "GLC",
                Year = 2022,
                Kilometrage = 29000,
                Price = 8_400_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Dealer,
                VIN = "WDD2530151N223456",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 23,
                Brand = "Audi",
                Model = "Q7",
                Year = 2021,
                Kilometrage = 42000,
                Price = 9_900_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Import,
                VIN = "WAUZZZ4M5N0234567",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 24,
                Brand = "Audi",
                Model = "Q5",
                Year = 2022,
                Kilometrage = 31000,
                Price = 7_650_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Dealer,
                VIN = "WAUZZZ8R5N0245678",
                Status = VehicleStatus.Available
            },
            new Vehicle
            {
                Id = 25,
                Brand = "Lexus",
                Model = "RX",
                Year = 2023,
                Kilometrage = 19000,
                Price = 8_900_000m,
                Segment = Segment.Premium,
                Source = SourceOfPurchase.Import,
                VIN = "JTJGAMBA5P0256789",
                Status = VehicleStatus.Available
            }
        };

        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
