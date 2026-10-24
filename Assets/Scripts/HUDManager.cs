using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickAudio;

    public void ReturnToStartScene()
    {
        PlayClickSoundAndLoad("StartScene"); 
    }

   
    private void PlayClickSoundAndLoad(string sceneName)
    {
        if (buttonClickAudio != null)
        {
            buttonClickAudio.Play();
        }

        
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneName}' is not added to Build Settings yet!");
        }
    }

}