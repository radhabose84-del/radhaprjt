using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MassTransit;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Commands
{
    public sealed class UpdatePartyMasterCommandHandlerTests
    {
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ILocationLookup> _mockLocationLookup = new(MockBehavior.Loose);
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdatePartyMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQueryRepo.Object, _mockLocationLookup.Object,
                _mockPublishEndpoint.Object, _mockIp.Object);

        private void SetupHappyPath(int id = 1)
        {
            var beforeDto = new PartyMasterDto { Id = id, PartyName = "Test Party", IsPortalAccessEnabled = 0 };

            _mockQueryRepo
                .Setup(r => r.GetByIdPartyMasterAsync(id))
                .ReturnsAsync(beforeDto);

            _mockCommandRepo
                .Setup(r => r.GetPartyDocumentIdsAsync(id))
                .ReturnsAsync(new List<int>());

            _mockQueryRepo
                .Setup(r => r.GetCompanyUnitMapAsync(id))
                .ReturnsAsync(((IReadOnlyList<int>)new List<int> { 1 }, (IReadOnlyList<int>)new List<int> { 1 }));

            _mockQueryRepo
                .Setup(r => r.GetPartyTypeCodesAsync(id))
                .ReturnsAsync(new List<string> { "CUSTOMER" });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(id, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>()))
                .Returns(new PartyManagement.Domain.Entities.PartyMaster { Id = id, PartyName = "Test Party" });
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(1);
            var command = new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto
                {
                    Id = 1,
                    PartyName = "Updated Party",
                    IsPortalAccessEnabled = 0,
                    PartyAddressesUpdate = null,
                    SalesTypesUpdate = null,
                    AgentConfigsUpdate = null,
                    PartyDocumentsUpdate = null
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(1);
            var command = new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto
                {
                    Id = 1,
                    PartyName = "Updated Party",
                    IsPortalAccessEnabled = 0
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(1, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PartyNotFound_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdPartyMasterAsync(99))
                .ReturnsAsync((PartyMasterDto)null!);

            var command = new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto { Id = 99, PartyName = "Test" }
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Contracts.Common.ExceptionRules>();
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsExceptionRules()
        {
            var beforeDto = new PartyMasterDto { Id = 1, PartyName = "Test Party", IsPortalAccessEnabled = 0 };

            _mockQueryRepo.Setup(r => r.GetByIdPartyMasterAsync(1)).ReturnsAsync(beforeDto);
            _mockCommandRepo.Setup(r => r.GetPartyDocumentIdsAsync(1)).ReturnsAsync(new List<int>());
            _mockQueryRepo.Setup(r => r.GetCompanyUnitMapAsync(1))
                .ReturnsAsync(((IReadOnlyList<int>)new List<int> { 1 }, (IReadOnlyList<int>)new List<int> { 1 }));
            _mockQueryRepo.Setup(r => r.GetPartyTypeCodesAsync(1)).ReturnsAsync(new List<string>());
            _mockCommandRepo.Setup(r => r.UpdateAsync(1, It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>()))
                .Returns(new PartyManagement.Domain.Entities.PartyMaster { Id = 1 });

            var command = new UpdatePartyMasterCommand
            {
                UpdatePartyMaster = new UpdatePartyMasterDto { Id = 1, IsPortalAccessEnabled = 0 }
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Contracts.Common.ExceptionRules>();
        }
    }
}
