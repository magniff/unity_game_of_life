using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneReloader : MonoBehaviour
{
    private void OnMouseDown() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
