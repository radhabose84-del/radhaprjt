// Functional workflow tests hit a live server and share the single `testsales` user
// (single-session-per-user). Disable cross-collection parallelization so only one
// session is active at a time.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
