// Functional tests hit a live server and share the single `testsales` session.
// Disable cross-collection parallelization so only one session is active at a time.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
