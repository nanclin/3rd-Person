using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCulling : MonoBehaviour {

    [SerializeField] private CharController CharController = null;
    [SerializeField] private Renderer Renderer = null;
    [SerializeField] private GameObject GameObject = null;
    [SerializeField] private float MinCullTreshold = -1;
    [SerializeField] private float MaxCullTreshold = 3;
    
    // Update is called once per frame
    void Update () {
        float d = CharController.transform.position.y - transform.position.y;
        bool inRange = d > MinCullTreshold && d < MaxCullTreshold;
        if (Renderer.enabled && !inRange)
        {
            Renderer.enabled = false;
            GameObject.SetActive(false);
        }
        else if (!Renderer.enabled && inRange) {
            Renderer.enabled = true;
            GameObject.SetActive(true);
        }
    }
}
