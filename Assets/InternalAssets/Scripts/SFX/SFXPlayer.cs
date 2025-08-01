using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    public AudioSource Source => _source;
    [SerializeField] private AudioSource _source;


    [SerializeField] private AudioClip[] _clips;

    public void Play()
    {
        _source.clip = _clips[0];
        _source.Play();
    }

    public void Play(AudioClip clip)
    {
        _source.clip = clip;
        _source.Play();
    }

    public void PlayRandom()
    {
        _source.clip = _clips[Random.Range(0, _clips.Length)];
        _source.Play();
    }

    private void OnDestroy()
    {
        _source.Stop();
    }
}