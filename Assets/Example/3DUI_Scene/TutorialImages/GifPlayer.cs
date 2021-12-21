using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GifPlayer : MonoBehaviour
{
    public Texture[] frames; 
    public int framesPerSecond = 10;


    private MeshRenderer renderer;

    private void Start()
    {
        renderer = GetComponent<MeshRenderer>();
    }
    void Update() 
    {
        if(renderer)
        {
            int index = (int)((Time.time * framesPerSecond) % frames.Length);
            renderer.material.mainTexture = frames[index];
        }

    }
}
