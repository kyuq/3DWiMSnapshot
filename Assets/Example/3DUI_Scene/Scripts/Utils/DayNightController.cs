using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public static DayNightController Instance = null;

    public Material SkyBoxBlended;
    public Light LightDirection;

    public bool TriggerNight = false;
    public bool TriggerDay = false;

    [Serializable]
    public class Lamps
    {
        [HideInInspector] public bool IsOn;
        public Light LightSource;
        public float LightIntensity;
        public float ThresholdBrightness;
        public Material BulbMaterial;
    }
    public List<Lamps> LampsInScene;

    private void Start()
    {
        Instance = this;

        if (SkyBoxBlended)
            SkyBoxBlended.SetFloat("_Blend", 0);
        if (LightDirection)
            LightDirection.transform.rotation = Quaternion.Euler(112f, 0, 0);

        SetLampsOff(0);
    }

    private void Update()
    {
        if (TriggerNight || Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
        {
            TriggerNight = false;
            SetToNight();
        }

        if (TriggerDay || Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            TriggerDay = false;
            SetToDay();
        }
    }

    public void SetToNight()
    {
        StartCoroutine(SetToNight_Coroutine());
    }

    private IEnumerator SetToNight_Coroutine()
    {
        float blendValue = 0;
        while (blendValue < 1)
        {
            blendValue += 0.2f * Time.deltaTime;
            if (SkyBoxBlended)
                SkyBoxBlended.SetFloat("_Blend", blendValue);

            if (LightDirection)
                LightDirection.transform.rotation = Quaternion.Euler(112f, 0, 0) * Quaternion.Euler(blendValue * 180f, 0, 0);
            
            SetLampsOn(blendValue);

            yield return null;
        }
        RocketLauncher.Instance.IsFiring = true;
    }

    public void SetToDay()
    {
        StartCoroutine(SetToDay_Coroutine());
    }

    private IEnumerator SetToDay_Coroutine()
    {
        RocketLauncher.Instance.IsFiring = false;
        float blendValue = 1;
        while (blendValue > 0)
        {
            blendValue -= 0.2f * Time.deltaTime;
            if (SkyBoxBlended)
                SkyBoxBlended.SetFloat("_Blend", blendValue);

            if (LightDirection)
                LightDirection.transform.rotation = Quaternion.Euler(292f, 0, 0) * Quaternion.Euler((1-blendValue) * 180f, 0, 0);

            SetLampsOff(blendValue);

            yield return null;
        }
    }

    private void SetLampsOn(float currentBrightness)
    {
        for (int i = 0; i < LampsInScene.Count; i++)
        {
            var l = LampsInScene[i];
            if (!l.IsOn)
            {
                if (l.ThresholdBrightness < currentBrightness)
                {
                    l.IsOn = true;
                    if(l.LightSource) l.LightSource.intensity = l.LightIntensity;
                    if(l.BulbMaterial) l.BulbMaterial.EnableKeyword("_EMISSION");

                }
            }
        }
    }

    private void SetLampsOff(float currentBrightness)
    {
        for (int i = 0; i < LampsInScene.Count; i++)
        {
            var l = LampsInScene[i];
            if (l.IsOn)
            {
                if (l.ThresholdBrightness > currentBrightness)
                {
                    l.IsOn = false;
                    if(l.LightSource) l.LightSource.intensity = 0;
                    if (l.BulbMaterial) l.BulbMaterial.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (SkyBoxBlended)
            SkyBoxBlended.SetFloat("_Blend", 0);
        if (LightDirection)
            LightDirection.transform.rotation = Quaternion.Euler(112f, 0, 0);

        SetLampsOff(0);
    }
}
