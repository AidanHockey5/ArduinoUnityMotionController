using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    static GameObject musicInstance;
    public AudioClip[] tracks;
    public float fadeInSpeed = 1f;
    public float leadTime = 10f;
    int trackCount = 0;

    private void Awake()
    {
        if (musicInstance)
            Destroy(gameObject);
        else
        {
            musicInstance = this.gameObject;
            DontDestroyOnLoad(musicInstance);
        }
    }

    public void StartMusic()
    {
        StartCoroutine(FadeControl());
    }

    IEnumerator FadeControl()
    {
        while(true)
        {
            GameObject audioGO = new GameObject("Audio_Track");
            DontDestroyOnLoad(audioGO);
            AudioSource aud = audioGO.AddComponent<AudioSource>();
            aud.clip = tracks[trackCount];
            float clipTime = aud.clip.length;
            aud.volume = 0;
            aud.Play();
            while (aud.volume != 1f)
            {
                aud.volume = Mathf.MoveTowards(aud.volume, 1f, Time.deltaTime * fadeInSpeed);
                yield return null;
            }
            yield return new WaitForSeconds(clipTime-leadTime);
            StartCoroutine(FadeOutAudio(aud));
            if (trackCount + 1 >= tracks.Length)
                trackCount = 0;
            else
                trackCount++;
        }
    }
    IEnumerator FadeOutAudio(AudioSource aud)
    {
        while (aud.volume != 0f)
        {
            aud.volume = Mathf.MoveTowards(aud.volume, 0f, Time.deltaTime * fadeInSpeed);
            yield return null;
        }
        Destroy(aud.gameObject, leadTime);
    }
}
