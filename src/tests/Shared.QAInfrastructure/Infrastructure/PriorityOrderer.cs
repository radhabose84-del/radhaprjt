using Xunit.Abstractions;
using Xunit.Sdk;

namespace Shared.QAInfrastructure.Infrastructure;

public sealed class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var sorted = new SortedDictionary<int, List<TTestCase>>();

        foreach (var tc in testCases)
        {
            var priority = tc.TestMethod.Method
                .GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName!)
                .FirstOrDefault()
                ?.GetNamedArgument<int>("Priority") ?? 0;

            if (!sorted.ContainsKey(priority))
                sorted[priority] = new List<TTestCase>();

            sorted[priority].Add(tc);
        }

        foreach (var list in sorted.Values)
            foreach (var testCase in list)
                yield return testCase;
    }
}
