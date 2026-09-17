using System.Collections;
using UnityEngine;

public class IntroMusicController : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip ghostNormalMusic;
    
    void Start()
    {
        if (audioSource != null && introMusic != null && ghostNormalMusic != null)
        {
            StartCoroutine(PlayIntroThenBackgroundLoop());
        }
    }

    private IEnumerator PlayIntroThenBackgroundLoop()
    {
        audioSource.clip = introMusic;
        audioSource.loop = false;
        audioSource.Play();

        float waitTime = Mathf.Min(introMusic.length, 3.3f);
        yield return new WaitForSeconds(waitTime);

        audioSource.clip = ghostNormalMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}
