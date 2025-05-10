using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoColliderGenerator))]
public class AutoColliderGeneratorEditor : Editor
{
    private SerializedProperty showPreview;
    private SerializedProperty settings;
    private SerializedProperty tag;
    private SerializedProperty layer;
    private SerializedProperty isTrigger;
    private SerializedProperty material;
    private SerializedProperty generatedObjectName;

    private SerializedProperty additionalScripts;


    private void OnEnable()
    {
        showPreview = serializedObject.FindProperty("showPreview");
        settings = serializedObject.FindProperty("settings");
        additionalScripts = serializedObject.FindProperty("additionalScripts");

        if (settings != null)
        {
            tag = settings.FindPropertyRelative("tag");
            layer = settings.FindPropertyRelative("layer");
            isTrigger = settings.FindPropertyRelative("isTrigger");
            material = settings.FindPropertyRelative("material");
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(showPreview, new GUIContent("Preview Colliders"));

        if (settings == null)
        {
            EditorGUILayout.HelpBox("Settings not found or not serializable. Make sure 'ColliderGenerationSettings' is marked as [System.Serializable].", MessageType.Error);
        }
        else
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Collider Generation Settings", EditorStyles.boldLabel);

            var tagList = UnityEditorInternal.InternalEditorUtility.tags;
            int currentTagIndex = System.Array.IndexOf(tagList, tag.stringValue);
            int newTagIndex = EditorGUILayout.Popup("Tag", Mathf.Max(0, currentTagIndex), tagList);
            if (newTagIndex >= 0 && newTagIndex < tagList.Length)
                tag.stringValue = tagList[newTagIndex];

            var layerList = UnityEditorInternal.InternalEditorUtility.layers;
            int currentLayerIndex = System.Array.IndexOf(layerList, LayerMask.LayerToName(layer.intValue));
            int newLayerIndex = EditorGUILayout.Popup("Layer", Mathf.Max(0, currentLayerIndex), layerList);
            if (newLayerIndex >= 0 && newLayerIndex < layerList.Length)
                layer.intValue = LayerMask.NameToLayer(layerList[newLayerIndex]);

            EditorGUILayout.PropertyField(isTrigger);
            EditorGUILayout.PropertyField(material);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Additional Scripts to Add", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(additionalScripts, true);


        EditorGUILayout.Space(10);
        if (GUILayout.Button("Generate Colliders"))
        {
            AutoColliderGenerator script = (AutoColliderGenerator)target;
            Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Generate Colliders");
            script.GenerateColliders();
        }

        if (GUILayout.Button("Remove Generated Colliders (only colliders)"))
        {
            AutoColliderGenerator script = (AutoColliderGenerator)target;
            Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Remove Generated Colliders (only colliders)");
            script.RemoveGeneratedColliders();
        }


        EditorGUILayout.Space(20);

        if (GUILayout.Button("Remove Marker Colliders (for build)"))
        {
            AutoColliderGenerator script = (AutoColliderGenerator)target;
            Undo.RegisterFullObjectHierarchyUndo(script.gameObject, "Remove Marker Colliders (for build)");
            script.RemoveMarkerColliders();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
