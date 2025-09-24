using Hangfire;
using Hangfire.Application.VideoDownloader;
using MongoDB.Driver;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Application.Report;
using Hangfire.Application.FileHandling;
using Hangfire.Application.Config;

var builder = WebApplication.CreateBuilder(args);

void AddMongo(IServiceCollection services, WebApplicationBuilder builder)
{
    var hangfireConnString = builder.Configuration["AppSettings:ConnectionStrings:Hangfire"];
    Console.WriteLine("MONGO DB CONN STRING: " + hangfireConnString);
    // var mongoUrlBuilder = new MongoUrlBuilder("mongodb://hangfire:hangfirePassword@127.0.0.1:27017,127.0.0.1:27018,127.0.0.1:27019/hangfire?replicaSet=rs0"); // TODO: move this to configuration
    var mongoUrlBuilder = new MongoUrlBuilder(hangfireConnString); // TODO: move this to configuration
    var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

    services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            Prefix = "hangfire.mongo",
            CheckConnection = false
        })
    );
}

AddMongo(builder.Services, builder);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IVideoDownloader, VideoDownloader>();
builder.Services.AddSingleton<IFileHandling, FileHandling>();
builder.Services.AddSingleton<ReportWebSocketProvider>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseHangfireDashboard();
app.MapHangfireDashboard();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
