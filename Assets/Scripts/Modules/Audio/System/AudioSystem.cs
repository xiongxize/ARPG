// 所属模块: Audio.System, 职责: 音频系统核心逻辑
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARPG.Interfaces;
using ARPG.Events;
using ARPG.Core.EventBus;
using ARPG.Data.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ARPG.System.Audio
{
    public class AudioSystem : MonoBehaviour, IAudioService
    {
        [Header("音频设置")]
        [SerializeField] private AudioConfig _config;
        private AudioMixer _mixer;

        [Header("Audio Sources")]
        private AudioSource _bgmSource;

        // BGM 状态
        private string _currentBgmKey;
        private Coroutine _bgmFadeCoroutine;
        private float _currentBgmDefaultVolume = 1f;

        // AudioClip 缓存
        private readonly Dictionary<string, AudioClip> _clipCache = new();

        // 音量缓存（与 Mixer 同步）
        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;

        private const string MIXER_MASTER_VOL = "MasterVolume";
        private const string MIXER_BGM_VOL = "BgmVolume";

        private void Awake()
        {
            // 自动创建 BGM AudioSource
            var bgmGo = new GameObject("BGM_Source");
            bgmGo.transform.SetParent(transform);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;
            _bgmSource.playOnAwake = false;

            // 自动加载默认 Mixer（如已创建），不存在时降级为直接音量控制
            _mixer = Resources.Load<AudioMixer>("Audio/Mixer");

            LoadVolumesFromPrefs();

            // 订阅事件
            EventBus<PlayBgmEvent>.Subscribe(OnPlayBgmEvent);
            EventBus<StopBgmEvent>.Subscribe(OnStopBgmEvent);
            EventBus<SetMasterVolumeEvent>.Subscribe(OnSetMasterVolumeEvent);
            EventBus<SetBgmVolumeEvent>.Subscribe(OnSetBgmVolumeEvent);
        }

        private void OnDestroy()
        {
            EventBus<PlayBgmEvent>.Unsubscribe(OnPlayBgmEvent);
            EventBus<StopBgmEvent>.Unsubscribe(OnStopBgmEvent);
            EventBus<SetMasterVolumeEvent>.Unsubscribe(OnSetMasterVolumeEvent);
            EventBus<SetBgmVolumeEvent>.Unsubscribe(OnSetBgmVolumeEvent);

            StopBgmFade();
        }

        // ===== BGM 淡入淡出 =====

        private void StopBgmFade()
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
                _bgmFadeCoroutine = null;
            }
        }

        private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            source.volume = to;
        }

        private IEnumerator FadeOutThenPlay(AudioClip clip, float defaultVolume, float fadeOutDuration, float fadeInDuration)
        {
            float startVol = _bgmSource.volume;
            yield return StartCoroutine(FadeVolume(_bgmSource, startVol, 0f, fadeOutDuration));
            _bgmSource.Stop();
            PlayBgmInternal(clip, defaultVolume, fadeInDuration);
        }

        private IEnumerator StopAfterFade(float delay)
        {
            yield return new WaitForSeconds(delay);
            _bgmSource.Stop();
            _bgmSource.volume = _currentBgmDefaultVolume * _masterVolume * _bgmVolume;
            _bgmFadeCoroutine = null;
        }

        // ===== IAudioService 实现 =====

        public void PlayBgm(string key, float fadeDuration = 1f)
        {
            if (string.IsNullOrEmpty(key) || _config == null) return;

            var entry = _config.BgmList.FirstOrDefault(e => e.Key == key);
            if (entry == null)
            {
                Debug.LogWarning($"[AudioSystem] BGM config not found: {key}");
                return;
            }

            StopBgmFade();

            // 同一首 BGM 正在播放 → 忽略
            if (_bgmSource.isPlaying && _currentBgmKey == key)
                return;

            _currentBgmKey = key;
            _currentBgmDefaultVolume = entry.DefaultVolume;

            // 优先使用缓存
            if (_clipCache.TryGetValue(key, out var clip))
            {
                PlayBgmWithClip(clip, entry.DefaultVolume, fadeDuration);
            }
            else
            {
                StartCoroutine(LoadAndPlayBgm(entry, fadeDuration));
            }
        }

        private IEnumerator LoadAndPlayBgm(BgmEntry entry, float fadeDuration)
        {
            var handle = entry.ClipRef.LoadAssetAsync<AudioClip>();
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _clipCache[entry.Key] = handle.Result;
                PlayBgmWithClip(handle.Result, entry.DefaultVolume, fadeDuration);
            }
            else
            {
                Debug.LogError($"[AudioSystem] Failed to load BGM: {entry.Key}");
            }
        }

        private void PlayBgmWithClip(AudioClip clip, float defaultVolume, float fadeDuration)
        {
            if (_bgmSource.isPlaying)
            {
                var halfFade = fadeDuration * 0.5f;
                _bgmFadeCoroutine = StartCoroutine(FadeOutThenPlay(clip, defaultVolume, halfFade, halfFade));
            }
            else
            {
                PlayBgmInternal(clip, defaultVolume, fadeDuration);
            }
        }

        private void PlayBgmInternal(AudioClip clip, float defaultVolume, float fadeDuration)
        {
            _bgmSource.clip = clip;
            _bgmSource.volume = 0f;
            _bgmSource.Play();
            float targetVol = defaultVolume * _masterVolume * _bgmVolume;
            _bgmFadeCoroutine = StartCoroutine(FadeVolume(_bgmSource, 0f, targetVol, fadeDuration));
        }

        public void StopBgm(float fadeDuration = 1f)
        {
            StopBgmFade();
            _currentBgmKey = null;

            if (fadeDuration > 0f && _bgmSource.isPlaying)
            {
                float currentVol = _bgmSource.volume;
                _bgmFadeCoroutine = StartCoroutine(FadeVolume(_bgmSource, currentVol, 0f, fadeDuration));
                StartCoroutine(StopAfterFade(fadeDuration));
            }
            else
            {
                _bgmSource.Stop();
            }
        }

        public void PlaySfx(string key, Vector3? position = null, float volumeScale = 1f)
        {
            // SFX 功能暂未实现
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            ApplyMasterVolume();
            SaveVolumesToPrefs();
            NotifyVolumeChanged();
        }

        public void SetBgmVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            ApplyBgmVolume();
            SaveVolumesToPrefs();
            NotifyVolumeChanged();
        }

        public void SetSfxVolume(float volume)
        {
            // SFX 功能暂未实现
        }

        public (float master, float bgm, float sfx) GetCurrentVolumes()
        {
            return (_masterVolume, _bgmVolume, 1f);
        }

        // ===== 音量管理 =====

        private void ApplyMasterVolume()
        {
            if (_mixer != null)
            {
                _mixer.SetFloat(MIXER_MASTER_VOL, VolumeToDb(_masterVolume));
            }
            else
            {
                // 无 Mixer 时直接控制 BGM 音量
                ApplyBgmVolume();
            }
        }

        private void ApplyBgmVolume()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _currentBgmDefaultVolume * _masterVolume * _bgmVolume;
            }
            if (_mixer != null)
                _mixer.SetFloat(MIXER_BGM_VOL, VolumeToDb(_bgmVolume));
        }

        private static float VolumeToDb(float volume)
        {
            return volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
        }

        private const string PREFS_KEY_MASTER = "Audio_MasterVolume";
        private const string PREFS_KEY_BGM = "Audio_BgmVolume";

        private void LoadVolumesFromPrefs()
        {
            _masterVolume = PlayerPrefs.GetFloat(PREFS_KEY_MASTER, 1f);
            _bgmVolume = PlayerPrefs.GetFloat(PREFS_KEY_BGM, 1f);
            ApplyMasterVolume();
            ApplyBgmVolume();
        }

        private void SaveVolumesToPrefs()
        {
            PlayerPrefs.SetFloat(PREFS_KEY_MASTER, _masterVolume);
            PlayerPrefs.SetFloat(PREFS_KEY_BGM, _bgmVolume);
            PlayerPrefs.Save();
        }

        // ===== 事件处理方法 =====

        private void OnPlayBgmEvent(PlayBgmEvent evt)
        {
            PlayBgm(evt.BgmKey, evt.FadeDuration);
        }

        private void OnStopBgmEvent(StopBgmEvent evt)
        {
            StopBgm(evt.FadeDuration);
        }

        private void OnSetMasterVolumeEvent(SetMasterVolumeEvent evt)
        {
            SetMasterVolume(evt.Volume);
        }

        private void OnSetBgmVolumeEvent(SetBgmVolumeEvent evt)
        {
            SetBgmVolume(evt.Volume);
        }

        private void NotifyVolumeChanged()
        {
            EventBus<VolumeChangedEvent>.Publish(new VolumeChangedEvent
            {
                MasterVolume = _masterVolume,
                BgmVolume = _bgmVolume,
                SfxVolume = 1f
            });
        }
    }
}
