using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;
using MongoEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.EmployerProfile;
using MongoAddress = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Address;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMapper(
    ILogger<EmployerProfilesMapper> logger,
    IEncodingService encodingService)
{
    public EmployerProfile MapProfileFrom(MongoEmployerProfile source)
    {
        encodingService.TryDecode(source.EmployerAccountId, EncodingType.AccountId, out var accountId);

        if (string.IsNullOrWhiteSpace(source.AccountLegalEntityPublicHashedId))
        {
            logger.LogWarning("[{EmployerProfileId}] Failed to find AccountLegalEntityPublicHashedId value on record", source.Id);
            return EmployerProfile.None;
        }
        
        if (!encodingService.TryDecode(source.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityId))
        {
            logger.LogWarning("[{EmployerProfileId}] Failed to decode AccountLegalEntityPublicHashedId value: {value}", source.Id, source.AccountLegalEntityPublicHashedId);
            return EmployerProfile.None;
        }

        return new EmployerProfile
        {
            AccountLegalEntityId = accountLegalEntityId,
            AccountId = accountId,
            AboutOrganisation = source.AboutOrganisation,
            TradingName = source.TradingName,
        };
    }

    public IEnumerable<EmployerProfileAddress?> MapAddressesFrom(List<MongoEmployerProfile> source)
    {
        return source.SelectMany(x =>
        {
            encodingService.TryDecode(x.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityId);
            var employerProfileAddresses = x.OtherLocations.Select(address => MapAddressFrom(address, accountLegalEntityId)).Where(c=>c != null);
            return employerProfileAddresses.Where(c=>c!=null) ?? [];
        });
    }
    
    private static EmployerProfileAddress? MapAddressFrom(MongoAddress source, long accountLegalEntityId)
    {
        if (source.Postcode.Length > 8)
        {
            if (source.Postcode.Contains(','))
            {
                var addressParts = source.Postcode.Split(',');
                //This might not be 100% accurate
                return new EmployerProfileAddress
                {
                    AccountLegalEntityId = accountLegalEntityId,
                    AddressLine1 = addressParts.First().Trim(),
                    AddressLine2 = addressParts.Length > 1 ? addressParts[1].Trim() : null,
                    AddressLine3 = addressParts.Length > 2 ? addressParts[2].Trim() : null,
                    AddressLine4 = addressParts[^2].Trim(),
                    Postcode = addressParts.Last().Length <= 8 ? addressParts.Last().Trim() : addressParts.Last()[..8],
                    Latitude = source.Latitude,
                    Longitude = source.Longitude,
                };    
            }
            else
            {
                return null;
            }
        }
        
        return new EmployerProfileAddress
        {
            AccountLegalEntityId = accountLegalEntityId,
            AddressLine1 = source.AddressLine1,
            AddressLine2 = string.IsNullOrWhiteSpace(source.AddressLine2) ? null : source.AddressLine2,
            AddressLine3 = string.IsNullOrWhiteSpace(source.AddressLine3) ? null : source.AddressLine3,
            AddressLine4 = string.IsNullOrWhiteSpace(source.AddressLine4) ? null : source.AddressLine4,
            Postcode = source.Postcode,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
        };
    }
}