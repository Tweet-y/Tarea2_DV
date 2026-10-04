using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ValidarAnimacionCJ
{
    [MenuItem("Tools/CJ/Verificar animaciones")]
    public static void Ejecutar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Deten Play antes de verificar las animaciones de CJ.");
            return;
        }
        var preview = new PreviewRenderUtility();
        var materials = new List<Material>();
        try
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Personaje/cj/CJ.prefab"));
            preview.AddSingleGO(root);
            var animation = root.GetComponentInChildren<CJLocomotionAnimation>();
            animation.Initialize();
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                {
                    var old = renderer.sharedMaterials;
                    var replacements = new Material[old.Length];
                    for (var i = 0; i < old.Length; i++)
                    {
                        var m = new Material(shader);
                        if (old[i].HasProperty("_BaseMap"))
                        {
                            m.SetTexture("_BaseMap", old[i].GetTexture("_BaseMap"));
                            m.SetTextureScale("_BaseMap", old[i].GetTextureScale("_BaseMap"));
                            m.SetTextureOffset("_BaseMap", old[i].GetTextureOffset("_BaseMap"));
                        }
                        replacements[i] = m;
                        materials.Add(m);
                    }
                    renderer.sharedMaterials = replacements;
                }
            }
            preview.camera.transform.position = new Vector3(2.2f, 1.4f, 3.8f);
            preview.camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            preview.camera.nearClipPlane = 0.01f;
            preview.camera.farClipPlane = 30f;
            preview.camera.fieldOfView = 30f;
            Directory.CreateDirectory("Logs");
            var distances = new System.Text.StringBuilder();
            foreach (var pose in new[] {
                (name: "idle", speed: 0f, phase: 0f, jump: 0f),
                (name: "walk", speed: 2f, phase: 0.8f, jump: 0f),
                (name: "walk2", speed: 2f, phase: 2.4f, jump: 0f),
                (name: "run", speed: 5.335f, phase: 0.8f, jump: 0f),
                (name: "run2", speed: 5.335f, phase: 2.4f, jump: 0f),
                (name: "jump", speed: 0f, phase: 0f, jump: 1f) })
            {
                animation.ApplyPose(pose.speed, pose.phase, pose.jump);
                var torso = root.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in torso)
                {
                    if (filter.name != "TORSO") continue;
                    foreach (var hand in root.GetComponentsInChildren<Transform>())
                    {
                        if (hand.name != "MANO_IZQ" && hand.name != "MANO_DER") continue;
                        var minimum = float.MaxValue;
                        foreach (var v in filter.sharedMesh.vertices)
                            minimum = Mathf.Min(minimum, Vector3.Distance(filter.transform.TransformPoint(v), hand.position));
                        distances.AppendLine(pose.name + " " + hand.name + " wrist distance=" + minimum);
                    }
                }
                preview.BeginPreview(new Rect(0f, 0f, 600f, 600f), GUIStyle.none);
                preview.Render(true);
                preview.Render(true);
                var texture = (RenderTexture)preview.EndPreview();
                var previous = RenderTexture.active;
                RenderTexture.active = texture;
                var image = new Texture2D(600, 600, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0f, 0f, 600f, 600f), 0, 0);
                image.Apply();
                File.WriteAllBytes("Logs/cj-procedural-" + pose.name + ".png", image.EncodeToPNG());
                RenderTexture.active = previous;
                Object.DestroyImmediate(image);
            }
            File.WriteAllText("Logs/cj-procedural-v2-verificado.txt", distances.ToString());
        }
        finally
        {
            preview.Cleanup();
            foreach (var m in materials) Object.DestroyImmediate(m);
        }
    }
}
