using UnityEngine;
using System.Runtime.InteropServices;

public class BspGameLump
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV4_t
    {
        public Vector3 m_Origin;
        public Vector3 m_Angles;
        public ushort m_PropType;
        public ushort m_FirstLeaf;
        public ushort m_LeafCount;
        public byte m_Solid;
        public byte m_Flags;
        public int m_Skin;
        public float m_FadeMinDist;
        public float m_FadeMaxDist;
        public Vector3 m_LightingOrigin;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV5_t
    {
        public float m_flForcedFadeScale;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV6_t
    {
        public float m_flForcedFadeScale;
        public ushort m_nMinDXLevel;
        public ushort m_nMaxDXLevel;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV7_t
    {
        public float m_flForcedFadeScale;
        public ushort m_nMinDXLevel;
        public ushort m_nMaxDXLevel;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_DiffuseModulation;
        // per instance color and alpha modulation
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV8_t
    {
        public float m_flForcedFadeScale;
        public byte m_nMinCPULevel;
        public byte m_nMaxCPULevel;
        public byte m_nMinGPULevel;
        public byte m_nMaxGPULevel;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_DiffuseModulation;
        // per instance color and alpha modulation
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV9_t
    {
        public float m_flForcedFadeScale;
        public byte m_nMinCPULevel;
        public byte m_nMaxCPULevel;
        public byte m_nMinGPULevel;
        public byte m_nMaxGPULevel;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_DiffuseModulation;
        // per instance color and alpha modulation

        public bool DisableX360;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticPropLumpV10_t
    {
        public float m_flForcedFadeScale;
        public byte m_nMinCPULevel;
        public byte m_nMaxCPULevel;
        public byte m_nMinGPULevel;
        public byte m_nMaxGPULevel;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_DiffuseModulation;
        // per instance color and alpha modulation

        public float unknown;

        public bool DisableX360;
    }
}
