using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preloader : MonoBehaviour
{
    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
    }

}
