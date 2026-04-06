using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.CreateCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Companies;

namespace UserManagement.UnitTests.Validators.Companies
{
    public sealed class CreateCompanyCommandValidatorTests
    {
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CompanyValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateCompanyCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object);

        private static CreateCompanyCommand ValidCommand() =>
            new CreateCompanyCommand
            {
                Company = new CompanyDTO
                {
                    CompanyName = "Test Company Ltd",
                    LegalName = "Test Legal Name",
                    GstNumber = "22AAAAA1234A1Z5",
                    YearOfEstablishment = 2000,
                    Website = "https://www.testcompany.com",
                    PanNumber = "AAAAA1234A",
                    EntityId = 1,
                    CompanyAddress = new CompanyAddressDTO
                    {
                        AddressLine1 = "123 Test Street",
                        PinCode = "600001",
                        CityId = 1,
                        StateId = 1,
                        CountryId = 1
                    },
                    CompanyContact = new CompanyContactDTO
                    {
                        Name = "John Doe",
                        Designation = "Manager",
                        Email = "john@testcompany.com",
                        Phone = "9876543210"
                    }
                }
            };

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo
                .Setup(r => r.CompanyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.PanNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCompanyName_FailsValidation(string? name)
        {
            SetupAllAsyncMocks();
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
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Company.LegalName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_DuplicateCompanyName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.CompanyExistsAsync("Duplicate Company"))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.PanNumberExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var command = ValidCommand();
            command.Company.CompanyName = "Duplicate Company";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_DuplicatePanNumber_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.CompanyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.PanNumberExistsAsync("AAAAA1234A"))
                .ReturnsAsync(true);

            var command = ValidCommand();
            command.Company.PanNumber = "AAAAA1234A";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_CompanyNameExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Company.CompanyName = new string('A', 51);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_InvalidGstNumber_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Company.GstNumber = "INVALID-GST";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
