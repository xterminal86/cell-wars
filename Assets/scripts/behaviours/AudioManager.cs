using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager> 
{
  public AudioSource AudioSourcePrefab;

  public List<AudioClip> MusicTracks = new List<AudioClip>();

  public float SoundVolume = 1.0f;
  public float MusicVolume = 1.0f;

  Dictionary<string, AudioSource> _audioSourcesByName = new Dictionary<string, AudioSource>();

  bool _initialized = false;
  public override void Initialize()
  {    
    if (_initialized)
    {
      return;
    }      
    
    foreach (var item in MusicTracks)
    {
      if (item == null)
      {
        Debug.LogWarning("Music track didn't load (is null) - rebuild media list in Inspector!");
        continue;
      }

      AudioSource s = (AudioSource)Instantiate(AudioSourcePrefab);
      s.transform.parent = transform;

      s.clip = item;
      s.volume = MusicVolume;
      s.name = item.name;
      s.loop = true;

      _audioSourcesByName.Add(s.name, s);
    }

    _initialized = true;
  }

  string _currentMusicTrackPlaying = string.Empty;
  bool _isMusicLooped = false;
  public void PlayMusic(string name, bool loop = true)
  {
    _isMusicLooped = loop;
    _currentMusicTrackPlaying = name;
    _audioSourcesByName[name].Play();
  }
   
  bool _musicFading = false;
  public void StopMusic()
  {
    if (_musicFading)
    {
      return;
    }

    _musicFading = true;
    StartCoroutine(FadeOutMusicRoutine());
  }

  IEnumerator FadeOutMusicRoutine()
  {
    float volume = MusicVolume;
    while (volume > 0.0f)
    {
      volume -= Time.smoothDeltaTime;
      _audioSourcesByName[_currentMusicTrackPlaying].volume -= Time.smoothDeltaTime;

      yield return null;
    }

    _audioSourcesByName[_currentMusicTrackPlaying].Stop();
    _audioSourcesByName[_currentMusicTrackPlaying].volume = MusicVolume;

    _musicFading = false;

    yield return null;
  }

  void Update()
  {
    if (_isMusicLooped)
    {
      if (_audioSourcesByName[_currentMusicTrackPlaying].timeSamples > GlobalConstants.MusicTrackByName[_currentMusicTrackPlaying].Y)
      {  
        _audioSourcesByName[_currentMusicTrackPlaying].timeSamples = GlobalConstants.MusicTrackByName[_currentMusicTrackPlaying].X;
      }
    }
  }
}
