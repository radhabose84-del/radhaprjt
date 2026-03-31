using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Presentation.Controllers;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Controllers
{
    public sealed class PartyMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PartyMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePartyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(PartyMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreatePartyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(PartyMasterBuilders.ValidCreateCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreatePartyMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePartyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(PartyMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePartyMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GetPartyMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<GetPartyMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllPartyMasterAsync(1, 10, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartyMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyMasterDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFoundResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPartyMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById.PartyMasterDto)!);

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
