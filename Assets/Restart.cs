using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    private void OnMouseDown() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
