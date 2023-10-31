using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager i;
    
    public float masterVolume = 1;
    public int poolSize = 20;

    public float randPitchAmplitude = 0.1f;

    private AudioSource[] audioSources;

    public ClipInfo[] clips;

    private List<LoopSoundHandle> loopSounds = new List<LoopSoundHandle>();

    private void Awake()
    {
        if (i != null) 
        {
            Destroy(gameObject); // Prevent loading 2 SoundManagers
            return;
        } 

        i = this;

        DontDestroyOnLoad(gameObject);

        audioSources = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            audioSources[i] = new GameObject("Audio Source").AddComponent<AudioSource>();
            DontDestroyOnLoad(audioSources[i]);
        }
    }

    private void Update()
    {
        foreach (LoopSoundHandle handle in loopSounds)
        {   
            if (!handle.startedLooping)
            {
                if (handle.startClip.clip.length + handle.startTime < Time.time)
                {
                    handle.startedLooping = true;
                    handle.loopHandle = PlaySoundInstance(handle.loopClip, true);
                }
            }
        }
    }

    public static LoopSoundHandle PlayLoopSound(string start, string loop, string end)
    {
        return i.PlayLoopSoundInstance(GetClipByName(start), GetClipByName(loop), GetClipByName(end));
    }

    public static SoundHandle PlaySound(ClipInfo clip, bool loop = false)
    {
        return i.PlaySoundInstance(clip, loop);
    }

    public static SoundHandle PlaySound(string clipName, bool loop = false)
    {
        return i.PlaySoundInstance(GetClipByName(clipName), loop);
    }

    public SoundHandle PlaySoundInstance(ClipInfo clip, bool loop = false)
    {
        int pos = 0;
        for (pos = 0; pos < poolSize; pos++) // Loop through all sources
        {
            if (!audioSources[pos].isPlaying) // Find one that isn't playing
                break;
        }

        if (pos == poolSize) // All already playing
        {
            Debug.LogError("Audio sources pool size exceeded");

            pos = 0;
            float bestImportance = 100000;
            for (int i = 0; i < poolSize; i++)
            {
                float importance = (audioSources[i].loop ? 100 : 0) + audioSources[i].clip.length;

                if (importance < bestImportance)
                {
                    bestImportance = importance; // Find the least important sound to replace
                    pos = i;
                }
            }
        }

        AudioSource source = audioSources[pos];

        source.spatialBlend = 0;
        source.clip = clip.clip;
        source.volume = masterVolume * clip.volume;
        source.pitch = clip.usePitch ? RandPitch() : 1;
        source.loop = loop;
        source.Play();

        return new SoundHandle {
            source = source,
            audioClip = clip.clip,
        };
    }

    public LoopSoundHandle PlayLoopSoundInstance(ClipInfo start, ClipInfo loop, ClipInfo end)
    {
        LoopSoundHandle res = new LoopSoundHandle {
            startClip = start,
            loopClip = loop,
            endClip = end,
            startTime = Time.time
        };

        PlaySoundInstance(start, false);

        Debug.Log("Add");
        loopSounds.Add(res);

        return res;
    }

    public static float RandPitch()
    {
        return UnityEngine.Random.Range(1 - i.randPitchAmplitude * 0.5f, 1 + i.randPitchAmplitude * 0.5f);
    }

    public static ClipInfo GetClipByName(string clipName)
    {
        foreach (ClipInfo clip in i.clips) // OPTI: use a map or something
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }

        Debug.LogError("No clip found with name " + clipName);
        return i.clips[0];
    }

    public class SoundHandle
    {
        public AudioClip audioClip;
        public AudioSource source;

        public void Stop()
        {
            if (source != null && source.clip == audioClip)
            {
                source.Stop();
            }
        }

        public void FadeAndStop(float duration)
        {
            if (source == null || source.clip != audioClip || !source.isPlaying) 
                return;

            AudioSource capturedSource = source;

            LeanTween.value(source.volume, 0, duration).setOnUpdate(t => {
                capturedSource.volume = t;
            });
        }
    }

    
    public class LoopSoundHandle
    {
        public ClipInfo startClip;
        public ClipInfo loopClip;
        public ClipInfo endClip;
        public SoundHandle loopHandle = null;
        public float startTime;
        public bool startedLooping = false;

        public void Stop()
        {
            Debug.Log("STOP - " + (startedLooping ? "" : "NOT STARTED"));

            if (startedLooping)
            {
                loopHandle.Stop();
            }

            i.loopSounds.Remove(this);

            PlaySound(endClip, false);
        }
    }
    
    [System.Serializable]
    public struct ClipInfo
    {
        public AudioClip clip;
        public string name;
        public bool usePitch; 
        public float volume; 
    }
}

