using UnityEngine;
using System.Runtime.InteropServices;

public class BspSpecification : BspGameLump
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dheader_t
    {
        public int ident; // BSP file identifier
        public int version; // BSP file version

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
        public lump_t[] lumps; // lump directory array

        public int mapRevision; // the map's revision (iteration, version) number
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct lump_t
    {
        public int fileofs; // offset into file (bytes)
        public int filelen; // length of lump (bytes)
        public int version; // lump format version

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] fourCC;    // lump ident code
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dmodel_t
    {
        public Vector3 mins, maxs; // bounding box
        public Vector3 origin; // for sounds or lights
        public int headnode; // index into node array
        public int firstface, numfaces; // index into fvace array
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dface_t
    {
        public ushort planenum; // the plane number
        public byte side; // faces opposite to the node's plane direction
        public byte onNode; // 1 of on node, 0 if in leaf
        public int firstedge; // index into surfedges
        public short numedges; // number of surfedges
        public short texinfo; // texture info
        public short dispinfo;  // displacement info
        public short surfaceFogVolumeID; // ?

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] styles; // switchable lighting info

        public int lightofs; // offset into lightmap lump
        public float area; // face area in units^2

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] LightmapTextureMinsInLuxels; // texture lighting info

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] LightmapTextureSizeInLuxels; // texture lighting info

        public int origFace; // original face this was split from
        public ushort numPrims; // primitives
        public ushort firstPrimID;
        public uint smoothingGroups; // lightmap smoothing group
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ddispinfo_t
    {
        public Vector3 startPosition; // start position used for orientation
        public int DispVertStart; // Index into LUMP_DISP_VERTS.
        public int DispTriStart; // Index into LUMP_DISP_TRIS.
        public int power; // power - indicates size of surface (2^power + 1)
        public int minTess; // minimum tesselation allowed
        public float smoothingAngle; // lighting smoothing angle
        public int contents; // surface contents
        public ushort MapFace; // Which map face this displacement comes from.
        public int LightmapAlphaStart; // Index into ddisplightmapalpha.
        public int LightmapSamplePositionStart; // Index into LUMP_DISP_LIGHTMAP_SAMPLE_POSITIONS.

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 130)]
        public byte[] unknown;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class dDispVert
    {
        public Vector3 vec; // Vector field defining displacement volume.
        public float dist; // Displacement distances.
        public float alpha; // "per vertex" alpha values.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dtexdata_t
    {
        public Vector3 reflectivity; // RGB reflectivity
        public int nameStringTableID; // index into TexdataStringTable
        public int width, height; // source image
        public int view_width, view_height;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct texinfo_t
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public Vector4[] textureVecs, lightmapVecs;

        public int flags; // miptex flags overrides
        public int texdata; // Pointer to texture name, size, etc.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dgamelump_t
    {
        public int id;
        public ushort flags;
        public ushort version;
        public int fileofs;
        public int filelen;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct dedge_t
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
        public ushort[] v; // vertex indices
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ColorRGBExp32
    {
        public byte r, g, b;
        public sbyte exponent;
    }

    public struct face
    {
        public int index;

        public Vector3[] points;
        public Vector2[] uv;
        public Vector2[] uv2;

        public int[] triangles;

        public int lightMapW;
        public int lightMapH;
    }
}
