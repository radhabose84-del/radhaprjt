using AutoMapper;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            var entities = new List<FAM.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { MiscMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("MTYPE", "TestType"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { MiscTypeCode = "MTYPE", MiscTypeName = "TestType" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((List<FAM.Domain.Entities.MiscMaster>?)null!);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(
                new GetMiscMasterAutoCompleteQuery { MiscTypeCode = "XYZ", MiscTypeName = "Unknown" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<FAM.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { MiscMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("MTYPE", "TestType"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { MiscTypeCode = "MTYPE", MiscTypeName = "TestType" },
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
