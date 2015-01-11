﻿#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //
#endregion
    
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using System;
using Kernel.Hardware.Processes;

namespace Kernel
{
    /// <summary>
    /// The main class (containing the kernel entry point) for the Fling OS kernel.
    /// </summary>
    [Compiler.PluggedClass]
    public static class Kernel
    {
        /// <summary>
        /// Initialises static stuff within the kernel (such as calling GC.Init and BasicDebug.Init)
        /// </summary>
        [Compiler.NoDebug]
        static Kernel()
        {
            BasicConsole.Init();
            BasicConsole.Clear();

#if DEBUG
            Debug.BasicDebug.Init();
#endif
            FOS_System.GC.Init();

            BasicConsole.WriteLine();
        }

        /// <summary>
        /// Filled-in by the compiler.
        /// </summary>
        [Compiler.CallStaticConstructorsMethod]
        public static void CallStaticConstructors()
        {
        }

        /// <summary>
        /// Main kernel entry point
        /// </summary>
        [Compiler.KernelMainMethod]
        [Compiler.NoGC]
        [Compiler.NoDebug]
        static unsafe void Main()
        {
            //Necessary for exception handling stuff to work
            //  - We must have an intial, empty handler to always 
            //    "return" to.
            ExceptionMethods.AddExceptionHandlerInfo((void*)0, (void*)0);

            BasicConsole.WriteLine("Fling OS  Copyright (C) 2015  Edward Nutting");
            BasicConsole.WriteLine("This program comes with ABSOLUTELY NO WARRANTY;.");
            BasicConsole.WriteLine("This is free software, and you are welcome to redistribute it");
            BasicConsole.WriteLine("under certain conditions; See GPL V3 for details, a copy of");
            BasicConsole.WriteLine("which should have been provided with the executable.");
        
            BasicConsole.WriteLine("Fling OS Running...");

            try
            {
                Hardware.VirtMemManager.Init();
                Hardware.Devices.CPU.InitDefault();
                Hardware.Devices.Timer.InitDefault();
                Core.Processes.SystemCalls.Init();

                uint bpm = 140;
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.C4,
                    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.Silent,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.E4,
                    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.Silent,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.G4,
                    Hardware.Timers.PIT.MusicalNoteValue.Quaver,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.Silent,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.C5,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.Silent,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.G4,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                Hardware.Timers.PIT.ThePIT.PlayNote(
                    Hardware.Timers.PIT.MusicalNote.C5,
                    Hardware.Timers.PIT.MusicalNoteValue.Minim,
                    bpm);
                
                Process ManagedMainProcess = ProcessManager.CreateProcess(ManagedMain, "Managed Main", false);                
                Thread ManagedMain_MainThread = ((Thread)ManagedMainProcess.Threads[0]);
                Hardware.VirtMemManager.Unmap(ManagedMain_MainThread.State->ThreadStackTop - 4092);
                ManagedMainProcess.TheMemoryLayout.RemovePage((uint)ManagedMain_MainThread.State->ThreadStackTop - 4092);
                ManagedMain_MainThread.State->ThreadStackTop = GetKernelStackPtr();
                ManagedMain_MainThread.State->ESP = (uint)ManagedMain_MainThread.State->ThreadStackTop;
                ProcessManager.RegisterProcess(ManagedMainProcess, Scheduler.Priority.Normal);

                Scheduler.Init();

                // Busy wait until the scheduler interrupts us. 
                while (true)
                {
                    ;
                }
                // We will never return to this point since there is no way for the scheduler to point
                //  to it.
            }
            catch
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.WriteLine("Page fault exception unhandled!");
                }
                else
                {
                    BasicConsole.WriteLine("Startup error! " + ExceptionMethods.CurrentException.Message);
                }
                BasicConsole.WriteLine("Fling OS forced to halt!");
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.WriteLine("Cleaning up...");
            FOS_System.GC.Cleanup();

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.Write("GC num objs: ");
            BasicConsole.WriteLine(FOS_System.GC.NumObjs);
            BasicConsole.Write("GC num strings: ");
            BasicConsole.WriteLine(FOS_System.GC.NumStrings);
            BasicConsole.Write("Heap memory use: ");
            BasicConsole.Write(Heap.FBlock->used * Heap.FBlock->bsize);
            BasicConsole.Write(" / ");
            BasicConsole.WriteLine(Heap.FBlock->size);
            BasicConsole.SetTextColour(BasicConsole.default_colour);

            BasicConsole.WriteLine("Fling OS Ended.");

            //Necessary - no way of returning from this method since add exception info 
            //            at start cannot be "undone" so stack is "corrupted" if we try
            //            to "ret"
            //So we just halt the CPU for want of a better solution later when ACPI is 
            //implemented.
            ExceptionMethods.HaltReason = "End of Main";
            Halt(0xFFFFFFFF);
            //TODO: Proper shutdown method
        }

        /// <summary>
        /// Halts the kernel and halts the CPU.
        /// </summary>
        /// <param name="lastAddress">The address of the last line of code which ran or 0xFFFFFFFF.</param>
        [Compiler.HaltMethod]
        [Compiler.NoGC]
        public static void Halt(uint lastAddress)
        {
            try
            {
                Hardware.Devices.Keyboard.CleanDefault();
                Hardware.Devices.Timer.CleanDefault();
            }
            catch
            {
            }

            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.Write("Halt Reason: ");
            BasicConsole.WriteLine(ExceptionMethods.HaltReason);

            FOS_System.String LastAddressStr = "Last address: 0x        ";
            uint y = lastAddress;
            int offset = 23;
            #region Address
            while (offset > 15)
            {
                uint rem = y & 0xFu;
                switch (rem)
                {
                    case 0:
                        LastAddressStr[offset] = '0';
                        break;
                    case 1:
                        LastAddressStr[offset] = '1';
                        break;
                    case 2:
                        LastAddressStr[offset] = '2';
                        break;
                    case 3:
                        LastAddressStr[offset] = '3';
                        break;
                    case 4:
                        LastAddressStr[offset] = '4';
                        break;
                    case 5:
                        LastAddressStr[offset] = '5';
                        break;
                    case 6:
                        LastAddressStr[offset] = '6';
                        break;
                    case 7:
                        LastAddressStr[offset] = '7';
                        break;
                    case 8:
                        LastAddressStr[offset] = '8';
                        break;
                    case 9:
                        LastAddressStr[offset] = '9';
                        break;
                    case 10:
                        LastAddressStr[offset] = 'A';
                        break;
                    case 11:
                        LastAddressStr[offset] = 'B';
                        break;
                    case 12:
                        LastAddressStr[offset] = 'C';
                        break;
                    case 13:
                        LastAddressStr[offset] = 'D';
                        break;
                    case 14:
                        LastAddressStr[offset] = 'E';
                        break;
                    case 15:
                        LastAddressStr[offset] = 'F';
                        break;
                }
                y >>= 4;
                offset--;
            }

            #endregion
            BasicConsole.WriteLine(LastAddressStr);

            BasicConsole.SetTextColour(BasicConsole.default_colour);

            if (ExceptionMethods.CurrentException != null)
            {
                BasicConsole.SetTextColour(BasicConsole.error_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                if (ExceptionMethods.CurrentException is FOS_System.Exceptions.PageFaultException)
                {
                    BasicConsole.Write("Address: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).address);
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.PageFaultException)ExceptionMethods.CurrentException).errorCode);
                }
                else if (ExceptionMethods.CurrentException is FOS_System.Exceptions.DoubleFaultException)
                {
                    BasicConsole.Write("Code: ");
                    BasicConsole.WriteLine(((FOS_System.Exceptions.DoubleFaultException)ExceptionMethods.CurrentException).ErrorCode);
                }
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }

            BasicConsole.SetTextColour(BasicConsole.error_colour);
            BasicConsole.WriteLine("Kernel halting!");
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            PreReqs.Reset();
        }

        /// <summary>
        /// The actual main method for the kernel - by this point, all memory management, exception handling 
        /// etc has been set up properly.
        /// </summary>
        [Compiler.NoDebug]
        private static unsafe void ManagedMain()
        {
            BasicConsole.WriteLine(" Managed Main! ");
            BasicConsole.WriteLine(" > Executing normally...");

            //BasicConsole.WriteLine("Disabling scheduler...");
            //Scheduler.Disable();
   
            try
            {
                BasicConsole.WriteLine(" > Starting GC Cleanup task...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.GCCleanupTask.Main);

                BasicConsole.WriteLine(" > Starting Idle task...");
                ProcessManager.CurrentProcess.CreateThread(Core.Tasks.IdleTask.Main);

                BasicConsole.WriteLine(" > Starting Non-critical interrupts task...");
                ProcessManager.CurrentProcess.CreateThread(Hardware.Interrupts.NonCriticalInterruptsTask.Main);

                //BasicConsole.WriteLine("Initialising ATA...");
                //Hardware.ATA.ATAManager.Init();
                //BasicConsole.WriteLine("Initialising FS...");
                //FileSystemManager.Init();

                //BasicConsole.WriteLine("Creating & registering test process...");
                //Hardware.Processes.ProcessManager.RegisterProcess(
                //    Core.Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(
                //        File.Open("a:/test.bin"), false),
                //    Hardware.Processes.Scheduler.Priority.Normal);

                //BasicConsole.WriteLine("Creating & registering test 2 process...");
                //Hardware.Processes.ProcessManager.RegisterProcess(
                //    Core.Processes.DynamicLinkerLoader.LoadProcess_FromRawExe(
                //        File.Open("a:/test2.bin"), false),
                //    Hardware.Processes.Scheduler.Priority.Normal);

                //BasicConsole.WriteLine("Enabling scheduler...");
                //Scheduler.Enable();

                Hardware.Devices.Keyboard.InitDefault();
                Core.Console.InitDefault();
                Core.Shell.InitDefault();
                Core.Shell.Default.Execute();

                if (!Core.Shell.Default.Terminating)
                {
                    Core.Console.Default.WarningColour();
                    Core.Console.Default.WriteLine("Abnormal shell shutdown!");
                    Core.Console.Default.DefaultColour();
                }
                else
                {
                    Core.Console.Default.Clear();
                }
            }
            catch
            {
                OutputCurrentExceptionInfo();
            }
            
            BasicConsole.WriteLine();
            OutputDivider();
            BasicConsole.WriteLine();
            BasicConsole.WriteLine("End of managed main.");

            ExceptionMethods.HaltReason = "Managed main thread ended.";
            Halt(0);
        }

        /// <summary>
        /// Outputs the current exception information.
        /// </summary>
        [Compiler.NoDebug]
        private static void OutputCurrentExceptionInfo()
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);

            BasicConsole.SetTextColour(BasicConsole.default_colour);

            ExceptionMethods.CurrentException = null;
        }

        /// <summary>
        /// Outputs a divider line.
        /// </summary>
        private static void OutputDivider()
        {
            BasicConsole.WriteLine("---------------------");
        }

        public static void OutputAddressDetectedMethod(uint eip)
        {
            BasicConsole.WriteLine(((FOS_System.String)"Test address detected! EIP: ") + eip);
            BasicConsole.DelayOutput(5);
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Kernel")]
        private static unsafe void* GetManagedMainMethodPtr()
        {
            return null;
        }
        [Compiler.PluggedMethod(ASMFilePath=null)]
        private static unsafe byte* GetKernelStackPtr()
        {
            return null;
        }
    }
}
