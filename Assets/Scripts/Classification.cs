using System.Collections;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;
using System.Collections.Generic;
using TMPro;
public class Classification : MonoBehaviour
{
    const int IMAGE_SIZE = 224;
    const string INPUT_NAME = "images";


    public NNModel modelSource;
    public TextAsset label_map;
    public TMP_Text resultText;
    IWorker worker;
    string[] labels;
    public Main main;
    public preprocess preprocess;

    void Start()
    {
        var model = ModelLoader.Load(modelSource);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
        LoadLabels();
    }

    void LoadLabels()
    {
        var stringArray = label_map.text.Split('"').Where((item, index) => index % 2 != 0);
        labels = stringArray.Where((x, i) => i % 2 != 0).ToArray();

    }

    void Update()
    {
        WebCamTexture webCamTexture = main.getWebcamTexture();
        if (webCamTexture.didUpdateThisFrame)
        {
            preprocess.ScaleAndCropImage(webCamTexture, IMAGE_SIZE, RunModel);
        }
    }
    void RunModel(byte[] pixels)
    {
        StartCoroutine(RunModelRoutine(pixels));
    }
    IEnumerator RunModelRoutine(byte[] pixels)
     {
        
        Tensor tensor = TransformInput(pixels);
        var inputs = new Dictionary<string, Tensor>();
        inputs.Add(INPUT_NAME, tensor);
        worker.Execute(inputs);
        Tensor output = worker.PeekOutput();
        List<float> results = output.ToReadOnlyArray().ToList();
        int maxIndex = results.IndexOf(results.Max());
        resultText.text = labels[maxIndex];
        tensor.Dispose();
        output.Dispose();
        yield return null;
     }
    Tensor TransformInput(byte[] pixels)
    {
        float[] transformPixels = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            transformPixels[i] = (float)(pixels[i] - 127.0f) / 128.0f;
        }
        return new Tensor(1, IMAGE_SIZE, IMAGE_SIZE, 3, transformPixels);
    }
    
}
