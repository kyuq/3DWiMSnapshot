using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Magnorama;
public class Snapshot3DUI_Main : Mag_Controller
{
    public static new Snapshot3DUI_Main Instance;

    [Tooltip("Enabling this flag renders all cameras but requires lots of GPU power." +
        "This is currently only enabled for few frames for taking snapshots")]
    public bool RenderAllCameras = false;

    [Tooltip("A value greater than 0 overrides all automatic depth thresholds to prevent artefacts in the point cloud rendering")]
    public float OverrideDistanceThreshold = -1f;
    private float maxDistanceThresholdDepth = 0.03f;

    // Views onto the region of interest. Only normal view is enabled for every frame.
    private Mag_RGBD NormalMiddleView;
    private Mag_RGBD FrontView;
    private Mag_RGBD LeftView;
    private Mag_RGBD RightView;
    private Mag_RGBD BehindView;
    private Mag_RGBD TopView;
    private Mag_RGBD BottomView;

    // List of placeholders for non-objective snapshots
    public List<GameObject> PlaceHolderPositions;
    private int _NextPlaceHolderPosition = 0;
    private Dictionary<int, GameObject> PhotoID2PhotoObject = new Dictionary<int, GameObject>();

    // Counter for grab priority. A larger value also has higher priority when two or more boxes overlap
    private int GrabPriorityCounter = 1;

    // UI Text Objects
    public GameObject ObjectiveText;
    public GameObject CaptureText;

    // Objectives
    [Serializable]
    public struct Objectives
    {
        public string ID;
        public GameObject PlaceHolder;
        public GameObjective TargetObjective;
        public GameObject SnapshotGO;
    }
    public List<Objectives> ObjectivePositions;

    private void Start()
    {
        Instance = this;

        CaptureText = Instantiate(CaptureText);
        ObjectiveText = Instantiate(ObjectiveText);
        StartCoroutine(HideObjectiveText());
        StartCoroutine(CheckForObjectiveCompletion());

        // Instantiate virtual depth and color cameras using CreateMagnoramaRGBD from Mag_Controller.cs and create a new Mag_RGBD instance
        CreateMagnoramaRGBD("NormalView", Mag_Camera.RenderViewPoint.NormalMiddle, out var n_depth, out var n_color);
        NormalMiddleView = new Mag_RGBD(n_depth, n_color);

        CreateMagnoramaRGBD("FrontView", Mag_Camera.RenderViewPoint.Front, out var f_depth, out var f_color);
        FrontView = new Mag_RGBD(f_depth, f_color);

        CreateMagnoramaRGBD("LeftView", Mag_Camera.RenderViewPoint.Left, out var l_depth, out var l_color);
        LeftView = new Mag_RGBD(l_depth, l_color);

        CreateMagnoramaRGBD("RightView", Mag_Camera.RenderViewPoint.Right, out var r_depth, out var r_color);
        RightView = new Mag_RGBD(r_depth, r_color);

        CreateMagnoramaRGBD("BehindView", Mag_Camera.RenderViewPoint.Behind, out var be_depth, out var be_color);
        BehindView = new Mag_RGBD(be_depth, be_color);

        CreateMagnoramaRGBD("TopView", Mag_Camera.RenderViewPoint.Top, out var t_depth, out var t_color);
        TopView = new Mag_RGBD(t_depth, t_color);

        CreateMagnoramaRGBD("BottomView", Mag_Camera.RenderViewPoint.Bottom, out var b_depth, out var b_color);
        BottomView = new Mag_RGBD(b_depth, b_color);

        Resources.UnloadUnusedAssets();
    }

    // Enables and positions UI object for the objective notification
    public void NotifyUserOfObjective()
    {
        if(ObjectiveText)
        {
            ObjectiveText.SetActive(true);
            ObjectiveText.transform.position = (ControllerManager.PositionLeft + ControllerManager.PositionRight) / 2;
            ObjectiveText.transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }

    // Disables ObjectiveText at the end of every frame
    public IEnumerator HideObjectiveText()
    {
        while(true)
        {
            yield return new WaitForEndOfFrame();
            if(ObjectiveText) ObjectiveText.SetActive(false);
            yield return null;
        }
    }

    // Trigger export of a non-objective snapshot
    public void ExportSnapshot(int id)
    {
        if(PhotoID2PhotoObject.ContainsKey(id))
        {
            PhotoID2PhotoObject[id].GetComponent<Snapshot3D>()?.Export();
        }
    }

    // Trigger export of an objective snapshot
    public void ExportObjectiveSnapshot(string id)
    {
        for(int i=0;i<ObjectivePositions.Count;i++)
        {
            if(ObjectivePositions[i].ID.Equals(id))
            {
                if (ObjectivePositions[i].SnapshotGO)
                    ObjectivePositions[i].SnapshotGO.GetComponent<Snapshot3D>()?.Export();
                return;
            }
        }
    }

    // Trigger snapshot of the current ROI selection
    public void SnapShot()
    {
        StartCoroutine(SnapShotCoroutine());
    }

    private IEnumerator SnapShotCoroutine()
    {
        // Enable all cameras and let them render for few frames. This allows them to initialize internal buffers for the first time.
        RenderAllCameras = true;
        SoundManager.PlayCapture(Mag.Instance.transform.position);

        yield return null; yield return null; yield return null;

        // Create a Gameobject that holds the new Snapshot mesh.
        GameObject SnapShotG = new GameObject("Snapshot_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
        Mag_RGBD[] views = new Mag_RGBD[]
        {
            NormalMiddleView, FrontView, LeftView, RightView, BehindView, TopView, BottomView
        };

        SnapShotG.AddComponent<Snapshot3D>().CreatePhoto(views, GrabPriorityCounter++);


        // Position the snapshot to the row of objectives, or to the non-objective row in the scene.
        bool ObjectiveFound = false;
        for(int i=0;i<ObjectivePositions.Count;i++)
        {
            var o = ObjectivePositions[i];
            if(o.TargetObjective)
            {
                if(o.TargetObjective.ObjectiveFullfilled())
                {
                    ObjectiveFound = true;
                    if (o.SnapshotGO) Destroy(o.SnapshotGO);
                    o.SnapshotGO = SnapShotG;

                    SnapShotG.transform.SetPositionAndRotation(o.PlaceHolder.transform.position, o.PlaceHolder.transform.rotation);
                    SnapShotG.transform.localScale = o.PlaceHolder.transform.localScale;

                    ObjectivePositions[i] = o;
                }
            }
        }
        if(!ObjectiveFound)
        {
            var placeHolder = PlaceHolderPositions[_NextPlaceHolderPosition];
            SnapShotG.transform.SetPositionAndRotation(placeHolder.transform.position, placeHolder.transform.rotation);
            SnapShotG.transform.localScale = placeHolder.transform.localScale;

            if (PhotoID2PhotoObject.ContainsKey(_NextPlaceHolderPosition)) Destroy(PhotoID2PhotoObject[_NextPlaceHolderPosition]);
            PhotoID2PhotoObject[_NextPlaceHolderPosition] = SnapShotG;
            _NextPlaceHolderPosition = (_NextPlaceHolderPosition + 1) % PlaceHolderPositions.Count;
        }

        RenderAllCameras = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (Input.GetKeyDown(KeyCode.Space) || 
            (ControllerManager.GripLeftPressed && ControllerManager.GripRightDown) ||
            (ControllerManager.GripLeftDown && ControllerManager.GripRightPressed))
        {
            SnapShot();
        }

        if(CaptureText)
        {
            CaptureText.transform.position = (ControllerManager.PositionLeft + ControllerManager.PositionRight) / 2;
            CaptureText.transform.LookAt(Camera.main.transform.position, Vector3.up);
        }


        // During triangulating, if depth between two neighboring vertices are greater than this threshold, no triangle will be created. 
        // Thresholding is done on original scale and done before scaling for WiM
        // When the ROI is large, the depth threshold should grow as well since distances between pixels increase.
        // The threshold can not be set abitrarly large since then unwanted triangles will be connected.
        if (OverrideDistanceThreshold > 0.001f)
        {
            maxDistanceThresholdDepth = OverrideDistanceThreshold * ROI.Instance.Scale;
        }
        else
        {
            maxDistanceThresholdDepth = 0.02f * ROI.Instance.Scale;
        }


        NormalMiddleView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        FrontView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        LeftView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        RightView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        BehindView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        TopView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;
        BottomView.maxDistanceThresholdDepth = maxDistanceThresholdDepth;

        // Render and update rendering inside the WiM
        NormalMiddleView.Update();

        if (RenderAllCameras)
        {
            FrontView.EnableRendering = true;
            LeftView.EnableRendering = true;
            RightView.EnableRendering = true;
            BehindView.EnableRendering = true;
            TopView.EnableRendering = true;
            BottomView.EnableRendering = true;

            FrontView.Update();
            LeftView.Update();
            RightView.Update();
            BehindView.Update();
            TopView.Update();
            BottomView.Update();
        }else
        {
            FrontView.EnableRendering = false;
            LeftView.EnableRendering = false;
            RightView.EnableRendering = false;
            BehindView.EnableRendering = false;
            TopView.EnableRendering = false;
            BottomView.EnableRendering = false;
        }



    }

    // Checks if all objectives are found. If yes, trigger night time
    private IEnumerator CheckForObjectiveCompletion()
    {
        for(int i=0;i<ObjectivePositions.Count;i++)
        {
            yield return new WaitUntil(() => ObjectivePositions[i].SnapshotGO != null);
        }
        
        if(DayNightController.Instance)
            DayNightController.Instance.TriggerNight = true;
    }

    // Making sure that all compute buffers are released from virtual cameras
    private void OnDestroy()
    {
        NormalMiddleView.Release();
        FrontView.Release();
        LeftView.Release();
        RightView.Release();
        BehindView.Release();
        TopView.Release();
        BottomView.Release();
    }

    // Update RGBD cameras
    public override void UpdateDepthFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture depth, Vector3 pos, Quaternion rot, float fov, float near, float far)
    {
        switch (viewpoint)
        {
            case Mag_Camera.RenderViewPoint.NormalMiddle:
                if (NormalMiddleView != null)
                    NormalMiddleView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Front:
                if (FrontView != null)
                    FrontView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Left:
                if (LeftView != null)
                    LeftView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Right:
                if (RightView != null)
                    RightView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Behind:
                if (BehindView != null)
                    BehindView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Top:
                if (TopView != null)
                    TopView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
            case Mag_Camera.RenderViewPoint.Bottom:
                if (BottomView != null)
                    BottomView.UpdateDepthFrame(ref depth, pos, rot, fov, near, far);
                break;
        }
    }

    public override void UpdateColorFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture color)
    {
        switch (viewpoint)
        {
            case Mag_Camera.RenderViewPoint.NormalMiddle:
                if (NormalMiddleView != null)
                    NormalMiddleView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Front:
                if (FrontView != null)
                    FrontView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Left:
                if (LeftView != null)
                    LeftView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Right:
                if (RightView != null)
                    RightView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Behind:
                if (BehindView != null)
                    BehindView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Top:
                if (TopView != null)
                    TopView.UpdateColorFrame(ref color);
                break;
            case Mag_Camera.RenderViewPoint.Bottom:
                if (BottomView != null)
                    BottomView.UpdateColorFrame(ref color);
                break;
        }
    }
}
