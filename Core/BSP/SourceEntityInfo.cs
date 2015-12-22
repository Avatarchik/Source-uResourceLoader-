using UnityEngine;
using System.Collections.Generic;

public class SourceEntityInfo : MonoBehaviour 
{
    public int ModelId;
    public int EntityId;

    public List<string> baseDescription = new List<string>();

    public string classname = null;
    public string targetname = null;
    public string target = null;

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(base.transform.position, Vector3.one / 8f);
    }

    public void Configure()
    {
        classname = baseDescription[baseDescription.FindIndex(n => n == "classname") + 1];
        targetname = baseDescription[baseDescription.FindIndex(n => n == "targetname") + 1];
        target = baseDescription[baseDescription.FindIndex(n => n == "target") + 1];

        // Rename an object use entity data
        gameObject.name = targetname + " (" + classname + ")";

        // Get and apply entity position
        if (baseDescription.Contains("origin"))
        {
            string[] array = baseDescription[baseDescription.FindIndex(n => n == "origin") + 1].Split(new char[] { ' ' });
            transform.position = new Vector3(-float.Parse(array[0]) * WorldController.WorldScale, float.Parse(array[2]) * WorldController.WorldScale, -float.Parse(array[1]) * WorldController.WorldScale);
        }

        // Load prop_*dymanic, static, physics, etc.*
        if (classname.Contains("prop_"))
        {
            // Get model name use entity data
            string modelName = baseDescription[baseDescription.FindIndex(n => n == "model") + 1];

            // Load studio model and apply position
            Transform mdlTransform = StudioMdlLoader.LoadMdl (modelName.Replace (".mdl", ""));
            mdlTransform.localPosition = transform.position;

            // Calculate rotation for model 
            // TODO: This is incorrect calculate. Need fix
            string[] array = baseDescription[baseDescription.FindIndex(n => n == "angles") + 1].Split(new char[] { ' ' });
            Vector3 eulerAngles = new Vector3(float.Parse(array[2]), -float.Parse(array[1]), float.Parse(array[0]));

            if (baseDescription.Contains ("pitch"))
                eulerAngles.x = float.Parse(baseDescription [baseDescription.FindIndex (n => n == "pitch") + 1]);
          
            mdlTransform.eulerAngles = eulerAngles;
            mdlTransform.transform.parent = transform; 
        }
    }
}
