using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetExistingVendorDetails;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetAmc.Queries
{
    public sealed class GetExistingVendorDetailsQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IAssetAmcQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetExistingVendorDetailsQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsVendorList()
        {
            var entities = new List<ExistingVendorDetails> { new() };
            var dtos = new List<GetExistingVendorDetailsDto> { new() };

            _mockRepo
                .Setup(r => r.GetVendorDetails("UNIT1", "V001"))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<GetExistingVendorDetailsDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetExistingVendorDetailsQuery { OldUnitCode = "UNIT1", VendorCode = "V001" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetVendorDetails("UNIT1", "V001"))
                .ReturnsAsync(new List<ExistingVendorDetails>());
            _mockMapper
                .Setup(m => m.Map<List<GetExistingVendorDetailsDto>>(It.IsAny<object>()))
                .Returns(new List<GetExistingVendorDetailsDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetExistingVendorDetailsQuery { OldUnitCode = "UNIT1", VendorCode = "V001" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
