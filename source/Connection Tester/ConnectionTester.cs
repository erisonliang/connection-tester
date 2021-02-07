using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Data.Common;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using Launcher;
using System.Linq;
using System.Xml;

static class ConnectionTester
{

   private static string db;
   private static string user;
   private static string pw;
   private static bool wait = false;
   private static bool connectionString = false;

   private static List<string> loadedDLL = new List<string>();
   private static string printSpecial; // = @"SELECT GLOBAL_NAME FROM GLOBAL_NAME";

   public static void Main(string[] args)
   {

      var newArgs = default(List<string>);
      var param = default(CommandLineArgs);

      try {
         param = new CommandLineArgs(args);
      } catch ( ArgumentNullException ) {
         return;
      }

      if ( String.IsNullOrEmpty(param.Database) )
         Environment.Exit(0);

      var runProvider = default(Dictionary<CommandLineArgs.EnumProvider, bool>);
      db = param.Database;
      user = param.Username;
      pw = param.Password;
      runProvider = param.Provider;
      wait = param.Wait;
      newArgs = param.Args;
      connectionString = param.ConnectionString;

      Console.WriteLine(String.Empty);
      Console.WriteLine("Running Test on {0}", Environment.Is64BitProcess ? "64 bit" : "32 bit");

      bool runTest = false;
      Console.WriteLine("\n" + new string('*', 60) + "\n");
      try {

         if ( runProvider[CommandLineArgs.EnumProvider.ODP] ) {
            runTest = true;

            var providerName = "Oracle Data Provider for .NET";
            Console.WriteLine("Try to connect via \"{0}\"\n", providerName);

            // Determine Oracle Client version
            var oraFileVersion = OracleClientVersion();
            double oraVersion = 0.0;
            if ( oraFileVersion != null )
               oraVersion = oraFileVersion.FileMajorPart + (double)oraFileVersion.FileMinorPart / 10;

            var oraVersions = new List<Version>();
            var str = new DbConnectionStringBuilder(false);
            str.Add("Data Source", db);
            str.Add("User ID", user);
            str.Add("Password", pw);

            bool allVersions = false;
            var xmlDoc = new XmlDocument();
            try {
               xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
               var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
               nsmgr.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");

               var DataAccess = xmlDoc.SelectSingleNode("//ns:assemblyIdentity[@name='Oracle.DataAccess']/parent::node()/ns:publisherPolicy", nsmgr);
               allVersions = DataAccess.Attributes["apply"].Value == "no";
            } catch { }

            loadedDLL.Clear();
            if ( oraFileVersion != null ) {
               if ( oraVersion <= 11.1 ) {
                  // Connect with Oracle ODP.NET version 1.x
                  oraVersions.Clear();
                  if ( allVersions ) {
                     oraVersions.Add(new Version(1, 100, 2));
                     oraVersions.Add(new Version(1, 111, 6));
                  } else {
                     oraVersions.Add(new Version(1, 100));
                     oraVersions.Add(new Version(1, 111));
                  }
                  Console.WriteLine("Try to connect via \"{0}\" (ODP.NET 1.x)\n", providerName);
                  ConOraODP(oraVersions, allVersions, "DataAccess", str);
               } else {
                  Console.ForegroundColor = ConsoleColor.Cyan;
                  Console.WriteLine("{0} (ODP.NET 1.x) not available in Oracle version {1}, it was supported only up to Oracle 11.1\n", providerName, oraVersion);
                  Console.ResetColor();
                  Console.WriteLine(new string('*', 60) + "\n");
               }

               if ( oraVersion >= 10.2 ) {
                  // Connect with Oracle ODP.NET version 2.x
                  oraVersions.Clear();
                  if ( allVersions ) {
                     oraVersions.Add(new Version(2, 102, 2));
                     oraVersions.Add(new Version(2, 111, 6));
                     oraVersions.Add(new Version(2, 111, 7));
                     oraVersions.Add(new Version(2, 112, 1));
                     oraVersions.Add(new Version(2, 112, 2));
                     oraVersions.Add(new Version(2, 112, 3));
                     oraVersions.Add(new Version(2, 112, 4));
                     oraVersions.Add(new Version(2, 121, 1));
                     oraVersions.Add(new Version(2, 121, 2));
                     oraVersions.Add(new Version(2, 122, 1));
                     oraVersions.Add(new Version(2, 122, 18));
                     oraVersions.Add(new Version(2, 122, 19));
                  } else {
                     oraVersions.Add(new Version(2, 102));
                     oraVersions.Add(new Version(2, 111));
                     oraVersions.Add(new Version(2, 112));
                     oraVersions.Add(new Version(2, 121));
                     oraVersions.Add(new Version(2, 122));
                  }
                  Console.WriteLine("Try to connect via \"{0}\" (ODP.NET 2.0)\n", providerName);
                  ConOraODP(oraVersions, allVersions, "DataAccess", str);
               } else {
                  Console.WriteLine("{0} (ODP.NET 2.0) not available in Oracle version {1}, it is supported only for Oracle version >= 10.2\n", providerName, oraVersion);
                  Console.WriteLine(new string('*', 60) + "\n");
               }

               if ( oraVersion >= 11.2 ) {
                  // Connect with Oracle ODP.NET version 4.x
                  oraVersions.Clear();
                  if ( allVersions ) {
                     oraVersions.Add(new Version(4, 112, 1));
                     oraVersions.Add(new Version(4, 112, 2));
                     oraVersions.Add(new Version(4, 112, 3));
                     oraVersions.Add(new Version(4, 112, 4));
                     oraVersions.Add(new Version(4, 121, 1));
                     oraVersions.Add(new Version(4, 121, 2));
                     oraVersions.Add(new Version(4, 122, 1));
                     oraVersions.Add(new Version(4, 122, 18));
                     oraVersions.Add(new Version(4, 122, 19));
                  } else {
                     oraVersions.Add(new Version(4, 112));
                     oraVersions.Add(new Version(4, 121));
                     oraVersions.Add(new Version(4, 122));
                  }
                  Console.WriteLine("Try to connect via \"{0}\" (ODP.NET 4.0)\n", providerName);
                  ConOraODP(oraVersions, allVersions, "DataAccess", str);
               } else {
                  Console.WriteLine("{0} (ODP.NET 4.0) not available in Oracle version {1}, it is supported only for Oracle version >= 11.2\n", providerName, oraVersion);
                  Console.WriteLine(new string('*', 60) + "\n");
               }
            }

            if ( allVersions && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Oracle.DataAccess.dll")) ) {
               // Connect with Oracle ODP.NET any version
               Console.WriteLine("Try to connect via \"{0}\" (from application directory)\n", providerName);
               oraVersions.Clear();
               oraVersions.Add(null);
               ConOraODP(oraVersions, false, "DataAccess", str);
            }

         }


         if ( runProvider[CommandLineArgs.EnumProvider.ODPM] ) {
            runTest = true;

            var providerName = "Oracle Data Provider for .NET Managed Driver";
            bool allVersions = false;

            var xmlDoc = new XmlDocument();
            try {
               xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
               var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
               nsmgr.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");

               var ManagedDataAccess = xmlDoc.SelectSingleNode("//ns:assemblyIdentity[@name='Oracle.ManagedDataAccess']/parent::node()/ns:publisherPolicy", nsmgr);
               allVersions = ManagedDataAccess.Attributes["apply"].Value == "no";
            } catch { }

            var oraVersions = new List<Version>();
            var str = new DbConnectionStringBuilder(false);
            str.Add("Data Source", db);
            str.Add("User ID", user);
            str.Add("Password", pw);

            loadedDLL.Clear();
            if ( allVersions ) {
               oraVersions.Add(new Version(4, 121, 1));
               oraVersions.Add(new Version(4, 121, 2));
               oraVersions.Add(new Version(4, 122, 1));
               oraVersions.Add(new Version(4, 122, 18));
               oraVersions.Add(new Version(4, 122, 19));
               if ( File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Oracle.ManagedDataAccess.dll")) )
                  oraVersions.Add(null);
            } else {
               oraVersions.Add(new Version(4, 121));
               oraVersions.Add(new Version(4, 122));
            }

            Console.WriteLine("Try to connect via \"{0}\" (ODP.NET 4.0 Managed Driver)\n", providerName);
            ConOraODP(oraVersions, allVersions, "ManagedDataAccess", str);
         }


         if ( runProvider[CommandLineArgs.EnumProvider.OleDB] ) {
            runTest = true;
            // Connect with Oracle OLE DB
            ConOleDB("OraOLEDB.Oracle", "Oracle Provider for OLE DB");

            if ( Environment.Is64BitProcess ) {
               Console.ForegroundColor = ConsoleColor.Cyan;
               Console.WriteLine("\"Microsoft OLE DB Provider for Oracle\" is only available for x86 architecture\n");
               Console.ResetColor();
               Console.WriteLine(new string('*', 60) + "\n");
            } else {
               ConOleDB("MSDAORA", "Microsoft OLE DB Provider for Oracle");
            }
         }

         if ( runProvider[CommandLineArgs.EnumProvider.Devart] ) {
            runTest = true;
            Console.WriteLine("Try to connect via \"dotConnect for Oracle\" from Devart (a.k.a. CoreLab)\n");
            ConDevartOracle();
            Console.WriteLine("Try to connect via \"dotConnect Universal\" from Devart (a.k.a. CoreLab)\n");
            ConDevartUniversal();
         }

         if ( runProvider[CommandLineArgs.EnumProvider.Progress] ) {
            runTest = true;
            Console.WriteLine("Try to connect via \"Progress DataDirect Connect for ADO.NET\"\n");
            // Progress drivers are crap. DataSource is not taken from OID (LDAP), tnsnames.ora must not contain domain
            ConProgressOracle();
         }


         if ( runProvider[CommandLineArgs.EnumProvider.CData] ) {
            runTest = true;
            Console.WriteLine("Try to connect via \"CData ADO.NET Provider for Oracle OCI\"\n");
            ConCDataOracle();
         }

         if ( runProvider[CommandLineArgs.EnumProvider.ADOforOracle] ) {
            runTest = true;
            ConADOforOracle();
         }

         if ( runProvider[CommandLineArgs.EnumProvider.ODBC] ) {
            runTest = true;
            foreach ( EnumVendor vendor in Enum.GetValues(typeof(EnumVendor)) ) {
               foreach ( OdbcDdriver aDriver in GetODBC(vendor, runProvider) )
                  ConODBC(aDriver);
            }
         }
      } catch ( System.Security.Authentication.InvalidCredentialException ) {
         Console.WriteLine("\nStop testing to prevent that your Oracle account gets locked!\n");
      }

      if ( !runTest ) {
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.WriteLine("You did not specify any driver to test!\n");
         Console.ResetColor();
      }

      var query = String.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", Process.GetCurrentProcess().Id);
      var results = new ManagementObjectSearcher("root\\CIMV2", query).Get().GetEnumerator();
      results.MoveNext();
      try {
         var parent = Process.GetProcessById(Convert.ToInt32(results.Current["ParentProcessId"]));
         if ( !( parent.ProcessName == "cmd" || parent.ProcessName == "ConnectionTester" ) ) {
            // Do not wait for "any key" if program was started from command line
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
         }
      } catch {
         Console.WriteLine("Press any key to exit");
         Console.ReadKey(true);
      }


   }

   /// <summary>
   /// Get location of ODBC drivers from Registry
   /// </summary>
   /// <param name="vendor">The vendor of the driver. Currently Microsoft, Oracle, Devart and Progress are supported</param>
   /// <returns>List of ODBC driver locations</returns>
   private static List<OdbcDdriver> GetODBC(EnumVendor vendor, Dictionary<CommandLineArgs.EnumProvider, bool> runProvider)
   {
      var res = new List<OdbcDdriver>();

      var rootKey = Registry.LocalMachine.OpenSubKey("Software\\ODBC\\ODBCINST.INI\\ODBC Drivers", false);
      foreach ( string aDriver in rootKey.GetValueNames() ) {
         var theKey = Registry.LocalMachine.OpenSubKey("Software\\ODBC\\ODBCINST.INI", false).OpenSubKey(aDriver, false);
         if ( Array.IndexOf(theKey.GetValueNames(), "Driver") > -1 ) {
            if ( vendor == EnumVendor.Oracle && theKey.GetValue("Driver").ToString().EndsWith("SQORA32.dll", StringComparison.CurrentCultureIgnoreCase) )
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "DBQ"));

            if ( vendor == EnumVendor.Microsoft && theKey.GetValue("Driver").ToString().EndsWith("msorcl32.dll", StringComparison.CurrentCultureIgnoreCase) )
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "Server"));

            if ( vendor == EnumVendor.Devart && theKey.GetValue("Driver").ToString().EndsWith("DevartODBCOracle.dll", StringComparison.CurrentCultureIgnoreCase) )
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "Server"));

            // Progress drivers are crap. DataSource is not taken from OID (LDAP), tnsnames.ora must not contain domain
            if ( vendor == EnumVendor.Progress && Regex.IsMatch(theKey.GetValue("Driver").ToString(), @"ivora\d+.dll$", RegexOptions.IgnoreCase) ) // 32-bit version
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "ServerName"));
            if ( vendor == EnumVendor.Progress && Regex.IsMatch(theKey.GetValue("Driver").ToString(), @"ddora\d+.dll$", RegexOptions.IgnoreCase) ) // 64-bit version
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "ServerName"));

            // Did not work: ERROR [IM006] [Microsoft][ODBC Driver Manager] Driver's SQLSetConnectAttr failed
            if ( vendor == EnumVendor.EasySoft && theKey.GetValue("Driver").ToString().EndsWith("esoracle.dll", StringComparison.CurrentCultureIgnoreCase) ) // Oracle Call Interface (OCI) 
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "Database", new Dictionary<string, string> { { "Server", db }, { "SID", db } }));
            if ( vendor == EnumVendor.EasySoft && theKey.GetValue("Driver").ToString().EndsWith("esorawp.dll", StringComparison.CurrentCultureIgnoreCase) ) // Wire Protocol (WP)
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "Database", new Dictionary<string, string> { { "Server", db }, { "SID", db } }));

            if ( vendor == EnumVendor.CData && theKey.GetValue("Driver").ToString().EndsWith("CData.ODBC.OracleOci.dll", StringComparison.CurrentCultureIgnoreCase) )
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "Data Source"));

            if ( vendor == EnumVendor.Simba && theKey.GetValue("Driver").ToString().EndsWith("OracleODBC_sb64.dll", StringComparison.CurrentCultureIgnoreCase) ) // 64-bit version
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "TNS"));
            if ( vendor == EnumVendor.Simba && theKey.GetValue("Driver").ToString().EndsWith("OracleODBC_sb32.dll", StringComparison.CurrentCultureIgnoreCase) ) // 32-bit version
               res.Add(new OdbcDdriver(aDriver, theKey.GetValue("Driver").ToString(), "TNS"));
         }
         theKey.Close();
      }
      rootKey.Close();
      if ( res.Count == 0 ) {
         var missingDriver = false;
         missingDriver |= vendor == EnumVendor.Progress && runProvider[CommandLineArgs.EnumProvider.Progress];
         missingDriver |= vendor == EnumVendor.Devart && runProvider[CommandLineArgs.EnumProvider.Devart];
         missingDriver |= vendor == EnumVendor.CData && runProvider[CommandLineArgs.EnumProvider.CData];
         missingDriver |= vendor == EnumVendor.Microsoft && !Environment.Is64BitProcess;
         missingDriver |= vendor == EnumVendor.Oracle;
         if ( missingDriver ) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Did not find any ODBC driver for \"{0}\"", vendor.ToString());
            Console.ResetColor();
         }
      }
      return res;
   }


   private static string ProviderLocation(string provider)
   {
      using ( var rootKey = Registry.ClassesRoot.OpenSubKey(provider, false) ) {
         if ( rootKey != null ) {
            var clsId = rootKey.OpenSubKey("Clsid", false).GetValue(null).ToString();
            using ( var theKey = Registry.ClassesRoot.OpenSubKey(String.Format("CLSID\\{0}\\InprocServer32", clsId), false) ) {
               if ( theKey != null ) {
                  return ExpandEnvironment(theKey.GetValue(null).ToString());
               }
            }
         }
         rootKey.Close();
      }
      return String.Empty;
   }


   /// <summary>
   /// Opens a connection using ODBC driver
   /// </summary>
   /// <remarks>For error "LoadLibraryFromPath: FQ Dll ... is not found, error: 0x7e" see: https://support.microsoft.com/en-us/kb/3126587</remarks>
   /// <param name="driver">The vendor of the driver.</param>
   private static void ConODBC(OdbcDdriver driver)
   {
      var str = new System.Data.Odbc.OdbcConnectionStringBuilder();
      str.Driver = driver.Provider;
      str.Add(driver.ServerProperty, db);
      str.Add("Uid", user);
      str.Add("Pwd", pw);
      if ( driver.OtherProperties != null ) {
         foreach ( var p in driver.OtherProperties )
            str.Add(p.Key, p.Value);
      }

      Console.WriteLine("Try to connect via ODBC Driver \"{0}\"\n", driver.Provider);
      if ( driver.Provider.IndexOf("Microsoft") >= 0 ) {
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.WriteLine("Note: This driver has been deprecated by Microsoft!");
         Console.ResetColor();
      }
      try {
         using ( var con = new System.Data.Odbc.OdbcConnection(str.ConnectionString) ) {
            Console.WriteLine("DLL-Location: {0}", ResolveWoW64(driver.Location));
            if ( !File.Exists(driver.Location) )
               throw new FileNotFoundException(String.Format("Could not find DLL at {0}", driver.Location));
            Console.WriteLine("Version: " + FileVersionInfo.GetVersionInfo(driver.Location).FileVersion);
            Connect(con);
         }
      } catch ( Exception ex ) {
         PrintError(ex);
         if ( ex is System.Security.Authentication.InvalidCredentialException )
            throw ex;
      }
      NextConnection();

   }

   /// <summary>
   /// Opens a connection using the OLE DB provider
   /// </summary>
   /// <remarks>For error "LoadLibraryFromPath: FQ Dll ... is not found, error: 0x7e" see: https://support.microsoft.com/en-us/kb/3126587</remarks>
   /// <param name="provider">The name of the OLE DB provider. Required for the ConnectionString</param>
   /// <param name="text">Human friendly name of the provider</param>
   private static void ConOleDB(string provider, string text)
   {
      var str = new System.Data.OleDb.OleDbConnectionStringBuilder();
      str.Provider = provider;
      str.DataSource = db;
      str.Add("User Id", user);
      str.Add("Password", pw);

      Console.WriteLine("Try to connect via \"{0}\"", text);
      if ( provider.Equals("msdaora", StringComparison.InvariantCultureIgnoreCase) ) {
         Console.ForegroundColor = ConsoleColor.Cyan;
         Console.WriteLine("Note: This provider has been deprecated by Microsoft!");
         Console.ResetColor();
      }
      Console.WriteLine(String.Empty);

      try {
         using ( var con = new System.Data.OleDb.OleDbConnection(str.ConnectionString) ) {
            var location = ProviderLocation(provider);
            Console.WriteLine("DLL-Location: {0}", ResolveWoW64(location));
            if ( !File.Exists(location) )
               throw new FileNotFoundException(String.Format("Could not find DLL at {0}", location));
            Console.WriteLine("Version: " + FileVersionInfo.GetVersionInfo(location).FileVersion);
            Connect(con);
         }
      } catch ( Exception ex ) {
         PrintError(ex);
         if ( ex is System.Security.Authentication.InvalidCredentialException )
            throw ex;
      }
      NextConnection();

   }


   /// <summary>
   /// Opens a connection uisng the Microsoft .NET Framework Data Provider for Oracle
   /// </summary>
   private static void ConADOforOracle()
   {
      var str = new System.Data.OracleClient.OracleConnectionStringBuilder();
      str.DataSource = db;
      str.UserID = user;
      str.Password = pw;

      Console.WriteLine("Try to connect via \"Microsoft .NET Framework Data Provider for Oracle\"");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("Note: This provider has been deprecated by Microsoft!\n");
      Console.ResetColor();

      try {
         using ( var con = new System.Data.OracleClient.OracleConnection(str.ConnectionString) ) {
            PrintAssemblyInfo(con.GetType().Assembly);
            Connect(con);
         }
      } catch ( Exception ex ) {
         PrintError(ex);
         if ( ex is System.Security.Authentication.InvalidCredentialException )
            throw ex;
      }
      NextConnection();

   }

   /// <summary>
   /// Opens a connection using the Oracle ODP.NET provider
   /// </summary>
   /// <param name="ver">Required version of the provider. This is used because version 1.x, 2.0 and 4.0 are not fully compatible to each other</param>
   /// <returns>True if connection was successful, otherwise false</returns>
   public static void ConOraODP(List<Version> versions, bool checkAll, string provider, DbConnectionStringBuilder str)
   {

      var DLL = default(Assembly);
      foreach ( Version ver in versions ) {
         try {
            string assembly = String.Empty;
            string versionNumber = String.Empty;
            if ( ver != null ) {
               versionNumber = ver.ToString() + ".*";
               if ( ver.Build == -1 )
                  versionNumber += ".*";
               assembly = String.Format("Oracle.{0}, Version={1}, Culture=neutral, PublicKeyToken=89b483f429c47342", provider, versionNumber);
            } else {
               assembly = String.Format("Oracle.{0}", provider);
            }

            var policyString = AppDomain.CurrentDomain.ApplyPolicy(assembly);
            var policyVersion = Regex.Match(policyString, @"Version=\d+\.\d+\.\d+.\d+").ToString().Replace("Version=", String.Empty);
            if ( !loadedDLL.Contains(policyVersion) ) {
               if ( ver != null )
                  Console.WriteLine("Try to load Oracle.{0}.dll (Version {1} -> Version {2})", provider, versionNumber, policyVersion);
               else
                  Console.WriteLine("Try to load Oracle.{0}.dll (Version not specified)", provider);

               DLL = Assembly.Load(assembly);
               var oraVer = FileVersionInfo.GetVersionInfo(DLL.Location).FileVersion;
               PrintAssemblyInfo(DLL);
               try {
                  loadedDLL.Add(oraVer);
                  var type = DLL.GetType(String.Format("Oracle.{0}.Client.OracleConnection", provider), true, false);
                  using ( var con = (DbConnection)Activator.CreateInstance(type, str.ConnectionString) ) {
                     Connect(con);
                  }
                  if ( !checkAll ) {
                     Console.WriteLine(String.Empty);
                     NextConnection();
                     return;
                  }
               } catch ( Exception ex ) {
                  PrintError(ex);
                  if ( ex is System.Security.Authentication.InvalidCredentialException )
                     throw ex;
                  if ( wait ) {
                     Console.WriteLine(String.Empty);
                     Console.WriteLine("Press any key to continue");
                     Console.ReadKey(true);
                  }

               }
               Console.WriteLine(String.Empty);
            } else {
               if ( ver != null )
                  Console.WriteLine("Version {0} -> {1} already done\n", versionNumber, policyVersion);
               else
                  Console.WriteLine("Version {0} already done\n", policyVersion);
            }
         } catch ( Exception ex ) {
            if ( ex is System.Security.Authentication.InvalidCredentialException )
               throw ex;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}\n", ex.Message);
            Console.ResetColor();
            if ( wait ) {
               Console.WriteLine(String.Empty);
               Console.WriteLine("Press any key to continue");
               Console.ReadKey(true);
            }
         }
      }

      NextConnection();

   }


   /// <summary>
   /// Opens a connection using provider dotConnect for Oracle from Devart
   /// </summary>
   private static void ConDevartOracle()
   {
      var str = new DbConnectionStringBuilder(false);
      str.Add("Data Source", db);
      str.Add("User ID", user);
      str.Add("Password", pw);

      var DLL = default(Assembly);
      try {
         // This DLL works if file Devart.Data.Oracle.dll is found in current application directory
         DLL = Assembly.Load("Devart.Data.Oracle");
      } catch { }

      if ( DLL == null ) {
         try {
            //Higher versions are covered by Policy files
            DLL = Assembly.Load("Devart.Data.Oracle, Version=5.0.*.*, Culture=neutral, PublicKeyToken=09af7300eec23701");
         } catch ( Exception ex ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}\n", ex.Message);
            Console.ResetColor();
         }
      }


      if ( DLL != null ) {
         PrintAssemblyInfo(DLL);
         try {
            var type = DLL.GetType("Devart.Data.Oracle.OracleConnection", true, false);
            using ( var con = (DbConnection)Activator.CreateInstance(type, str.ConnectionString) )
               Connect(con);
         } catch ( Exception ex ) {
            PrintError(ex);
            if ( ex is System.Security.Authentication.InvalidCredentialException )
               throw ex;
         }
      }
      NextConnection();

   }


   /// <summary>
   /// Opens a connection using provider dotConnect Universal from Devart
   /// </summary>
   private static void ConDevartUniversal()
   {
      var str = new DbConnectionStringBuilder(false);
      str.Add("Provider", "OracleClient");
      str.Add("Data Source", db);
      str.Add("User ID", user);
      str.Add("Password", pw);

      var DLL = default(Assembly);
      try {
         // This DLL works if file Devart.Data.Universal.dll is found in current application directory
         DLL = Assembly.Load("Devart.Data.Universal");
      } catch { }

      if ( DLL == null ) {
         try {
            //Higher versions are covered by Policy files
            DLL = Assembly.Load("Devart.Data.Universal, Version=2.10.*.*, Culture=neutral, PublicKeyToken=09af7300eec23701");
         } catch ( Exception ex ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}\n", ex.Message);
            Console.ResetColor();
         }
      }


      if ( DLL != null ) {
         PrintAssemblyInfo(DLL);
         try {
            var type = DLL.GetType("Devart.Data.Universal.UniConnection", true, false);
            using ( var con = (DbConnection)Activator.CreateInstance(type, str.ConnectionString) )
               Connect(con);
         } catch ( Exception ex ) {
            PrintError(ex);
            if ( ex is System.Security.Authentication.InvalidCredentialException )
               throw ex;
         }
      }
      NextConnection();

   }

   /// <summary>
   /// Opens a connection using provider dotConnect for Oracle from Devart
   /// </summary>
   private static void ConProgressOracle()
   {
      var str = new DbConnectionStringBuilder(false);
      str.Add("Data Source", db);
      str.Add("User ID", user);
      str.Add("Password", pw);

      var DLL = default(Assembly);
      try {
         // This DLL works if file DDTek.Oracle.dll is found in current application directory
         DLL = Assembly.Load("DDTek.Oracle");
      } catch { }

      if ( DLL == null ) {
         try {
            DLL = Assembly.Load("DDTek.Oracle, Version=4.2.*.*, Culture=neutral, PublicKeyToken=c84cd5c63851e072");
         } catch ( Exception ex ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}\n", ex.Message);
            Console.ResetColor();
         }
      }

      if ( DLL != null ) {
         PrintAssemblyInfo(DLL);
         try {
            var type = DLL.GetType("DDTek.Oracle.OracleConnection", true, false);
            using ( var con = (DbConnection)Activator.CreateInstance(type, str.ConnectionString) )
               Connect(con);
         } catch ( Exception ex ) {
            if ( ex.Source == "mscorlib" ) {
               // This is required  because constructor of DDTek.Oracle.OracleConnection evaluates the ConnectionString (not while Open() like all others!)
               try {
                  var type = DLL.GetType("DDTek.Oracle.OracleConnection", true, false);
                  using ( var con = (DbConnection)Activator.CreateInstance(type) )
                     Console.WriteLine("Connection Type: {0}", con.GetType().ToString());
               } catch { }
               if ( connectionString )
                  Console.WriteLine("ConnectionString: \"{0}\"", str.ConnectionString);
            }
            PrintError(ex);
            if ( ex is System.Security.Authentication.InvalidCredentialException )
               throw ex;
         }
      }
      NextConnection();

   }


   /// <summary>
   /// Opens a connection using provider CData ADO.NET Provider for Oracle OCI
   /// </summary>
   private static void ConCDataOracle()
   {

      var str = new DbConnectionStringBuilder(false);
      str.Add("Data Source", db);
      str.Add("User", user);
      str.Add("Password", pw);

      var DLL = default(Assembly);
      try {
         // This DLL works if file System.Data.CData.OracleOci is found in current application directory
         DLL = Assembly.Load("System.Data.CData.OracleOci");
      } catch { }

      if ( DLL == null ) {
         try {
            DLL = Assembly.Load("System.Data.CData.OracleOci, Version=19.*.*.*, Culture=neutral, PublicKeyToken=cdc168f89cffe9cf, processorArchitecture=MSIL");
         } catch ( Exception ex ) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0}\n", ex.Message);
            Console.ResetColor();
         }
      }


      if ( DLL != null ) {
         PrintAssemblyInfo(DLL);
         try {
            var type = DLL.GetType("System.Data.CData.OracleOci.OracleOciConnection", true, false);
            using ( var con = (DbConnection)Activator.CreateInstance(type, str.ConnectionString) )
               Connect(con);
         } catch ( Exception ex ) {
            PrintError(ex);
            if ( ex is System.Security.Authentication.InvalidCredentialException )
               throw ex;
         }
      }
      NextConnection();

   }



   /// <summary>
   /// Opens a connection and close it afterwards
   /// </summary>
   /// <param name="con">A generic DB-Connection, super-class for all Connetion objects</param>
   private static void Connect(DbConnection con)
   {
      try {
         Console.WriteLine("Connection Type: {0}", con.GetType().ToString());
         if ( connectionString )
            Console.WriteLine("ConnectionString: \"{0}\"", con.ConnectionString);
         con.Open();

         if ( printSpecial != null ) {
            Type type = null;
            // For "Microsoft OLE DB Provider for Oracle" and "Microsoft ODBC for Oracle" then NLS_LANG value has to be set properly.
            if ( con.GetType().Name == "OdbcConnection" ) {
               type = con.GetType().Assembly.GetType(String.Format("{0}.OdbcCommand", con.GetType().Namespace), true, false);
            } else if ( con.GetType().Name == "OleDbConnection" ) {
               type = con.GetType().Assembly.GetType(String.Format("{0}.OleDbCommand", con.GetType().Namespace), true, false);
            } else if ( con.GetType().Name == "OracleConnection" ) {
               type = con.GetType().Assembly.GetType(String.Format("{0}.OracleCommand", con.GetType().Namespace), true, false);
               //dynamic conOra = (object)con;
               //Console.WriteLine("ClientCharacterSet = {0}", conOra.GetSessionInfo().ClientCharacterSet);
            }
            if ( type != null )
               Console.WriteLine(( (DbCommand)Activator.CreateInstance((Type)type, printSpecial, con) ).ExecuteScalar());

         }

      } catch ( Exception ex ) {
         if ( ex.Message.Contains("ORA-01017") || ex.Message.Contains("ORA-1017") )
            throw new System.Security.Authentication.InvalidCredentialException("Wrong password");
         else
            throw ex;
      }
      PrintSuccess();

      if ( wait ) {
         Console.WriteLine(String.Empty);
         Console.WriteLine("Press any key to continue");
         Console.ReadKey(true);
      }
      con.Close();
   }

   /// <summary>
   /// Prints basic information of loaded assembly
   /// </summary>
   /// <param name="con">A generic DB-Connection, super-class for all Connetion objects</param>
   private static void PrintAssemblyInfo(Assembly dll)
   {
      var architecture = AssemblyName.GetAssemblyName(dll.Location).ProcessorArchitecture.ToString();
      Console.WriteLine("Assembly: {0}, processorArchitecture={1}", dll.FullName, architecture);
      if ( dll.GlobalAssemblyCache ) {
         Console.WriteLine("DLL-Location: Loaded from GAC");
      } else {
         Console.WriteLine("DLL-Location: {0}", ResolveWoW64(dll.Location));
      }
   }


   /// <summary>
   /// Message when connection was successful
   /// </summary>
   private static void PrintSuccess()
   {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine("Connection successful");
      Console.ResetColor();
   }

   /// <summary>
   /// Message when connection was not successful
   /// </summary>
   private static void PrintError(Exception ex)
   {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(ex.Message);
      if ( ex.InnerException != null )
         Console.WriteLine(ex.InnerException.Message);
      Console.ResetColor();
   }

   /// <summary>
   /// Message printed after each connection for better layout
   /// </summary>
   private static void NextConnection()
   {
      Console.WriteLine(String.Empty);
      Console.WriteLine(new string('*', 60));
      Console.WriteLine(String.Empty);
   }


   /// <summary>
   /// Returns version of installed Oracle Client
   /// </summary>
   /// <returns>FileVersion of oci.dll file</returns>
   private static FileVersionInfo OracleClientVersion()
   {
      // Determine Oracle Client version
      var oraFileVersion = default(FileVersionInfo);
      var folders = new List<String>(new string[] { AppDomain.CurrentDomain.BaseDirectory });
      folders.AddRange(Environment.GetEnvironmentVariable("PATH").Split(';'));
      for ( int i = 0; i < folders.Count; i++ ) {
         if ( File.Exists(Path.Combine(folders[i], "oci.dll")) ) {
            oraFileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(folders[i], "oci.dll"));
            if ( oraFileVersion.CompanyName.StartsWith("SecuPi", StringComparison.CurrentCultureIgnoreCase) ) {
               if ( File.Exists(Path.Combine(folders[i], "oci-orig.dll")) ) {
                  oraFileVersion = FileVersionInfo.GetVersionInfo(Path.Combine(folders[i], "oci-orig.dll"));
                  Console.WriteLine("Found {0} Version: {1}\n", ResolveWoW64(Path.Combine(folders[i], "oci-orig.dll")), oraFileVersion.FileVersion);
                  return oraFileVersion;
               }
            }
            Console.WriteLine("Found {0} Version: {1}\n", ResolveWoW64(Path.Combine(folders[i], "oci.dll")), oraFileVersion.FileVersion);
            return oraFileVersion;
         }
      }

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("Cannot find any \"oci.dll\" file. Please check your PATH Environment variable\n");
      Console.ResetColor();
      return oraFileVersion;

   }


   /// <summary>
   /// Expand %% environment variables to real value
   /// </summary>
   /// <param name="val"></param>
   /// <returns></returns>
   private static string ExpandEnvironment(string val)
   {
      string res = val;
      foreach ( Match aMatch in Regex.Matches(res, "%.+?%") )
         val = val.Replace(aMatch.Value, Environment.GetEnvironmentVariable(aMatch.Value.Replace("%", String.Empty)));
      return val;
   }

   /// <summary>
   /// Replaces "C:\Windows\system32\" by "C:\Windows\SysWOW64" in case of X86
   /// </summary>
   /// <param name="file"></param>
   /// <returns></returns>
   private static string ResolveWoW64(string file)
   {
      if ( !Environment.Is64BitProcess && ExpandEnvironment(file).StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.System), StringComparison.InvariantCultureIgnoreCase) ) {
         var ff = ExpandEnvironment(file).ToLowerInvariant();
         return String.Format("{0} -> {1}", file,
            ff.Replace(Environment.GetFolderPath(Environment.SpecialFolder.System).ToLowerInvariant(), Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)));
      } else {
         return file;
      }
   }

   struct OdbcDdriver
   {
      public string Provider { get; private set; }
      public string Location { get; private set; }
      public string ServerProperty { get; private set; }
      public Dictionary<string, string> OtherProperties { get; private set; }
      public OdbcDdriver(string provider, string location, string serverProperty)
         : this()
      {
         this.Provider = provider;
         this.Location = location;
         this.ServerProperty = serverProperty;
      }

      public OdbcDdriver(string provider, string location, string serverProperty, Dictionary<string, string> otherProperties)
         : this()
      {
         this.Provider = provider;
         this.Location = location;
         this.ServerProperty = serverProperty;
         this.OtherProperties = otherProperties;
      }

   }


   enum EnumVendor: byte
   {
      Oracle,
      Microsoft,
      Devart, // https://www.devart.com/odbc/oracle/download.html
      Progress, // https://www.progress.com/connectors/oracle-database
      EasySoft, // https://www.easysoft.com/products/data_access/odbc_oracle_driver/index.html
      CData, // https://www.cdata.com/drivers/oracledb/
      Simba // https://www.simba.com/drivers/oracle-odbc-jdbc/
   }




}

