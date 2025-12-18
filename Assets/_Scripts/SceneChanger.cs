using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(sceneToLoad == "HubScene")
            {
                GameManager.AdvanceRun();
            }
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}