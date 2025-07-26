using System.Collections;
using UnityEngine;

public class QuequedSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    public AudioSource Source => _source;

    [SerializeField] private AudioClip[] _clips;

    public void Play()
    {
        _source.clip = _clips[0];
        NineSidesSFXPlayer.Instance.Enqueue(this);
        StartCoroutine(DisposeAfterPlay());
    }

    private IEnumerator DisposeAfterPlay()
    {
        float timer = 0f;
        while (timer < _source.clip.length)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        NineSidesSFXPlayer.Instance.Dequeue(this);
    }

    public void PlayRandom()
    {
        _source.clip = _clips[Random.Range(0, _clips.Length)];
        NineSidesSFXPlayer.Instance.Enqueue(this);
    }

    private void OnDestroy()
    {
        NineSidesSFXPlayer.Instance.Dequeue(this);
    }
}
