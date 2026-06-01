using QCManagement.Application.Common.Services;

namespace QCManagement.UnitTests.Application.QcInspection.Services
{
    public class InspectionEvaluatorTests
    {
        private readonly InspectionEvaluator _sut = new();

        [Theory]
        [InlineData("40", "PASS")]
        [InlineData("10", "PASS")]
        [InlineData("50", "PASS")]
        [InlineData("9", "FAIL")]
        [InlineData("51", "FAIL")]
        [InlineData("abc", "FAIL")]
        public void Rng_EvaluatesWithinRange(string actual, string expected)
        {
            _sut.Evaluate("RNG", actual, 10m, 50m, null, null).Should().Be(expected);
        }

        [Theory]
        [InlineData("10", "PASS")]
        [InlineData("15", "PASS")]
        [InlineData("9", "FAIL")]
        public void Min_EvaluatesMinimum(string actual, string expected)
        {
            _sut.Evaluate("MIN", actual, 10m, null, null, null).Should().Be(expected);
        }

        [Theory]
        [InlineData("50", "PASS")]
        [InlineData("10", "PASS")]
        [InlineData("51", "FAIL")]
        public void Max_EvaluatesMaximum(string actual, string expected)
        {
            _sut.Evaluate("MAX", actual, null, 50m, null, null).Should().Be(expected);
        }

        [Theory]
        [InlineData("Red", "PASS")]
        [InlineData("red", "PASS")]
        [InlineData("RED", "PASS")]
        [InlineData("Blue", "FAIL")]
        public void Fix_EvaluatesExactCaseInsensitive(string actual, string expected)
        {
            _sut.Evaluate("FIX", actual, null, null, "Red", null).Should().Be(expected);
        }

        [Theory]
        [InlineData("PASS", "PASS")]
        [InlineData("pass", "PASS")]
        [InlineData("FAIL", "FAIL")]
        [InlineData("fail", "FAIL")]
        [InlineData("maybe", "FAIL")]
        public void Pfl_UsesInspectorChoice(string actual, string expected)
        {
            _sut.Evaluate("PFL", actual, null, null, null, null).Should().Be(expected);
        }

        [Theory]
        [InlineData("B", "PASS")]
        [InlineData("b", "PASS")]
        [InlineData("D", "FAIL")]
        public void Lst_EvaluatesMembership(string actual, string expected)
        {
            _sut.Evaluate("LST", actual, null, null, null, "A|B|C").Should().Be(expected);
        }

        [Fact]
        public void Lst_EmptyAllowedValues_Fails()
        {
            _sut.Evaluate("LST", "A", null, null, null, null).Should().Be("FAIL");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void BlankActual_ReturnsNullIncomplete(string? actual)
        {
            _sut.Evaluate("RNG", actual, 10m, 50m, null, null).Should().BeNull();
        }

        [Fact]
        public void UnknownValidationType_Fails()
        {
            _sut.Evaluate("XXX", "anything", null, null, null, null).Should().Be("FAIL");
        }
    }
}
