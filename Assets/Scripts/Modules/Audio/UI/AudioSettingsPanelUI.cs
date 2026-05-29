// 所属模块: Audio.UI, 职责: 音频设置面板 UI 逻辑
using UnityEngine;
using UnityEngine.UI;
using ARPG.Core.EventBus;
using ARPG.Events;

namespace ARPG.UI.Audio
{
    public class AudioSettingsPanelUI : AutoEventView
    {
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        private bool _isUpdatingSliders;

        private void Awake()
        {
            _masterVolumeSlider.onValueChanged.AddListener(value =>
                EventBus<SetMasterVolumeEvent>.Publish(new SetMasterVolumeEvent { Volume = value }));
            _bgmVolumeSlider.onValueChanged.AddListener(value =>
                EventBus<SetBgmVolumeEvent>.Publish(new SetBgmVolumeEvent { Volume = value }));
            _sfxVolumeSlider.onValueChanged.AddListener(value =>
                EventBus<SetSfxVolumeEvent>.Publish(new SetSfxVolumeEvent { Volume = value }));
        }

        [EventSubscriber]
        private void OnVolumeChanged(VolumeChangedEvent evt)
        {
            // 使用 SetValueWithoutNotify 避免触发 onValueChanged 回环
            _masterVolumeSlider.SetValueWithoutNotify(evt.MasterVolume);
            _bgmVolumeSlider.SetValueWithoutNotify(evt.BgmVolume);
            _sfxVolumeSlider.SetValueWithoutNotify(evt.SfxVolume);
        }
    }
}
