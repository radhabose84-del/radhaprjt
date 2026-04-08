using AutoMapper;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTranferIssueApproval.Queries
{
    public sealed class GetAssetTransferIssueByIdQueryHandlerTests
    {
        private readonly Mock<IAssetTransferIssueApprovalQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetTransferIssueByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsList()
        {
            var entities = new List<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval> { new() };
            var dtos = new List<AssetTransferIssueByIdDto> { new() };

            _mockRepo
                .Setup(r => r.GetByAssetTransferIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<AssetTransferIssueByIdDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAssetTransferIssueByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetTransferIdAsync(99))
                .ReturnsAsync((List<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval>?)null!);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetTransferIssueByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockRepo
                .Setup(r => r.GetByAssetTransferIdAsync(99))
                .ReturnsAsync(new List<FAM.Domain.Entities.AssetMaster.AssetTransferIssueApproval>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetTransferIssueByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
