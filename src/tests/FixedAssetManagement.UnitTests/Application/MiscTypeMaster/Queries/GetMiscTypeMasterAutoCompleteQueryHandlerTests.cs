using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<FAM.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { MiscTypeMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("mt"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "mt" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("xyz"))
                .ReturnsAsync(new List<FAM.Domain.Entities.MiscTypeMaster>());

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { MiscTypeMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("mt"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "mt" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
