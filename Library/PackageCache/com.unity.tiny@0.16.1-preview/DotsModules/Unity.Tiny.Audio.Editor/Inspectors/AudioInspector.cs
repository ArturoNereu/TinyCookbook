using JetBrains.Annotations;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using AudioSource = Unity.Tiny.Audio.AudioSource;

namespace Unity.Editor
{
    /// Customizes the view of the AudioSource component. The default
    /// works, except for the volume, which needs to be a slider (0.0->1.0)
    /// instead of a raw value. The slider also limits the volume
    /// to a typical range; script does not limit the volume range.
    [UsedImplicitly]
    internal class AudioSourceInspector : IComponentInspector<AudioSource>
    {
        private InspectorDataProxy<AudioSource> m_Proxy;    // cache the proxy for the callbacks; Data field is the AudioSource
        private IAssetManager m_AssetManager;  // Assets for the project (in this case the AudioClip entity)

        private ObjectField m_Clip;
        private Slider m_Volume;
        private Toggle m_Toggle;

        public VisualElement Build(InspectorDataProxy<AudioSource> proxy)
        {
            m_Proxy = proxy;
            m_AssetManager = m_Proxy.Session.GetManager<IAssetManager>();
            m_Clip = new ObjectField(nameof(AudioSource.clip)) {objectType = typeof(UnityEngine.AudioClip)};
            m_Clip.RegisterValueChangedCallback(ClipChanged);

            m_Volume = new Slider(nameof(AudioSource.volume), 0.0f, 1.0f);
            m_Volume.RegisterValueChangedCallback(VolumeChanged);

            m_Toggle = new Toggle(nameof(AudioSource.loop));
            m_Toggle.RegisterValueChangedCallback(LoopChanged);

            var root = new VisualElement();
            root.contentContainer.Add(m_Clip);
            root.contentContainer.Add(m_Volume);
            root.contentContainer.Add(m_Toggle);
            return root;
        }

        // Update the clip from the UI
        private void ClipChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            AudioSource data = m_Proxy.Data;
            data.clip = m_AssetManager.GetEntity(evt.newValue);
            m_Proxy.Data = data;
        }

        // Update the volume from the UI
        private void VolumeChanged(ChangeEvent<float> evt)
        {
            AudioSource data = m_Proxy.Data;
            data.volume = evt.newValue;
            m_Proxy.Data = data;
        }

        // Update the loop from the UI
        private void LoopChanged(ChangeEvent<bool> evt)
        {
            AudioSource data = m_Proxy.Data;
            data.loop = evt.newValue;
            m_Proxy.Data = data;
        }

        // Pushes values to the UI
        public void Update(InspectorDataProxy<AudioSource> proxy)
        {
            var data = proxy.Data;
            m_Clip.SetValueWithoutNotify(m_AssetManager.GetUnityObject<UnityEngine.AudioClip>(data.clip));
            m_Volume.SetValueWithoutNotify(Mathf.Clamp01(data.volume));
            m_Toggle.SetValueWithoutNotify(data.loop);
        }
    }
}
