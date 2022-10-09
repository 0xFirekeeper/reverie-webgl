// Filename: SoundManager.cs
// Author: 0xFirekeeper
// Description: Provides a serializable dictionary setup and interface to play sounds 
//              easily, can be used for Event Triggers on buttons as well.

using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

public enum SoundTrigger
{
    CorrectAnswer,
    WrongAnswer,
    LevelFailed,
    LevelWon,
    Revive
}

[System.Serializable]
public class Sounds : SerializableDictionaryBase<SoundTrigger, AudioSource> { }

public class SoundManager : MonoBehaviour
{
    public Sounds sounds;

    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(SoundTrigger soundTrigger, bool onlyIfThisSoundNotPlaying = false, bool onlyIfNoSoundsPlaying = false)
    {
        if (onlyIfThisSoundNotPlaying)
        {
            if (sounds[soundTrigger].isPlaying)
                return;
        }
        else if (onlyIfNoSoundsPlaying)
        {
            foreach (var sound in sounds)
            {
                if (sound.Value.isPlaying)
                    return;
            }
        }

        sounds[soundTrigger].Play();
    }

    public void TryPlaySound(string sound)
    {
        SoundTrigger soundTrigger;
        if (System.Enum.TryParse<SoundTrigger>(sound, out soundTrigger))
        {
            sounds[soundTrigger].Play();
        }
    }
}
