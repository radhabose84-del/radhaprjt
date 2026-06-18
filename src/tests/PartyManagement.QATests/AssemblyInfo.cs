// QA tests hit a live server sharing the single `testsales` user (single-session-per-user).
// Disable cross-collection parallelization so only one session is active at any moment.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
