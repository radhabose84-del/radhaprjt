using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using MaintenanceManagement.Application.MRS.Queries;
using MaintenanceManagement.Application.MRS.Queries.GetCategory;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MaintenanceManagement.Application.MRS.Queries.GetPendingQty;
using MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter;
using MaintenanceManagement.Application.MRS.Queries.GetSubDepartment;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MRSControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MRSController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetDepartment_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetDepartmentbyIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MDepartmentDto>());

            var result = await CreateSut().GetDepartment("U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSubCostCenter_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubCostCenterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MSubCostCenterDto>());

            var result = await CreateSut().GetSubCostCenter("U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCategory_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MCategoryDto>());

            var result = await CreateSut().GetCategory("U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSubDepartment_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSubDepartmentQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MSubDepartment>());

            var result = await CreateSut().GetSubDepartment("U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateMRS_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateMRSCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateMRS(new HeaderRequest());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingIssue_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPendingQtyQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetPendingQtyDto { PendingQty = 5 });

            var result = await CreateSut().GetPendingIssue("U001", "ITEM001");
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
