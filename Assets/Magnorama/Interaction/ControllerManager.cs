// Comment out this line if no SteamVR is installed and is not required
#define STEAM_VR

using UnityEngine;


#if STEAM_VR
using Valve.VR;
#endif
public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance = null;
#if STEAM_VR
    public SteamVR_ActionSet VR_ActionSet;
    public SteamVR_Input_Sources LeftHand_Input;
    public SteamVR_Input_Sources RightHand_Input;
#endif
    [Header("Debug")]
    public bool EnableVerbose = false;

    private Transform _CameraRoot;

#if STEAM_VR
    // Trigger
    public static bool TriggerLeftDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetStateDown(Instance.LeftHand_Input); } }
    public static bool TriggerLeftUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetStateUp(Instance.LeftHand_Input); } }
    public static bool TriggerLeftPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetState(Instance.LeftHand_Input); } }

    public static bool TriggerRightDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetStateDown(Instance.RightHand_Input); } }
    public static bool TriggerRightUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetStateUp(Instance.RightHand_Input); } }
    public static bool TriggerRightPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Trigger").GetState(Instance.RightHand_Input); } }

    // Grip
    public static bool GripLeftDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetStateDown(Instance.LeftHand_Input); } }
    public static bool GripLeftUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetStateUp(Instance.LeftHand_Input); } }
    public static bool GripLeftPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetState(Instance.LeftHand_Input); } }

    public static bool GripRightDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetStateDown(Instance.RightHand_Input); } }
    public static bool GripRightUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetStateUp(Instance.RightHand_Input); } }
    public static bool GripRightPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "Grip").GetState(Instance.RightHand_Input); } }

    // Pads or Joystick
    public static bool PadClickLeftDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetStateDown(Instance.LeftHand_Input); } }
    public static bool PadClickLeftUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetStateUp(Instance.LeftHand_Input); } }
    public static bool PadClickLeftPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetState(Instance.LeftHand_Input); } }

    public static bool PadClickRightDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetStateDown(Instance.RightHand_Input); } }
    public static bool PadClickRightUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetStateUp(Instance.RightHand_Input); } }
    public static bool PadClickRightPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadClick").GetState(Instance.RightHand_Input); } }

    public static bool PadTouchLeftDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetStateDown(Instance.LeftHand_Input); } }
    public static bool PadTouchLeftUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetStateUp(Instance.LeftHand_Input); } }
    public static bool PadTouchLeftPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetState(Instance.LeftHand_Input); } }

    public static bool PadTouchRightDown { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetStateDown(Instance.RightHand_Input); } }
    public static bool PadTouchRightUp { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetStateUp(Instance.RightHand_Input); } }
    public static bool PadTouchRightPressed { get { return SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Magnorama_Input", "PadTouch").GetState(Instance.RightHand_Input); } }

    public static Vector2 PadLeftTouch2D { get { return SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Magnorama_Input", "PadTouch2D").GetAxis(Instance.LeftHand_Input); } }
    public static Vector2 PadRightTouch2D { get { return SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Magnorama_Input", "PadTouch2D").GetAxis(Instance.RightHand_Input); } }

    // Pose
    public static Vector3 PositionLeft { get { return Instance._CameraRoot.TransformPoint(SteamVR_Input.GetAction<SteamVR_Action_Pose>("Magnorama_Input", "Pose").GetLocalPosition(Instance.LeftHand_Input)); } }
    public static Vector3 PositionRight { get { return Instance._CameraRoot.TransformPoint(SteamVR_Input.GetAction<SteamVR_Action_Pose>("Magnorama_Input", "Pose").GetLocalPosition(Instance.RightHand_Input)); } }
    public static Quaternion RotationLeft { get { return Instance._CameraRoot.rotation * SteamVR_Input.GetAction<SteamVR_Action_Pose>("Magnorama_Input", "Pose").GetLocalRotation(Instance.LeftHand_Input); } }
    public static Quaternion RotationRight { get { return Instance._CameraRoot.rotation * SteamVR_Input.GetAction<SteamVR_Action_Pose>("Magnorama_Input", "Pose").GetLocalRotation(Instance.RightHand_Input); } }



    private void Awake()
    {
        Instance = this;

        _CameraRoot = Camera.main.transform;
        while (_CameraRoot.parent != null) _CameraRoot = _CameraRoot.parent;

        if (VR_ActionSet == null) VR_ActionSet = SteamVR_Input.GetActionSet("Magnorama_Input");
        VR_ActionSet.Activate();
    }
   

    void Update()
    {
        if(EnableVerbose)
        {
            if (TriggerLeftDown) Debug.Log("Left Trigger Down");
            if (TriggerLeftUp) Debug.Log("Left Trigger Up");

            if (TriggerRightDown) Debug.Log("Right Trigger Down");
            if (TriggerRightUp) Debug.Log("Right Trigger Up");

            if (GripLeftDown) Debug.Log("Left Grip Down");
            if (GripLeftUp) Debug.Log("Left Grip Up");

            if (GripRightDown) Debug.Log("Right Grip Down");
            if (GripRightUp) Debug.Log("Right Grip Up");

            if (PadClickLeftDown) Debug.Log("Left Pad Click Down");
            if (PadClickLeftUp) Debug.Log("Left Pad Click Up");

            if (PadClickRightDown) Debug.Log("Right Pad Click Down");
            if (PadClickRightUp) Debug.Log("Right Pad Click Up");

            if (PadTouchLeftDown) Debug.Log("Left Pad Touch Down");
            if (PadTouchLeftUp) Debug.Log("Left Pad Touch Up");

            if (PadTouchRightDown) Debug.Log("Right Pad Touch Down");
            if (PadTouchRightUp) Debug.Log("Right Pad Touch Up");
        }
    }

    public static void RightVibrate(float duration, float frequency, float amplitude)
    {
        SteamVR_Actions.default_Haptic[Instance.RightHand_Input].Execute(0, duration, frequency, amplitude);
    }

    public static void LeftVibrate(float duration, float frequency, float amplitude)
    {
        SteamVR_Actions.default_Haptic[Instance.LeftHand_Input].Execute(0, duration, frequency, amplitude);
    }
#else

    public static bool TriggerLeftDown { get { return false; } }
    public static bool TriggerLeftUp { get { return false; } }
    public static bool TriggerLeftPressed { get { return false; } }

    public static bool TriggerRightDown { get { return false; } }
    public static bool TriggerRightUp { get { return false; } }
    public static bool TriggerRightPressed { get { return false; } }

    // Grip
    public static bool GripLeftDown { get { return false; } }
    public static bool GripLeftUp { get { return false; } }
    public static bool GripLeftPressed { get { return false; } }

    public static bool GripRightDown { get { return false; } }
    public static bool GripRightUp { get { return false; } }
    public static bool GripRightPressed { get { return false; } }

    // Pads or Joystick
    public static bool PadClickLeftDown { get { return false; } }
    public static bool PadClickLeftUp { get { return false; } }
    public static bool PadClickLeftPressed { get { return false; } }

    public static bool PadClickRightDown { get { return false; } }
    public static bool PadClickRightUp { get { return false; } }
    public static bool PadClickRightPressed { get { return false; } }

    public static bool PadTouchLeftDown { get { return false; } }
    public static bool PadTouchLeftUp { get { return false; } }
    public static bool PadTouchLeftPressed { get { return false; } }

    public static bool PadTouchRightDown { get { return false; } }
    public static bool PadTouchRightUp { get { return false; } }
    public static bool PadTouchRightPressed { get { return false; } }

    public static Vector2 PadLeftTouch2D { get { return Vector2.zero; } }
    public static Vector2 PadRightTouch2D { get { return Vector2.zero; } }

    // Pose
    public static Vector3 PositionLeft { get { return Vector3.zero; } }
    public static Vector3 PositionRight { get { return Vector3.zero; } }
    public static Quaternion RotationLeft { get { return Quaternion.identity; } }
    public static Quaternion RotationRight { get { return Quaternion.identity; } }

    public static void RightVibrate(float duration, float frequency, float amplitude)
    {
    }

    public static void LeftVibrate(float duration, float frequency, float amplitude)
    {
    }
#endif
}
