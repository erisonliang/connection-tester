using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Launcher
{
   /// <summary>
   /// Common class to parse command-line arguments
   /// </summary>
   class CommandLineArgs
   {

      public Dictionary<EnumProvider, bool> Provider { private set; get; }
      public String Username { private set; get; }
      public String Password { private set; get; }
      public String Database { private set; get; }
      public bool Wait { private set; get; }
      public bool ConnectionString { private set; get; }
      public List<string> Args { private set; get; }

      public CommandLineArgs(string[] args)
      {
         Match re = default(Match);
         ConsoleKeyInfo cki = default(ConsoleKeyInfo);

         this.Args = new List<string>();
         this.Provider = new Dictionary<EnumProvider, bool>();
         foreach ( var p in Enum.GetValues(typeof(EnumProvider)).Cast<EnumProvider>() ) {
            this.Provider.Add(p, false);
         }

         this.Provider[EnumProvider.ODP] = true;
         this.Provider[EnumProvider.ODPM] = true;
         this.Provider[EnumProvider.OleDB] = true;
         this.Provider[EnumProvider.ODBC] = true;
         this.Provider[EnumProvider.ADOforOracle] = true;
         this.Provider[EnumProvider.OleDB] = true;

         foreach ( string arg in args ) {
            if ( arg.ToLower() == "-h" || arg.ToLower() == "/h" || arg == "/?" || arg.ToLower() == "-help" || arg.ToLower() == "--help" ) {
               var thisFile = new FileInfo(Assembly.GetEntryAssembly().Location).Name;
               Console.WriteLine("\nUsage:\n");
               Console.WriteLine(String.Format("{0} [<logon>] [+|-ODP] [+|-ADO] [+|-OleDB] [+|-ODBC] [+|-DevArt] [+|-Progress] [+|-CData] [+|-all] [wait] [cs]\n", thisFile));
               Console.WriteLine("   <logon> is <username>[/<password>]@<connect_identifier> (same as SQL*Plus)\n");
               Console.WriteLine("   +|-ODP: Test (+) or skip (-) Oracle Data Provider for .NET");
               Console.WriteLine("   +|-ODPM: Test (+) or skip (-) Oracle Data Provider for .NET Managed Driver");
               Console.WriteLine("   +|-ADO: Test (+) or skip (-) Microsoft .NET Framework Data Provider for Oracle");
               Console.WriteLine("   +|-OleDB: Test (+) or skip (-) OLE DB (Microsoft provider and Oracle provider)");
               Console.WriteLine("   +|-ODBC: Test (+) or skip (-) ODBC drivers (Microsoft, Oracle, Devart, Progress, Easysoft, Simba ODBC driver if installed)");
               Console.WriteLine("   +|-DevArt: Test (+) or skip (-) Devart dotConnect");
               Console.WriteLine("   +|-Progress: Test (+) or skip (-) Progress DataDirect Connect for ADO.NET");
               Console.WriteLine("   +|-CData: Test (+) or skip (-) CData ADO.NET Provider for Oracle OCI");
               Console.WriteLine("   +|-Simba: Test (+) or skip (-) Simba ODBC Driver (only relevant in combination with '+ODBC')");
               Console.WriteLine("   +|-all: Test all possible Oracle providers/drivers, evaluated before other switches");
               Console.WriteLine("   Default tested drivers are: ODP ODPM ADO OleDB ODBC\n");
               Console.WriteLine("   wait: Wait for key stroke after each connetion");
               Console.WriteLine("   cs: Print ConnectionString for each connection (Consider security, password is shown as clear text)\n");
               Console.WriteLine("   Switches are not case-sensitive\n");

               Console.WriteLine("Example: \n");
               Console.WriteLine(String.Format("{0} scott@ora1 -oledb -odp +devart", thisFile));
               Environment.Exit(0);
            } else if ( arg.Equals("+wait", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("wait", StringComparison.InvariantCultureIgnoreCase) ) {
               this.Args.Add(arg);
               this.Wait = true;
            } else if ( arg.Equals("+cs", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("cs", StringComparison.InvariantCultureIgnoreCase) ) {
               this.Args.Add(arg);
               this.ConnectionString = true;
            } else if ( arg.Equals("+all", StringComparison.InvariantCultureIgnoreCase) || arg.Equals("-all", StringComparison.InvariantCultureIgnoreCase) ) {
               this.Args.Add(arg);
               this.Provider[EnumProvider.OleDB] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.ODP] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.ODPM] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.ODBC] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.Devart] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.ADOforOracle] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.Progress] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.CData] = arg.Substring(0, 1) == "+";
               this.Provider[EnumProvider.Simba] = arg.Substring(0, 1) == "+";
            } else {
               re = Regex.Match(arg, "^(.+)/(.*)@(.+)$");
               if ( re.Success ) {
                  this.Database = re.Groups[3].Value;
                  this.Username = re.Groups[1].Value;
                  this.Password = re.Groups[2].Value;
               } else {
                  re = Regex.Match(arg, "^(.+)@(.+)$");
                  if ( re.Success ) {
                     this.Database = re.Groups[2].Value;
                     this.Username = re.Groups[1].Value;
                  }
               }
            }
         }

         // read all args again for proper handling of +/-all switch
         foreach ( string arg in args ) {
            if ( arg.StartsWith("-") || arg.StartsWith("+") ) {
               this.Args.Add(arg);
               if ( arg.Substring(1).Equals("oledb", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.OleDB] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("odp", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.ODP] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("odpm", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.ODPM] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("odbc", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.ODBC] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("devart", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.Devart] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("ado", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.ADOforOracle] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("progress", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.Progress] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("cdata", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.CData] = arg.Substring(0, 1) == "+";
               if ( arg.Substring(1).Equals("simba", StringComparison.InvariantCultureIgnoreCase) )
                  this.Provider[EnumProvider.Simba] = arg.Substring(0, 1) == "+";
            }
         }

         #region Ask for Database credentials if not provided by command-line
         if ( String.IsNullOrEmpty(this.Database) ) {
            Console.Write("Database or <ENTER> for exit: ");
            this.Database = Console.ReadLine();
            if ( String.IsNullOrEmpty(this.Database) )
               throw new ArgumentNullException();
         }

         if ( String.IsNullOrEmpty(this.Username) ) {
            Console.Write("User: ");
            this.Username = Console.ReadLine();
            if ( String.IsNullOrEmpty(this.Username) )
               throw new ArgumentNullException();
         }

         if ( String.IsNullOrEmpty(this.Password) ) {
            Console.Write("Password: ");
            do {
               cki = Console.ReadKey(true);
               if ( cki.Key == ConsoleKey.Escape )
                  return;
               this.Password += cki.KeyChar;
               Console.Write("*");
            } while ( !( cki.Key == ConsoleKey.Enter ) );
            if ( String.IsNullOrEmpty(this.Password) )
               throw new ArgumentNullException();
         }
         #endregion

         this.Password = this.Password.Trim();
         this.Username = this.Username.Trim();
         this.Database = this.Database.Trim();

      }


      public enum EnumProvider: byte
      {
         OleDB,
         ODP,
         ODPM,
         ODBC,
         Devart,
         ADOforOracle,
         Progress,
         CData,
         Simba
      }


   }

}
