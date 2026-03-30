using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Application.Manufacture.Queries.GetManufactureById;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Manufactures.Queries
{
    public sealed class GetManufactureByIdQueryHandlerTests
    {
        private readonly Mock<IManufactureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICountryLookup> _mockCountryLookup = new(MockBehavior.Loose);
        private readonly Mock<IStateLookup> _mockStateLookup = new(MockBehavior.Loose);
        private readonly Mock<ICityLookup> _mockCityLookup = new(MockBehavior.Loose);

        private GetManufactureByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockCountryLookup.Object, _mockStateLookup.Object, _mockCityLookup.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = ManufacturesBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<ManufactureDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockCountryLookup.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.CountryLookupDto?)null);
            _mockStateLookup.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.StateLookupDto?)null);
            _mockCityLookup.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.CityLookupDto?)null);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetManufactureByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((ManufactureDTO?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetManufactureByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var dto = ManufacturesBuilders.ValidDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<ManufactureDTO>(It.IsAny<object>()))
                .Returns(dto);

            _mockCountryLookup.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.CountryLookupDto?)null);
            _mockStateLookup.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.StateLookupDto?)null);
            _mockCityLookup.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Contracts.Dtos.Lookups.Users.CityLookupDto?)null);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetManufactureByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
