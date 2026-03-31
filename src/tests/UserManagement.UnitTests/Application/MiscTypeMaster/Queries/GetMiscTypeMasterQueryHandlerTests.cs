using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(
            List<UserManagement.Domain.Entities.MiscTypeMaster> entities,
            List<GetMiscTypeMasterDto> dtos,
            int pageNumber, int pageSize, string? searchTerm, int totalCount)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(pageNumber, pageSize, searchTerm))
                .ReturnsAsync((entities, totalCount));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01", Description = "Test" }
            };
            var dtos = new List<GetMiscTypeMasterDto>
            {
                new() { Id = 1, MiscTypeCode = "MISC01", Description = "Test" }
            };
            SetupHappyPath(entities, dtos, 1, 10, null, 1);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };
            var dtos = new List<GetMiscTypeMasterDto> { new() { Id = 1 } };
            SetupHappyPath(entities, dtos, 2, 5, "test", 11);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>();
            var dtos = new List<GetMiscTypeMasterDto>();
            SetupHappyPath(entities, dtos, 1, 10, null, 0);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };
            var dtos = new List<GetMiscTypeMasterDto> { new() { Id = 1 } };
            SetupHappyPath(entities, dtos, 1, 10, null, 1);

            await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
