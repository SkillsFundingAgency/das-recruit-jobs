namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public record SendEmailRequest(Guid TemplateId, string RecipientAddress, Dictionary<string, string> Tokens);