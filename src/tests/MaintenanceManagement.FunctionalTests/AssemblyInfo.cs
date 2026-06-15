// Functional workflow tests hit a live server and share the single `testsales` user
// (single-session-per-user). Disable cross-collection parallelization.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
