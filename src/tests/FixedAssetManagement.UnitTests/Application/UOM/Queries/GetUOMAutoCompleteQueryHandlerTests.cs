using AutoMapper;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMAutoComplete;
using FAM.Application.UOM.Queries.GetUOMs;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.UOM.Queries
{
    public sealed class GetUOMAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUOMAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<FAM.Domain.Entities.UOM> { FAMUOMBuilders.ValidEntity() };
            var dtos = new List<UOMAutoCompleteDto> { FAMUOMBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetUOM("kg"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UOMAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "kg" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.UOM> { FAMUOMBuilders.ValidEntity() };
            var dtos = new List<UOMAutoCompleteDto> { FAMUOMBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetUOM("kg"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UOMAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "kg" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetUOM("xyz"))
                .ReturnsAsync(new List<FAM.Domain.Entities.UOM>());

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetUOM(It.IsAny<string>()))
                .ReturnsAsync((List<FAM.Domain.Entities.UOM>?)null!);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetUOMAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
