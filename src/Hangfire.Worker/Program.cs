using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Worker;
using MongoDB.Driver;
using Hangfire.Application;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

void AddMongo(IServiceCollection services)
{
    var mongoUrlBuilder = new MongoUrlBuilder("mongodb://hangfire:hangfirePassword@127.0.0.1:27017,127.0.0.1:27018,127.0.0.1:27019/hangfire?replicaSet=rs0"); // TODO: move this to configuration
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

    services.AddHangfireServer(serverOptions =>
    {
        serverOptions.ServerName = "Worker Hangfire Server";
    });
}

AddMongo(builder.Services);

var host = builder.Build();
host.Run();
