using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoScreenBehaviour : MonoBehaviour
{
    static bool _shown = false;

    void Start()
    {
        if (_shown)
            Destroy(gameObject);
        else
            Time.timeScale = 0;
        _shown = true;
        
    }

    void Update()
    {
        if (Input.anyKeyDown) 
        {
            Time.timeScale = 1;
            Destroy(gameObject);
        }
    }
}
