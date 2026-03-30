using AutoMapper;
using MediatR;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.City.Queries.GetCityById;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.City.Queries
{
    public sealed class GetCityByIdQueryHandlerTests
    {
        private readonly Mock<ICityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetCityByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsCityDto()
        {
            var entity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CityDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCityByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CityCode.Should().Be("CTY01");
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Cities?)null);

            // The handler maps before null-check, so we need to handle Map(null)
            _mockMapper
                .Setup(m => m.Map<CityDto>((Cities?)null))
                .Returns((CityDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetCityByIdQuery { Id = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entity = CityBuilders.ValidEntity();
            var dto = CityBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<CityDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetCityByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "City"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
