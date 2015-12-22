// This is my reader information. Please leave copyright for use in their projects.

using UnityEngine;

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System;

public class CustomReader
{
    private BinaryReader BinaryReader;

    // ----- READ BYTES ----- //

    // Read all byte's starting at the custom offset
    public byte[] GetBytes(int OffsetInFile)
    {
        BinaryReader.BaseStream.Position = BinaryReader.BaseStream.Length - OffsetInFile;
        return BinaryReader.ReadBytes((int)(BinaryReader.BaseStream.Length - BinaryReader.BaseStream.Position));
    }

    // Read byte's starting at the custom offset
    public byte[] GetBytes(int OffsetInFile, int Count)
    {
        BinaryReader.BaseStream.Position = OffsetInFile;
        return BinaryReader.ReadBytes(Count);
    }

    // ----- READ STRUCT'S ----- //

    // Read structure starting at the current offset
    public T ReadType<T>()
    {
        byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
        GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    }

    // Read structure starting at the custom offset
    public T ReadType<T>(int OffsetInFile)
    {
        BinaryReader.BaseStream.Position = OffsetInFile;
        byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
        GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
        return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    }

    // Read structure's starting at the current offset
    public T[] ReadType<T>(uint Count)
    {
        T[] TypeArray = new T[Count];

        for (int i = 0; i < Count; i++)
        {
            byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            TypeArray[i] = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }

        return TypeArray;
    }

    // Read structure's starting at the custom offset
    public T[] ReadType<T>(int OffsetInFile, int Count)
    {
        BinaryReader.BaseStream.Position = OffsetInFile;
        T[] TypeArray = new T[Count];

        for (int i = 0; i < Count; i++)
        {
            byte[] Buffer = BinaryReader.ReadBytes(Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
            TypeArray[i] = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }

        return TypeArray;
    }

    // ----- READ N-T STRING'S ----- //

    // Read null-terminated string starting at the custom offset
    public string ReadNullTerminatedString(int OffsetInFile)
    {
        BinaryReader.BaseStream.Position = OffsetInFile;

        List<byte> StrBytes = new List<byte> (); byte b;
        while ((b = BinaryReader.ReadByte()) != 0x00)
            StrBytes.Add(b);

        return Encoding.ASCII.GetString (StrBytes.ToArray());
    }

    // Read null-terminated string's starting at the custom offset
    public string[] ReadNullTerminatedString(int OffsetInFile, int[] Array)
    {
        string[] StrArray = new string[Array.Length];

        for (int i = 0; i < Array.Length; i++) 
        {
            BinaryReader.BaseStream.Position = OffsetInFile + Array[i];

            List<byte> StrBytes = new List<byte> (); byte b;
            while ((b = BinaryReader.ReadByte ()) != 0x00)
                StrBytes.Add (b);

            StrArray[i] = Encoding.ASCII.GetString (StrBytes.ToArray());
        }

        return StrArray;
    }

    // ----- ADDITIONAL METHODS ----- //

    public CustomReader(BinaryReader FileReader)
    {
        BinaryReader = FileReader;
    }
}
