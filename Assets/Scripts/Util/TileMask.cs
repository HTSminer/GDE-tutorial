using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TileMask : MonoBehaviour
{
    private Light2D globalLight;
    public Material maskMaterial;
    public Color maskColor = Color.black;

    void Start()
    {
        globalLight = FindObjectOfType<Light2D>(); // Find the global Light2D in the scene
        if (maskMaterial != null)
        {
            maskMaterial.SetColor("_MaskColor", maskColor);
        }
    }

    void OnRenderObject()
    {
        if (globalLight != null && maskMaterial != null)
        {
            maskMaterial.SetPass(0);
            Graphics.DrawMeshNow(GetComponent<MeshFilter>().mesh, transform.localToWorldMatrix);
        }
    }
}