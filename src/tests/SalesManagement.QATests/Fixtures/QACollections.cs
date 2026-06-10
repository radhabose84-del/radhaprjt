namespace SalesManagement.QATests.Fixtures;

[CollectionDefinition("SalesOrganisationCollection")]
public sealed class SalesOrganisationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
