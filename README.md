## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# SFA.DAS.Recruit.Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/_projectname_?branchName=master)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=_projectid_&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=_projectId_&metric=alert_status)](https://sonarcloud.io/dashboard?id=_projectId_)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This Azure function app contains various tasks that are part of the Recruit An Apprentice (RAA) system which allows Employers and Providers to create Vacancies for advertising with Find And Apprenticeship (FAA).

## How It Works

The function is a series of triggers that are fired from either a http message, schedule or respond to an event.

## 🚀 Installation

* A clone of this repository
* A code editor that supports Azure functions and .Net Core 8
* A CosmosDB or MongoDb instance or emulator
* AzureTableStorage or Azurite (if running locally) 
* An Azure Active Directory account with the appropriate roles as per the [config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-tools-servicebus-support/SFA.DAS.Tools.Servicebus.Support.json)

### Config

This utility uses the standard Apprenticeship Service configuration. All configuration can be found in the [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config).

Azure Table Storage config

Row Key: SFA.DAS.Recruit.Jobs_2.0

Partition Key: LOCAL

Data:
````json
{
  "ConnectionStrings": {
    "MongoDb": "<insert connection string here>",
    "SqlServer": "<insert connection string here>"
  }
}
````

Important: You will also need the configuration for the `SFA.DAS.Encoding` service.

## Current triggers
### ApplicationReviewsMigrationTimerTrigger
Used to migrate Application Reviews from the existing MongoDb instance into the new SQL db.

_This is a temporary trigger which will be removed once migration is complete._

### ApplicationReviewsMigratinHttpTrigger
Similar to the above, but allows individual records to be (re)migrated.

_This is a temporary trigger which will be removed once migration is complete._

## 🔗 External Dependencies

Currently none.

## Technologies

_For Example_
```
* .Net Core 8
* Azure Functions V3
* CosmosDB
* SQL Server
* Azure Table Storage
* NUnit
* Moq
* FluentAssertions
```

## 🐛 Known Issues

None known at the current time.