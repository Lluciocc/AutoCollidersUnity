using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoColliderGenerator))]
public class AutoColliderGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AutoColliderGenerator script = (AutoColliderGenerator)target;

        GUILayout.Space(10);
        script.showPreview = EditorGUILayout.Toggle("Preview Colliders", script.showPreview);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Colliders"))
        {
            Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Generate Colliders");
            script.GenerateColliders();
        }

        if (GUILayout.Button("Remove Generated Colliders"))
        {
            Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Remove Generated Colliders");
            script.RemoveGeneratedColliders();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(script);
    }
}
