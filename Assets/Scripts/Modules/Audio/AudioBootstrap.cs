// 所属模块: Audio, 职责: 音频模块初始化入口
using ARPG.Core.Module;
using ARPG.Core;
using ARPG.Interfaces;
using ARPG.System.Audio;
using UnityEngine;

namespace ARPG.System.Audio
{
    public class AudioBootstrap : IModuleBootstrap
    {
        // 优先级设为 5，在战斗模块(10)之前初始化，确保音频服务先就绪
        public int Priority => 5;

        public void Init()
        {
            // 创建持久化的 AudioSystem GameObject 并注册服务
            var go = new GameObject("[AudioSystem]");
            var audioSystem = go.AddComponent<AudioSystem>();
            ServiceLocator.Register<IAudioService>(audioSystem);
            Object.DontDestroyOnLoad(go);

            Debug.Log($"[{nameof(AudioBootstrap)}] 优先级： {Priority} 初始化音频模块");
        }
    }
}
