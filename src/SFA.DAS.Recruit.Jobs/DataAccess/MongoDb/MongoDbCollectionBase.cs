using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Polly.Retry;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
public abstract class MongoDbCollectionBase
    {
        private readonly string _dbName;
        private readonly IMongoClient? _mongoClient;
        private readonly MongoDbConnectionDetails _config;
        private readonly Lazy<ILogger> _mongoCommandLogger;
        private readonly string[] _excludedCommands = { "isMaster", "buildInfo", "saslStart", "saslContinue", "getLastError" };

        protected ILogger Logger { get; }

        protected AsyncRetryPolicy RetryPolicy { get; set; }

        protected MongoDbCollectionBase(ILoggerFactory loggerFactory, string dbName, IOptions<MongoDbConnectionDetails> config, IMongoClient? mongoClient= null)
        {
            _dbName = dbName;
            _mongoClient = mongoClient;

            _config = config.Value;

            Logger = loggerFactory.CreateLogger(this.GetType().FullName);
            _mongoCommandLogger = new Lazy<ILogger>(() => loggerFactory.CreateLogger("Mongo command"));

            RetryPolicy = MongoDbRetryPolicy.GetRetryPolicy(Logger);
        }

        protected IMongoDatabase GetDatabase()
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(_config.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.ConnectTimeout = TimeSpan.FromMinutes(10);
            settings.SocketTimeout = TimeSpan.FromMinutes(10);
            
            
            //if (_config.ConnectionString.Contains("localhost:27017"))
            LogMongoCommands(settings);

            var client = _mongoClient ?? new MongoClient(settings);
            var database = client.GetDatabase(_dbName);

            return database;
        }

        protected IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            var database = GetDatabase();
            var collection = database.GetCollection<T>(collectionName);

            return collection;
        }

        protected ProjectionDefinition<T> GetProjection<T>()
        {
            ProjectionDefinition<T> projection = null;

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                projection = projection == null ?
                    Builders<T>.Projection.Include(propertyInfo.Name) :
                    projection.Include(propertyInfo.Name);
            }
            return projection;
        }

        private void LogMongoCommands(MongoClientSettings settings)
        {
            settings.ClusterConfigurator = cc => cc.Subscribe<CommandStartedEvent>(e =>
            {
                if (_excludedCommands.Contains(e.CommandName))
                    return;

                _mongoCommandLogger.Value.LogTrace($"{e.CommandName} = {e.Command.ToJson()}");
                Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
            });
        }
    }