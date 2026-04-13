using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            List<UserManagement.Domain.Entities.MiscMaster> entities,
            List<GetMiscMasterDto> dtos,
            int totalCount)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(pageNumber, pageSize, searchTerm))
                .ReturnsAsync((entities, totalCount));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "MM001", Description = "Test" }
            };
            var dtos = new List<GetMiscMasterDto>
            {
                new() { Id = 1, Code = "MM001", Description = "Test" }
            };
            SetupHappyPath(1, 10, null, entities, dtos, 1);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "MM001" }
            };
            var dtos = new List<GetMiscMasterDto> { new() { Id = 1, Code = "MM001" } };
            SetupHappyPath(2, 5, "search", entities, dtos, 11);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyDataList()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterDto>();
            SetupHappyPath(1, 10, null, entities, dtos, 0);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsQueryRepositoryOnce()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>();
            var dtos = new List<GetMiscMasterDto>();
            SetupHappyPath(1, 10, null, entities, dtos, 0);

            await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllMiscMasterAsync(1, 10, null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1 }
            };
            var dtos = new List<GetMiscMasterDto> { new() { Id = 1 } };
            SetupHappyPath(1, 10, null, entities, dtos, 1);

            await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
