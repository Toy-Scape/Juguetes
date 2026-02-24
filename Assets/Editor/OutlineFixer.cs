using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class OutlineFixer : EditorWindow
{
    [MenuItem("Tools/QuickOutline/Fix All Mesh Readability")]
    public static void FixAllMeshReadability()
    {
        var outlines = FindObjectsOfType<Outline>();
        var meshesToFix = new HashSet<Mesh>();
        int fixedCount = 0;

        foreach (var outline in outlines)
        {
            // Check MeshFilters
            var meshFilters = outline.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh != null && !mf.sharedMesh.isReadable)
                {
                    meshesToFix.Add(mf.sharedMesh);
                }
            }

            // Check SkinnedMeshRenderers
            var skinnedMeshRenderers = outline.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in skinnedMeshRenderers)
            {
                if (smr.sharedMesh != null && !smr.sharedMesh.isReadable)
                {
                    meshesToFix.Add(smr.sharedMesh);
                }
            }
        }

        if (meshesToFix.Count == 0)
        {
            EditorUtility.DisplayDialog("Outline Fixer", "All meshes used by Outline components are already readable.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Outline Fixer", 
            $"Found {meshesToFix.Count} meshes that need 'Read/Write Enabled' to work with QuickOutline.\n\nThis will modify import settings and reimport assets. Continue?", 
            "Yes, Fix Them", "Cancel");

        if (!confirm) return;

        foreach (var mesh in meshesToFix)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(path)) continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                Debug.Log($"[OutlineFixer] Fixed 'Read/Write Enabled' for: {path}");
                fixedCount++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Outline Fixer", $"Successfully fixed {fixedCount} assets.", "OK");
    }
}
