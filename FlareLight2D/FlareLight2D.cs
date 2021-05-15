using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class FlareLight2D : MonoBehaviour
{
    protected Light2D myLight;
    public float FlareRradius = 0.1f;
    [Tooltip("The layer the player is on")]
    public LayerMask PlayerMask = LayerManager.PlayerLayerMask;
    float lastIntensity;

    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light2D>();

    }

    // Update is called once per frame
    void Update()
    {

        if(Physics2D.OverlapCircle(transform.position, FlareRradius, PlayerMask))
        {
            myLight.enabled = false;
        }
        else
        {
            myLight.enabled=true;
        }
    }
}
