// 所属模块: Interfaces, 职责: 音频服务接口
using UnityEngine;

namespace ARPG.Interfaces
{
    public interface IAudioService
    {
        void PlayBgm(string key, float fadeDuration = 1f);
        void StopBgm(float fadeDuration = 1f);
        void PlaySfx(string key, Vector3? position = null, float volumeScale = 1f);
        void SetMasterVolume(float volume);
        void SetBgmVolume(float volume);
        void SetSfxVolume(float volume);
        (float master, float bgm, float sfx) GetCurrentVolumes();
    }
}
