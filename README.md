# Oracle Connection Tester
Testing various Oracle client installation and drivers in Windows

Installing an Oracle client can be a challenge. Sometimes you install the Oracle client and drivers but your application fails to connect to the Oracle database. This small tool helps you to find the problem in your Oracle client installation. 

## Common problems in Oracle client installations

- You mix 32-bit and 64-bit assemblies. 
   
   **All** assemblies of your application, i.e. the Oracle client, the driver and your application itself must run in the same architecture (i.e. 32-bit or 64-bit). 
   Note, the *"Oracle Data Provider for .NET Managed Driver"* does not depend on 32-bit/64-bit.
- Oracle client and *"Oracle Data Provider for .NET Driver"* are not the same version. The versions have to match **exactly**
- The required driver is not installed. 

   Your application may use a certain driver - you see there are many of them - but the required driver is not installed. ODBC or ODP.NET providers are not included in bare Oracle Instant client. 

   Note, in 12.1 and later ODP.NET is not added to GAC anymore, see [Installation Does Not Register Oracle Data Provider for .Net in the GAC](https://support.oracle.com/knowledge/Oracle%20Database%20Products/2272241_1.html). You need to register the assembly manually.


Java/JDBC based Oracle drivers are not in scope of this tool. 

Problems with alias resolution, typically related to `tnsnames.ora` file, are not in scope of this tool. 

Problems with ODBC DSN Data Source definitinos are not in scope of this tool. 


# How to use it

```
c:\>ConnectionTester -h

Usage:

ConnectionTester.exe [<logon>] [+|-ODP] [+|-ADO] [+|-OleDB] [+|-ODBC] [+|-DevArt] [+|-Progress] [+|-CData] [+|-all] [wait] [cs]

   <logon> is <username>[/<password>]@<connect_identifier> (same as SQL*Plus)

   +|-ODP: Test (+) or skip (-) Oracle Data Provider for .NET
   +|-ODPM: Test (+) or skip (-) Oracle Data Provider for .NET Managed Driver
   +|-ADO: Test (+) or skip (-) Microsoft .NET Framework Data Provider for Oracle
   +|-OleDB: Test (+) or skip (-) OLE DB (Microsoft provider and Oracle provider)
   +|-ODBC: Test (+) or skip (-) ODBC drivers (Microsoft, Oracle, Devart, Progress, Easysoft, Simba ODBC driver if installed)
   +|-DevArt: Test (+) or skip (-) Devart dotConnect
   +|-Progress: Test (+) or skip (-) Progress DataDirect Connect for ADO.NET
   +|-CData: Test (+) or skip (-) CData ADO.NET Provider for Oracle OCI
   +|-all: Test (+) or skip (-) all possible Oracle providers/drivers, evaluated before other switches
   Default tested drivers are: ODP ODPM ADO OleDB ODBC

   wait: Wait for key stroke after each connetion
   cs: Print ConnectionString for each connection (Consider security, password is shown as clear text)

   Switches are not case-sensitive

Example:

ConnectionTester.exe scott@ora1 -oledb -odp +devart
```

The `ConnectionTester.exe` is just wrapper for 32-bit and 64-bit call. Running
```
ConnectionTester.exe scott/tiger@ora1
```
is eqivalent to 
```
ConnectionTester_x64.exe scott/tiger@ora1
ConnectionTester_x86.exe scott/tiger@ora1
```

## Supported native drivers

- Oracle Provider for OLE DB
- Microsoft OLE DB Provider for Oracle (only for 32bit, [deprecated](https://msdn.microsoft.com/en-us/library/ms675851%28v=vs.85%29.aspx), does not work anymore with Oracle Client 18c or newer)
- Microsoft .NET Framework Data Provider for Oracle ([deprecated](https://docs.microsoft.com/de-de/archive/blogs/adonet/system-data-oracleclient-update))
- Oracle Data Provider for .NET (ODP.NET)
- Oracle Data Provider for .NET, Managed Driver (ODP.NET Managed Driver)
- dotConnect for Oracle from [Devart](https://www.devart.com/dotconnect/oracle/) (formerly known as OraDirect .NET from Core Lab)
- dotConnect Universal from Devart (uses deprecated System.Data.OracleClient)
- DataDirect Connect for ADO.NET from [Progress](https://www.progress.com/connectors/oracle-database)
- ADO.NET Provider for Oracle OCI from [CData](https://www.cdata.com/drivers/oracledb/)


## Supported ODBC drivers

- ODBC Driver from Oracle
- ODBC driver from Microsoft (only for 32bit, [deprecated](https://msdn.microsoft.com/en-us/library/ms713590%28v=vs.85%29.aspx), does not work anymore with Oracle Client 18c or newer)
- ODBC driver from [Devart](https://www.devart.com/odbc/oracle/)
- ODBC driver from [Progress](https://www.progress.com/odbc/oracle-database)
- ODBC Oracle Driver from [Easysoft](https://www.easysoft.com/products/data_access/odbc_oracle_driver/index.html)
- ODBC Oracle WP Driver from Easysoft
- ODBC Driver for Oracle OCI from [CData](https://www.cdata.com/drivers/oracledb/odbc/)
- ODBC Oracle Driver with SQL Connector from [Simba](https://www.simba.com/drivers/oracle-odbc-jdbc/)

All ODBC drviers are tested directly, i.e. "DSN-less". It only tests the actual ODBC driver, not the ODBC DSN Data Soucre. Thus no DSN entry is required in your ODBC Data Source Administrator.

## Testing different client versions

By default this tool test only one version of each Oracle driver. You may require to test different versions or Oracle client. Version of .NET assemblies loded from GAC are determined by policy files. 

In order to test **all** versions of *Oracle Data Provider for .NET* use the `.config` files (included in "ConnectionTester-Full.zip") and set `<publisherPolicy apply="no"/>` accordingly. Multi version testing is only available only for the *"Oracle Data Provider for .NET Driver"* and *"Oracle Data Provider for .NET Managed Driver"* providers.

In most use cases the .NET `.config` are not required. Release "ConnectionTester.zip" comes without them.



# Deploy assembly

Feel free to compile the source by yourself. Oracle based applications may run in 32-bit or 64-bit mode, this tool intents to test both.

You should build the solution only with `AnyCPU`. Visual Studio will create the files in solution root `bin` folder. 

- `ConnectionTester_x64.exe`: The 64-bit version
- `ConnectionTester_x86.exe`: The 32-bit version
- `ConnectionTester.exe`: A wrapper to run both versions with one call
- `ConnectionTester_x64.exe.config`: .NET config file for 64-bit version
- `ConnectionTester_x86.exe.config`: .NET config file for 32-bit version


File `ConnectionTester_x.exe` is just used temporarily while build.

All drivers and providers are loaded dynamically. In order to build the assembly your developing PC **does not require any Oracle client** installation. 

## Installation

This project provides only stand-alone `.exe`, no setup.
Purpose of this tool is just to **test** the Oracle client installation on your machine. Thus it does not perform any modifications on your system.









