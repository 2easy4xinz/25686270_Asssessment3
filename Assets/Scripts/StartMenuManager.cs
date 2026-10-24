using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickAudio;

    public void LoadLevel1()
    {
        PlayClickSoundAndLoad("Level1"); 
    }

    public void LoadLevel2()
    {
        
        PlayClickSoundAndLoad("SampleScene");
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