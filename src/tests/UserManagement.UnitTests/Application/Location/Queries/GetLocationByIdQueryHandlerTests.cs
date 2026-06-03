using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Queries.GetLocationById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Location.Queries
{
    public sealed class GetLocationByIdQueryHandlerTests
    {
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLocationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.Location ValidEntity() =>
            new() { Id = 1, Code = "LOC-0001", LocationName = "Test Location" };

        private static LocationByIdDto ValidDto() =>
            new() { Id = 1, Code = "LOC-0001", LocationName = "Test Location" };

        [Fact]
        public async Task Handle_ExistingId_ReturnsLocationDto()
        {
            var entity = ValidEntity();
            var query = new GetLocationByIdQuery { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetLocationByIdAsync(query.Id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<LocationByIdDto>(entity)).Returns(ValidDto());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("LOC-0001");
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            var query = new GetLocationByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetLocationByIdAsync(query.Id)).ReturnsAsync((UserManagement.Domain.Entities.Location?)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            var entity = ValidEntity();
            var query = new GetLocationByIdQuery { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetLocationByIdAsync(query.Id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<LocationByIdDto>(entity)).Returns(ValidDto());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetLocationByIdAsync(query.Id), Times.Once);
        }
    }
}
