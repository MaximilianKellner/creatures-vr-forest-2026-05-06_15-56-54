using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName()
    {
        SceneManager.LoadScene("7_PlayerMovement");
    }
}