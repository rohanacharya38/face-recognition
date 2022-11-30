
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;

public class Main : MonoBehaviour
{

    public const string URL = "http://192.168.254.143:5000";
    public RawImage Rimage;
    public  WebCamTexture webcamTexture;
    Texture webTexture;
    Texture received;
    float aspectRatio;
    bool processOn=false;
    // Start is called before the first frame update
    void Start()
    {
        received = new Texture2D(1, 1);
        webTexture = new Texture2D(1,1);
        webcamTexture = new WebCamTexture();
        webTexture = webcamTexture;

        webcamTexture.Play();
        aspectRatio = (float)webcamTexture.height / webcamTexture.width;
        Rimage.material.mainTexture = webTexture;

    }

    private void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            
            if(Screen.orientation==ScreenOrientation.Portrait || Screen.orientation==ScreenOrientation.PortraitUpsideDown)
            Rimage.rectTransform.localScale = new Vector3(1, aspectRatio, 1);
            else
            Rimage.rectTransform.localScale = new Vector3(1, 1, 1);
            if (processOn)
                Rimage.texture = received;
            else
                Rimage.texture = webTexture;

        }


    }
    public WebCamTexture getWebcamTexture()
    {
        return webcamTexture;
    }
    IEnumerator Upload()
    {

        Texture2D tex = GetReadableTexture2d(Rimage.texture);
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("hello.png", bytes);
        WWWForm form = new WWWForm();
        form.AddField("name", "file");
        form.AddBinaryData("file", bytes, "file.png", "image/png");
        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("ERROR:" + www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }


            byte[] recieved_data = Convert.FromBase64String(www.downloadHandler.text);
            File.WriteAllBytes("recieved.png", recieved_data);
            tex.LoadImage(recieved_data);
            
            received = (Texture)tex;
            //change size of received texture to size of screen
            //received.Resize(Screen.width, Screen.height);
           
        }
    }

    private static Texture2D GetReadableTexture2d(Texture texture)
    {
        var tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );
        Graphics.Blit(texture, tmp);

        var previousRenderTexture = RenderTexture.active;
        RenderTexture.active = tmp;

        var texture2d = new Texture2D(texture.width, texture.height);
        texture2d.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture2d.Apply();
        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(tmp);
        return texture2d;
    }
    

   public void onProcessClick()
    {
        processOn = true;
         StartCoroutine(Upload());
    }
    public void onNextClick()
    {
        processOn = false;
        received = new Texture2D(1, 1);
    }
}
