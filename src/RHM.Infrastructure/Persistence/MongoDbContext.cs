using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RHM.Infrastructure.Documents;

namespace RHM.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured.");
        var databaseName = configuration["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName is not configured.");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<FormSchema> FormSchemas =>
        _database.GetCollection<FormSchema>("form_schemas");

    public IMongoCollection<FormResponse> FormResponses =>
        _database.GetCollection<FormResponse>("form_responses");

    public IMongoCollection<GlobalRiasConfig> GlobalRiasConfig =>
        _database.GetCollection<GlobalRiasConfig>("global_rias_config");
}
