using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

[ExcludeFromCodeCoverage]
public class ProhibitedContentMigrationStrategy(
    ReferenceDataMigrationMongoRepository mongoRepository,
    ProhibitedContentMigrationSqlRepository sqlRepository)
{
    public async Task RunAsync()
    {
        // Banned Phrases
        var mongoBannedPhrases = await mongoRepository.GetReferenceData<BannedPhraseList>();
        var bannedPhrases = mongoBannedPhrases.BannedPhrases.Select(x => new ProhibitedContent { ContentType = ProhibitedContentType.BannedPhrases, Content = x }).ToList();
        
        // Profanities
        var mongoProfanities = await mongoRepository.GetReferenceData<ProfanityList>();
        var profanities = mongoProfanities.Profanities.Select(x => new ProhibitedContent { ContentType = ProhibitedContentType.Profanity, Content = x }).ToList();
        
        await sqlRepository.UpsertProhibitedContentBatchAsync([..bannedPhrases, ..profanities]);
    }
}