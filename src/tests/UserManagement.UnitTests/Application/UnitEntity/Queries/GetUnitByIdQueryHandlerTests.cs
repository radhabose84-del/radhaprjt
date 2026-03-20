using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Queries.GetUnitById;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.UnitEntity.Queries
{
    public sealed class GetUnitByIdQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUnitByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetUnitByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var repoResult = UnitEntityBuilders.ValidGetUnitsByIdDto();
            var mappedDto = UnitEntityBuilders.ValidGetUnitsByIdDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<GetUnitsByIdDto>(repoResult))
                .Returns(mappedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.UnitName.Should().Be("Test Unit");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GetUnitsByIdDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetUnitByIdQuery { Id = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var repoResult = UnitEntityBuilders.ValidGetUnitsByIdDto();
            var mappedDto = UnitEntityBuilders.ValidGetUnitsByIdDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(repoResult);

            _mockMapper
                .Setup(m => m.Map<GetUnitsByIdDto>(repoResult))
                .Returns(mappedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetUnitByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetUnitByIdQuery" &&
                        e.Module == "Unit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
