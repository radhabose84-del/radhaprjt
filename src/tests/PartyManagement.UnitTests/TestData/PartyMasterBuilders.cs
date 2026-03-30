using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.TestData
{
    public static class PartyMasterBuilders
    {
        public static CreatePartyMasterCommand ValidCreateCommand(string partyName = "Test Party") =>
            new CreatePartyMasterCommand
            {
                PartyMaster = new CreatePartyMasterDto
                {
                    PartyName = partyName,
                    RegistrationTypeId = 1,
                    PAN = "ABCPA1234A",
                    PartyTypes = new List<CreatePartyMasterDto.PartyTypeDto>
                    {
                        new CreatePartyMasterDto.PartyTypeDto { PartyTypeId = 1, PartyGroupId = 1 }
                    },
                    PartyUnitCompanies = new List<CreatePartyMasterDto.PartyUnitCompanyDto>
                    {
                        new CreatePartyMasterDto.PartyUnitCompanyDto { CompanyId = 1, UnitId = 1 }
                    },
                    PartyContacts = new List<CreatePartyMasterDto.PartyContactDto>
                    {
                        new CreatePartyMasterDto.PartyContactDto
                        {
                            FirstName = "John",
                            MobileNo = "9876543210",
                            EmailID = "test@example.com",
                            ContactBy = "Phone"
                        }
                    },
                    PartyAddresses = new List<CreatePartyMasterDto.PartyAddressDto>
                    {
                        new CreatePartyMasterDto.PartyAddressDto
                        {
                            AddressType = "Office",
                            City = "Mumbai",
                            State = "Maharashtra",
                            Country = "India"
                        }
                    },
                    PartyBanks = null,
                    SalesTypes = null,
                    AgentConfigs = null,
                    PartyDocuments = null
                }
            };

        public static UpdatePartyMasterCommand ValidUpdateCommand(int id = 1, string partyName = "Updated Party") =>
            new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto
                {
                    Id = id,
                    PartyName = partyName,
                    RegistrationTypeId = 1,
                    PAN = "ABCPA1234A",
                    PartyTypesUpdate = new List<UpdatePartyMasterDto.UpdatePartyTypeDto>
                    {
                        new UpdatePartyMasterDto.UpdatePartyTypeDto { PartyTypeId = 1, PartyGroupId = 1 }
                    },
                    PartyUnitCompaniesUpdate = new List<UpdatePartyMasterDto.UpdatePartyUniCompanyDto>
                    {
                        new UpdatePartyMasterDto.UpdatePartyUniCompanyDto { CompanyId = 1, UnitId = 1 }
                    },
                    PartyContactsUpdate = new List<UpdatePartyMasterDto.UpdatePartyContactDto>
                    {
                        new UpdatePartyMasterDto.UpdatePartyContactDto
                        {
                            FirstName = "John",
                            MobileNo = "9876543210",
                            EmailID = "test@example.com",
                            ContactBy = "Phone"
                        }
                    },
                    PartyAddressesUpdate = new List<UpdatePartyMasterDto.UpdatePartyAddressDto>
                    {
                        new UpdatePartyMasterDto.UpdatePartyAddressDto
                        {
                            AddressType = "Office",
                            City = "Mumbai",
                            State = "Maharashtra",
                            Country = "India"
                        }
                    }
                }
            };

        public static DeletePartyMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeletePartyMasterCommand { Id = id };

        public static PartyManagement.Domain.Entities.PartyMaster ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.PartyMaster
            {
                Id = id,
                PartyCode = "PAR001",
                PartyName = "Test Party",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
