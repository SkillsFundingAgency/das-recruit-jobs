namespace SFA.DAS.Recruit.Jobs.OuterApi.Common;

public class NotificationEmail
{
    public required Guid TemplateId { get; set; }
    public required string RecipientAddress { get; set; }
    public required Dictionary<string, string> Tokens { get; set; } = [];
    public required IEnumerable<long> SourceIds { get; set; } = [];
}