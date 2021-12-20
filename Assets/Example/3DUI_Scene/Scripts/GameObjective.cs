using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Magnorama;

public class GameObjective : MonoBehaviour
{
    public string ObjectiveName;

    public float AllowedScaleThreshold;

    private void Update()
    {
        if(ObjectiveFullfilled())
        {
            Snapshot3DUI_Main.Instance.NotifyUserOfObjective();
        }
    }

    // Returns true, if the ROI is sufficiently close to this transform and its scale within the AllowedScaleThreshold
    public bool ObjectiveFullfilled()
    {
        if(Vector3.Distance( ROI.Instance.transform.position, transform.position) < ROI.Instance.Scale / 2)
        {
            if(ROI.Instance.Scale < AllowedScaleThreshold)
            {
                return true;
            }
        }
        return false;
    }

}
