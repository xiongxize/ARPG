// 所属模块: Audio.Presentation, 职责: 3D 世界中的音效播放点
using UnityEngine;
using ARPG.Core.EventBus;
using ARPG.Events;

namespace ARPG.Presentation.Audio
{
    public class AudioSourceView : AutoEventView
    {
        [SerializeField] private AudioSource _source;

        // TODO: 可添加 3D 音效衰减参数（min/max distance）、AudioMixerGroup 引用等

        [EventSubscriber]
        private void OnPlaySfx(PlaySfxEvent evt)
        {
            // TODO: 根据事件中的 position 与自身 Transform.position 的距离
            //       判断是否需要由当前 AudioSource 播放该音效
            //       将来可引入 AudioSourceId 做精确匹配
        }
    }
}
