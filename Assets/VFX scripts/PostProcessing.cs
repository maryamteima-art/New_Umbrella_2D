using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostProcessing : MonoBehaviour
{
    //Assign the material with the shader (.shader code + LUT_test png mapped to it)
    public Material postProcessingMaterial; 

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (postProcessingMaterial != null)
        {
            //Pass the source texture to the material and process it
            Graphics.Blit(source, destination, postProcessingMaterial);
        }
        else
        {
            //If no material, just render the original image
            Graphics.Blit(source, destination);
        }
    }
}
