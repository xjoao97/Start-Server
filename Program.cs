#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using log4net.Config;
using Oblivion.Core;

#endregion

namespace Oblivion
{
    public class Program
    {
        public const int ScClose = 0xF060;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessWorkingSetSize(IntPtr process,
            UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);


        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] args)
        {

            XmlConfigurator.Configure();

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;
            minimizeMemory();
            OblivionServer.Initialize();

            while (true)
            {
                Console.CursorVisible = true;
                if (Logging.DisabledState)
                    Console.Write("Oblivion> ");

                ConsoleCommandHandler.InvokeCommand(Console.ReadLine());
            }
        }

        public static void minimizeMemory()
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
                (UIntPtr) 0xFFFFFFFF, (UIntPtr) 0xFFFFFFFF);
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            var e = (Exception) args.ExceptionObject;
            Logging.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + e);
//            OblivionServer.PerformShutDown();
        }
    }
}