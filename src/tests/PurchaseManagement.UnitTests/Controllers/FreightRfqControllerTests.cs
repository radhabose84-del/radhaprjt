using Contracts.Common;
using Contracts.Dtos.Lookups.Party;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.FreightRfq.Commands.ApproveFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.DeleteFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.RejectFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations;
using PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval;
using PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Application.FreightRfq.Queries.GetAllFreightRfq;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqById;
using PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqTransporters;
using PurchaseManagement.Application.FreightRfq.Queries.GetPendingPoReferences;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class FreightRfqControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private FreightRfqController CreateSut() => new(_mockMediator.Object);

        private static ApiResponseDTO<int> Ok(int id = 1) => new() { IsSuccess = true, Message = "ok", Data = id };

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllFreightRfqQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FreightRfqListDto>>
                {
                    IsSuccess = true,
                    Data = new List<FreightRfqListDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            (await CreateSut().GetAllAsync(1, 10)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFreightRfqByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(FreightRfqBuilders.ValidDto());

            (await CreateSut().GetByIdAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task PendingPoReferences_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPendingPoReferencesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PoReferenceLookupDto>());

            (await CreateSut().GetPendingPoReferencesAsync("PO")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Transporters_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetFreightRfqTransportersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransporterLookupDto>());

            (await CreateSut().GetTransportersAsync("blue")).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            (await CreateSut().CreateAsync(FreightRfqBuilders.ValidCreateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            (await CreateSut().UpdateAsync(FreightRfqBuilders.ValidUpdateCommand())).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SaveQuotations_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SaveFreightRfqQuotationsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            (await CreateSut().SaveQuotationsAsync(FreightRfqBuilders.ValidSaveQuotationsCommand()))
                .Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SubmitForApproval_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<SubmitFreightRfqForApprovalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            var cmd = new SubmitFreightRfqForApprovalCommand { FreightRfqId = 1, SelectedQuotationId = 2 };
            (await CreateSut().SubmitForApprovalAsync(cmd)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Approve_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<ApproveFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            (await CreateSut().ApproveAsync(new ApproveFreightRfqCommand { FreightRfqId = 1 }))
                .Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Reject_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<RejectFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Ok());

            (await CreateSut().RejectAsync(new RejectFreightRfqCommand { FreightRfqId = 1, ApprovalRemarks = "no" }))
                .Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            (await CreateSut().DeleteAsync(1)).Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteFreightRfqCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteFreightRfqCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
