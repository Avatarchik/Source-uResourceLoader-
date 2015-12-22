using UnityEngine;
using System.Runtime.InteropServices;

public class MdlSpecification
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct studiohdr_t
    {
        public int id; // Model format ID, such as "IDST" (0x49 0x44 0x53 0x54)
        public int version; // Format version number, such as 48 (0x30,0x00,0x00,0x00)

        public int checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] name; // The internal name of the model, padding with null bytes.

        public int dataLength; // Data size of MDL file in bytes.

        public Vector3 eyeposition; // Position of player viewpoint relative to model origin
        public Vector3 illumposition; // ?? Presumably the point used for lighting when per-vertex lighting is not enabled.
        public Vector3 hull_min; // Corner of model hull box with the least X/Y/Z values
        public Vector3 hull_max; // Opposite corner of model hull box
        public Vector3 view_bbmin;
        public Vector3 view_bbmax;
        
        public int flags; // Binary flags in little-endian order. 
        
        // mstudiobone_t
        public int bone_count;    // Number of data sections (of type mstudiobone_t)
        public int bone_offset; // Offset of first data section
        
        // mstudiobonecontroller_t
        public int bonecontroller_count;
        public int bonecontroller_offset;
        
        // mstudiohitboxset_t
        public int hitbox_count;
        public int hitbox_offset;
        
        // mstudioanimdesc_t
        public int localanim_count;
        public int localanim_offset;
        
        // mstudioseqdesc_t
        public int localseq_count;
        public int localseq_offset;
        
        public int activitylistversion;
        public int eventsindexed;

        // mstudiotexture_t
        public int texture_count;
        public int texture_offset;

        public int texturedir_count;
        public int texturedir_offset;

        public int skinreference_count;
        public int skinrfamily_count;
        public int skinreference_index;
        
        // mstudiobodyparts_t
        public int bodypart_count;
        public int bodypart_offset;
            
        // mstudioattachment_t
        public int attachment_count;
        public int attachment_offset;

        public int localnode_count;
        public int localnode_index;
        public int localnode_name_index;
        
        // mstudioflexdesc_t
        public int flexdesc_count;
        public int flexdesc_index;
        
        // mstudioflexcontroller_t
        public int flexcontroller_count;
        public int flexcontroller_index;
        
        // mstudioflexrule_t
        public int flexrules_count;
        public int flexrules_index;

        // mstudioikchain_t
        public int ikchain_count;
        public int ikchain_index;

        // mstudiomouth_t
        public int mouths_count; 
        public int mouths_index;
        
        // mstudioposeparamdesc_t
        public int localposeparam_count;
        public int localposeparam_index;

        public int surfaceprop_index;

        public int keyvalue_index;
        public int keyvalue_count;    

        // mstudioiklock_t
        public int iklock_count;
        public int iklock_index;

        public float mass;
        public int contents;    

        // mstudiomodelgroup_t
        public int includemodel_count;
        public int includemodel_index;
        
        public int virtualModel; // Placeholder for mutable-void*
        
        // mstudioanimblock_t
        public int animblocks_name_index;
        public int animblocks_count;
        public int animblocks_index;
        
        public int animblockModel; // Placeholder for mutable-void*

        public int bonetablename_index;
        
        public int vertex_base; 
        public int offset_base;
        
        // Used with $constantdirectionallight from the QC 
        // Model should have flag #13 set if enabled
        public byte directionaldotproduct;
        
        public byte rootLod; // Preferred rather than clamped
        
        // 0 means any allowed, N means Lod 0 -> (N-1)
        public byte numAllowedRootLods;    
        
        public byte unused;
        public int unused2; 
        
        // mstudioflexcontrollerui_t
        public int flexcontrollerui_count;
        public int flexcontrollerui_index;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiobone_t
    {
        public int sznameindex;
        public int parent; // parent bone

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
        public int[] bonecontroller; // bone controller index, -1 == none

        public Vector3 pos;
        public Quaternion quat;
        public Vector3 rot;

        public Vector3 posscale;
        public Vector3 rotscale;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12)]
        public float[] poseToBone;

        public Quaternion qAlignment;
        public int flags;
        public int proctype;
        public int procindex; // procedural rule
        public int physicsbone; // index into physically simulated bone
        public int surfacepropidx; // index into string tablefor property name
        public int contents; // See BSPFlags.h for the contents flags

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiotexture_t
    {
        public int sznameindex;
        public int flags;
        public int used;
        public int unused1;
        public int material; // fixme: this needs to go away . .isn't used by the engine, but is used by studiomdl
        public int clientmaterial; // gary, replace with client material pointer if used

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
        public int[] unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiobodyparts_t
    {
        public int sznameindex;
        public int nummodels;
        public int _base;
        public int modelindex; // index into models array
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiomodel_t
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] name;
        
        public int type;
        
        public float boundingradius;
        
        public int nummeshes;    
        public int meshindex;

        public int numvertices; // number of unique vertices/normals/texcoords
        public int vertexindex; // vertex Vector
        public int tangentsindex; // tangents Vector
        
        public int numattachments;
        public int attachmentindex;
        
        public int numeyeballs;
        public int eyeballindex;

        public mstudio_modelvertexdata_t vertexdata;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiomesh_t
    {
        public int material;
        public int modelindex;
        
        public int numvertices;
        public int vertexoffset;
        
        public int numflexes;
        public int flexoffset;
        
        public int materialtype;
        public int materialparam;
        
        public int meshid;
        
        public Vector3 center;
        public mstudio_meshvertexdata_t vertexdata;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudio_modelvertexdata_t
    {
        public int vertexdata;
        public int tangentdata;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudio_meshvertexdata_t
    {
        public int modelvertexdata;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] numlodvertices;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct vertexFileHeader_t
    {
        public int id; // MODEL_VERTEX_FILE_ID
        public int version; // MODEL_VERTEX_FILE_VERSION

        public int checksum; // same as studiohdr_t, ensures sync

        public int numLODs; // num of valid lods

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] numLODVertexes; // num verts for desired root lod

        public int numFixups; // num of vertexFileFixup_t

        public int fixupTableStart; // offset from base to fixup table
        public int vertexDataStart; // offset from base to vertex block
        public int tangentDataStart; // offset from base to tangent block
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct vertexFileFixup_t
    {
        public int lod; // used to skip culled root lod
        public int sourceVertexID; // absolute index from start of vertex/tangent blocks
        public int numVertexes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudiovertex_t
    {
        public mstudioboneweight_t m_BoneWeights;
        public Vector3 m_vecPosition;
        public Vector3 m_vecNormal;
        public Vector2 m_vecTexCoord;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct mstudioboneweight_t
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] weight;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] bone; 

        public byte numbones;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileHeader_t
    {
        public int version;

        public int vertCacheSize;
        public ushort maxBonesPerStrip;
        public ushort maxBonesPerFace;
        public int maxBonesPerVert;

        public int checkSum;
        
        public int numLODs;

        public int materialReplacementListOffset;
        
        public int numBodyParts;
        public int bodyPartOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BodyPartHeader_t
    {
        public int numModels;
        public int modelOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModelHeader_t
    {
        public int numLODs;
        public int lodOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModelLODHeader_t
    {
        public int numMeshes;
        public int meshOffset;
        public float switchPoint;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MeshHeader_t
    {
        public int numStripGroups;
        public int stripGroupHeaderOffset;
        public byte flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StripGroupHeader_t
    {
        public int numVerts;
        public int vertOffset;
        
        public int numIndices;
        public int indexOffset;
        
        public int numStrips;
        public int stripOffset;
        
        public byte flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex_t
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] boneWeightIndex;

        public byte numBones;
        
        public ushort origMeshVertID;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] boneID;
    }
}
