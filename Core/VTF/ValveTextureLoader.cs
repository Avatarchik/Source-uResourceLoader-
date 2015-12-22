using UnityEngine;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System;

public class ValveTextureLoader : VtfSpecification
{
    private static CustomReader CRead;

    private static tagVTFHEADER VTF_Header;
    private static string[] VMT_File = null;
    private static string FindInVMT = null;

    public static Shader MaterialShader;
    public static Color32 MaterialColor;

    public static Material LoadMaterial(string MaterialName)
    {
        // Set default parameters
        MaterialShader = Shader.Find("Lightmapped/Diffuse");
        MaterialColor = new Color32(255, 255, 255, 255);

        Material material = new Material(MaterialShader);
        MaterialName = MaterialName.Replace(".vmt", "");

        // ----- BEGIN PARSE VMT ----- //

        if (File.Exists(WorldController.CurrentTexPath + MaterialName + ".vmt"))
        {
            VMT_File = File.ReadAllLines(WorldController.CurrentTexPath + MaterialName + ".vmt");
            ParseVmtFile(ref FindInVMT);
        }

        while (true)
        {
            if (File.Exists(WorldController.DefaultTexPath + FindInVMT + ".vtf")
                && !File.Exists(WorldController.DefaultTexPath + FindInVMT + ".vmt"))
                break;
            
            if (File.Exists(WorldController.DefaultTexPath + FindInVMT + ".vmt"))
            {
                string _FindInVMT = FindInVMT;

                VMT_File = File.ReadAllLines(WorldController.DefaultTexPath + FindInVMT + ".vmt");
                ParseVmtFile(ref FindInVMT); 

                if (FindInVMT == _FindInVMT) break;
                continue;
            }

            if (File.Exists(WorldController.DefaultTexPath + FindInVMT + ".vtf"))
                break;

            return material;
        }

        // ----- END PARSE VMT ----- //

        // Initialize reader and read VTF header
        CRead = new CustomReader(new BinaryReader(File.OpenRead(WorldController.DefaultTexPath + FindInVMT + ".vtf")));
        VTF_Header = CRead.ReadType<tagVTFHEADER>(0);

        // Apply texture, shader, color to material
        material.mainTexture = GetTexture();
        material.shader = MaterialShader;
        material.color = MaterialColor;

        return material;
    }

    private static void ParseVmtFile(ref string item)
    {
        // Search of the custom shader
        for (int i = 0; i < 2; i++)
        {
            if (!VMT_File[i].Contains("//"))
            {
                if (VMT_File[i].Contains("UnlitGeneric"))
                    MaterialShader = Shader.Find("Mobile/Unlit (Supports Lightmap)");

                if (VMT_File[i].Contains("VertexLitGeneric"))
                    MaterialShader = Shader.Find("Mobile/VertexLit");
            }
        }

        item = null; // Clear old information

        for (int i = 0; i < VMT_File.Length; i++)
        {
            if (!VMT_File[i].Contains("//"))
            {
                List<string> data = new List<string>();
                string pattern = "\"[^\"]*\"";
                    
                foreach (Match match in Regex.Matches(VMT_File[i], pattern, RegexOptions.IgnoreCase))
                    data.Add(match.Value.Trim('"'));

                // Get texture or material name
                foreach (string el in alp)
                {
                    if (VMT_File[i].Contains(el))
                        FindItem(data, ref item);
                }       

                // Get custom color from material
                if ((uint)VMT_File[i].IndexOf("$color") <= 2)
                {
                    if (data.Count > 0)
                    {
                        data[data.Count - 1] = data[data.Count - 1].Replace("[", "").Replace("]", "");
                        string[] colors = data[data.Count - 1].Replace("{", "").Replace("}", "").Split(' ');
                        MaterialColor = new Color32((byte)float.Parse(colors[0]), (byte)float.Parse(colors[1]), (byte)float.Parse(colors[2]), 255);
                    }
                }

                // Check material for transparency
                if (VMT_File[i].Contains("$translucent")
                        || VMT_File[i].Contains("$alphatest")
                        || VMT_File[i].Contains("$AlphaTest"))
                {
                    if (VMT_File [i].Contains ("1"))
                        MaterialShader = Shader.Find ("Lightmapped/Transparent");
                }
            }
        }
 
    }

    private static void FindItem(List<string> data, ref string item)
    {
        if (item != null) return;
        
        string path = data[data.Count - 1].Replace("materials/", "").Replace("\\", "/")
            .Replace(".vmt", "").Replace(".vtf", "");

        if (path.Contains("/"))
        {
            if (CheckExistFile(path))
                item = path;
        }
    }

    // http://nemesis.thewavelength.net/index.php?p=40
    private static Texture2D GetTexture()
    {
        Texture2D VTF_Texture, Mip_Texture;
        int OffsetInFile = VTF_Header.width * VTF_Header.height * uiBytesPerPixels[VTF_Header.highResImageFormat];
        
        switch (VTF_Header.highResImageFormat)
        {
            case (int)ImageFormat.IMAGE_FORMAT_DXT1: 
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.DXT1, false);
                OffsetInFile = ((VTF_Header.width + 3) / 4) * ((VTF_Header.height + 3) / 4) * 8;
                break;
            
            case (int)ImageFormat.IMAGE_FORMAT_DXT3: 
            case (int)ImageFormat.IMAGE_FORMAT_DXT5: 
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.DXT5, false);
                OffsetInFile = ((VTF_Header.width + 3) / 4) * ((VTF_Header.height + 3) / 4) * 16;
                break;

            case (int)ImageFormat.IMAGE_FORMAT_RGB888:
            case (int)ImageFormat.IMAGE_FORMAT_BGR888: 
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.RGB24, false); 
                break;
            
            case (int)ImageFormat.IMAGE_FORMAT_RGBA8888:
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.RGBA32, false); 
                break;

            case (int)ImageFormat.IMAGE_FORMAT_BGRA8888:
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.BGRA32, false); 
                break;
            
            case (int)ImageFormat.IMAGE_FORMAT_ARGB8888:
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.ARGB32, false); 
                break;
            
            case (int)ImageFormat.IMAGE_FORMAT_A8:
                VTF_Texture = new Texture2D(VTF_Header.width, VTF_Header.height, TextureFormat.Alpha8, false); 
                break;
            
            default:
                return new Texture2D(1, 1);
        }

        // Load texture from file
        byte[] VTF_File = FixOperation(CRead.GetBytes(OffsetInFile));
        VTF_Texture.LoadRawTextureData(VTF_File);
        VTF_Texture.Apply ();

        {
            // Apply necessary shader
            if (MaterialShader.Equals (Shader.Find ("Lightmapped/Transparent")))
                Mip_Texture = new Texture2D (VTF_Header.width, VTF_Header.height, TextureFormat.RGBA32, true);
            else Mip_Texture = new Texture2D (VTF_Header.width, VTF_Header.height, TextureFormat.RGB24, true);

            // Generation mips and compress
            Mip_Texture.SetPixels32 (VTF_Texture.GetPixels32 ());
            Mip_Texture.Apply ();
            Mip_Texture.Compress (false);
        }

        return Mip_Texture;
    }

    private static byte[] FixOperation(byte[] input)
    {
        // Convert BRG888 (BGR24) to RGB888 (RGB24)
        if (VTF_Header.highResImageFormat == (int)ImageFormat.IMAGE_FORMAT_BGR888) 
        {
            for (int i = 0; i < input.Length - 1; i += 3) 
            {
                byte temp = input [i];
                input [i] = input [i + 2];
                input [i + 2] = temp;
            }
        }

        return input;
    }

    private static bool CheckExistFile(string input)
    {
        if (File.Exists(WorldController.DefaultTexPath + input + ".vmt")
            || File.Exists(WorldController.DefaultTexPath + input + ".vtf"))
            return true;
        
        return false;
    }
}
