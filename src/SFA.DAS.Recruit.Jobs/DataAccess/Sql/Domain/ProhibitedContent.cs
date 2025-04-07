using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

[ExcludeFromCodeCoverage]
public class ProhibitedContent
{
    public ProhibitedContentType ContentType { get; init; }
    public required string Content { get; init; }
}