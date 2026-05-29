using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace ARPG.Data.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "ARPG/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        public List<BgmEntry> BgmList;
        public List<SfxEntry> SfxList;
    }
    
    [Serializable]
    public class BgmEntry
    {
        public string Key;
        public AssetReferenceT<AudioClip> ClipRef;   // Addressable 引用
        [Range(0,1)] public float DefaultVolume = 1f;
    }

    [Serializable]
    public class SfxEntry
    {
        public string Key;
        public AssetReferenceT<AudioClip> ClipRef;
        [Range(0,1)] public float DefaultVolume = 1f;
        public bool Is3D;
    }
}
