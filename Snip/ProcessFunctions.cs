#region File Information
/*
 * Copyright (C) 2007-2018 David Rudie
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
 */
#endregion

namespace Winter
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ProcessFunctions
    {
        #region Public Methods

        // This will get the process ID of the specificed executable
        public static int GetProcessId(string exeName)
        {
            Process[] processes = Process.GetProcessesByName(exeName);

            int processId = 0;

            if (processes.Length > 0)
            {
                processId = processes[0].Id;
            }

            foreach (var process in processes)
            {
                process.Dispose();
            }

            processes = null;

            return processId;
        }

        // https://www.rhyous.com/2010/04/30/how-to-get-the-parent-process-that-launched-a-c-application/
        public static int GetParentProcessId(int processId, string exeName)
        {
            int parentProcessId = 0;

            if (processId > 0)
            {
                IntPtr oHnd = UnsafeNativeMethods.CreateToolhelp32Snapshot(Enumerations.Snapshots.Process, 0);

                if (oHnd == IntPtr.Zero)
                {
                    return 0;
                }

                ProcessEntry oProcInfo = new ProcessEntry();

                oProcInfo.Size = (uint)Marshal.SizeOf(typeof(ProcessEntry));

                if (UnsafeNativeMethods.Process32First(oHnd, ref oProcInfo) == false)
                {
                    return 0;
                }

                do
                {
                    if (processId == oProcInfo.ProcessId)
                    {
                        Process parentProcess = Process.GetProcessById((int)oProcInfo.ParentProcessId);

                        if (parentProcess.ProcessName.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                        {
                            parentProcessId = (int)oProcInfo.ParentProcessId;
                        }
                        
                    }
                }
                while (parentProcessId == 0 && UnsafeNativeMethods.Process32Next(oHnd, ref oProcInfo));

                if (parentProcessId > 0)
                {
                    return parentProcessId;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }

        // This will get the base address of the specified process ID
        public static int GetBaseAddress(int processId)
        {
            Process process = Process.GetProcessById(processId);

            try
            {
                bool notNull = false;

                while (!notNull)
                {
                    if (process != null)
                    {
                        if (process.MainModule != null)
                        {
                            if (process.MainModule.BaseAddress != IntPtr.Zero)
                            {
                                notNull = true;
                            }
                        }
                    }
                }

                return process.MainModule.BaseAddress.ToInt32();
            }
            catch
            {
                return 0;

                throw;
            }
        }

        // This will get the module information of the specified module
        public static ModuleInfo GetModuleInfo(int processId, string moduleName)
        {
            if (!string.IsNullOrEmpty(moduleName))
            {
                Process process = Process.GetProcessById(processId);

                // Less than or equal to Windows XP/2000/2003
                if (Environment.OSVersion.Version.Major <= 5)
                {
                    try
                    {
                        bool notNull = false;

                        while (!notNull)
                        {
                            if (process != null)
                            {
                                if (process.Modules != null)
                                {
                                    notNull = true;
                                }
                            }
                        }

                        ProcessModuleCollection moduleCollection = process.Modules;

                        for (int i = 0; i < moduleCollection.Count; i++)
                        {
                            if (moduleCollection[i].ModuleName.ToUpperInvariant().StartsWith(moduleName.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
                            {
                                ModuleInfo moduleInfo;

                                moduleInfo.BaseOfDll = moduleCollection[i].BaseAddress;
                                moduleInfo.SizeOfImage = moduleCollection[i].ModuleMemorySize;
                                moduleInfo.EntryPoint = moduleCollection[i].EntryPointAddress;

                                return moduleInfo;
                            }
                        }
                    }
                    catch
                    {
                        return new ModuleInfo();

                        throw;
                    }
                }
                else
                {
                    try
                    {
                        bool notNull = false;

                        while (!notNull)
                        {
                            if (process != null)
                            {
                                notNull = true;
                            }
                        }

                        IntPtr[] moduleHandles = new IntPtr[1024];

                        GCHandle handleGc = GCHandle.Alloc(moduleHandles, GCHandleType.Pinned);
                        // IntPtr modules_p = handleGc.AddrOfPinnedObject();

                        // int size = Marshal.SizeOf(typeof(IntPtr)) * moduleHandles.Length;

                        UnsafeNativeMethods.IsWow64Process(process.Handle, out bool is32Bit);

                        Enumerations.ModuleFilter moduleFilter;

                        if (is32Bit)
                        {
                            moduleFilter = Enumerations.ModuleFilter.X32Bit;
                        }
                        else
                        {
                            moduleFilter = Enumerations.ModuleFilter.X64Bit;
                        }

                        if (UnsafeNativeMethods.EnumProcessModulesEx(process.Handle, moduleHandles, 1024, out int requiredSize, moduleFilter))
                        {
                            foreach (IntPtr module in moduleHandles)
                            {
                                StringBuilder sb = new StringBuilder(1024);

                                int moduleLength = UnsafeNativeMethods.GetModuleBaseName(process.Handle, module, sb, sb.Capacity);

                                if (moduleLength > 0)
                                {
                                    if (sb.ToString().ToUpperInvariant() == moduleName.ToUpperInvariant())
                                    {
                                        IntPtr pointer = IntPtr.Zero;
                                        pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ModuleInfo)));

                                        ModuleInfo moduleInfo;

                                        UnsafeNativeMethods.GetModuleInformation(process.Handle, module, pointer, ModuleInfo.SizeOf);

                                        moduleInfo = (ModuleInfo)Marshal.PtrToStructure(pointer, typeof(ModuleInfo));

                                        Marshal.FreeHGlobal(pointer);

                                        return moduleInfo;
                                    }
                                }
                            }
                        }

                        handleGc.Free();
                    }
                    catch
                    {
                        return new ModuleInfo();

                        throw;
                    }
                }

                return new ModuleInfo();
            }

            return new ModuleInfo();
        }

        // Reads from a process's memory.
        public static byte[] ReadMemory(IntPtr processHandle, IntPtr address, int bufferSize)
        {
            //IntPtr addressPtr = (IntPtr)address;

            byte[] buffer = new byte[bufferSize];

            UnsafeNativeMethods.ReadProcessMemory(processHandle, address, buffer, new IntPtr(bufferSize), IntPtr.Zero);

            return buffer;
        }

        // Opens an existing local process object. This method assumes you do not wish to inherit the specified process's handle.
        public static IntPtr OpenProcess(int processId, Enumerations.ProcessAccess desiredAccess)
        {
            return UnsafeNativeMethods.OpenProcess(desiredAccess, false, processId);
        }

        // This will close a handle to an open process
        public static bool CloseMemory(IntPtr processHandle)
        {
            return UnsafeNativeMethods.CloseHandle(processHandle);
        }

        // This will get the main window handle for the specified process
        public static IntPtr GetProcessWindowHandle(int processId)
        {
            if (processId > 0)
            {
                Process process = System.Diagnostics.Process.GetProcessById(processId);
                return process.MainWindowHandle;
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        // This will get the main window title for the specified process
        public static string GetProcessWindowTitle(int processId)
        {
            if (processId > 0)
            {
                // There's a possibility that processId is passed here before it's cleared so we need
                // to catch it in case processId is good here but the process is actually gone.
                try
                {
                    Process process = System.Diagnostics.Process.GetProcessById(processId);
                    return process.MainWindowTitle;
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        // This can be used to find strings of text in memory
        public static int FindInMemory(int processId, int address, int size, byte[] searchValue)
        {
            if (searchValue.Length > 0)
            {
                try
                {
                    Process process = Process.GetProcessById(processId);

                    if (process != null)
                    {
                        int foundAddress = 0;

                        int readSize = 1024 * 64;

                        for (int j = address; j < (address + size); j += readSize)
                        {
                            ManagedWindowsFunctions.ProcessMemoryChunk memoryChunk = new ManagedWindowsFunctions.ProcessMemoryChunk(process, (IntPtr)j, readSize + searchValue.Length);

                            byte[] chunk = memoryChunk.Read();

                            for (int k = 0; k < chunk.Length - searchValue.Length; k++)
                            {
                                bool foundOffset = true;

                                for (int l = 0; l < searchValue.Length; l++)
                                {
                                    if (chunk[k + l] != searchValue[l])
                                    {
                                        foundOffset = false;

                                        break;
                                    }
                                }

                                if (foundOffset)
                                {
                                    foundAddress = k + j;

                                    break;
                                }
                            }

                            memoryChunk.Dispose();

                            if (foundAddress != 0)
                            {
                                break;
                            }
                        }

                        return foundAddress;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }

            return 0;
        }

        #endregion

        #region Public Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct ModuleInfo
        {
            public static readonly int SizeOf;

            public IntPtr BaseOfDll;
            public int SizeOfImage;
            public IntPtr EntryPoint;

            static ModuleInfo()
            {
                SizeOf = Marshal.SizeOf(typeof(ModuleInfo));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProcessEntry
        {
            public uint Size;
            private uint NotUsed0;
            public uint ProcessId;
            private IntPtr NotUsed1;
            private uint NotUsed2;
            public uint Threads;
            public uint ParentProcessId;
            public int BasePriority;
            private uint NotUsed3;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string ExeFile;
        };

        #endregion
    }

    public static class Enumerations
    {
        #region Enumerations

        // This enum contains the flags for standard access rights
        public enum StandardRight : long
        {
            // No rights
            None = 0x00000000,

            // The right to delete the object.
            Delete = 0x00010000,

            // The right to read data from the security descriptor of the object, not including the data in the SACL.
            ReadControl = 0x00020000,

            // The right to modify the discretionary access-control list (DACL) in the object security descriptor.
            WriteDac = 0x00040000,

            // The right to assume ownership of the object. The user must be an object trustee. The user cannot transfer the ownership to other users.
            WriteOwner = 0x00080000,

            // The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
            Synchronize = 0x00100000,

            // I could not find documentation on this.
            Required = 0x000f0000,

            // I could not find documentation on this.
            Read = ReadControl,

            // I could not find documentation on this.
            Write = ReadControl,

            // I could not find documentation on this.
            Execute = ReadControl,

            // I could not find documentation on this.
            All = 0x001f0000,

            // I could not find documentation on this.
            SpecificRightsAll = 0x0000ffff,

            // The right to get or set the SACL in the object security descriptor.
            AccessSystemSecurity = 0x01000000,

            // I could not find documentation on this.
            MaximumAllowed = 0x02000000,

            // The right to read permissions on this object, read all the properties on this object, list this object name when the parent container is listed, and list the contents of this object if it is a container.
            GenericRead = 0x80000000,

            // The right to read permissions on this object, write all the properties on this object, and perform all validated writes to this object.
            GenericWrite = 0x40000000,

            // The right to read permissions on, and list the contents of, a container object.
            GenericExecute = 0x20000000,

            // The right to create or delete child objects, delete a subtree, read and write properties, examine child objects and the object itself, add and remove the object from the directory, and read or write with an extended right.
            GenericAll = 0x10000000
        }

        // This enum contains the flags for process access
        public enum ProcessAccess : long
        {
            None = 0x0000,
            Terminate = 0x0001,
            CreateThread = 0x0002,
            SetSessionId = 0x0004,
            VMOperation = 0x0008,
            VMRead = 0x0010,
            VMWrite = 0x0020,
            VMAll = 0x0038, // VmOperation | VmRead | VmWrite
            DupHandle = 0x0040,
            CreateProcess = 0x0080,
            SetQuota = 0x0100,
            SetInformation = 0x0200,
            QueryInformation = 0x0400,
            SetPort = 0x0800,
            SuspendResume = 0x0800,
            QueryLimitedInformation = 0x1000,

            // Should be 0x1fff on Vista but is 0xfff for backwards compatibility
            All = StandardRight.Required | StandardRight.Synchronize | 0xfff
        }

        // This enum contains the flags for module filters
        public enum ModuleFilter : int
        {
            None = 0x00,
            X32Bit = 0x01,
            X64Bit = 0x02,
            All = 0x03
        }

        // This enum contains the flags for snapshot access
        [Flags]
        public enum Snapshots : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            All = (HeapList | Process | Thread | Module),
            Inherit = 0x80000000,
            None = 0x40000000
        }

        #endregion
    }
}
