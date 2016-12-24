using UnityEngine;
using System.Reflection;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Photograph : MonoBehaviour {

    public Texture2D texture;
    public Material baseMaterial;
    public GameObject plane;

    private Material material;

#if UNITY_EDITOR
    // Update is called once per frame
    void OnEnable()
    {
        material = null;
    }

    void Update() {
        if (!plane)
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.parent = transform;
            plane.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        if (!material)
        {
            Debug.Log("New material created");
            material = new Material(baseMaterial);
            plane.GetComponent<Renderer>().material = material;
        }

        if (material.GetTexture("_MainTex") != texture)
        {
            Vector2 texSize = GetTextureSize();
            float aspect = texSize.x / texSize.y;
            plane.transform.localScale = new Vector3(aspect, 1, 1);
            material.SetTexture("_MainTex", texture);
        }
    }


    private Vector2 GetTextureSize()
    {
        if (texture != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                object[] args = new object[2] { 0, 0 };
                MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(importer, args);

                return new Vector2((int)args[0], (int)args[1]);

            }
        }

        return new Vector2();
    }
#endif
}

