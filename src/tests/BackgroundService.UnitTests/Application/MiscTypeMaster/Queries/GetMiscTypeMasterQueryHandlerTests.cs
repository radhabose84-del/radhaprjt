using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster> ValidEntities() =>
            new()
            {
                new() { Id = 1, MiscTypeCode = "TYPE1", Description = "Type One" },
                new() { Id = 2, MiscTypeCode = "TYPE2", Description = "Type Two" }
            };

        private static List<GetMiscTypeMasterDto> ValidDtos() =>
            new()
            {
                new() { Id = 1, MiscTypeCode = "TYPE1", Description = "Type One" },
                new() { Id = 2, MiscTypeCode = "TYPE2", Description = "Type Two" }
            };

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((ValidEntities(), 2));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(2, 5, "search"))
                .ReturnsAsync((ValidEntities(), 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((new List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(new List<GetMiscTypeMasterDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((ValidEntities(), 2));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

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
