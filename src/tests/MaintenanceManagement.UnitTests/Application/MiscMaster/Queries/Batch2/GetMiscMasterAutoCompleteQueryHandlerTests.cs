using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscMaster.Queries.Batch2
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(List<GetMiscMasterAutoCompleteDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<GetMiscMasterAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<GetMiscMasterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "abc", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "abc", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMiscMaster("abc", "TYPE1"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "abc", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "Division"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "x", MiscTypeCode = "T" },
                CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
