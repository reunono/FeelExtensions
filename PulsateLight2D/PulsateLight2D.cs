using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PulsateLight2D : MonoBehaviour
{
    private Light2D myLight;

    public float maxIntensity = 1f;
    public float minIntensity = 0f;
    public float pulseSpeed = 1f; //here, a value of 0.5f would take 2 seconds and a value of 2f would take half a second

    private float targetIntensity;
    private float currentIntensity;


    void Start()
    {
        targetIntensity = maxIntensity;
        myLight = GetComponent<Light2D>();
    }
    void Update()
    {
        currentIntensity = (Mathf.Sin(Time.time * pulseSpeed) * maxIntensity);
        currentIntensity =  MMMaths.Remap(currentIntensity, -maxIntensity, maxIntensity, 0, maxIntensity);
  
        myLight.intensity = currentIntensity;
    }
}