// 所属模块: Events, 职责: 音频相关事件定义
using UnityEngine;

namespace ARPG.Events
{
    public struct PlayBgmEvent
    {
        public string BgmKey;
        public float FadeDuration;
    }

    public struct StopBgmEvent
    {
        public float FadeDuration;
    }

    public struct PlaySfxEvent
    {
        public string SfxKey;
        public Vector3? Position;
        public float VolumeScale;
    }

    public struct SetMasterVolumeEvent
    {
        public float Volume;
    }

    public struct SetBgmVolumeEvent
    {
        public float Volume;
    }

    public struct SetSfxVolumeEvent
    {
        public float Volume;
    }

    public struct VolumeChangedEvent
    {
        public float MasterVolume;
        public float BgmVolume;
        public float SfxVolume;
    }
}
