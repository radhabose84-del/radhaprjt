using Contracts.Interfaces;
using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.CreateLotMaster;
using ProductionManagement.Presentation.Validation.LotMaster;
using ProductionManagement.Presentation.Validation.Common;

using ProductionManagement.UnitTests.TestHelpers;
namespace ProductionManagement.UnitTests.Validators.LotMaster
{
    public sealed class CreateLotMasterCommandValidatorTests
    {
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateLotMasterCommandValidator CreateValidator()
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(1);
            return new CreateLotMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockIp.Object);
        }

        [Fact]
        public async Task Validate_EmptyCommand_HasValidationErrors()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateLotMasterCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_PassesUnitIdFromToken_ToAlreadyExists()
        {
            _mockIp.Setup(x => x.GetUnitId()).Returns(5);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var cmd = new CreateLotMasterCommand
            {
                LotCode = "LOT001",
                BatchNumber = "B1",
                LotTypeId = 1,
                ItemId = 1,
                StatusId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Today)
            };

            await CreateValidator().TestValidateAsync(cmd);

            _mockQueryRepo.Verify(r => r.AlreadyExistsAsync("LOT001", 5), Times.AtLeastOnce);
        }
    }
}
