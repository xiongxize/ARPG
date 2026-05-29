using ARPG.Core.EventBus;
using ARPG.Events;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Header("BGM 测试参数")]
    [SerializeField] private string _bgmKey = "BGM";
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _playDuration = 5f;
    [SerializeField] private float _fadeOutDuration = 1f;

    private void Start()
    {
        // 延迟一帧确保 AudioSystem 已初始化
        Invoke(nameof(PlayTestBgm), 0.1f);
    }

    private void PlayTestBgm()
    {
        Debug.Log($"[Test] 发布 PlayBgmEvent — Key: {_bgmKey}, FadeIn: {_fadeInDuration}s");
        EventBus<PlayBgmEvent>.Publish(new PlayBgmEvent
        {
            BgmKey = _bgmKey,
            FadeDuration = _fadeInDuration
        });

        // 播放 _playDuration 秒后停止
        // Invoke(nameof(StopTestBgm), _playDuration);
    }

    private void StopTestBgm()
    {
        Debug.Log($"[Test] 发布 StopBgmEvent — FadeOut: {_fadeOutDuration}s");
        EventBus<StopBgmEvent>.Publish(new StopBgmEvent
        {
            FadeDuration = _fadeOutDuration
        });
    }
}
