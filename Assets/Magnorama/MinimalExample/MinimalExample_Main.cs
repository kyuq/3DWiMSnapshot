using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Magnorama;

public class MinimalExample_Main : Mag_Controller
{

    private Mag_RGBD NormalMiddleView;

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        
        CreateMagnoramaRGBD("RGBDView", Mag_Camera.RenderViewPoint.NormalMiddle, out var n_depth, out var n_color);
        NormalMiddleView = new Mag_RGBD(n_depth, n_color);

        Resources.UnloadUnusedAssets();
    }



    public override void UpdateDepthFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture depth, Vector3 pos, Quaternion rot, float fov, float near, float far)
    {
        switch (viewpoint)
        {
            case Mag_Camera.RenderViewPoint.NormalMiddle:
                if (NormalMiddleView != null)
                    NormalMiddleView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            default:
                Debug.Log(System.Enum.GetName(typeof(Mag_Camera.RenderViewPoint), viewpoint) + "not implemented for this example");
                break;
        }
    }

    public override void UpdateColorFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture color)
    {
        if (Instance)
            switch (viewpoint)
            {
                case Mag_Camera.RenderViewPoint.NormalMiddle:
                    if (NormalMiddleView != null)
                        NormalMiddleView.UpdateColorFrame(ref color);
                    break;
                default:
                    Debug.Log(System.Enum.GetName(typeof(Mag_Camera.RenderViewPoint), viewpoint) + "not implemented for this example");
                    break;
            }
    }


    public void SnapShot()
    {
        SoundManager.PlayCapture(Mag.Instance.transform.position);

        GameObject SnapShotG = new GameObject("Snapshot_" + DateTime.Now.ToString("T"));

        Mag_RGBD[] views = new Mag_RGBD[]
        {
            NormalMiddleView
        };

        SnapShotG.AddComponent<Snapshot3D>().CreatePhoto(views);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (Input.GetKeyDown(KeyCode.Space)) SnapShot();

        float maxDistanceThresholdDepth = 0.015f * (ROI.Instance.Scale / Mag.Instance.Scale);

        NormalMiddleView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;

        NormalMiddleView.Update();

    }

    private void OnDestroy()
    {
        NormalMiddleView.Release();
    }
}
