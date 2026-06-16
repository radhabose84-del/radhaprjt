namespace UserManagement.FunctionalTests.Fixtures;

// One collection per workflow story. Each story is a single ordered test class that shares
// one QAServerFixture (one login, one run-unique EntityCode) across its steps. Keeping each
// story in its own collection lets stories run in parallel with each other while the steps
// WITHIN a story stay serialized via [TestPriority].

[CollectionDefinition("US-UM-01-OrgSetup")]
public sealed class OrgSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-UM-02-AccessControl")]
public sealed class AccessControlCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-UM-03-NavigationRbac")]
public sealed class NavigationRbacCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-UM-04-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-UM-05-SecurityPolicy")]
public sealed class SecurityPolicyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-UM-06-UserOnboarding")]
public sealed class UserOnboardingCollection : ICollectionFixture<QAServerFixture> { }
