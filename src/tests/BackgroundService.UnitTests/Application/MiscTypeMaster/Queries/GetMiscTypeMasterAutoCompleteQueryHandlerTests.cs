using AutoMapper;
using BackgroundService.Application.Common.Interfaces.IMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster> ValidEntities() =>
            new()
            {
                new() { Id = 1, MiscTypeCode = "TYPE1" },
                new() { Id = 2, MiscTypeCode = "TYPE2" }
            };

        private static List<GetMiscTypeMasterAutocompleteDto> ValidDtos() =>
            new()
            {
                new() { Id = 1, MiscTypeCode = "TYPE1" },
                new() { Id = 2, MiscTypeCode = "TYPE2" }
            };

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("Type"))
                .ReturnsAsync(ValidEntities());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "Type" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("NoMatch"))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "NoMatch" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_NullResults_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("Null"))
                .ReturnsAsync((List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>?)null!);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "Null" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("Type"))
                .ReturnsAsync(ValidEntities());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "Type" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAll"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoResults_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("NoMatch"))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Notification.MiscTypeMaster>());

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "NoMatch" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
