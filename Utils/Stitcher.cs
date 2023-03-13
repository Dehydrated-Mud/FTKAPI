using System.Collections.Generic;
using UnityEngine;
namespace FTKAPI.Utils;
public class Stitcher
{
    private class TransformCatalog : Dictionary<string, Transform>
    {
        public TransformCatalog(Transform transform)
        {
            Catalog(transform);
        }

        private void Catalog(Transform transform)
        {
            
            if (ContainsKey(transform.name))
            {
                Remove(transform.name);
                Add(transform.name, transform);
            }
            else
                Add(transform.name, transform);
            foreach (Transform child in transform)
                Catalog(child);
           
        }
    }

    private class DictionaryExtensions
    {
        public static TValue Find<TKey, TValue>(Dictionary<TKey, TValue> source, TKey key)
        {
            source.TryGetValue(key, out var value);
            return value;
        }
    }

    public GameObject Stitch(GameObject sourceClothing, GameObject targetAvatar)
    {
        TransformCatalog transformCatalog = new TransformCatalog(targetAvatar.transform);
        SkinnedMeshRenderer[] componentsInChildren = sourceClothing.GetComponentsInChildren<SkinnedMeshRenderer>();
        GameObject gameObject = AddChild(sourceClothing, targetAvatar.transform);
        SkinnedMeshRenderer[] array = componentsInChildren;
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
        {
            SkinnedMeshRenderer skinnedMeshRenderer2 = AddSkinnedMeshRenderer(skinnedMeshRenderer, gameObject);
            skinnedMeshRenderer2.bones = TranslateTransforms(skinnedMeshRenderer.bones, transformCatalog);
        }
        return gameObject;
    }

    private GameObject AddChild(GameObject source, Transform parent)
    {
        GameObject gameObject = new GameObject(source.name);
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = source.transform.localPosition;
        gameObject.transform.localRotation = source.transform.localRotation;
        gameObject.transform.localScale = source.transform.localScale;
        return gameObject;
    }

    private SkinnedMeshRenderer AddSkinnedMeshRenderer(SkinnedMeshRenderer source, GameObject parent)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = parent.AddComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.sharedMesh = source.sharedMesh;
        skinnedMeshRenderer.materials = source.materials;
        return skinnedMeshRenderer;
    }

    private Transform[] TranslateTransforms(Transform[] sources, TransformCatalog transformCatalog)
    {
        Transform[] array = new Transform[sources.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            array[i] = DictionaryExtensions.Find(transformCatalog, sources[i].name);
        }
        return array;
    }
}




