#if MM_UI
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This class can be added to a UI slider to control the volume of a specific sound
    /// via the MMSoundManager.
    /// </summary>
    public class MMSoundManagerSoundVolumeSlider : MonoBehaviour,
                                                    MMEventListener<MMSoundManagerEvent>,
                                                    MMEventListener<MMSoundManagerSoundControlEvent>,
                                                    MMEventListener<MMSoundManagerSoundFadeEvent>
    {
        [Header("Sound Volume Settings")]
        [Tooltip("The ID of the sound to control")]
        public int SoundID;
        [Tooltip("The volume to apply to the sound when the slider is at its minimum")]
        public float MinVolume = 0f;
        [Tooltip("The volume to apply to the sound when the slider is at its maximum")]
        public float MaxVolume = 1f;

        [Header("Read/Write Mode")]
        [Tooltip("In write mode, the slider's value will be applied to the sound's volume. In read mode, the slider will reflect the sound's current volume.")]
        public Modes Mode = Modes.Write;
        [Tooltip("If true, the slider will automatically switch to read mode when a sound fade event is caught.")]
        public bool ChangeModeOnSoundFade = true;
        [Tooltip("If true, the slider will automatically switch to read mode when a sound control event is caught.")]
        public bool ChangeModeOnSoundControl = true;
        [Tooltip("Duration the slider remains in read mode after an automatic switch.")]
        public float ModeSwitchBufferTime = 0.1f;

        public enum Modes { Read, Write }

        protected Slider _slider;
        protected Modes _resetToMode;
        protected bool _resetNeeded = false;
        protected float _resetTimestamp;

        protected virtual void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        protected virtual void Start()
        {
            if (MMSoundManager.HasInstance)
            {
                UpdateSliderValueWithSoundVolume();
            }
        }

        protected virtual void LateUpdate()
        {
            if (Mode == Modes.Read)
            {
                AudioSource soundSource = MMSoundManager.Instance.FindByID(SoundID);
                if (soundSource != null)
                {
                    _slider.value = soundSource.volume;
                }
            }

            if (_resetNeeded && (Time.unscaledTime >= _resetTimestamp))
            {
                Mode = _resetToMode;
                _resetNeeded = false;
            }
        }

        public virtual void ChangeModeToRead(float duration)
        {
            _resetToMode = Modes.Write;
            Mode = Modes.Read;
            _resetTimestamp = Time.unscaledTime + duration;
            _resetNeeded = true;
        }

        public virtual void UpdateVolume(float newValue)
        {
            if (Mode == Modes.Read)
            {
                return;
            }
            float newVolume = Mathf.Lerp(MinVolume, MaxVolume, newValue);
            AudioSource soundSource = MMSoundManager.Instance.FindByID(SoundID);
            if (soundSource != null)
            {
                soundSource.volume = newVolume;
            }
        }

        public void OnMMEvent(MMSoundManagerEvent soundManagerEvent)
        {
            if (soundManagerEvent.EventType == MMSoundManagerEventTypes.SettingsLoaded)
            {
                UpdateSliderValueWithSoundVolume();
            }
        }

        public virtual void UpdateSliderValueWithSoundVolume()
        {
            AudioSource soundSource = MMSoundManager.Instance.FindByID(SoundID);
            if (soundSource != null)
            {
                _slider.value = Mathf.InverseLerp(MinVolume, MaxVolume, soundSource.volume);
            }
        }

        public void OnMMEvent(MMSoundManagerSoundControlEvent soundControlEvent)
        {
            if (ChangeModeOnSoundControl && soundControlEvent.SoundID == SoundID)
            {
                ChangeModeToRead(ModeSwitchBufferTime);
            }
        }

        public void OnMMEvent(MMSoundManagerSoundFadeEvent soundFadeEvent)
        {
            if (ChangeModeOnSoundFade && soundFadeEvent.SoundID == SoundID)
            {
                ChangeModeToRead(soundFadeEvent.FadeDuration + ModeSwitchBufferTime);
            }
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMSoundManagerEvent>();
            this.MMEventStartListening<MMSoundManagerSoundControlEvent>();
            this.MMEventStartListening<MMSoundManagerSoundFadeEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMSoundManagerEvent>();
            this.MMEventStopListening<MMSoundManagerSoundControlEvent>();
            this.MMEventStopListening<MMSoundManagerSoundFadeEvent>();
        }
    }
}
#endif