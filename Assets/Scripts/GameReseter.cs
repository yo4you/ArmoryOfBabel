using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameReseter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6)) 
        {
			string currentSceneName = SceneManager.GetActiveScene().name;
			SceneManager.LoadScene(currentSceneName);
		}
    }
}
