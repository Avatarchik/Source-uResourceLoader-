using UnityEngine;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

public class StudioMdlLoader : MdlSpecification
{
    private static CustomReader CRead;

    private static studiohdr_t MDL_Header;
    private static List<mstudiobodyparts_t> MDL_BodyParts = new List<mstudiobodyparts_t>();
    private static List<mstudiomodel_t> MDL_Models = new List<mstudiomodel_t>();
    private static List<mstudiomesh_t> MDL_Meshes = new List<mstudiomesh_t>();

    private static List<string> MDL_TDirectories = new List<string>();
    private static List<string> MDL_Textures = new List<string>();

    private static List<Transform> MDL_Bones = new List<Transform>();

    private static vertexFileHeader_t VVD_Header;
    private static List<mstudiovertex_t> VVD_Vertexes = new List<mstudiovertex_t>();
    private static List<vertexFileFixup_t> VVD_Fixups = new List<vertexFileFixup_t>();

    private static FileHeader_t VTX_Header;
    private static List<MeshHeader_t> VTX_Meshes = new List<MeshHeader_t>();

    private static GameObject ModelObject;

    private static void Clear()
    {
        MDL_BodyParts.Clear();
        MDL_Models.Clear();
        MDL_Meshes.Clear();

        MDL_TDirectories.Clear();
        MDL_Textures.Clear();

        MDL_Bones.Clear();

        VVD_Vertexes.Clear();
        VVD_Fixups.Clear();

        VTX_Meshes.Clear();
        ModelObject = null;
    }

    public static Transform LoadMdl(string ModelName)
    {
        Clear();

        string OpenPath = string.Concat(WorldController.GamePath, WorldController.ModName, ModelName);
        ModelObject = new GameObject(ModelName);

        if (!File.Exists(OpenPath + ".mdl"))
            return ModelObject.transform;

        // ----- BEGIN READ MDL, VVD, VTX ----- //

        CRead = new CustomReader(new BinaryReader(File.OpenRead(OpenPath + ".mdl"))); 
        MDL_Header = CRead.ReadType<studiohdr_t>(0);
        ParseMdlFile();

        CRead = new CustomReader(new BinaryReader(File.OpenRead(OpenPath + ".vvd"))); 
        VVD_Header = CRead.ReadType<vertexFileHeader_t>(0);
        ParseVvdFile();

        CRead = new CustomReader(new BinaryReader(File.OpenRead(OpenPath + ".dx90.vtx"))); 
        VTX_Header = CRead.ReadType<FileHeader_t>(0);
        ParseVtxFile();

        // ----- END READ MDL, VVD, VTX ----- //
            
        return ModelObject.transform;
    }

    private static void ParseMdlFile()
    {
        // ----- LOAD BODYPARTS ----- //

        MDL_BodyParts.AddRange(CRead.ReadType<mstudiobodyparts_t>(MDL_Header.bodypart_offset, MDL_Header.bodypart_count));
        int ModelArrayOffset = 0;

        for (int i = 0; i < MDL_Header.bodypart_count; i++)
        {
            int ModelInputFilePosition = MDL_Header.bodypart_offset + (Marshal.SizeOf(typeof(mstudiobodyparts_t)) * i) + MDL_BodyParts[i].modelindex;
            MDL_Models.AddRange(CRead.ReadType<mstudiomodel_t>(ModelInputFilePosition, MDL_BodyParts[i].nummodels));

            for (int l = 0; l < MDL_BodyParts[i].nummodels; l++)
            {
                int MeshInputFilePosition = ModelInputFilePosition + (Marshal.SizeOf(typeof(mstudiomodel_t)) * l) + MDL_Models[ModelArrayOffset + l].meshindex;
                MDL_Meshes.AddRange(CRead.ReadType<mstudiomesh_t>(MeshInputFilePosition, MDL_Models[l].nummeshes));
            }

            ModelArrayOffset += (MDL_BodyParts[i].nummodels - 1);
        }

        // ----- LOAD TEXTURES INFO ----- //

        List<mstudiotexture_t> MDL_TexturesInfo = new List<mstudiotexture_t>();
        MDL_TexturesInfo.AddRange(CRead.ReadType<mstudiotexture_t>(MDL_Header.texture_offset, MDL_Header.texture_count));

        for (int i = 0; i < MDL_Header.texture_count; i++)
        {
            int StringInputFilePosition = MDL_Header.texture_offset + (Marshal.SizeOf(typeof(mstudiotexture_t)) * i) + MDL_TexturesInfo[i].sznameindex;
            MDL_Textures.Add(CRead.ReadNullTerminatedString(StringInputFilePosition));
        }

        int[] TDirOffsets = CRead.ReadType<int>(MDL_Header.texturedir_offset, MDL_Header.texturedir_count);

        for (int i = 0; i < MDL_Header.texturedir_count; i++)
            MDL_TDirectories.Add(CRead.ReadNullTerminatedString(TDirOffsets[i]));

        // ----- LOAD BONES ----- //

        List<mstudiobone_t> MDL_BonesInfo = new List<mstudiobone_t>();
        MDL_BonesInfo.AddRange(CRead.ReadType<mstudiobone_t>(MDL_Header.bone_offset, MDL_Header.bone_count));

        for (int i = 0; i < MDL_Header.bone_count; i++)
        {
            int StringInputFilePosition = MDL_Header.bone_offset + (Marshal.SizeOf(typeof(mstudiobone_t)) * i) + MDL_BonesInfo[i].sznameindex;

            GameObject BoneObject = new GameObject(CRead.ReadNullTerminatedString(StringInputFilePosition));
            BoneObject.transform.parent = ModelObject.transform;

            MDL_Bones.Add(BoneObject.transform);

            if (MDL_BonesInfo[i].parent >= 0)
                MDL_Bones[i].transform.parent = MDL_Bones[MDL_BonesInfo[i].parent].transform;
        }
    }

    private static void ParseVtxFile()
    {
        List<BoneWeight> pBoneWeight = new List<BoneWeight>();
        List<Vector3> pVertices = new List<Vector3>();
        List<Vector3> pNormals = new List<Vector3>();
        List<Vector2> pUvBuffer = new List<Vector2>();

        // Load necessary information
        mstudiomodel_t pModel = MDL_Models[0]; mstudiomesh_t pStudioMesh;
        BodyPartHeader_t vBodypart = CRead.ReadType<BodyPartHeader_t>(VTX_Header.bodyPartOffset);

        int ModelInputFilePosition = VTX_Header.bodyPartOffset + vBodypart.modelOffset;
        ModelHeader_t vModel = CRead.ReadType<ModelHeader_t>(ModelInputFilePosition);

        int ModelLODInputFilePosition = ModelInputFilePosition + vModel.lodOffset;
        ModelLODHeader_t vLod = CRead.ReadType<ModelLODHeader_t>(ModelLODInputFilePosition);

        int MeshInputFilePosition = ModelLODInputFilePosition + vLod.meshOffset;
        VTX_Meshes.AddRange(CRead.ReadType<MeshHeader_t>(MeshInputFilePosition, vLod.numMeshes));

        // Get bone weight's, vertices, normals, uv
        for (int i = 0; i < pModel.numvertices; i++)
        {
            pBoneWeight.Add(GetBoneWeight(VVD_Vertexes[pModel.vertexindex + i].m_BoneWeights));

            pVertices.Add(WorldController.SwapZY(VVD_Vertexes[pModel.vertexindex + i].m_vecPosition * WorldController.WorldScale));
            pNormals.Add(WorldController.SwapZY(VVD_Vertexes[pModel.vertexindex + i].m_vecNormal));
            pUvBuffer.Add(VVD_Vertexes[pModel.vertexindex + i].m_vecTexCoord);
        }

        GameObject meshObject = new GameObject(new string(pModel.name));
        meshObject.transform.parent = ModelObject.transform;

        SkinnedMeshRenderer smr = meshObject.AddComponent<SkinnedMeshRenderer>();
        smr.materials = new Material[vLod.numMeshes];

        // Calculate bindposes
        Matrix4x4[] bindPoses = new Matrix4x4[MDL_Bones.Count];
        for (int i = 0; i < bindPoses.Length; i++)
        {
            MDL_Bones[i].localPosition = Vector3.zero;
            bindPoses[i] = MDL_Bones[i].worldToLocalMatrix * ModelObject.transform.localToWorldMatrix;
        }

        // Generate skin mesh
        smr.sharedMesh = new Mesh();
        smr.sharedMesh.name = new string(pModel.name);

        smr.sharedMesh.subMeshCount = vLod.numMeshes;
        smr.sharedMesh.vertices = pVertices.ToArray();
        smr.sharedMesh.normals = pNormals.ToArray();
        smr.sharedMesh.uv = pUvBuffer.ToArray();
        
        smr.sharedMesh.boneWeights = pBoneWeight.ToArray();
        smr.sharedMesh.bindposes = bindPoses;

        smr.sharedMesh.Optimize();

        smr.bones = MDL_Bones.ToArray();
        smr.updateWhenOffscreen = true;

        for (int i = 0; i < vLod.numMeshes; i++)
        {
            List<int> pIndices = new List<int>();
            
            List<StripGroupHeader_t> StripGroups = new List<StripGroupHeader_t>();
            int StripGroupFilePosition = MeshInputFilePosition + (Marshal.SizeOf(typeof(MeshHeader_t)) * i) + VTX_Meshes[i].stripGroupHeaderOffset;
            StripGroups.AddRange(CRead.ReadType<StripGroupHeader_t>(StripGroupFilePosition, VTX_Meshes[i].numStripGroups)); pStudioMesh = MDL_Meshes[i];

            // Get indices for model
            for (int l = 0; l < VTX_Meshes[i].numStripGroups; l++)
            {
                List<Vertex_t> pVertexBuffer = new List<Vertex_t>();
                pVertexBuffer.AddRange(CRead.ReadType<Vertex_t>(StripGroupFilePosition + (Marshal.SizeOf(typeof(StripGroupHeader_t)) * l) + StripGroups[l].vertOffset, StripGroups[l].numVerts));
                
                List<ushort> Indices = new List<ushort>();
                Indices.AddRange(CRead.ReadType<ushort>(StripGroupFilePosition + (Marshal.SizeOf(typeof(StripGroupHeader_t)) * l) + StripGroups[l].indexOffset, StripGroups[l].numIndices));
                
                for (int n = 0; n < Indices.Count; n++)
                    pIndices.Add(pVertexBuffer[Indices[n]].origMeshVertID + pStudioMesh.vertexoffset);
            }
            
            smr.sharedMesh.SetTriangles(pIndices.ToArray(), i);
            smr.materials[i].name = MDL_Textures[pStudioMesh.material];

            string pathToTex = null; Material material = null;

            // Find path to mesh material
            for (int l = 0; l < MDL_TDirectories.Count; l++)
                if (File.Exists(WorldController.DefaultTexPath + MDL_TDirectories[l] + MDL_Textures[pStudioMesh.material] + ".vmt"))
                    pathToTex = WorldController.DefaultTexPath + MDL_TDirectories[l] + MDL_Textures[pStudioMesh.material] + ".vmt";

            if (pathToTex != null)
                material = ValveTextureLoader.LoadMaterial(pathToTex.Replace(WorldController.DefaultTexPath, ""));
            
            // Apply loaded material to mesh
            if (material != null)
            {
                smr.materials[i].CopyPropertiesFromMaterial(material); 
                smr.materials[i].shader = material.shader;
            }
        }
    }

    private static BoneWeight GetBoneWeight(mstudioboneweight_t mBoneWeight)
    {
        BoneWeight boneWeight = new BoneWeight();
        
        boneWeight.boneIndex0 = mBoneWeight.bone[0];
        boneWeight.boneIndex1 = mBoneWeight.bone[1];
        boneWeight.boneIndex2 = mBoneWeight.bone[2];
        
        boneWeight.weight0 = mBoneWeight.weight[0];
        boneWeight.weight1 = mBoneWeight.weight[1];
        boneWeight.weight2 = mBoneWeight.weight[2];
        
        return boneWeight;
    }

    private static void ParseVvdFile()
    {
        VVD_Fixups.AddRange(CRead.ReadType<vertexFileFixup_t>(VVD_Header.fixupTableStart, VVD_Header.numFixups));
        VVD_Vertexes.AddRange(CRead.ReadType<mstudiovertex_t>(VVD_Header.vertexDataStart, VVD_Header.numLODVertexes[0]));

        if (VVD_Header.numFixups > 0)
            VVD_Vertexes.Clear();

        // Apply fixup's for vertices
        for (int i = 0; i < VVD_Header.numFixups; i++)
        {
            if (VVD_Fixups[i].lod >= 0)
            {
                VVD_Vertexes.AddRange(CRead.ReadType<mstudiovertex_t>(VVD_Header.vertexDataStart + (VVD_Fixups[i].sourceVertexID * 48), VVD_Fixups[i].numVertexes));
            }
        }
    }
}