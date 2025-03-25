using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson.Serialization.Conventions;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
internal static class MongoDbConventions
{
    public static void RegisterMongoConventions()
    {
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(MongoDB.Bson.BsonType.String),
            new IgnoreExtraElementsConvention(true),
            new IgnoreIfNullConvention(true)
        };
        ConventionRegistry.Register("recruit conventions", pack, t => true);
    }
}