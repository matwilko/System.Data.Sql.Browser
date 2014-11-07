System.Data.Sql.Browser
=======================
Lightweight interface to SQL Server Browser services, created to directly replace `System.Data.Sql.SqlDataSourceEnumerator` with a more responsive and type-safe API.

Direct implementation of [MC-SQLR: SQL Server Resolution Protocol](http://msdn.microsoft.com/en-us/library/cc219703.aspx)

##Usage
There are only two basic operations that this protocol provides: getting information about instances, and obtaining the Dedicated Administrator Connection port number.

### Instance information
You can obtain information about a specific instance by using `Browser.GetInstance`, information about all instances on a specific host by using `Browser.GetInstancesOn`, or information about all instances on the local network (within the same subnet) by using `Browser.GetInstances`.

Both `Browser.GetInstances` and `Browser.GetInstancesOn` use deferred execution, so that the request is only sent at the point at which the returned `IEnumerable<SqlInstance>` is enumerated. They will then yield instance information as it becomes available, rather than coalescing all the data until timeout is reached. This can allow UI to be much more responsive when enumerating network SQL Server instances than when using `System.Data.Sql.SqlDataSourceEnumerator`, which blocks until all information has been retrieved.

Enumeration is thread-safe, as each enumerator creates its own UdpClient with unique local port.

### Dedicated Administrator Connection (DAC) port
You can obtain the DAC port for a specific instance by using `Browser.GetDacPort`. Note that an instance may not necessarily have DAC enabled, in particular SQL Server Express editions, in which case, the call will throw an exception.
