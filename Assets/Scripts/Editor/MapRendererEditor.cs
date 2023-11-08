using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MapRenderer))]
class MapRendererEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Render"))
        {
            MapRenderer mr = target as MapRenderer;
            Camera cam = mr.gameObject.GetComponent<Camera>();

            RenderTexture rt = new RenderTexture(mr.resolution.x, mr.resolution.y, 0);
            RenderTexture finalTex = new RenderTexture(mr.resolution.x, mr.resolution.y, 0);
            rt.Create();

            cam.targetTexture = rt;
            cam.Render(); 

            Graphics.Blit(rt, finalTex, mr.blitMaterial);

            RenderTexture.active = finalTex;

            Texture2D tex = new Texture2D(mr.resolution.x, mr.resolution.y);
            tex.ReadPixels(new Rect(0, 0, mr.resolution.x, mr.resolution.y), 0, 0);

            byte[] bytes = tex.EncodeToPNG();
            string path = Application.dataPath + "/Resources/" + SceneManager.GetActiveScene().name + ".png";
            System.IO.File.WriteAllBytes(path, bytes);

            Debug.Log("Saved image at: " + path);

            RenderTexture.active = null;
            rt.Release();
            finalTex.Release();
        }
    }
}
