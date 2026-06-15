// QA tests hit a live server and share the single `testsales` user, which the server
// enforces as single-session-per-user. Disable cross-collection parallelization so only
// one `testsales` session is active at any moment.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
