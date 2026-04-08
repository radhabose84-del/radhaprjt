using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Dto;
using GateEntryManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetAllMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllMiscMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<MiscMasterDto>
            {
                new MiscMasterDto { Id = 1, Code = "MM001", Description = "Test MiscMaster", MiscTypeId = 1 }
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((dtoList, 1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<MiscMasterDto>
            {
                new MiscMasterDto { Id = 1, Code = "MM001", Description = "Test" }
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search", 1))
                .ReturnsAsync((dtoList, 11));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search", MiscTypeId = 1 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<MiscMasterDto>(), 0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<MiscMasterDto>(), 0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAllMiscMasterQuery" &&
                        e.ActionCode == "Get"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
