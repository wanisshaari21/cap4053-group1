using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

public static class GenerateShadows
{
    [MenuItem("Tools/2D Lights/Generate Shadow Casters From CompositeCollider2D")]
    public static void Generate()
    {
        var go = Selection.activeGameObject;
        if (!go) { Debug.LogError("Select the Tilemap GameObject that has a CompositeCollider2D."); return; }

        var comp = go.GetComponent<CompositeCollider2D>();
        if (!comp) { Debug.LogError("Selected object has no CompositeCollider2D."); return; }

        // Delete previous generated children
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            if (go.transform.GetChild(i).name.StartsWith("Shadow_"))
                Object.DestroyImmediate(go.transform.GetChild(i).gameObject);

        int pathCount = comp.pathCount;
        var pts = new Vector2[comp.pointCount];

        for (int i = 0; i < pathCount; i++)
        {
            int count = comp.GetPath(i, pts);
            var child = new GameObject($"Shadow_{i}");
            child.transform.SetParent(go.transform, false);

            var sc = child.AddComponent<ShadowCaster2D>();
            sc.useRendererSilhouette = false;
            sc.selfShadows = false;

            // write shape via SerializedObject
            var so = new SerializedObject(sc);
            var shape = so.FindProperty("m_ShapePath");
            shape.arraySize = count;
            for (int p = 0; p < count; p++)
                shape.GetArrayElementAtIndex(p).vector3Value = pts[p];
            so.ApplyModifiedProperties();
        }

        Debug.Log($"Generated {pathCount} ShadowCaster2D objects from CompositeCollider2D.");
    }
}
