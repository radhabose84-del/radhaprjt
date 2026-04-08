using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetServiceEntrySheetByIdHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetServiceEntrySheetByIdHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockRepo
                .Setup(r => r.GetSesByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceEntrySheetDetailDto.SesDto?)null);

            var result = await CreateSut().Handle(
                new GetServiceEntrySheetByIdQuery { SesId = 99 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetServiceEntrySheetByIdQuery { SesId = 42 };
            query.SesId.Should().Be(42);
        }
    }
}
