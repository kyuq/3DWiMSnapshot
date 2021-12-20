using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{

    public static RocketLauncher Instance = null;
    public bool IsFiring = false;

    public List<GameObject> RocketPrefabs;

    public List<GameObject> RocketLaunchLocations;

    public float ChanceToLaunch;

    private Dictionary<GameObject, DateTime> LiveDuration = new Dictionary<GameObject, DateTime>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(IsFiring)
        {
            if (UnityEngine.Random.Range(0, 100) < ChanceToLaunch)
            {
                var prefabID = UnityEngine.Random.Range(0, RocketPrefabs.Count - 1);
                var locationID = UnityEngine.Random.Range(0, RocketLaunchLocations.Count - 1);

                GameObject newRocket = Instantiate(RocketPrefabs[prefabID]);
                newRocket.transform.position = RocketLaunchLocations[locationID].transform.position;

                LiveDuration[newRocket] = DateTime.Now;
            }
        }


        List<GameObject> toDelete = new List<GameObject>();
        foreach(var g in LiveDuration)
        {
            if ((DateTime.Now - g.Value).TotalSeconds > 10)
            {
                toDelete.Add(g.Key);
            }
        }
     

        foreach (var g in toDelete)
        {
            LiveDuration.Remove(g);
            Destroy(g);
        }
         

    }


}
