Core.Audit
======================

##### Development

Automatically builds when committed to gitlab.

Creates nuget package Core.Audit

By default the providers log in UTC time unless useLocalTime is set to true.

#### Default Configuration

```
<core>
	<audit default="SqlAuditProvider" useLocalTime="false">
		<providers>
			<add name="SqlAuditProvider" type="Core.Audit.Providers.SqlAuditProvider" assembly="Core.Audit"/>
		</providers>
	</audit>
</core>
```

#### Module Specific Remapping Audit Logging

By default the module code passed to the AuditManager will be used to determine the connection string used for the SqlAuditProvider.

Below the audit entries are from the "steve" are redirected to the "bob" database connection string

```
<core>
	<audit default="SqlAuditProvider">
		<providers>
			<add name="SqlAuditProvider" type="Core.Audit.Providers.SqlAuditProvider" assembly="Core.Audit">
				<add name="steve" to="bob"/>
			</add>
		</providers>
	</audit>
</core>
```

#### Remap All Default Audit Logging

This changes the default functionality of using the module as the connection string.

This can also be used in conjunction with Module Specific Remapping.

```
<core>
	<audit default="SqlAuditProvider">
		<providers>
			<add name="SqlAuditProvider" type="Core.Audit.Providers.SqlAuditProvider" assembly="Core.Audit" default="civic"/>
		</providers>
	</audit>
</core>
```
