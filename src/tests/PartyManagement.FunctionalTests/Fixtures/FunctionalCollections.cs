namespace PartyManagement.FunctionalTests.Fixtures;

// One collection per workflow story; each shares one QAServerFixture across its ordered steps.

[CollectionDefinition("US-PTY-01-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PTY-02-PartyOnboarding")]
public sealed class PartyOnboardingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PTY-03-AuditAndGst")]
public sealed class AuditAndGstCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-PTY-04-BrokerGinnerLocationStation")]
public sealed class BrokerGinnerLocationStationCollection : ICollectionFixture<QAServerFixture> { }
