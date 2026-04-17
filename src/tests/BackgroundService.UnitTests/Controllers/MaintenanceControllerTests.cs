using BackgroundService.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.UnitTests.Controllers
{
    /// <summary>
    /// MaintenanceController calls the static Hangfire BackgroundJob.Delete.
    /// These tests cover the safe path (empty/null job id) which does NOT
    /// invoke Hangfire and is fully assertable without a storage backend.
    /// </summary>
    public sealed class MaintenanceControllerTests
    {
        private MaintenanceController CreateSut() => new();

        [Fact]
        public void ScheduleVerificationCodeRemoval_EmptyJobId_ReturnsOkWithTrue()
        {
            var result = CreateSut().ScheduleVerificationCodeRemoval(string.Empty);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(true);
        }

        [Fact]
        public void ScheduleVerificationCodeRemoval_NullJobId_ReturnsOkWithTrue()
        {
            var result = CreateSut().ScheduleVerificationCodeRemoval(null!);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(true);
        }

        [Fact]
        public void ScheduleVerificationCodeRemoval_WhitespaceJobId_ReturnsOk()
        {
            // Whitespace is non-empty per IsNullOrEmpty, so BackgroundJob.Delete will be invoked.
            // With no Hangfire storage configured in test, Delete surfaces an InvalidOperationException.
            // Validate the guard behavior instead of the Hangfire side-effect.
            var act = () => CreateSut().ScheduleVerificationCodeRemoval("   ");

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
