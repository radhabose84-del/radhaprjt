using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Issue.Queries.GetApprovedMrsById;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetApprovedMrsByIdQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetApprovedMrsByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsList()
        {
            var dtos = new List<GetApprovedMrsByIdDto> { new() };
            _mockQueryRepo
                .Setup(r => r.GetApprovedMrsDetails(It.IsAny<string>()))
                .ReturnsAsync(dtos);
            _mockMapper.Setup(m => m.Map<List<GetApprovedMrsByIdDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetApprovedMrsByIdQuery { SearchPattern = "MRS-001" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetApprovedMrsDetails(It.IsAny<string>()))
                .ReturnsAsync(new List<GetApprovedMrsByIdDto>());
            _mockMapper.Setup(m => m.Map<List<GetApprovedMrsByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetApprovedMrsByIdDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetApprovedMrsByIdQuery { SearchPattern = "NONEXIST" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetApprovedMrsDetails("MRS-001"))
                .ReturnsAsync(new List<GetApprovedMrsByIdDto>());
            _mockMapper.Setup(m => m.Map<List<GetApprovedMrsByIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetApprovedMrsByIdDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetApprovedMrsByIdQuery { SearchPattern = "MRS-001" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetApprovedMrsDetails("MRS-001"), Times.Once);
        }
    }
}
