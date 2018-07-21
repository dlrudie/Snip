#region File Information
/*
 * Copyright (C) 2006 Michael Schierl
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

namespace ManagedWindowsFunctions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// A chunk in another processes memory. Mostly used to allocate buffers
    /// in another process for sending messages to its windows.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class ProcessMemoryChunk : IDisposable
    {
        readonly Process process;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        readonly IntPtr location, hProcess;
        readonly int size;
        readonly bool free;

        /// <summary>
        /// Create a new memory chunk that points to existing memory.
        /// Mostly used to read that memory.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ProcessMemoryChunk(Process process, IntPtr location, int size)
        {
            this.process = process;
            this.hProcess = OpenProcess(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite, false, process.Id);
            ApiHelper.FailIfZero(this.hProcess);
            this.location = location;
            this.size = size;
            this.free = false;
        }

        private ProcessMemoryChunk(Process process, IntPtr hProcess, IntPtr location, int size, bool free)
        {
            this.process = process;
            this.hProcess = hProcess;
            this.location = location;
            this.size = size;
            this.free = free;
        }

        /// <summary>
        /// The process this chunk refers to.
        /// </summary>
        public Process Process { get { return this.process; } }

        /// <summary>
        /// The location in memory (of the other process) this chunk refers to.
        /// </summary>
        public IntPtr Location { get { return this.location; } }

        /// <summary>
        /// The size of the chunk.
        /// </summary>
        public int Size { get { return this.size; } }

        /// <summary>
        /// Allocate a chunk in another process.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alloc")]
        public static ProcessMemoryChunk Alloc(Process process, int size)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite, false, process.Id);
            IntPtr remotePointer = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)size,
                MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            ApiHelper.FailIfZero(remotePointer);
            return new ProcessMemoryChunk(process, hProcess, remotePointer, size, true);
        }

        /// <summary>
        /// Allocate a chunk in another process and unmarshal a struct
        /// there.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alloc")]
        public static ProcessMemoryChunk AllocStruct(Process process, object structure)
        {
            int size = Marshal.SizeOf(structure);
            ProcessMemoryChunk result = Alloc(process, size);
            result.WriteStructure(0, structure);
            return result;
        }

        /// <summary>
        /// Free the memory in the other process, if it has been allocated before.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "success")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2216:DisposableTypesShouldDeclareFinalizer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        public void Dispose()
        {
            if (this.free)
            {
                if (!VirtualFreeEx(this.hProcess, this.location, UIntPtr.Zero, MEM_RELEASE))
                {
                    //throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            int success = CloseHandle(this.hProcess);
        }

        /// <summary>
        /// Write a structure into this chunk.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "size")]
        public void WriteStructure(int offset, object structure)
        {
            int size = Marshal.SizeOf(structure);
            IntPtr localPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, localPtr, false);
                this.Write(offset, localPtr, size);
            }
            finally
            {
                Marshal.FreeHGlobal(localPtr);
            }
        }

        /// <summary>
        /// Write into this chunk.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ptr")]
        public void Write(int offset, IntPtr ptr, int length)
        {
            if (offset < 0) throw new ArgumentException("Offset may not be negative", "offset");
            if (offset + length > this.size) throw new ArgumentException("Exceeding chunk size");
            WriteProcessMemory(this.hProcess, new IntPtr(this.location.ToInt64() + offset), ptr, new UIntPtr((uint)length), IntPtr.Zero);
        }

        /// <summary>
        /// Write a byte array into this chunk.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void Write(int offset, byte[] ptr)
        {
            if (offset < 0) throw new ArgumentException("Offset may not be negative", "offset");
            if (offset + ptr.Length > this.size) throw new ArgumentException("Exceeding chunk size");
            WriteProcessMemory(this.hProcess, new IntPtr(this.location.ToInt64() + offset), ptr, new UIntPtr((uint)ptr.Length), IntPtr.Zero);
        }

        /// <summary>
        /// Read this chunk.
        /// </summary>
        /// <returns></returns>
        public byte[] Read() { return this.Read(0, this.size); }

        /// <summary>
        /// Read a part of this chunk.
        /// </summary>
        public byte[] Read(int offset, int length)
        {
            if (offset + length > this.size) throw new ArgumentException("Exceeding chunk size");
            byte[] result = new byte[length];
            ReadProcessMemory(this.hProcess, new IntPtr(this.location.ToInt64() + offset), result, new UIntPtr((uint)length), IntPtr.Zero);
            return result;
        }

        /// <summary>
        /// Read this chunk to a pointer in this process.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ptr")]
        public void ReadToPtr(IntPtr ptr)
        {
            this.ReadToPtr(0, this.size, ptr);
        }

        /// <summary>
        /// Read a part of this chunk to a pointer in this process.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ptr")]
        public void ReadToPtr(int offset, int length, IntPtr ptr)
        {
            if (offset + length > this.size) throw new ArgumentException("Exceeding chunk size");
            ReadProcessMemory(this.hProcess, new IntPtr(this.location.ToInt64() + offset), ptr, new UIntPtr((uint)length), IntPtr.Zero);
        }

        /// <summary>
        /// Read a part of this chunk to a structure.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "size")]
        public object ReadToStructure(int offset, Type structureType)
        {
            int size = Marshal.SizeOf(structureType);
            IntPtr localPtr = Marshal.AllocHGlobal(size);
            try
            {
                this.ReadToPtr(offset, size, localPtr);
                return Marshal.PtrToStructure(localPtr, structureType);
            }
            finally
            {
                Marshal.FreeHGlobal(localPtr);
            }
        }

        #region PInvoke Declarations

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
           int dwProcessId);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        private static readonly uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000,
            MEM_RELEASE = 0x8000, PAGE_READWRITE = 0x04;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
           UIntPtr dwSize, uint dwFreeType);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           IntPtr lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           IntPtr lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

        #endregion
    }

    internal enum ProcessAccessFlags : int
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }
}