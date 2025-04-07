﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;
using MongoEmployerProfile = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.EmployerProfile;
using MongoAddress = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Address;

namespace SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;

[ExcludeFromCodeCoverage]
public class EmployerProfilesMapper(
    ILogger<UserNotificationPreferencesMapper> logger,
    IEncodingService encodingService)
{
    public EmployerProfile MapProfileFrom(MongoEmployerProfile source)
    {
        encodingService.TryDecode(source.EmployerAccountId, EncodingType.AccountId, out var accountId);
        encodingService.TryDecode(source.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityId);

        return new EmployerProfile
        {
            AccountLegalEntityId = accountLegalEntityId,
            AccountId = accountId,
            AboutOrganisation = source.AboutOrganisation,
            TradingName = source.TradingName,
        };
    }

    public IEnumerable<EmployerProfileAddress> MapAddressesFrom(List<MongoEmployerProfile> source)
    {
        return source.SelectMany(x =>
        {
            encodingService.TryDecode(x.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId, out var accountLegalEntityId);
            return x.OtherLocations?.Select(address => MapAddressFrom(address, accountLegalEntityId)) ?? [];
        });
    }
    
    private static EmployerProfileAddress MapAddressFrom(MongoAddress source, long accountLegalEntityId)
    {
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