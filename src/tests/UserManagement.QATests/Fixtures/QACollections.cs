namespace UserManagement.QATests.Fixtures;

[CollectionDefinition("AccessPolicyCollection")]
public sealed class AccessPolicyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CountryCollection")]
public sealed class CountryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StateCollection")]
public sealed class StateCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CityCollection")]
public sealed class CityCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CurrencyCollection")]
public sealed class CurrencyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MiscMasterCollection")]
public sealed class MiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MiscTypeMasterCollection")]
public sealed class MiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepartmentCollection")]
public sealed class DepartmentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepartmentGroupCollection")]
public sealed class DepartmentGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DivisionCollection")]
public sealed class DivisionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UnitCollection")]
public sealed class UnitCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CompanyCollection")]
public sealed class CompanyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("FinancialYearCollection")]
public sealed class FinancialYearCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LanguageCollection")]
public sealed class LanguageCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserGroupCollection")]
public sealed class UserGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserRoleCollection")]
public sealed class UserRoleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AsyncCollection")]
public sealed class AsyncCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("IsolationCollection")]
public sealed class IsolationCollection : ICollectionFixture<TwoCompanyFixture> { }
