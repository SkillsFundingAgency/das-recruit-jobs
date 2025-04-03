using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public class ReferenceDataMigrationMongoRepository(
    ILoggerFactory loggerFactory,
    IOptions<MongoDbConnectionDetails> config)
    : MongoDbCollectionBase(loggerFactory, MongoDbNames.RecruitDb, config)
{
    private const string Id = "_id";
    private static readonly IDictionary<Type, string> TypeLookup = new Dictionary<Type, string> 
    {
        { typeof(ProfanityList), "Profanities" },
        { typeof(BannedPhraseList), "BannedPhrases" },
    };

    public async Task<T> GetReferenceData<T>() where T : class, IReferenceDataItem
    {
        if (!TypeLookup.TryGetValue(typeof(T), out var id))
        {
            throw new ArgumentOutOfRangeException($"{typeof(T).Name} is not a recognised reference data type");
        }
        
        var filter = Builders<T>.Filter.Eq(Id, id);
        var collection = GetCollection<T>(MongoDbCollectionNames.ReferenceData);
        var result = await RetryPolicy.ExecuteAsync(_=> collection.Find(filter).SingleOrDefaultAsync(), new Context(nameof(GetReferenceData)));
        return result;
    }
}