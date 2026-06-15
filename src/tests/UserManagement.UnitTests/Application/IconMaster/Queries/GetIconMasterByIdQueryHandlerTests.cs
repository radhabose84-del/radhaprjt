using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMasterById;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Queries
{
    public sealed class GetIconMasterByIdQueryHandlerTests
    {
        private readonly Mock<IIconMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetIconMasterByIdQueryHandler>> _mockLogger = new();

        private GetIconMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_Found_ReturnsDto()
        {
            var entity = IconMasterBuilders.ValidEntity(id: 2);
            var dto = IconMasterBuilders.ValidDto(id: 2);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<IconMasterDto>(entity)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterByIdQuery { IconMasterId = 2 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(2);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.IconMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(new GetIconMasterByIdQuery { IconMasterId = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }
    }
}
