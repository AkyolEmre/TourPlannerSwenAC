using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Tour_Planner.BL;
using Tour_Planner.Models;

namespace Tour_Planner.DAL
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public IConfiguration Configuration { get; }

        public App()
        {
            // Config aus appsettings.json laden
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("confsettings\\appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Host mit DI und DbContext konfigurieren
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<TourContext>(options =>
                        options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
                    services.AddScoped<ITourRepository, TourRepository>();
                    services.AddScoped<ITourService, TourService>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // log4net konfigurieren
            var log4netConfigFile = new FileInfo(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration["log4net:fileName"]));
            XmlConfigurator.ConfigureAndWatch(log4netConfigFile);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();
            SeedDatabase();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.StopAsync().Wait();
            base.OnExit(e);
        }

        private void SeedDatabase()
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TourContext>();
            // Erst beim allerersten Start neue DB inkl. Tabellen anlegen
            if (context.Database.EnsureCreated())
            {
                SeedData(context);
            }
        }

        private void SeedData(TourContext context)
        {
            if (context.Tours.Any())
                return;

            // Beispiel-Tour anlegen
            var t1 = new Tour
            {
                Name = "Mittagspause",
                Description = "Tour von da HTL zum Mci",
                From = "Hammerweg 1, 3910 Zwettl",
                To = "Andre Freyskorn Str. 2, 3910 Zwettl",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };
            context.Tours.Add(t1);

            // Weitere Beispiel-Touren
            context.Tours.AddRange(
                new Tour
                {
                    Name = "Arbeitsweg",
                    Description = "Jürgens Arbeitsweg im Sommer",
                    From = "Gröblingerstraße 366, 3920 Groß Gerungs",
                    To = "Kreuzberg 107, 3920 Groß Gerungs",
                    TransportType = TransportType.Car,
                    Distance = 0,
                    EstimatedTime = 0,
                    Img = "tour2.jpg"
                },
                new Tour
                {
                    Name = "Einkaufstour",
                    Description = "Shoppen zum Pfeiffer-Vogl idk",
                    From = "Arbesbach 224",
                    To = "Arbesbach 64",
                    TransportType = TransportType.Walk,
                    Distance = 0,
                    EstimatedTime = 0,
                    Img = "tour2.jpg"
                },
                new Tour
                {
                    Name = "FH-Weg",
                    Description = "Meine Wohnung zur FH",
                    From = "Aignerstraße 6, 1200 Wien",
                    To = "Höchstädtpl. 6, 1200 Wien",
                    TransportType = TransportType.Walk,
                    Distance = 0,
                    EstimatedTime = 0,
                    Img = "tour2.jpg"
                },
                new Tour
                {
                    Name = "LOCO-SQUAD",
                    Description = "GEMMA LOCO?!",
                    From = "Aignerstraße 6, 1200 Wien",
                    To = "Loco-Bar",
                    TransportType = TransportType.Car,
                    Distance = 0,
                    EstimatedTime = 0,
                    Img = "tour2.jpg"
                }
            );

            // Beispiel-Log mit UTC-Timestamp
            context.TourLogs.Add(new TourLog
            {
                Tour = t1,
                DateTime = DateTime.UtcNow,  // <-- Kind=Utc, kompatibel mit timestamptz
                Comment = "Machen lieber nicht nochmal",
                Difficulty = DifficultyLevel.Hard,
                TotalDistance = "5000",
                TotalTime = "3600",
                Rating = 3
            });

            context.SaveChanges();
        }
    }
}
