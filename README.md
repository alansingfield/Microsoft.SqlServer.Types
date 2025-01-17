EDIT: Looks like I didn't spot the latest prerelease from Microsoft:

https://www.nuget.org/packages/Microsoft.SqlServer.Types/160.900.6-rc0

There's no need for this fork.

# Microsoft.SqlServer.Types
a .NET Standard implementation of the spatial types in `Microsoft.SqlServer.Types` with support for Microsoft.Data.SqlClient 5.0.0.

## Sponsoring

This is a temporary fork to resolve a problem with Microsoft.Data.SqlClient; please refer to the original author's message:

"If you like this library and use it a lot, consider sponsoring me. Anything helps and encourages me to keep going."

"See here for details: https://github.com/sponsors/dotMorten "

### NuGet:

Install the package `alansingfield.Microsoft.SqlServer.Types` from NuGet.

### Examples


**Input parameter**

Assigning SqlGeometry or SqlGeography to a command parameter:

```cs
   command.Parameters.AddWithValue("@GeographyColumn", mySqlGeography);
   command.Parameters["@GeometryColumn"].UdtTypeName = "Geography";

   command.Parameters.AddWithValue("@GeographyColumn", mySqlGeometry);
   command.Parameters["@GeometryColumn"].UdtTypeName = "Geometry" 
```
The geometry will automatically be correctly serialized.

**Reading geometry and geography**

Use the common methods for getting fields of specific types:

```cs
   var geom1 = reader.GetValue(geomColumn) as SqlGeometry;
   var geom2 = reader.GetFieldValue<SqlGeometry>(geomColumn);
   var geom3 = SqlGeometry.Deserialize(reader.GetSqlBytes(geomColumn)); //Avoids any potential assembly-redirect issue. See https://docs.microsoft.com/en-us/previous-versions/sql/2014/sql-server/install/warning-about-client-side-usage-of-geometry-geography-and-hierarchyid?view=sql-server-2014#corrective-action
```

### Notes:

The spatial operations like intersection, area etc are not included here. You can perform these as part of your query instead and get them returned in a column.
