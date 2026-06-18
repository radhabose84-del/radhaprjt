namespace BackgroundService.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("BgMiscMasterCollection")] public sealed class BgMiscMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("BgMiscTypeMasterCollection")] public sealed class BgMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("WorkflowTypeCollection")] public sealed class WorkflowTypeCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ApprovalRuleCollection")] public sealed class ApprovalRuleCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ApprovalStepDetailCollection")] public sealed class ApprovalStepDetailCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ApprovalRequestCollection")] public sealed class ApprovalRequestCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationGroupCollection")] public sealed class NotificationGroupCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationGroupMemberCollection")] public sealed class NotificationGroupMemberCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationTemplateCollection")] public sealed class NotificationTemplateCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationConfigCollection")] public sealed class NotificationConfigCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationEventRuleCollection")] public sealed class NotificationEventRuleCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationWhatsAppGroupCollection")] public sealed class NotificationWhatsAppGroupCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("NotificationDetailCollection")] public sealed class NotificationDetailCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("SecurityCollection")] public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
