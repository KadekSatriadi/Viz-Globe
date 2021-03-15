using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnityScreenshot : MonoBehaviour
{
    [Header("Cameras and resolution")]
    public List<Camera> cameras;
    public int width;
    public int height;
    [Header("Files")]
    public string absolutePathToFolder;
    public string prefixFileName; 
    [Header("Trigger")]
    public KeyCode saveButton = KeyCode.Space;

    public IEnumerator Shoot(Camera camera, string fileName)
    {

        //create a render texture and set to the camera
        RenderTexture renTex = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        RenderTexture originRenTex = camera.activeTexture;
        camera.targetTexture = renTex;
        camera.forceIntoRenderTexture = true;

        //wait frame finished
        yield return new WaitForEndOfFrame();

        Debug.Log(fileName);

        //get pixels
        Texture2D tex = GetRTPixels(renTex);

        //save
        string path = Path.Combine(absolutePathToFolder, fileName + ".png");
        File.WriteAllBytes(path, tex.EncodeToPNG());

        Debug.Log(path + " has been saved.");
        camera.targetTexture = originRenTex;
        camera.forceIntoRenderTexture = false;
        renTex = null;
    }

    //source: https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
    static public Texture2D GetRTPixels(RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        return tex;
    }

    public void Shoots()
    {
        Debug.Log("Start shooting ...");
        foreach (Camera cam in cameras)
        {
            StartCoroutine(Shoot(cam, prefixFileName + "_Camera_" + cameras.IndexOf(cam).ToString()));
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(saveButton))
        {
            Shoots();
        }
    }
}
