// QA tests hit a live server and all share the single `testsales` user, which the
// server enforces as single-session-per-user. Running collections in parallel makes
// their deactivate-session + login calls clobber each other (400 "already logged in"
// and connection resets). Disable cross-collection parallelization so only one
// `testsales` session is active at any moment.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
