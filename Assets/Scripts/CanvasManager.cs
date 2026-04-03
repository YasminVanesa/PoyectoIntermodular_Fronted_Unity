using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public void SceneHome()
    {
        SceneManager.LoadScene("Home");
    }
    public void SceneRegister()
    {
        SceneManager.LoadScene("Register");
    }
}
