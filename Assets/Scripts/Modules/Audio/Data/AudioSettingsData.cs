// 所属模块: Audio.Data, 职责: 音频设置运行时数据
using System;

namespace ARPG.Data.Audio
{
    /// <summary>
    /// 音频设置的可序列化运行时数据
    /// 由 AudioSystem 持有，作为当前设置状态
    /// </summary>
    [Serializable]
    public class AudioSettingsData
    {
        public float MasterVolume = 1f;
        public float BgmVolume = 1f;
        public float SfxVolume = 1f;
    }
}
