using AutoMapper;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Application.Manufacture.Queries.GetManufactureAutoComplete;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.Manufactures.Queries
{
    public sealed class GetManufactureAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IManufactureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetManufactureAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var dtos = new List<ManufactureDTO> { ManufacturesBuilders.ValidDto() };
            var autoCompleteDtos = new List<ManufactureAutoCompleteDTO> { ManufacturesBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByManufactureNameAsync("mfg"))
                .ReturnsAsync(dtos);

            _mockMapper
                .Setup(m => m.Map<List<ManufactureAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(autoCompleteDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetManufactureAutoCompleteQuery { SearchPattern = "mfg" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByManufactureNameAsync("xyz"))
                .ReturnsAsync(new List<ManufactureDTO>());

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetManufactureAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var dtos = new List<ManufactureDTO> { ManufacturesBuilders.ValidDto() };
            var autoCompleteDtos = new List<ManufactureAutoCompleteDTO> { ManufacturesBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByManufactureNameAsync("mfg"))
                .ReturnsAsync(dtos);

            _mockMapper
                .Setup(m => m.Map<List<ManufactureAutoCompleteDTO>>(It.IsAny<object>()))
                .Returns(autoCompleteDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetManufactureAutoCompleteQuery { SearchPattern = "mfg" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
