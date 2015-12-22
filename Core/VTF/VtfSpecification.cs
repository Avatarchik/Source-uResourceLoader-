using UnityEngine;
using System.Runtime.InteropServices;

public class VtfSpecification
{
    public static int[] uiBytesPerPixels = new int[]
    { 4, 4, 3, 3, 2, 1, 2, 1, 1, 3, 3, 4, 4, 1, 1, 1, 4, 2, 2, 2, 1, 2, 2, 4, 8, 8, 4 };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct tagVTFHEADER
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] signature; // File signature ("VTF\0"). (or as little-endian integer, 0x00465456)

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] version; // version[0].version[1] (currently 7.2).

        public uint headerSize; // Size of the header struct (16 byte aligned; currently 80 bytes).\

        public ushort width; // Width of the largest mipmap in pixels. Must be a power of 2.
        public ushort height; // Height of the largest mipmap in pixels. Must be a power of 2.
        public uint flags; // VTF flags.

        public ushort frames; // Number of frames, if animated (1 for no animation).
        public ushort firstFrame; // First frame in animation (0 based).

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] padding0; // reflectivity padding (16 byte alignment).

        public Vector3 reflectivity; // reflectivity vector.

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] padding1; // reflectivity padding (8 byte packing).

        public float bumpmapScale; // Bumpmap scale.

        public uint highResImageFormat; // High resolution image format.
        public byte mipmapCount; // Number of mipmaps.
        public uint lowResImageFormat; // Low resolution image format (always DXT1).
        public byte lowResImageWidth; // Low resolution image width.
        public byte lowResImageHeight; // Low resolution image height.

        public ushort depth; // Depth of the largest mipmap in pixels.
        // Must be a power of 2. Can be 0 or 1 for a 2D texture (v7.2 only).
    }

    public enum ImageFormat
    {
        IMAGE_FORMAT_NONE = -1,
        IMAGE_FORMAT_RGBA8888 = 0,
        IMAGE_FORMAT_ABGR8888,
        IMAGE_FORMAT_RGB888,
        IMAGE_FORMAT_BGR888,
        IMAGE_FORMAT_RGB565,
        IMAGE_FORMAT_I8,
        IMAGE_FORMAT_IA88,
        IMAGE_FORMAT_P8,
        IMAGE_FORMAT_A8,
        IMAGE_FORMAT_RGB888_BLUESCREEN,
        IMAGE_FORMAT_BGR888_BLUESCREEN,
        IMAGE_FORMAT_ARGB8888,
        IMAGE_FORMAT_BGRA8888,
        IMAGE_FORMAT_DXT1,
        IMAGE_FORMAT_DXT3,
        IMAGE_FORMAT_DXT5,
        IMAGE_FORMAT_BGRX8888,
        IMAGE_FORMAT_BGR565,
        IMAGE_FORMAT_BGRX5551,
        IMAGE_FORMAT_BGRA4444,
        IMAGE_FORMAT_DXT1_ONEBITALPHA,
        IMAGE_FORMAT_BGRA5551,
        IMAGE_FORMAT_UV88,
        IMAGE_FORMAT_UVWQ8888,
        IMAGE_FORMAT_RGBA16161616F,
        IMAGE_FORMAT_RGBA16161616,
        IMAGE_FORMAT_UVLX8888
    }

    public readonly static string[] alp = 
    { "$texture2", 
        "$basetexture",
        "$baseTexture",
        "$fallbackmaterial",  
        "$bottommaterial", 
        // "%tooltexture", 
        "include" 
    };
}
