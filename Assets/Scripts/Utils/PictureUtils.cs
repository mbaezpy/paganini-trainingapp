using System;
using UnityEngine;
using UnityEngine.UI;


public static class PictureUtils 
{

    public static byte[] ConvertBase64ToByteArray(string base64){
        if (!string.IsNullOrWhiteSpace(base64)){
            return Convert.FromBase64String(base64);
        }
        return null;
    }

    public static string ConvertByteArrayToBase64(byte[] byteArray){
        return byteArray != null ? Convert.ToBase64String(byteArray) : null;
    }

    public static void RenderPicture(RawImage image, byte[] imageBytes, bool destroyPrevious = true)
    {

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        RenderPicture(image, texture, destroyPrevious);
    } 

    public static void RenderPicture(RawImage image, Texture2D texture,  bool destroyPrevious = true)  {

        if (image.texture != null && destroyPrevious)
        {
            UnityEngine.Object.DestroyImmediate(image.texture, true);
        }

        // Set the aspect ratio of the image
        var aspectFitter = image.GetComponent<AspectRatioFitter>();
        if (aspectFitter != null){
            aspectFitter.aspectRatio = (float)texture.width / (float)texture.height;
        }

        image.texture = texture;
        image.gameObject.SetActive(true);
    }

    public static Texture2D LoadImage(string path)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (System.IO.File.Exists(path))
        {
            fileData = System.IO.File.ReadAllBytes(path);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }    

    public static byte[] LoadImageFile(string path)
    {
        byte[] fileData = null;

        if (System.IO.File.Exists(path))
        {
            fileData = System.IO.File.ReadAllBytes(path);
        }
        return fileData;
    }       

}
