using UnityEngine;
using Ionic.Zip;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.IO;
using System;

public class SourceBspLoader : BspSpecification
{
    private static BinaryReader BinaryReader;
    private static CustomReader CRead;

    private static dheader_t BSP_Header;
    private static List<string> BSP_Entities = new List<string>();

    private static List<dface_t> BSP_Faces = new List<dface_t>();
    private static List<dmodel_t> BSP_Models = new List<dmodel_t>();

    private static List<ddispinfo_t> BSP_DispInfo = new List<ddispinfo_t>();
    private static List<dDispVert> BSP_DispVerts = new List<dDispVert>();

    private static List<string> BSP_TexStrData = new List<string>();
    private static List<dtexdata_t> BSP_Texdata = new List<dtexdata_t>();
    private static List<texinfo_t> BSP_Texinfo = new List<texinfo_t>();

    private static List<Vector3> BSP_Vertices = new List<Vector3>();
    private static List<dedge_t> BSP_Edges = new List<dedge_t>();
    private static List<int> BSP_Surfedges = new List<int>();

    private static byte[] BSP_PakFile;
    public static GameObject BSP_WorldSpawn;

    public static void LoadBSP()
    {
        // Initialize readers
        BinaryReader = new BinaryReader(File.OpenRead(WorldController.GamePath + WorldController.ModName + "/maps/" + WorldController.MapName + ".bsp"));
        CRead = new CustomReader(BinaryReader);

        // ----- BEGIN READ BSP ----- //

        BSP_Header = CRead.ReadType<dheader_t>();
        Debug.Log("BSP version: " + BSP_Header.version);

        string input = Encoding.ASCII.GetString(CRead.GetBytes(BSP_Header.lumps[0].fileofs, BSP_Header.lumps[0].filelen));
        foreach (Match match in Regex.Matches(input, @"{[^}]*}", RegexOptions.IgnoreCase))
            BSP_Entities.Add(match.Value);

        switch (BSP_Header.version)
        {
            // FITCH: If BSP version more than 20, you need load HDR lumps
            case 21: BSP_Faces.AddRange(CRead.ReadType<dface_t>(BSP_Header.lumps[58].fileofs, BSP_Header.lumps[58].filelen / 56)); break;
            default: BSP_Faces.AddRange(CRead.ReadType<dface_t>(BSP_Header.lumps[7].fileofs, BSP_Header.lumps[7].filelen / 56)); break; 
        }

        BSP_Models.AddRange(CRead.ReadType<dmodel_t>(BSP_Header.lumps[14].fileofs, BSP_Header.lumps[14].filelen / 48));

        BSP_Texdata.AddRange(CRead.ReadType<dtexdata_t>(BSP_Header.lumps[2].fileofs, BSP_Header.lumps[2].filelen / 32));
        BSP_Texinfo.AddRange(CRead.ReadType<texinfo_t>(BSP_Header.lumps[6].fileofs, BSP_Header.lumps[6].filelen / 72));

        BSP_DispInfo.AddRange(CRead.ReadType<ddispinfo_t>(BSP_Header.lumps[26].fileofs, BSP_Header.lumps[26].filelen / 176));
        BSP_DispVerts.AddRange(CRead.ReadType<dDispVert>(BSP_Header.lumps[33].fileofs, BSP_Header.lumps[33].filelen / 20));

        int[] BSP_TexStrTable = CRead.ReadType<int>(BSP_Header.lumps[44].fileofs, BSP_Header.lumps[44].filelen / 4);
        BSP_TexStrData.AddRange(CRead.ReadNullTerminatedString(BSP_Header.lumps[43].fileofs, BSP_TexStrTable));

        BSP_Vertices.AddRange(CRead.ReadType<Vector3>(BSP_Header.lumps[3].fileofs, BSP_Header.lumps[3].filelen / 12));
        BSP_Edges.AddRange(CRead.ReadType<dedge_t>(BSP_Header.lumps[12].fileofs, BSP_Header.lumps[12].filelen / 4));
        BSP_Surfedges.AddRange(CRead.ReadType<int>(BSP_Header.lumps[13].fileofs, BSP_Header.lumps[13].filelen / 4));

        BSP_PakFile = CRead.GetBytes(BSP_Header.lumps[40].fileofs, BSP_Header.lumps[40].filelen);

        // ----- END READ BSP ----- //

        for (int i = 0; i < BSP_Entities.Count; i++)
            LoadEntity(i);

        BinaryReader.BaseStream.Dispose();
    }

    private static void WorldSpawn()
    {
        LoadStaticProps();
        UnpackPakFile(); 

        // Load displacements if they are
        if (BSP_DispInfo.Count > 0)
            CreateDispSurface();

        // Create "worldspawn" object
        BSP_WorldSpawn = new GameObject(WorldController.MapName);

        for (int i = 0; i < BSP_Models.Count; i++)
            CreateModel(i);
    }

    private static void LoadEntity(int id)
    {
        List<string> data = new List<string>();

        foreach (Match match in Regex.Matches(BSP_Entities[id], "\"[^\"]*\"", RegexOptions.IgnoreCase))
            data.Add(match.Value.Trim('"'));

        int classNameIndex = data.FindIndex(n => n == "classname");
        WorldController.CurrentTexPath = WorldController.DefaultTexPath;

        if (data[classNameIndex + 1] == "worldspawn")
        {
            WorldSpawn();
            return;
        }

        if (data[0] == "model")
        {
            GameObject entObject = GameObject.Find(data[data.FindIndex(n => n == "model") + 1]);
            SourceEntityInfo entInfo = entObject.AddComponent<SourceEntityInfo>();

            entInfo.ModelId = int.Parse(entObject.name.Replace("*", ""));
            entInfo.baseDescription = data;
            entInfo.EntityId = id;
            entInfo.Configure();
        }
        else
        {
            GameObject entObject = new GameObject();
            entObject.transform.parent = BSP_WorldSpawn.transform;

            SourceEntityInfo entInfo = entObject.AddComponent<SourceEntityInfo>();
            entInfo.baseDescription = data;
            entInfo.EntityId = id;
            entInfo.Configure();
        }
    }

    private static face CreateFace(int index)
    {
        List<Vector3> faceVertices = new List<Vector3>();
        List<Vector2> textureCoordinates = new List<Vector2>();
        List<Vector2> lightmapCoordinates = new List<Vector2>();

        int startEdgeIndex = BSP_Faces[index].firstedge;
        int edgesCount = BSP_Faces[index].numedges;

        texinfo_t faceTexinfo = BSP_Texinfo[BSP_Faces[index].texinfo];
        dtexdata_t faceTexdata = BSP_Texdata[faceTexinfo.texdata];

        // Get vertices for this polygon
        for (int i = startEdgeIndex; i < startEdgeIndex + edgesCount; i++)
            faceVertices.Add(WorldController.SwapZY(BSP_Surfedges[i] > 0 ? BSP_Vertices[BSP_Edges[Mathf.Abs(BSP_Surfedges[i])].v[0]] : BSP_Vertices[BSP_Edges[Mathf.Abs(BSP_Surfedges[i])].v[1]]) * WorldController.WorldScale);

        List<int> templist = new List<int>();

        // Generate indices for this polygon
        for (int i = 1; i < faceVertices.Count - 1; i++)
        {
            templist.Add(0);
            templist.Add(i);
            templist.Add(i + 1);
        }

        // Generate texture UV for this polygon
        for (int i = 0; i < faceVertices.Count; i++)
        {
            float tU = Vector3.Dot(WorldController.SwapZY(new Vector3(faceTexinfo.textureVecs[0].x, faceTexinfo.textureVecs[0].y, faceTexinfo.textureVecs[0].z)), faceVertices[i]) + faceTexinfo.textureVecs[0].w * WorldController.WorldScale;
            float tV = Vector3.Dot(WorldController.SwapZY(new Vector3(faceTexinfo.textureVecs[1].x, faceTexinfo.textureVecs[1].y, faceTexinfo.textureVecs[1].z)), faceVertices[i]) + faceTexinfo.textureVecs[1].w * WorldController.WorldScale;

            tU /= (faceTexdata.width * WorldController.WorldScale);
            tV /= (faceTexdata.height * WorldController.WorldScale);
            textureCoordinates.Add(new Vector2(tU, tV));
        }

        // Generate lightmap UV for this polygon
        for (int i = 0; i < faceVertices.Count; i++)
        {
            // TODO: Remove "+ 0.5f" if you don't use atlases
            float lU = Vector3.Dot(WorldController.SwapZY(new Vector3(faceTexinfo.lightmapVecs[0].x, faceTexinfo.lightmapVecs[0].y, faceTexinfo.lightmapVecs[0].z)), faceVertices[i]) + (faceTexinfo.lightmapVecs[0].w + 0.5f - BSP_Faces[index].LightmapTextureMinsInLuxels[0]) * WorldController.WorldScale;
            float lV = Vector3.Dot(WorldController.SwapZY(new Vector3(faceTexinfo.lightmapVecs[1].x, faceTexinfo.lightmapVecs[1].y, faceTexinfo.lightmapVecs[1].z)), faceVertices[i]) + (faceTexinfo.lightmapVecs[1].w + 0.5f - BSP_Faces[index].LightmapTextureMinsInLuxels[1]) * WorldController.WorldScale;

            lU /= (BSP_Faces[index].LightmapTextureSizeInLuxels[0] + 1) * WorldController.WorldScale;
            lV /= (BSP_Faces[index].LightmapTextureSizeInLuxels[1] + 1) * WorldController.WorldScale;
            lightmapCoordinates.Add(new Vector2(lU, lV));
        }

        return new face()
        {
            index = index,

            points = faceVertices.ToArray(),
            triangles = templist.ToArray(),

            uv = textureCoordinates.ToArray(),
            uv2 = lightmapCoordinates.ToArray(),

            lightMapW = BSP_Faces[index].LightmapTextureSizeInLuxels[0] + 1,
            lightMapH = BSP_Faces[index].LightmapTextureSizeInLuxels[1] + 1
        };
    }

    private static void CreateModel(int index)
    {
        Dictionary<int, List<int>> subMeshData = new Dictionary<int, List<int>>();
        GameObject model = new GameObject("*" + index);
        model.transform.parent = BSP_WorldSpawn.transform;

        int firstFace = BSP_Models[index].firstface;
        int faces = BSP_Models[index].numfaces;

        for (int i = firstFace; i < firstFace + faces; i++)
        {
            if (!subMeshData.ContainsKey(BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID))
                subMeshData.Add(BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID, new List<int>());

            subMeshData[BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID].Add(i);
        }

        for (int i = 0; i < BSP_TexStrData.Count; i++)
        {
            if (!subMeshData.ContainsKey(i))
                continue;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();
            List<face> faceList = new List<face>();

            for (int k = 0; k < subMeshData[i].Count; k++)
            {
                // Get points, indices, UV's
                if (BSP_Faces[subMeshData[i][k]].dispinfo == -1)
                {
                    face f = CreateFace(subMeshData[i][k]);
                    int pointOffset = vertices.Count;

                    for (int j = 0; j < f.triangles.Length; j++)
                        triangles.Add(f.triangles[j] + pointOffset);
                        
                    vertices.AddRange(f.points);
                    uv.AddRange(f.uv);
                    faceList.Add(f);
                }
            }

            GameObject submesh = new GameObject(BSP_TexStrData[i]);
            submesh.transform.parent = model.transform;

            MeshRenderer meshRenderer = submesh.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = submesh.AddComponent<MeshFilter>();

            List<Vector2> uv2 = new List<Vector2>(); 
            Texture2D lightMap = new Texture2D(0, 0);

            // Load lightmaps and generate atlas
            CreateLightMap(faceList, ref lightMap, ref uv2);

            WorldController.CurrentTexPath = WorldController.DefaultTexPath;
            if (BSP_TexStrData[i].Contains(WorldController.MapName))
                WorldController.CurrentTexPath = WorldController.PakTexPath;

            // Load material for this object
            meshRenderer.sharedMaterial = ValveTextureLoader.LoadMaterial(BSP_TexStrData[i]);
            meshRenderer.sharedMaterial.SetTexture("_LightMap", lightMap);

            if (BSP_TexStrData[i].Contains("TOOLS/"))
                meshRenderer.enabled = false;

            // Generate submesh
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.vertices = vertices.ToArray();
            meshFilter.sharedMesh.triangles = triangles.ToArray();

            meshFilter.sharedMesh.uv = uv.ToArray();
            meshFilter.sharedMesh.uv2 = uv2.ToArray();

            meshFilter.sharedMesh.RecalculateNormals();
            meshFilter.sharedMesh.Optimize();
        }
    }
        
    private static void CreateDispSurface()
    {
        GameObject BSP_DispSurface = new GameObject(WorldController.MapName + "_disp");
        Dictionary<int, List<int>> subMeshData = new Dictionary<int, List<int>>();

        for (int i = 0; i < BSP_Faces.Count; i++)
        {
            if (!subMeshData.ContainsKey(BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID))
                subMeshData.Add(BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID, new List<int>());

            subMeshData[BSP_Texdata[BSP_Texinfo[BSP_Faces[i].texinfo].texdata].nameStringTableID].Add(i);
        }

        for (int i = 0; i < BSP_TexStrData.Count; i++)
        {
            if (!subMeshData.ContainsKey(i))
                continue;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int k = 0; k < subMeshData[i].Count; k++)
            {
                if (BSP_Faces[subMeshData[i][k]].dispinfo != -1)
                {
                    face f = CreateDispSurface(BSP_Faces[subMeshData[i][k]].dispinfo);
                    int pointOffset = vertices.Count;

                    for (int j = 0; j < f.triangles.Length; j++)
                        triangles.Add(f.triangles[j] + pointOffset);

                    vertices.AddRange(f.points);
                    uv.AddRange(f.uv);
                }
            }

            if (vertices.Count > 0)
            {
                GameObject submesh = new GameObject(BSP_TexStrData[i]);
                submesh.transform.localScale = new Vector3(1, 1, -1);
                submesh.transform.parent = BSP_DispSurface.transform;

                MeshRenderer meshRenderer = submesh.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = submesh.AddComponent<MeshFilter>();

                WorldController.CurrentTexPath = WorldController.DefaultTexPath;
                if (BSP_TexStrData[i].Contains(WorldController.MapName))
                    WorldController.CurrentTexPath = WorldController.PakTexPath;

                meshRenderer.sharedMaterial = ValveTextureLoader.LoadMaterial(BSP_TexStrData[i]);

                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.vertices = vertices.ToArray();
                meshFilter.sharedMesh.triangles = triangles.ToArray();
                meshFilter.sharedMesh.uv = uv.ToArray();

                meshFilter.sharedMesh.RecalculateNormals();
                meshFilter.sharedMesh.Optimize();
            }
        }
    }

    // I'm not the author of this method. I translated it on С#.
    // http://trac.openscenegraph.org/projects/osg/browser/OpenSceneGraph/branches/OpenSceneGraph-osgWidget-dev/src/osgPlugins/bsp/VBSPGeometry.cpp?rev=9236
    private static face CreateDispSurface(int dispIndex)
    {
        List<Vector3> faceVertices = new List<Vector3>(); 
        List<Vector3> dispVertices = new List<Vector3>(); 
        List<Int32> dispIndices = new List<int>();

        List<Vector2> textureCoordinates = new List<Vector2>(); 
        // List<Vector2> lightmapCoordinates = new List<Vector2>();

        dface_t faceInfo = BSP_Faces[BSP_DispInfo[dispIndex].MapFace];
        int minIndex = 0;

        for (int i = faceInfo.firstedge; i < (faceInfo.firstedge + faceInfo.numedges); i++)
            faceVertices.Add((BSP_Surfedges[i] > 0 ? BSP_Vertices[BSP_Edges[Mathf.Abs(BSP_Surfedges[i])].v[0]] : BSP_Vertices[BSP_Edges[Mathf.Abs(BSP_Surfedges[i])].v[1]]) * WorldController.WorldScale);

        float minDist = 1.0e9f;
        for (int i = 0; i < 4; i++)
        {
            float dist = (faceVertices[i] - BSP_DispInfo[dispIndex].startPosition * WorldController.WorldScale).magnitude;

            if (dist < minDist)
            {
                minDist = dist;
                minIndex = i;
            }
        }

        for (int i = 0; i < minIndex; i++)
        {
            Vector3 temp = faceVertices[0];
            faceVertices[0] = faceVertices[1];
            faceVertices[1] = faceVertices[2];
            faceVertices[2] = faceVertices[3];
            faceVertices[3] = temp;
        }

        Vector3 leftEdge = faceVertices[1] - faceVertices[0];
        Vector3 rightEdge = faceVertices[2] - faceVertices[3];

        int numEdgeVertices = (1 << BSP_DispInfo[dispIndex].power) + 1;
        float subdivideScale = 1.0f / (numEdgeVertices - 1);

        Vector3 leftEdgeStep = leftEdge * subdivideScale;
        Vector3 rightEdgeStep = rightEdge * subdivideScale;

        for (int i = 0; i < numEdgeVertices; i++)
        {
            Vector3 leftEnd = leftEdgeStep * i;
            leftEnd += faceVertices[0];

            Vector3 rightEnd = rightEdgeStep * i;
            rightEnd += faceVertices[3];

            Vector3 leftRightSeg = rightEnd - leftEnd;
            Vector3 leftRightStep = leftRightSeg * subdivideScale;

            for (int j = 0; j < numEdgeVertices; j++)
            {
                int dispVertIndex = BSP_DispInfo[dispIndex].DispVertStart;
                dispVertIndex += i * numEdgeVertices + j;
                dDispVert dispVertInfo = BSP_DispVerts[dispVertIndex];

                Vector3 flatVertex = leftEnd + (leftRightStep * j);
                Vector3 dispVertex = dispVertInfo.vec * (dispVertInfo.dist * WorldController.WorldScale);
                dispVertex += flatVertex;

                texinfo_t faceTexinfo = BSP_Texinfo[faceInfo.texinfo];
                float fU = Vector3.Dot(new Vector3(faceTexinfo.textureVecs[0].x, faceTexinfo.textureVecs[0].y, faceTexinfo.textureVecs[0].z), flatVertex) + faceTexinfo.textureVecs[0].w * WorldController.WorldScale;
                float fV = Vector3.Dot(new Vector3(faceTexinfo.textureVecs[1].x, faceTexinfo.textureVecs[1].y, faceTexinfo.textureVecs[1].z), flatVertex) + faceTexinfo.textureVecs[1].w * WorldController.WorldScale;

                fU /= (BSP_Texdata[faceTexinfo.texdata].width * WorldController.WorldScale);
                fV /= (BSP_Texdata[faceTexinfo.texdata].height * WorldController.WorldScale);
                textureCoordinates.Add(new Vector2(fU, fV));

                dispVertices.Add(new Vector3(-dispVertex.x, dispVertex.z, dispVertex.y));
            }
        }

        for (int i = 0; i < numEdgeVertices - 1; i++)
        {
            for (int j = 0; j < numEdgeVertices - 1; j++)
            {
                int index = i * numEdgeVertices + j;

                if ((index % 2) == 1)
                {
                    dispIndices.Add(index);
                    dispIndices.Add(index + 1);
                    dispIndices.Add(index + numEdgeVertices);
                    dispIndices.Add(index + 1);
                    dispIndices.Add(index + numEdgeVertices + 1);
                    dispIndices.Add(index + numEdgeVertices);
                }
                else
                {
                    dispIndices.Add(index);
                    dispIndices.Add(index + numEdgeVertices + 1);
                    dispIndices.Add(index + numEdgeVertices);
                    dispIndices.Add(index);
                    dispIndices.Add(index + 1);
                    dispIndices.Add(index + numEdgeVertices + 1);
                }
            }
        }

        return new face()
        {
            index = BSP_DispInfo[dispIndex].MapFace,

            points = dispVertices.ToArray(),
            triangles = dispIndices.ToArray(),
            uv = textureCoordinates.ToArray()
        };
    }

    private static void CreateLightMap(List<face> inpFaces, ref Texture2D lightMap, ref List<Vector2> lightMapUV)
    {
        Texture2D[] lightMaps = new Texture2D[inpFaces.Count];

        // Load lightmap for each face
        for (int i = 0; i < inpFaces.Count; i++)
        {
            lightMaps[i] = new Texture2D(inpFaces[i].lightMapW, inpFaces[i].lightMapH, TextureFormat.RGB24, false);

            Color32[] TexPixels = new Color32[inpFaces[i].lightMapW * inpFaces[i].lightMapH];
            int LightMapOffset = BSP_Faces[inpFaces[i].index].lightofs;

            if (LightMapOffset > 0)
            {
                for (int n = 0; n < TexPixels.Length; n++)
                {
                    ColorRGBExp32 ColorRGBExp32 = TexLightToLinear(LightMapOffset + (n * 4));
                    TexPixels[n] = new Color32(ColorRGBExp32.r, ColorRGBExp32.g, ColorRGBExp32.b, 255);
                }

                lightMaps[i].SetPixels32(TexPixels); 
            }
        }
        
        // Generate lightmap atlas
        Rect[] uvs2 = lightMap.PackTextures(lightMaps.ToArray(), 1);
        lightMap.wrapMode = TextureWrapMode.Clamp; lightMap.Apply(); 

        // Generate UV for lightmap atlas
        for (int i = 0; i < inpFaces.Count; i++)
            for (int l = 0; l < inpFaces[i].uv2.Length; l++)
                lightMapUV.Add(new Vector2((inpFaces[i].uv2[l].x * uvs2[i].width) + uvs2[i].x, (inpFaces[i].uv2[l].y * uvs2[i].height) + uvs2[i].y));
    }

    private static ColorRGBExp32 TexLightToLinear(int fileofs)
    {
        switch (BSP_Header.version)
        {
            // FITCH: If BSP version more than 20, you need load HDR lumps
            case 21: fileofs += BSP_Header.lumps[53].fileofs; break;
            default: fileofs += BSP_Header.lumps[8].fileofs; break;
        }

        ColorRGBExp32 ColorRGBExp32 = CRead.ReadType<ColorRGBExp32>(fileofs);

        // Convert HDR pixels to RGB
        ColorRGBExp32.r = (byte)Mathf.Clamp(ColorRGBExp32.r * Mathf.Pow(2, ColorRGBExp32.exponent), 0, 255);
        ColorRGBExp32.g = (byte)Mathf.Clamp(ColorRGBExp32.g * Mathf.Pow(2, ColorRGBExp32.exponent), 0, 255);
        ColorRGBExp32.b = (byte)Mathf.Clamp(ColorRGBExp32.b * Mathf.Pow(2, ColorRGBExp32.exponent), 0, 255);

        return ColorRGBExp32;
    }

    private static void LoadStaticProps()
    {
        BinaryReader.BaseStream.Position = BSP_Header.lumps[35].fileofs;
        GameObject staticProps = new GameObject(WorldController.MapName + "_props");

        int gamelumpCount = BinaryReader.ReadInt32();
        dgamelump_t[] gamelumps = CRead.ReadType<dgamelump_t>((uint)gamelumpCount);

        for (int i = 0; i < gamelumpCount; i++)
        {
            if (gamelumps[i].id == 1936749168)
            {
                BinaryReader.BaseStream.Position = gamelumps[i].fileofs;
                int dictEntries = BinaryReader.ReadInt32();
                string[] names = new string[dictEntries];

                for (int l = 0; l < dictEntries; l++)
                {
                    names[l] = new string(BinaryReader.ReadChars(128));

                    if (names[l].Contains(Convert.ToChar(0)))
                        names[l] = names[l].Remove(names[l].IndexOf(Convert.ToChar(0)));
                }

                int leafEntries = BinaryReader.ReadInt32();
                CRead.ReadType<ushort>((uint)leafEntries);

                int nStaticProps = BinaryReader.ReadInt32();
                for (int l = 0; l < nStaticProps; l++)
                {
                    StaticPropLumpV4_t StaticPropLump_t = CRead.ReadType<StaticPropLumpV4_t>();
                    switch (gamelumps[i].version)
                    {
                        case 5: CRead.ReadType<StaticPropLumpV5_t>(); break;
                        case 6: CRead.ReadType<StaticPropLumpV6_t>(); break;
                        case 7: CRead.ReadType<StaticPropLumpV7_t>(); break;
                        case 8: CRead.ReadType<StaticPropLumpV8_t>(); break;
                        case 9: CRead.ReadType<StaticPropLumpV9_t>(); break;
                        case 10: CRead.ReadType<StaticPropLumpV10_t>(); break;
                    }

                    // Load studio model and apply position
                    Transform mdlTransform = StudioMdlLoader.LoadMdl(names[StaticPropLump_t.m_PropType].Replace(".mdl", ""));
                    mdlTransform.localPosition = WorldController.SwapZY(StaticPropLump_t.m_Origin) * WorldController.WorldScale;

                    // Calculate rotation for model
                    Vector3 mdlRotation = new Vector3(StaticPropLump_t.m_Angles.z, -StaticPropLump_t.m_Angles.y, StaticPropLump_t.m_Angles.x);
                    mdlTransform.eulerAngles = mdlRotation;

                    mdlTransform.parent = staticProps.transform;
                }
            }
        }
    }

    private static void UnpackPakFile()
    {
        // For fast load if map is already loaded
        if (Directory.Exists(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile"))
            return;

        // Create ".zip" archive without compression
        File.WriteAllBytes(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile.zip", BSP_PakFile);
        Directory.CreateDirectory(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile");

        // Using plugin for unpack archive
        ZipFile PakFile = ZipFile.Read(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile.zip");
        PakFile.ExtractAll(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile"); PakFile.Dispose();

        // Delete a archive from disc
        File.Delete(Application.persistentDataPath + "/" + WorldController.MapName + "_pakFile.zip");
    }
}
