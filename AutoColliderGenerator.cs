using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class ColliderGenerationSettings
{
    public string tag = null;
    public int layer = 0;
    public bool isTrigger = false;
    public PhysicMaterial material = null;
}

public class AutoColliderGenerator : MonoBehaviour
{
    [HideInInspector] public bool showPreview = false;
    public ColliderGenerationSettings settings = new ColliderGenerationSettings();

#if UNITY_EDITOR
    public List<MonoScript> additionalScripts = new List<MonoScript>();
#endif

    public void GenerateColliders() => AddCollidersRecursively(transform);
    public void RemoveGeneratedColliders() => RemoveCollidersRecursively(transform);
    public void RemoveMarkerColliders() => RemoveMarkerCollidersRecursively(transform);

    private void AddCollidersRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                if (child.GetComponent<Collider>() != null) continue;

                Bounds bounds = meshFilter.sharedMesh.bounds;
                Vector3 size = bounds.size;
                Collider newCollider = null;

                if (IsSphere(size))
                {
                    var sphere = child.gameObject.AddComponent<SphereCollider>();
                    sphere.center = bounds.center;
                    sphere.radius = Mathf.Max(size.x, size.y, size.z) / 2f;
                    newCollider = sphere;
                }
                else if (IsCapsule(size))
                {
                    var capsule = child.gameObject.AddComponent<CapsuleCollider>();
                    capsule.center = bounds.center;
                    capsule.height = Mathf.Max(size.y, size.x, size.z);
                    capsule.radius = Mathf.Min(size.x, size.z) / 2f;
                    capsule.direction = GetCapsuleDirection(size);
                    newCollider = capsule;
                }
                else
                {
                    var box = child.gameObject.AddComponent<BoxCollider>();
                    box.center = bounds.center;
                    box.size = size;
                    newCollider = box;
                }

                if (newCollider != null)
                {
                    if (settings.tag != null)
                        child.gameObject.tag = settings.tag;
                    if (settings.layer != 0)
                        child.gameObject.layer = settings.layer;

                    newCollider.isTrigger = settings.isTrigger;
                    newCollider.material = settings.material;

#if UNITY_EDITOR
                    foreach (var script in additionalScripts)
                    {
                        if (script == null) continue;
                        var type = script.GetClass();
                        if (type != null && child.GetComponent(type) == null)
                        {
                            child.gameObject.AddComponent(type);
                        }
                    }
#endif

                    child.gameObject.AddComponent<GeneratedColliderMarker>();
                }
                    
            }

            AddCollidersRecursively(child);
        }
    }

    private void RemoveCollidersRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var marker = child.GetComponent<GeneratedColliderMarker>();
            if (marker != null)
            {
                var collider = child.GetComponent<Collider>();
                if (collider != null) DestroyImmediate(collider);
                DestroyImmediate(marker);
            } 
            else
            {
                Debug.LogWarning("AutoCollidersGenerator: It seems like there is no marker in this component. Did you remove it ?");
            }

            RemoveCollidersRecursively(child);
        }
    }

    private void RemoveMarkerCollidersRecursively(Transform parent)
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Confirmation",
            "Are you sure you want to remove all collider markers? You cannot undo!",
            "Yes, do it",
            "No"
        );

        if (confirm)
        {
            RemoveMarkers(parent); 
        }
        else
        {
            Debug.Log("Removal canceled.");
        }
    }

    private void RemoveMarkers(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var marker = child.GetComponent<GeneratedColliderMarker>();
            if (marker != null)
            {
                DestroyImmediate(marker);
            }

            RemoveMarkers(child);
        }
    }


    private bool IsSphere(Vector3 size)
    {
        float tolerance = 0.2f;
        float avg = (size.x + size.y + size.z) / 3f;
        return Mathf.Abs(size.x - avg) < tolerance * avg &&
               Mathf.Abs(size.y - avg) < tolerance * avg &&
               Mathf.Abs(size.z - avg) < tolerance * avg;
    }

    private bool IsCapsule(Vector3 size)
    {
        float tolerance = 0.2f;
        float[] dims = { size.x, size.y, size.z };
        System.Array.Sort(dims);
        bool flatEnds = Mathf.Abs(dims[0] - dims[1]) < tolerance * dims[1];
        bool longBody = dims[2] > dims[1] * 1.5f;
        return flatEnds && longBody;
    }

    private int GetCapsuleDirection(Vector3 size)
    {
        if (size.y > size.x && size.y > size.z) return 1;
        if (size.x > size.y && size.x > size.z) return 0;
        return 2;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showPreview) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        DrawColliderPreview(transform);
    }

    private void DrawColliderPreview(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.GetComponent<Collider>() != null) continue;

            var renderer = child.GetComponent<Renderer>();
            if (renderer == null) continue;

            Bounds bounds = renderer.bounds;

            Gizmos.matrix = Matrix4x4.identity;

            if (IsSphere(bounds.size))
            {
                Gizmos.DrawWireSphere(bounds.center, Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 2f);
            }
            else if (IsCapsule(bounds.size))
            {
                DrawWireCapsule(bounds.center, bounds.size, GetCapsuleDirection(bounds.size));
            }
            else
            {
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }

            DrawColliderPreview(child);
        }
    }

    private void DrawWireCapsule(Vector3 center, Vector3 size, int direction)
    {
        float radius;
        float height;
        Vector3 up = Vector3.up;

        switch (direction)
        {
            case 0:
                up = Vector3.right;
                radius = Mathf.Min(size.y, size.z) / 2f;
                height = size.x;
                break;
            case 2:
                up = Vector3.forward;
                radius = Mathf.Min(size.x, size.y) / 2f;
                height = size.z;
                break;
            default:
                up = Vector3.up;
                radius = Mathf.Min(size.x, size.z) / 2f;
                height = size.y;
                break;
        }

        float cylinderHeight = Mathf.Max(0, height - 2 * radius);
        Vector3 top = center + up * (cylinderHeight / 2);
        Vector3 bottom = center - up * (cylinderHeight / 2);
        Gizmos.DrawWireCube(center, size - 2 * radius * up);
        Gizmos.DrawWireSphere(top + up * radius, radius);
        Gizmos.DrawWireSphere(bottom - up * radius, radius);
    }

#endif
}
