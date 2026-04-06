using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Companies.Commands.UpdateCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Companies;

namespace UserManagement.UnitTests.Validators.Companies
{
    public sealed class UpdateCompanyCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateCompanyValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static UpdateCompanyCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static UpdateCompanyCommand ValidCommand() =>
            new UpdateCompanyCommand
            {
                Company = new UpdateCompanyDTO
                {
                    CompanyName = "Updated Company Ltd",
                    LegalName = "Updated Legal Name",
                    GstNumber = "22AAAAA1234A1Z5",
                    YearOfEstablishment = 2000,
                    Website = "https://www.updated.com",
                    PanNumber = "AAAAA1234A",
                    EntityId = 1,
                    CompanyAddress = new CompanyAddressDTO
                    {
                        AddressLine1 = "456 Updated Street",
                        PinCode = "600002",
                        CityId = 1,
                        StateId = 1,
                        CountryId = 1
                    },
                    CompanyContact = new CompanyContactDTO
                    {
                        Name = "Jane Doe",
                        Designation = "Director",
                        Email = "jane@updated.com",
                        Phone = "9876543211"
                    }
                }
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCompanyName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.Company.CompanyName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLegalName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.Company.LegalName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_CompanyNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.Company.CompanyName = new string('A', 51);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_InvalidGstNumber_FailsValidation()
        {
            var command = ValidCommand();
            command.Company.GstNumber = "INVALID-GST";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_YearOfEstablishmentTooOld_FailsValidation()
        {
            var command = ValidCommand();
            command.Company.YearOfEstablishment = 1800;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
