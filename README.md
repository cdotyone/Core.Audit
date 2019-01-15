Core.Audit
======================

##### Development

Automatically builds when committed to gitlab.

Creates nuget package Civic.Core.Audit available from nexus.civic360.com

By default the providers log in UTC time unless useLocalTime is set to true.

#### Default Configuration

```
<civic>
	<audit default="SqlAuditProvider" useLocalTime="false">
		<providers>
			<add name="SqlAuditProvider" type="Civic.Core.Audit.Providers.SqlAuditProvider" assembly="Civic.Core.Audit"/>
		</providers>
	</audit>
</civic>
```

#### Module Specific Remapping Audit Logging

By default the module code passed to the AuditManager will be used to determine the connection string used for the SqlAuditProvider.

Below the audit entries are from the "land" are redirected to the "civic" database connection string

```
<civic>
	<audit default="SqlAuditProvider">
		<providers>
			<add name="SqlAuditProvider" type="Civic.Core.Audit.Providers.SqlAuditProvider" assembly="Civic.Core.Audit">
				<add name="land" to="civic"/>
			</add>
		</providers>
	</audit>
</civic>
```

#### Remap All Default Audit Logging

This changes the default functionality of using the module as the connection string.

This can also be used in conjunction with Module Specific Remapping.

```
<civic>
	<audit default="SqlAuditProvider">
		<providers>
			<add name="SqlAuditProvider" type="Civic.Core.Audit.Providers.SqlAuditProvider" assembly="Civic.Core.Audit" default="civic"/>
		</providers>
	</audit>
</civic>
```
