﻿#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
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

using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Processes;
using Kernel.Framework.Processes.Requests.Pipes;
using Kernel.Pipes.File;

namespace Kernel.FileSystems
{
    public class FileSystemAccessor : Object
    {
        #region Non-Static

        public uint RemoteProcessId;
        public int CmdPipeId;
        public int DataOutPipeId;
        public FileDataInpoint DataInPipe;
        public String[] MappingPrefixes;

        public FileSystemAccessor(uint ARemoteProcessId)
        {
            RemoteProcessId = ARemoteProcessId;

            BasicConsole.WriteLine("FileSystemAccessor > Connecting to: " + (String)RemoteProcessId);
            DataInPipe = new FileDataInpoint(RemoteProcessId, true);

            BasicConsole.WriteLine("FileSystemAccessor > Connected.");

            try
            {
                bool Found = false;

                int position = 0;
                while (!Found)
                {
                    if (SystemCalls.WaitSemaphore(CmdOutPipesSemaphoreId) == SystemCallResults.OK)
                    {
                        ulong IdPair = CmdOutPipes[position];
                        if ((uint)(IdPair >> 32) == RemoteProcessId)
                        {
                            CmdPipeId = (int)(IdPair & 0xFFFFFFFF);
                            Found = true;
                            BasicConsole.WriteLine("FileSystemAccessor > Got command output pipe id. " +
                                                   (String)CmdPipeId);
                        }
                        position++;
                    }
                }

                Found = false;
                position = 0;
                while (!Found)
                {
                    if (SystemCalls.WaitSemaphore(DataOutPipesSemaphoreId) == SystemCallResults.OK)
                    {
                        ulong IdPair = DataOutPipes[position];
                        if ((uint)(IdPair >> 32) == RemoteProcessId)
                        {
                            DataOutPipeId = (int)(IdPair & 0xFFFFFFFF);
                            Found = true;
                            BasicConsole.WriteLine("FileSystemAccessor > Got data output pipe id. " +
                                                   (String)DataOutPipeId);
                        }
                        position++;
                    }
                }
            }
            catch
            {
                BasicConsole.WriteLine("FileSystemAccessor > Error probing File controller!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
        }

        public FileSystemAccessor(uint ARemoteProcessId, String Mapping)
        {
            RemoteProcessId = ARemoteProcessId;

            BasicConsole.WriteLine("FileSystemAccessor > Connecting to: " + (String)RemoteProcessId);
            DataInPipe = new FileDataInpoint(RemoteProcessId, true);

            BasicConsole.WriteLine("FileSystemAccessor > Connected.");

            try
            {
                bool Found = false;

                int position = 0;
                while (!Found)
                {
                    if (SystemCalls.WaitSemaphore(CmdOutPipesSemaphoreId) == SystemCallResults.OK)
                    {
                        ulong IdPair = CmdOutPipes[position];
                        if ((uint)(IdPair >> 32) == RemoteProcessId)
                        {
                            CmdPipeId = (int)(IdPair & 0xFFFFFFFF);
                        }
                        position++;

                        BasicConsole.WriteLine("FileSystemAccessor > Got command output pipe id.");
                    }
                }

                Found = false;
                position = 0;
                if (SystemCalls.WaitSemaphore(DataOutPipesSemaphoreId) == SystemCallResults.OK)
                {
                    ulong IdPair = DataOutPipes[position];
                    if ((uint)(IdPair >> 32) == RemoteProcessId)
                    {
                        DataOutPipeId = (int)(IdPair & 0xFFFFFFFF);
                    }
                    position++;

                    BasicConsole.WriteLine("FileSystemAccessor > Got data output pipe id.");
                }

                MappingPrefixes = new String[1];
                MappingPrefixes[0] = Mapping;
            }
            catch
            {
                BasicConsole.WriteLine("FileSystemAccessor > Error probing File controller!");
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
            }
        }

        public void StatFS()
        {
            BasicConsole.WriteLine("FileSystemAccessor > Sending StatFS command...");
            CmdOutpoint.Send_StatFS(CmdPipeId);
            DataOutpoint.WriteString(DataOutPipeId, "");

            BasicConsole.WriteLine("FileSystemAccessor > Reading mapping prefixes...");
            MappingPrefixes = DataInPipe.ReadFSInfos(true);

            BasicConsole.Write("FileSystemAccessor > Got file system mappings: ");
            for (int j = 0; j < MappingPrefixes.Length; j++)
            {
                BasicConsole.Write(MappingPrefixes[j]);
            }
        }

        public List ListDir(String Path)
        {
            CmdOutpoint.Write_ListDir(CmdPipeId);
            DataOutpoint.WriteString(DataOutPipeId, Path);
            return DataInPipe.ReadString(true).Split('\n');
        }

        public void PrintListings(String Path)
        {
            List Listings = ListDir(Path);
            for (int k = 0; k < Listings.Count; k++)
            {
                String Name = (String)Listings[k];
                BasicConsole.Write(Name);
                if (k < Listings.Count - 1)
                {
                    BasicConsole.Write(", ");
                }
            }
            if (Listings.Count == 0)
            {
                BasicConsole.WriteLine("[NO LISTINGS]");
            }
            else
            {
                BasicConsole.WriteLine();
            }
        }

        #endregion

        #region Static

        public static bool Terminating = false;

        private static FileCmdOutpoint CmdOutpoint;
        private static FileDataOutpoint DataOutpoint;
        private static int CmdOutPipesSemaphoreId;
        private static int DataOutPipesSemaphoreId;
        private static UInt64List CmdOutPipes;
        private static UInt64List DataOutPipes;

        public static void Init()
        {
            CmdOutPipes = new UInt64List();
            DataOutPipes = new UInt64List();
            CmdOutpoint = new FileCmdOutpoint(PipeConstants.UnlimitedConnections);
            DataOutpoint = new FileDataOutpoint(PipeConstants.UnlimitedConnections, false);

            if (SystemCalls.CreateSemaphore(-1, out CmdOutPipesSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("FileSystemAccessor > Failed to create a semaphore! (1)");
            }
            if (SystemCalls.CreateSemaphore(-1, out DataOutPipesSemaphoreId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("FileSystemAccessor > Failed to create a semaphore! (2)");
            }

            uint ThreadId;
            if (SystemCalls.StartThread(WaitForFileCmdPipes, out ThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("FileSystemAccessor > Failed to create a thread! (1)");
            }
            if (SystemCalls.StartThread(WaitForFileDataPipes, out ThreadId) != SystemCallResults.OK)
            {
                BasicConsole.WriteLine("FileSystemAccessor > Failed to create a thread! (2)");
            }
        }

        private static void WaitForFileCmdPipes()
        {
            while (!Terminating)
            {
                uint InProcessId;
                int PipeId = CmdOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("FileSystemAccessor > Storage command output connected.");
                CmdOutPipes.Add(((ulong)InProcessId << 32) | (uint)PipeId);
                SystemCalls.SignalSemaphore(CmdOutPipesSemaphoreId);
            }
        }

        private static void WaitForFileDataPipes()
        {
            while (!Terminating)
            {
                uint InProcessId;
                int PipeId = DataOutpoint.WaitForConnect(out InProcessId);
                BasicConsole.WriteLine("FileSystemAccessor > Storage data output connected.");
                DataOutPipes.Add(((ulong)InProcessId << 32) | (uint)PipeId);
                SystemCalls.SignalSemaphore(DataOutPipesSemaphoreId);
            }
        }

        #endregion
    }
}