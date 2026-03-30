using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(string searchPattern, string miscTypeCode,
            List<UserManagement.Domain.Entities.MiscMaster> entities,
            List<GetMiscMasterAutoCompleteDto> dtos)
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(searchPattern, miscTypeCode))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "MC001", Description = "Test" }
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto>
            {
                new() { Id = 1, Code = "MC001", Description = "Test" }
            };
            SetupHappyPath("MC", "TYPE01", entities, dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "MC", MiscTypeCode = "TYPE01" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("MC001");
        }

        [Fact]
        public async Task Handle_WithResults_CallsQueryRepo()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "MC001" }
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { new() { Id = 1 } };
            SetupHappyPath("MC", "TYPE01", entities, dtos);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "MC", MiscTypeCode = "TYPE01" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMiscMaster("MC", "TYPE01"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterAutoCompleteDto>();
            SetupHappyPath("NONE", "TYPE01", entities, dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "NONE", MiscTypeCode = "TYPE01" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "MC001" }
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { new() { Id = 1 } };
            SetupHappyPath("MC", "TYPE01", entities, dtos);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "MC", MiscTypeCode = "TYPE01" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
