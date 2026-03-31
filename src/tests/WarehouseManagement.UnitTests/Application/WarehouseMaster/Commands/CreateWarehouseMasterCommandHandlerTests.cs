using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Commands
{
    public sealed class CreateWarehouseMasterCommandHandlerTests
    {
        private readonly Mock<IWarehouseCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Strict);
        private readonly Mock<ILocationLookup> _mockLocationLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateWarehouseMasterCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateWarehouseMasterCommandHandler CreateSut() =>
            new(_mockCodeGen.Object, _mockCmdRepo.Object, _mockMapper.Object,
                _mockItemGroupLookup.Object, _mockLocationLookup.Object, _mockLogger.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockCodeGen
                .Setup(g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync("WH001");

            _mockMapper
                .Setup(m => m.Map<WarehouseManagement.Domain.Entities.WarehouseMaster>(It.IsAny<CreateWarehouseMasterCommand>()))
                .Returns(WarehouseMasterBuilders.ValidEntity(0));

            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            SetupHappyPath(42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            SetupHappyPath();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesWarehouseCode()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            SetupHappyPath();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCodeGen.Verify(g => g.GenerateAsync(command.UnitId, command.WarehouseTypeId), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyItemGroups_FetchesAllItemGroups()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            command.AllowedItemGroupIds = new List<int>();
            SetupHappyPath();

            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto>
                {
                    new ItemGroupLookupDto { Id = 10 },
                    new ItemGroupLookupDto { Id = 20 }
                });

            await CreateSut().Handle(command, CancellationToken.None);

            _mockItemGroupLookup.Verify(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NeedsLocationResolve_CallsLocationLookup()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            command.CityId = 0;
            command.StateId = 0;
            command.CountryId = 0;
            command.City = "Mumbai";
            command.State = "Maharashtra";
            command.Country = "India";

            _mockLocationLookup
                .Setup(l => l.GetLocationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LocationLookupDto { CityId = 1, StateId = 1, CountryId = 1 });

            SetupHappyPath();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockLocationLookup.Verify(
                l => l.GetLocationAsync("Mumbai", "Maharashtra", "India", It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
