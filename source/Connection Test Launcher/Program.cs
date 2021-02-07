using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace Launcher
{
   class Program
   {
      static void Main(string[] args)
      {

         var param = default(CommandLineArgs);
         try {
            param = new CommandLineArgs(args);
         } catch ( ArgumentNullException ) {
            return;
         }

         var fi = new FileInfo(Assembly.GetEntryAssembly().Location);
         var thisEXE = fi.Name.TrimEnd((char[])fi.Extension.ToCharArray());

         Process process;
         ProcessStartInfo psi;
         try {
            // Launch the 64-bit version
            process = new Process();
            psi = new ProcessStartInfo(String.Format("{0}{1}_x64.exe", AppDomain.CurrentDomain.BaseDirectory, thisEXE));
            psi.Arguments = String.Format("{0}/{1}@{2} {3}", param.Username, param.Password, param.Database, String.Join(" ", param.Args));
            psi.UseShellExecute = false;
            process.StartInfo = psi;
            process.Start();
            process.WaitForExit();
         } catch ( Exception ) {
            Console.WriteLine(new string('*', 60));
            Console.WriteLine("x64 not available");
            Console.WriteLine(new string('*', 60));
         }

         // Launch the 32-bit version
         process = new Process();
         psi = new ProcessStartInfo(String.Format("{0}{1}_x86.exe", AppDomain.CurrentDomain.BaseDirectory, thisEXE));
         psi.Arguments = String.Format("{0}/{1}@{2} {3}", param.Username, param.Password, param.Database, String.Join(" ", param.Args));
         psi.UseShellExecute = false;
         process.StartInfo = psi;
         process.Start();
         process.WaitForExit();


         var query = String.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", Process.GetCurrentProcess().Id);
         var results = new ManagementObjectSearcher("root\\CIMV2", query).Get().GetEnumerator();
         results.MoveNext();
         try {
            var parent = Process.GetProcessById(Convert.ToInt32(results.Current["ParentProcessId"]));
            if ( parent.ProcessName != "cmd" ) {
               // Do not wait for "any key" if program was started from command line
               Console.WriteLine("Press any key to exit");
               Console.ReadKey(true);
            }
         } catch {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
         }

      }
   }
}
