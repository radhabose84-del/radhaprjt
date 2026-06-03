using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IStation;
using UserManagement.Application.Station.Queries.GetStationById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Station.Queries
{
    public sealed class GetStationByIdQueryHandlerTests
    {
        private readonly Mock<IStationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.Station ValidEntity() =>
            new() { Id = 1, Code = "STA-0001", StationName = "Test Station" };

        private static StationByIdDto ValidDto() =>
            new() { Id = 1, Code = "STA-0001", StationName = "Test Station" };

        [Fact]
        public async Task Handle_ExistingId_ReturnsStationDto()
        {
            var entity = ValidEntity();
            var query = new GetStationByIdQuery { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(query.Id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<StationByIdDto>(entity)).Returns(ValidDto());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("STA-0001");
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            var query = new GetStationByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(query.Id)).ReturnsAsync((UserManagement.Domain.Entities.Station?)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            var entity = ValidEntity();
            var query = new GetStationByIdQuery { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetStationByIdAsync(query.Id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<StationByIdDto>(entity)).Returns(ValidDto());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetStationByIdAsync(query.Id), Times.Once);
        }
    }
}
