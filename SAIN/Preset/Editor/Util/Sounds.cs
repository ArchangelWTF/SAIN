using System.Reflection;
using Comfort.Common;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace SAIN.Editor;

internal class Sounds
{
    private static GUISounds GUISounds
    {
        get { return Singleton<GUISounds>.Instance; }
    }

    private static UISoundsWrapper _soundsWrapper;
    private static AudioSource _audioSource;

    // These are looked up by name, so a client-side rename silently breaks them
    // rather than failing the build -- SPT 4.1 renamed uisoundsWrapper_0 to _UISounds.
    private static readonly FieldInfo _wrapperField = AccessTools.Field(typeof(GUISounds), "_UISounds");
    private static readonly FieldInfo _audioSourceField = AccessTools.Field(typeof(GUISounds), "audioSource_0");

    private static void getWrapper()
    {
        GUISounds sounds = GUISounds;
        if (sounds == null)
        {
            return;
        }
        _soundsWrapper = _wrapperField?.GetValue(sounds) as UISoundsWrapper;
        _audioSource = _audioSourceField?.GetValue(sounds) as AudioSource;
    }

    public static void PlaySound(EUISoundType soundType, float volume = 1f)
    {
        volume = Mathf.Clamp(volume, 0f, 1f);
        if (_soundsWrapper == null)
        {
            getWrapper();
        }
        if (SoundLimiter < Time.time)
        {
            SoundLimiter = Time.time + 0.05f;
            playSound(soundType, volume);
        }
    }

    private static void playSound(EUISoundType soundType, float volume)
    {
        if (_soundsWrapper == null || _audioSource == null)
        {
#if DEBUG
            Logger.LogWarning($"null");
#endif
            Singleton<GUISounds>.Instance.PlayUISound(soundType);
        }
        else
        {
            var clip = _soundsWrapper.GetUIClip(soundType);
            if (clip == null)
            {
                return;
            }
            _audioSource.PlayOneShot(clip, volume);
        }
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            Logger.LogDebug(soundType);
        }
#endif
    }

    private static float SoundLimiter;
}
