using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;
      
    public void PlayAudio()
    {
        audioSource.Play();
    }
   
    public void StopAudio()
    {
        audioSource.Stop();
    }
}