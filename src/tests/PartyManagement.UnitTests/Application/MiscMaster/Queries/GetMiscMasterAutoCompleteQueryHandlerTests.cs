using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(
            List<PartyManagement.Domain.Entities.MiscMaster> entities,
            List<GetMiscMasterAutoCompleteDto> dtos)
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsListOfDtos()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "PENDING", Description = "Pending" }
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto>
            {
                new() { Id = 1, Code = "PENDING", Description = "Pending" }
            };
            SetupHappyPath(entities, dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "Pend", MiscTypeCode = "ApprovalStatus" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("PENDING");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterAutoCompleteDto>();
            SetupHappyPath(entities, dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "xyz", MiscTypeCode = "ApprovalStatus" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithCorrectParameters()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterAutoCompleteDto>();
            SetupHappyPath(entities, dtos);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "test", MiscTypeCode = "TypeCode" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetMiscMaster("test", "TypeCode"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterAutoCompleteDto>();
            SetupHappyPath(entities, dtos);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "test", MiscTypeCode = "Type" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleDtos_ReturnsAll()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "CODE1", Description = "Item1" },
                new() { Id = 2, Code = "CODE2", Description = "Item2" },
                new() { Id = 3, Code = "CODE3", Description = "Item3" }
            };
            var dtos = new List<GetMiscMasterAutoCompleteDto>
            {
                new() { Id = 1, Code = "CODE1", Description = "Item1" },
                new() { Id = 2, Code = "CODE2", Description = "Item2" },
                new() { Id = 3, Code = "CODE3", Description = "Item3" }
            };
            SetupHappyPath(entities, dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "Item", MiscTypeCode = "Type" },
                CancellationToken.None);

            result.Should().HaveCount(3);
        }
    }
}
