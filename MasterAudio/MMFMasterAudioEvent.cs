using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    // TODO: Test multiplayer functionality

    [ExecuteAlways]
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you control a group via the Master Audio. You will need a game object in your scene with a Master Audio Script on it for this to work.")]
    [FeedbackPath("Master Audio/MA Audio Event")]
    public class MMFMasterAudioEvent: MMF_Feedback
    {
#if UNITY_EDITOR
        public override Color FeedbackColor => new Color32(20, 59, 204, 255);
        public override bool EvaluateRequiresSetup()
        {
            bool requiresSetup = false;
            if (TranformOfCaller == null)
            {
                requiresSetup = true;
            }
            return requiresSetup;
        }
        public override string RequiredTargetText { get { return GetSubLabel();  } }

        public override string RequiresSetupText { get { return "This feedback requires that you set an Transform in its TranformOfCaller slot below."; } }
#endif
        public override float FeedbackDuration { get { return GetDuration(); } }

        [MMFInspectorGroup("Master Audio Settings", true, 0)]
        [Tooltip("the type of caller")]
        public MasterAudio.SoundSpawnLocationMode soundSpawnMode = MasterAudio.SoundSpawnLocationMode.AttachToCaller;
        [Tooltip("the Transform that will be considered as the caller of the event")]
        public Transform TranformOfCaller;
        [MAFeedbackHelp("Audio Event")]
        public FeedbackMAEvent AudioEvent;

#if MULTIPLAYER_ENABLED
            public bool willSendToAllPlayers = false;
#endif

        public override void InitializeCustomAttributes()
        {
            base.InitializeCustomAttributes();
            if (TranformOfCaller == null) TranformOfCaller = Owner.transform;
        }

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            PerformSingleAction(AudioEvent);
        }

        private void PerformSingleAction(FeedbackMAEvent aEvent, EventSounds.EventType eType = EventSounds.EventType.CodeTriggeredEvent1) {
            if (MasterAudio.AppIsShuttingDown || MasterAudio.SafeInstance == null) {
                return;
            }

            var volume = aEvent.volume;
            var sType = aEvent.soundType;
            float? pitch = aEvent.pitch;
            if (!aEvent.useFixedPitch) {
                pitch = null;
            }

            PlaySoundResult soundPlayed = null;

            var soundSpawnModeToUse = soundSpawnMode;

            // these events need a PlaySoundResult, the rest do not. Save on allocation!
            var needsResult = aEvent.glidePitchType != EventSounds.GlidePitchType.None;

            switch (aEvent.currentSoundFunctionType) {
                case MasterAudio.EventSoundFunctionType.PlaySound:
                    string variationName = null;
                    if (aEvent.variationType == EventSounds.VariationType.PlaySpecific) {
                        variationName = aEvent.variationName;
                    }

                    switch (soundSpawnModeToUse) {
                        case MasterAudio.SoundSpawnLocationMode.CallerLocation:
                            if (needsResult) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    soundPlayed = MasterAudioMultiplayerAdapter.PlaySound3DAtTransform(sType, TranformOfCaller, volume, pitch, aEvent.delaySound, variationName);
                                } else {
                                    soundPlayed = MasterAudio.PlaySound3DAtTransform(sType, TranformOfCaller, volume, pitch,
                                        aEvent.delaySound, variationName);
                                }
#else
                                soundPlayed = MasterAudio.PlaySound3DAtTransform(sType, TranformOfCaller, volume, pitch,
                                    aEvent.delaySound, variationName);
#endif
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PlaySound3DAtTransformAndForget(sType, TranformOfCaller, volume, pitch, aEvent.delaySound, variationName);
                                } else {
                                    MasterAudio.PlaySound3DAtTransformAndForget(sType, TranformOfCaller, volume, pitch, aEvent.delaySound, variationName);
                                }
#else
                                MasterAudio.PlaySound3DAtTransformAndForget(sType, TranformOfCaller, volume, pitch,
                                    aEvent.delaySound, variationName);
#endif
                            }
                            break;
                        case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
                        default:
                            if (needsResult) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    soundPlayed = MasterAudioMultiplayerAdapter.PlaySound3DFollowTransform(sType, TranformOfCaller, volume, pitch,
                                        aEvent.delaySound, variationName);
                                } else {
                                    soundPlayed = MasterAudio.PlaySound3DFollowTransform(sType, TranformOfCaller, volume, pitch,
                                        aEvent.delaySound, variationName);
                                }
#else
                                soundPlayed = MasterAudio.PlaySound3DFollowTransform(sType, TranformOfCaller, volume, pitch,
                                    aEvent.delaySound, variationName);
#endif
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PlaySound3DFollowTransformAndForget(sType, TranformOfCaller, volume, pitch,
                                        aEvent.delaySound, variationName);
                                } else {
                                    MasterAudio.PlaySound3DFollowTransformAndForget(sType, TranformOfCaller, volume, pitch,
                                        aEvent.delaySound, variationName);
                                }
#else
                                MasterAudio.PlaySound3DFollowTransformAndForget(sType, TranformOfCaller, volume, pitch,
                                    aEvent.delaySound, variationName);
#endif
                            }
                            break;
                    }

                    if (soundPlayed != null && soundPlayed.ActingVariation != null && aEvent.glidePitchType != EventSounds.GlidePitchType.None) {
                        switch (aEvent.glidePitchType) {
                            case EventSounds.GlidePitchType.RaisePitch:
                                if (!string.IsNullOrEmpty(aEvent.theCustomEventName)) {
                                    soundPlayed.ActingVariation.GlideByPitch(aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                        delegate {
                                            MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                        });
                                } else {
                                    soundPlayed.ActingVariation.GlideByPitch(aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                }
                                break;
                            case EventSounds.GlidePitchType.LowerPitch:
                                if (!string.IsNullOrEmpty(aEvent.theCustomEventName)) {
                                    soundPlayed.ActingVariation.GlideByPitch(aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                        delegate {
                                            MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                        });
                                } else {
                                    soundPlayed.ActingVariation.GlideByPitch(-aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                }
                                break;
                        }
                    }

#if UNITY_IPHONE || UNITY_ANDROID
    // no mouse events!
#else
                    if (eType == EventType.OnMouseDrag) {
                        _mouseDragResult = soundPlayed;
                    }
#endif
                    break;
                case MasterAudio.EventSoundFunctionType.PlaylistControl:
                    soundPlayed = new PlaySoundResult() {
                        ActingVariation = null,
                        SoundPlayed = true,
                        SoundScheduled = false
                    };

                    if (string.IsNullOrEmpty(aEvent.playlistControllerName)) {
                        aEvent.playlistControllerName = MasterAudio.OnlyPlaylistControllerName;
                    }

                    switch (aEvent.currentPlaylistCommand) {
                        case MasterAudio.PlaylistCommand.None:
                            soundPlayed.SoundPlayed = false;
                            break;
                        case MasterAudio.PlaylistCommand.Restart:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.RestartAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.RestartAllPlaylists();
                                }
#else
                                MasterAudio.RestartAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.RestartPlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.RestartPlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.RestartPlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Start:
                            if (aEvent.playlistControllerName == MasterAudio.NoGroupName ||
                                aEvent.playlistName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StartPlaylist(TranformOfCaller, aEvent.playlistControllerName, aEvent.playlistName);
                                } else {
                                    MasterAudio.StartPlaylist(aEvent.playlistControllerName, aEvent.playlistName);
                                }
#else
                                MasterAudio.StartPlaylist(aEvent.playlistControllerName, aEvent.playlistName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.ChangePlaylist:
                            if (string.IsNullOrEmpty(aEvent.playlistName)) {
                                Debug.Log("You have not specified a Playlist name for Event Sounds on '" +
                                            TranformOfCaller.name + "'.");
                                soundPlayed.SoundPlayed = false;
                            } else {
                                if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                    // don't play
                                } else {
#if MULTIPLAYER_ENABLED
                                    if (willSendToAllPlayers) {
                                        MasterAudioMultiplayerAdapter.ChangePlaylistByName(TranformOfCaller, aEvent.playlistControllerName, aEvent.playlistName, aEvent.startPlaylist);
                                    } else {
                                        MasterAudio.ChangePlaylistByName(aEvent.playlistControllerName,
                                            aEvent.playlistName, aEvent.startPlaylist);
                                    }
#else
                                    MasterAudio.ChangePlaylistByName(aEvent.playlistControllerName,
                                        aEvent.playlistName, aEvent.startPlaylist);
#endif
                                }
                            }

                            break;
                        case MasterAudio.PlaylistCommand.StopLoopingCurrentSong:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopLoopingAllCurrentSongs(TranformOfCaller);
                                } else {
                                    MasterAudio.StopLoopingAllCurrentSongs();
                                }
#else
                                MasterAudio.StopLoopingAllCurrentSongs();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopLoopingCurrentSong(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.StopLoopingCurrentSong(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.StopLoopingCurrentSong(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.StopPlaylistAfterCurrentSong:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopAllPlaylistsAfterCurrentSongs(TranformOfCaller);
                                } else {
                                    MasterAudio.StopAllPlaylistsAfterCurrentSongs();
                                }
#else
                                MasterAudio.StopAllPlaylistsAfterCurrentSongs();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopPlaylistAfterCurrentSong(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.StopPlaylistAfterCurrentSong(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.StopPlaylistAfterCurrentSong(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.FadeToVolume:
                            var targetVol = aEvent.fadeVolume;

                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeAllPlaylistsToVolume(TranformOfCaller, targetVol, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeAllPlaylistsToVolume(targetVol, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeAllPlaylistsToVolume(targetVol, aEvent.fadeTime);
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadePlaylistToVolume(TranformOfCaller, aEvent.playlistControllerName, targetVol,
                                        aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadePlaylistToVolume(aEvent.playlistControllerName, targetVol,
                                        aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadePlaylistToVolume(aEvent.playlistControllerName, targetVol,
                                    aEvent.fadeTime);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Mute:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.MuteAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.MuteAllPlaylists();
                                }
#else
                                MasterAudio.MuteAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.MutePlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.MutePlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.MutePlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Unmute:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnmuteAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.UnmuteAllPlaylists();
                                }
#else
                                MasterAudio.UnmuteAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnmutePlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.UnmutePlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.UnmutePlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.ToggleMute:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.ToggleMuteAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.ToggleMuteAllPlaylists();
                                }
#else
                                MasterAudio.ToggleMuteAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.ToggleMutePlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.ToggleMutePlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.ToggleMutePlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.PlaySong:
                            if (string.IsNullOrEmpty(aEvent.clipName)) {
                                Debug.Log("You have not specified a song name for Event Sounds on '" + TranformOfCaller.name +
                                            "'.");
                                soundPlayed.SoundPlayed = false;
                            } else {
                                if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                    // don't play
                                } else {
#if MULTIPLAYER_ENABLED
                                    if (willSendToAllPlayers) {
                                        if (!MasterAudioMultiplayerAdapter.TriggerPlaylistClip(TranformOfCaller, aEvent.playlistControllerName, aEvent.clipName)) {
                                            soundPlayed.SoundPlayed = false;
                                        }
                                    } else {
                                        if (!MasterAudio.TriggerPlaylistClip(aEvent.playlistControllerName, aEvent.clipName)) {
                                            soundPlayed.SoundPlayed = false;
                                        }
                                    }
#else
                                    if (!MasterAudio.TriggerPlaylistClip(aEvent.playlistControllerName, aEvent.clipName)) {
                                        soundPlayed.SoundPlayed = false;
                                    }
#endif
                                }
                            }

                            break;
                        case MasterAudio.PlaylistCommand.AddSongToQueue:
                            soundPlayed.SoundPlayed = false;

                            if (string.IsNullOrEmpty(aEvent.clipName)) {
                                Debug.Log("You have not specified a song name for Event Sounds on '" + TranformOfCaller.name + "'.");
                            } else {
                                if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                    // don't play
                                } else {
#if MULTIPLAYER_ENABLED
                                    if (willSendToAllPlayers) {
                                        MasterAudioMultiplayerAdapter.QueuePlaylistClip(TranformOfCaller, aEvent.playlistControllerName, aEvent.clipName);
                                    } else {
                                        MasterAudio.QueuePlaylistClip(aEvent.playlistControllerName, aEvent.clipName);
                                    }
#else
                                    MasterAudio.QueuePlaylistClip(aEvent.playlistControllerName, aEvent.clipName);
#endif
                                    soundPlayed.SoundPlayed = true;
                                }
                            }

                            break;
                        case MasterAudio.PlaylistCommand.PlayRandomSong:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.TriggerRandomClipAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.TriggerRandomClipAllPlaylists();
                                }
#else
                                MasterAudio.TriggerRandomClipAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.TriggerRandomPlaylistClip(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.TriggerRandomPlaylistClip(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.TriggerRandomPlaylistClip(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.PlayNextSong:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.TriggerNextClipAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.TriggerNextClipAllPlaylists();
                                }
#else
                                MasterAudio.TriggerNextClipAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.TriggerNextPlaylistClip(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.TriggerNextPlaylistClip(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.TriggerNextPlaylistClip(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Pause:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.PauseAllPlaylists();
                                }
#else
                                MasterAudio.PauseAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PausePlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.PausePlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.PausePlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Stop:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.StopAllPlaylists();
                                }
#else
                                MasterAudio.StopAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopPlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.StopPlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.StopPlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                        case MasterAudio.PlaylistCommand.Resume:
                            if (aEvent.allPlaylistControllersForGroupCmd) {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseAllPlaylists(TranformOfCaller);
                                } else {
                                    MasterAudio.UnpauseAllPlaylists();
                                }
#else
                                MasterAudio.UnpauseAllPlaylists();
#endif
                            } else if (aEvent.playlistControllerName == MasterAudio.NoGroupName) {
                                // don't play
                            } else {
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpausePlaylist(TranformOfCaller, aEvent.playlistControllerName);
                                } else {
                                    MasterAudio.UnpausePlaylist(aEvent.playlistControllerName);
                                }
#else
                                MasterAudio.UnpausePlaylist(aEvent.playlistControllerName);
#endif
                            }
                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.GroupControl:
                    soundPlayed = new PlaySoundResult() {
                        ActingVariation = null,
                        SoundPlayed = true,
                        SoundScheduled = false
                    };

                    var soundTypeOverride = string.Empty;

                    var soundTypesForCmd = new List<string>();
                    if (!aEvent.allSoundTypesForGroupCmd || MasterAudio.GroupCommandsWithNoAllGroupSelector.Contains(aEvent.currentSoundGroupCommand)) {
                        soundTypesForCmd.Add(aEvent.soundType);
                    } else {
                        soundTypesForCmd.AddRange(MasterAudio.RuntimeSoundGroupNames);
#if MULTIPLAYER_ENABLED
                        if (willSendToAllPlayers) {
                            soundTypeOverride = MasterAudio.AllBusesName;
                        }
#endif
                    }

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < soundTypesForCmd.Count; i++) {
                        var soundType = soundTypesForCmd[i];
                        if (!string.IsNullOrEmpty(soundTypeOverride)) { // for multiplayer "do all"
                            soundType = soundTypeOverride;
                        }

                        switch (aEvent.currentSoundGroupCommand) {
                            case MasterAudio.SoundGroupCommand.None:
                                soundPlayed.SoundPlayed = false;
                                break;
                            case MasterAudio.SoundGroupCommand.ToggleSoundGroup:
                                if (MasterAudio.IsSoundGroupPlaying(soundType)) {
#if MULTIPLAYER_ENABLED
                                    if (willSendToAllPlayers) {
                                        MasterAudioMultiplayerAdapter.FadeOutAllOfSound(TranformOfCaller, soundType, aEvent.fadeTime);
                                    } else {
                                        MasterAudio.FadeOutAllOfSound(soundType, aEvent.fadeTime);
                                    }
#else
                                    MasterAudio.FadeOutAllOfSound(soundType, aEvent.fadeTime);
#endif
                                } else {
                                    switch (soundSpawnModeToUse) {
                                        case MasterAudio.SoundSpawnLocationMode.CallerLocation:
#if MULTIPLAYER_ENABLED
                                            if (willSendToAllPlayers) {
                                                MasterAudioMultiplayerAdapter.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            } else {
                                                MasterAudio.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            }
#else
                                            MasterAudio.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
#endif
                                            break;
                                        case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
#if MULTIPLAYER_ENABLED
                                            if (willSendToAllPlayers) {
                                                MasterAudioMultiplayerAdapter.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            } else {
                                                MasterAudio.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            }
#else
                                            MasterAudio.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
#endif
                                            break;
                                    }
                                }
                                break;
                            case MasterAudio.SoundGroupCommand.ToggleSoundGroupOfTransform:
                                if (MasterAudio.IsTransformPlayingSoundGroup(soundType, TranformOfCaller)) {
#if MULTIPLAYER_ENABLED
                                    if (willSendToAllPlayers) {
                                        MasterAudioMultiplayerAdapter.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
                                    } else {
                                        MasterAudio.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
                                    }
#else
                                    MasterAudio.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
#endif
                                } else {
                                    switch (soundSpawnModeToUse) {
                                        case MasterAudio.SoundSpawnLocationMode.CallerLocation:
#if MULTIPLAYER_ENABLED
                                            if (willSendToAllPlayers) {
                                                MasterAudioMultiplayerAdapter.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            } else {
                                                MasterAudio.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            }
#else
                                            MasterAudio.PlaySound3DAtTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
#endif
                                            break;
                                        case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
#if MULTIPLAYER_ENABLED
                                            if (willSendToAllPlayers) {
                                                MasterAudioMultiplayerAdapter.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            } else {
                                                MasterAudio.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
                                            }
#else
                                            MasterAudio.PlaySound3DFollowTransformAndForget(soundType, TranformOfCaller, volume, pitch, aEvent.delaySound);
#endif
                                            break;
                                    }
                                }
                                break;
                            case MasterAudio.SoundGroupCommand.RefillSoundGroupPool:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.RefillSoundGroupPool(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.RefillSoundGroupPool(soundType);
                                }
#else
                                MasterAudio.RefillSoundGroupPool(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeToVolume:
                                var targetVol = aEvent.fadeVolume;
                                var hasDelegate = aEvent.fireCustomEventAfterFade && !string.IsNullOrEmpty(aEvent.theCustomEventName);
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    if (hasDelegate) {
                                        MasterAudioMultiplayerAdapter.FadeSoundGroupToVolume(TranformOfCaller, soundType, targetVol, aEvent.fadeTime,
                                            delegate {
                                                MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                            },
                                            aEvent.stopAfterFade,
                                            aEvent.restoreVolumeAfterFade);
                                    } else {
                                        MasterAudioMultiplayerAdapter.FadeSoundGroupToVolume(TranformOfCaller, soundType, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    }
                                } else {
                                    if (hasDelegate) {
                                        MasterAudio.FadeSoundGroupToVolume(soundType, targetVol, aEvent.fadeTime,
                                            delegate {
                                                MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                            },
                                            aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    } else {
                                        MasterAudio.FadeSoundGroupToVolume(soundType, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    }
                                }
#else
                                if (hasDelegate) {
                                    MasterAudio.FadeSoundGroupToVolume(soundType, targetVol, aEvent.fadeTime,
                                        delegate
                                        {
                                            MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                        },
                                        aEvent.stopAfterFade,
                                        aEvent.restoreVolumeAfterFade);
                                } else {
                                    MasterAudio.FadeSoundGroupToVolume(soundType, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                }
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutAllOfSound:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeOutAllOfSound(TranformOfCaller, soundType, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeOutAllOfSound(soundType, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeOutAllOfSound(soundType, aEvent.fadeTime);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeSoundGroupOfTransformToVolume:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeSoundGroupOfTransformToVolume(TranformOfCaller, soundType, aEvent.fadeTime, aEvent.fadeVolume);
                                } else {
                                    MasterAudio.FadeSoundGroupOfTransformToVolume(TranformOfCaller, soundType, aEvent.fadeTime, aEvent.fadeVolume);
                                }
#else
                                MasterAudio.FadeSoundGroupOfTransformToVolume(TranformOfCaller, soundType, aEvent.fadeTime, aEvent.fadeVolume);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Mute:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.MuteGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.MuteGroup(soundType);
                                }
#else
                                MasterAudio.MuteGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Pause:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseSoundGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.PauseSoundGroup(soundType);
                                }
#else
                                MasterAudio.PauseSoundGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Solo:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.SoloGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.SoloGroup(soundType);
                                }
#else
                                MasterAudio.SoloGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.StopAllOfSound:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopAllOfSound(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.StopAllOfSound(soundType);
                                }
#else
                                MasterAudio.StopAllOfSound(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Unmute:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnmuteGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.UnmuteGroup(soundType);
                                }
#else
                                MasterAudio.UnmuteGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Unpause:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseSoundGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.UnpauseSoundGroup(soundType);
                                }
#else
                                MasterAudio.UnpauseSoundGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.Unsolo:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnsoloGroup(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.UnsoloGroup(soundType);
                                }
#else
                                MasterAudio.UnsoloGroup(soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.StopAllSoundsOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopAllSoundsOfTransform(TranformOfCaller);
                                } else {
                                    MasterAudio.StopAllSoundsOfTransform(TranformOfCaller);
                                }
#else
                                MasterAudio.StopAllSoundsOfTransform(TranformOfCaller);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.StopSoundGroupOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopSoundGroupOfTransform(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.StopSoundGroupOfTransform(TranformOfCaller, soundType);
                                }
#else
                                MasterAudio.StopSoundGroupOfTransform(TranformOfCaller, soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.PauseAllSoundsOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseAllSoundsOfTransform(TranformOfCaller);
                                } else {
                                    MasterAudio.PauseAllSoundsOfTransform(TranformOfCaller);
                                }
#else
                                MasterAudio.PauseAllSoundsOfTransform(TranformOfCaller);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.PauseSoundGroupOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseSoundGroupOfTransform(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.PauseSoundGroupOfTransform(TranformOfCaller, soundType);
                                }
#else
                                MasterAudio.PauseSoundGroupOfTransform(TranformOfCaller, soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.UnpauseAllSoundsOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseAllSoundsOfTransform(TranformOfCaller);
                                } else {
                                    MasterAudio.UnpauseAllSoundsOfTransform(TranformOfCaller);
                                }
#else
                                MasterAudio.UnpauseAllSoundsOfTransform(TranformOfCaller);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.UnpauseSoundGroupOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseSoundGroupOfTransform(TranformOfCaller, soundType);
                                } else {
                                    MasterAudio.UnpauseSoundGroupOfTransform(TranformOfCaller, soundType);
                                }
#else
                                MasterAudio.UnpauseSoundGroupOfTransform(TranformOfCaller, soundType);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutSoundGroupOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeOutSoundGroupOfTransform(TranformOfCaller, soundType, aEvent.fadeTime);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutAllSoundsOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeOutAllSoundsOfTransform(TranformOfCaller, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeOutAllSoundsOfTransform(TranformOfCaller, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeOutAllSoundsOfTransform(TranformOfCaller, aEvent.fadeTime);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.RouteToBus:
                                var busName = aEvent.busName;
                                if (busName == MasterAudio.NoGroupName) {
                                    busName = null;
                                }

#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.RouteGroupToBus(TranformOfCaller, soundType, busName);
                                } else {
                                    MasterAudio.RouteGroupToBus(soundType, busName);
                                }
#else
                                MasterAudio.RouteGroupToBus(soundType, busName);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.GlideByPitch:
                                var hasActionDelegate = !string.IsNullOrEmpty(aEvent.theCustomEventName);

                                switch (aEvent.glidePitchType) {
                                    case EventSounds.GlidePitchType.RaisePitch:
#if MULTIPLAYER_ENABLED
                                        if (willSendToAllPlayers) {
                                            if (hasActionDelegate) {
                                                MasterAudioMultiplayerAdapter.GlideSoundGroupByPitch(TranformOfCaller, soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                    delegate {
                                                        MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                    });
                                            } else {
                                                MasterAudioMultiplayerAdapter.GlideSoundGroupByPitch(TranformOfCaller, soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime, null);
                                            }
                                        } else {
                                            if (hasActionDelegate) {
                                                MasterAudio.GlideSoundGroupByPitch(soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                    delegate {
                                                        MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                    });
                                            } else {
                                                MasterAudio.GlideSoundGroupByPitch(soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime, null);
                                            }
                                        }
#else
                                        if (hasActionDelegate) {
                                            MasterAudio.GlideSoundGroupByPitch(soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                delegate
                                                {
                                                    MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                });
                                        } else {
                                            MasterAudio.GlideSoundGroupByPitch(soundType, aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                        }
#endif
                                        break;
                                    case EventSounds.GlidePitchType.LowerPitch:
#if MULTIPLAYER_ENABLED
                                        if (willSendToAllPlayers) {
                                            if (hasActionDelegate) {
                                                MasterAudioMultiplayerAdapter.GlideSoundGroupByPitch(TranformOfCaller, soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                    delegate {
                                                        MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                    });
                                            } else {
                                                MasterAudioMultiplayerAdapter.GlideSoundGroupByPitch(TranformOfCaller, soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime, null);
                                            }
                                        } else {
                                            if (hasActionDelegate) {
                                                MasterAudio.GlideSoundGroupByPitch(soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                    delegate {
                                                        MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                    });
                                            } else {
                                                MasterAudio.GlideSoundGroupByPitch(soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                            }
                                        }
#else
                                        if (hasActionDelegate) {
                                            MasterAudio.GlideSoundGroupByPitch(soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                delegate
                                                {
                                                    MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                });
                                        } else {
                                            MasterAudio.GlideSoundGroupByPitch(soundType, -aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                        }
#endif
                                        break;
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.StopOldSoundGroupVoices:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopOldSoundGroupVoices(TranformOfCaller, soundType, aEvent.minAge);
                                } else {
                                    MasterAudio.StopOldSoundGroupVoices(soundType, aEvent.minAge);
                                }
#else
                                MasterAudio.StopOldSoundGroupVoices(soundType, aEvent.minAge);
#endif
                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutOldSoundGroupVoices:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeOutOldSoundGroupVoices(TranformOfCaller, soundType, aEvent.minAge, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeOutOldSoundGroupVoices(soundType, aEvent.minAge, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeOutOldSoundGroupVoices(soundType, aEvent.minAge, aEvent.fadeTime);
#endif
                                break;
                        }

#if MULTIPLAYER_ENABLED
                        if (willSendToAllPlayers) {
                            // don't continue loop, we've done everything already.
                            break;
                        }
#endif
                    }

                    break;
                case MasterAudio.EventSoundFunctionType.BusControl:
                    soundPlayed = new PlaySoundResult() {
                        ActingVariation = null,
                        SoundPlayed = true,
                        SoundScheduled = false
                    };

                    var busNameOverride = string.Empty;

                    var busesForCmd = new List<string>();
                    if (!aEvent.allSoundTypesForBusCmd) {
                        busesForCmd.Add(aEvent.busName);
                    } else {
                        busesForCmd.AddRange(MasterAudio.RuntimeBusNames);
#if MULTIPLAYER_ENABLED
                        if (willSendToAllPlayers) {
                            busNameOverride = MasterAudio.AllBusesName;
                        }
#endif
                    }

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < busesForCmd.Count; i++) {
                        var busName = busesForCmd[i];
                        if (!string.IsNullOrEmpty(busNameOverride)) { // for multiplayer "do all"
                            busName = busNameOverride;
                        }

                        switch (aEvent.currentBusCommand) {
                            case MasterAudio.BusCommand.None:
                                soundPlayed.SoundPlayed = false;
                                break;
                            case MasterAudio.BusCommand.FadeToVolume:
                                var targetVol = aEvent.fadeVolume;
                                var hasCustomEventAfter = aEvent.fireCustomEventAfterFade && !string.IsNullOrEmpty(aEvent.theCustomEventName);

#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    if (hasCustomEventAfter) {
                                        MasterAudioMultiplayerAdapter.FadeBusToVolume(TranformOfCaller, busName, targetVol, aEvent.fadeTime,
                                             delegate {
                                                 MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                             },
                                             aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    } else {
                                        MasterAudioMultiplayerAdapter.FadeBusToVolume(TranformOfCaller, busName, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    }
                                } else {
                                    if (hasCustomEventAfter) {
                                        MasterAudio.FadeBusToVolume(busName, targetVol, aEvent.fadeTime,
                                             delegate {
                                                 MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                             },
                                             aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    } else {
                                        MasterAudio.FadeBusToVolume(busName, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                    }
                                }
#else
                                if (hasCustomEventAfter) {
                                    MasterAudio.FadeBusToVolume(busName, targetVol, aEvent.fadeTime,
                                        delegate
                                        {
                                            MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                        }, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                } else {
                                    MasterAudio.FadeBusToVolume(busName, targetVol, aEvent.fadeTime, null, aEvent.stopAfterFade, aEvent.restoreVolumeAfterFade);
                                }
#endif
                                break;
                            case MasterAudio.BusCommand.GlideByPitch:
                                var willFireCustomEventAfter = !string.IsNullOrEmpty(aEvent.theCustomEventName);

                                switch (aEvent.glidePitchType) {
                                    case EventSounds.GlidePitchType.RaisePitch:
#if MULTIPLAYER_ENABLED
                                        if (willSendToAllPlayers) {
                                            if (willFireCustomEventAfter) {
                                                MasterAudioMultiplayerAdapter.GlideBusByPitch(TranformOfCaller, busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                     delegate {
                                                         MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                     });
                                            } else {
                                                MasterAudioMultiplayerAdapter.GlideBusByPitch(TranformOfCaller, busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime, null);
                                            }
                                        } else {
                                            if (willFireCustomEventAfter) {
                                                MasterAudio.GlideBusByPitch(busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                     delegate {
                                                         MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                     });
                                            } else {
                                                MasterAudio.GlideBusByPitch(busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                            }
                                        }
#else
                                        if (willFireCustomEventAfter) {
                                            MasterAudio.GlideBusByPitch(busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                delegate
                                                {
                                                    MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                });
                                        } else {
                                            MasterAudio.GlideBusByPitch(busName, aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                        }
#endif
                                        break;
                                    case EventSounds.GlidePitchType.LowerPitch:
#if MULTIPLAYER_ENABLED
                                        if (willSendToAllPlayers) {
                                            if (willFireCustomEventAfter) {
                                                MasterAudioMultiplayerAdapter.GlideBusByPitch(TranformOfCaller, busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                     delegate {
                                                         MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                     });
                                            } else {
                                                MasterAudioMultiplayerAdapter.GlideBusByPitch(TranformOfCaller, busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime, null);
                                            }
                                        } else {
                                            if (willFireCustomEventAfter) {
                                                MasterAudio.GlideBusByPitch(busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                     delegate {
                                                         MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                     });
                                            } else {
                                                MasterAudio.GlideBusByPitch(busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                            }
                                        }
#else
                                        if (willFireCustomEventAfter) {
                                            MasterAudio.GlideBusByPitch(busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime,
                                                delegate
                                                {
                                                    MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller);
                                                });
                                        } else {
                                            MasterAudio.GlideBusByPitch(busName, -aEvent.targetGlidePitch, aEvent.pitchGlideTime);
                                        }
#endif
                                        break;
                                }
                                break;
                            case MasterAudio.BusCommand.Pause:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.PauseBus(busName);
                                }
#else
                                MasterAudio.PauseBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Stop:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.StopBus(busName);
                                }
#else
                                MasterAudio.StopBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Unpause:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.UnpauseBus(busName);
                                }
#else
                                MasterAudio.UnpauseBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Mute:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.MuteBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.MuteBus(busName);
                                }
#else
                                MasterAudio.MuteBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Unmute:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnmuteBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.UnmuteBus(busName);
                                }
#else
                                MasterAudio.UnmuteBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.ToggleMute:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.ToggleMuteBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.ToggleMuteBus(busName);
                                }
#else
                                MasterAudio.ToggleMuteBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Solo:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.SoloBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.SoloBus(busName);
                                }
#else
                                MasterAudio.SoloBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.Unsolo:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnsoloBus(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.UnsoloBus(busName);
                                }
#else
                                MasterAudio.UnsoloBus(busName);
#endif
                                break;
                            case MasterAudio.BusCommand.ChangePitch:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.ChangeBusPitch(TranformOfCaller, busName, aEvent.pitch);
                                } else {
                                    MasterAudio.ChangeBusPitch(busName, aEvent.pitch);
                                }
#else
                                MasterAudio.ChangeBusPitch(busName, aEvent.pitch);
#endif
                                break;
                            case MasterAudio.BusCommand.PauseBusOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.PauseBusOfTransform(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.PauseBusOfTransform(TranformOfCaller, aEvent.busName);
                                }
#else
                                MasterAudio.PauseBusOfTransform(TranformOfCaller, aEvent.busName);
#endif
                                break;
                            case MasterAudio.BusCommand.UnpauseBusOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.UnpauseBusOfTransform(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.UnpauseBusOfTransform(TranformOfCaller, aEvent.busName);
                                }
#else
                                MasterAudio.UnpauseBusOfTransform(TranformOfCaller, aEvent.busName);
#endif
                                break;
                            case MasterAudio.BusCommand.StopBusOfTransform:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopBusOfTransform(TranformOfCaller, busName);
                                } else {
                                    MasterAudio.StopBusOfTransform(TranformOfCaller, aEvent.busName);
                                }
#else
                                MasterAudio.StopBusOfTransform(TranformOfCaller, aEvent.busName);
#endif
                                break;
                            case MasterAudio.BusCommand.StopOldBusVoices:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.StopOldBusVoices(TranformOfCaller, busName, aEvent.minAge);
                                } else {
                                    MasterAudio.StopOldBusVoices(busName, aEvent.minAge);
                                }
#else
                                MasterAudio.StopOldBusVoices(busName, aEvent.minAge);
#endif
                                break;
                            case MasterAudio.BusCommand.FadeOutOldBusVoices:
#if MULTIPLAYER_ENABLED
                                if (willSendToAllPlayers) {
                                    MasterAudioMultiplayerAdapter.FadeOutOldBusVoices(TranformOfCaller, busName, aEvent.minAge, aEvent.fadeTime);
                                } else {
                                    MasterAudio.FadeOutOldBusVoices(busName, aEvent.minAge, aEvent.fadeTime);
                                }
#else
                                MasterAudio.FadeOutOldBusVoices(busName, aEvent.minAge, aEvent.fadeTime);
#endif
                                break;
                        }

#if MULTIPLAYER_ENABLED
                        if (willSendToAllPlayers) {
                            // don't continue loop, we've done everything already.
                            break;
                        }
#endif
                    }

                    break;
                case MasterAudio.EventSoundFunctionType.CustomEventControl:
                    if (eType == EventSounds.EventType.UserDefinedEvent) {
                        Debug.LogError("Custom Event Receivers cannot fire events. Occured in Transform Caller.");
                        break;
                    }
                    switch (aEvent.currentCustomEventCommand) {
                        case MasterAudio.CustomEventCommand.FireEvent:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller, aEvent.logDupeEventFiring);
                            } else {
                                MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller, aEvent.logDupeEventFiring);
                            }
#else
                            MasterAudio.FireCustomEvent(aEvent.theCustomEventName, TranformOfCaller, aEvent.logDupeEventFiring);
#endif
                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.GlobalControl:
                    switch (aEvent.currentGlobalCommand) {
                        case MasterAudio.GlobalCommand.PauseAudioListener:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.AudioListenerPause(TranformOfCaller);
                            } else {
                                AudioListener.pause = true;
                            }
#else
                            AudioListener.pause = true;
#endif
                            break;
                        case MasterAudio.GlobalCommand.UnpauseAudioListener:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.AudioListenerUnpause(TranformOfCaller);
                            } else {
                                AudioListener.pause = false;
                            }
#else
                            AudioListener.pause = false;
#endif
                            break;
                        case MasterAudio.GlobalCommand.SetMasterMixerVolume:
                            var targetVol = aEvent.volume;
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.SetMasterMixerVolume(TranformOfCaller, targetVol);
                            } else {
                                MasterAudio.MasterVolumeLevel = targetVol;
                            }
#else
                            MasterAudio.MasterVolumeLevel = targetVol;
#endif
                            break;
                        case MasterAudio.GlobalCommand.SetMasterPlaylistVolume:
                            var tgtVol = aEvent.volume;
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.SetPlaylistMasterVolume(TranformOfCaller, tgtVol);
                            } else {
                                MasterAudio.PlaylistMasterVolume = tgtVol;
                            }
#else
                            MasterAudio.PlaylistMasterVolume = tgtVol;
#endif
                            break;
                        case MasterAudio.GlobalCommand.PauseMixer:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.PauseMixer(TranformOfCaller);
                            } else {
                                MasterAudio.PauseMixer();
                            }
#else
                            MasterAudio.PauseMixer();
#endif
                            break;
                        case MasterAudio.GlobalCommand.UnpauseMixer:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.UnpauseMixer(TranformOfCaller);
                            } else {
                                MasterAudio.UnpauseMixer();
                            }
#else
                            MasterAudio.UnpauseMixer();
#endif
                            break;
                        case MasterAudio.GlobalCommand.StopMixer:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.StopMixer(TranformOfCaller);
                            } else {
                                MasterAudio.StopMixer();
                            }
#else
                            MasterAudio.StopMixer();
#endif
                            break;
                        case MasterAudio.GlobalCommand.MuteEverything:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.MuteEverything(TranformOfCaller);
                            } else {
                                MasterAudio.MuteEverything();
                            }
#else
                            MasterAudio.MuteEverything();
#endif
                            break;
                        case MasterAudio.GlobalCommand.UnmuteEverything:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.UnmuteEverything(TranformOfCaller);
                            } else {
                                MasterAudio.UnmuteEverything();
                            }
#else
                            MasterAudio.UnmuteEverything();
#endif
                            break;
                        case MasterAudio.GlobalCommand.PauseEverything:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.PauseEverything(TranformOfCaller);
                            } else {
                                MasterAudio.PauseEverything();
                            }
#else
                            MasterAudio.PauseEverything();
#endif
                            break;
                        case MasterAudio.GlobalCommand.UnpauseEverything:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.UnpauseEverything(TranformOfCaller);
                            } else {
                                MasterAudio.UnpauseEverything();
                            }
#else
                            MasterAudio.UnpauseEverything();
#endif
                            break;
                        case MasterAudio.GlobalCommand.StopEverything:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.StopEverything(TranformOfCaller);
                            } else {
                                MasterAudio.StopEverything();
                            }
#else
                            MasterAudio.StopEverything();
#endif
                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.UnityMixerControl:
                    switch (aEvent.currentMixerCommand) {
                        case MasterAudio.UnityMixerCommand.TransitionToSnapshot:
                            var snapshot = aEvent.snapshotToTransitionTo;
                            if (snapshot != null) {
                                // if we add more mixer functionality, move this next line somewhere DRY.
                                snapshot.audioMixer.updateMode = MasterAudio.Instance.mixerUpdateMode;
                                snapshot.audioMixer.TransitionToSnapshots(
                                    new[] { snapshot },
                                    new[] { 1f },
                                    aEvent.snapshotTransitionTime);
                            }
                            break;
                        case MasterAudio.UnityMixerCommand.TransitionToSnapshotBlend:
                            var snapshots = new List<AudioMixerSnapshot>();
                            var weights = new List<float>();
                            AudioMixer theMixer = null;

                            // ReSharper disable once ForCanBeConvertedToForeach
                            for (var i = 0; i < aEvent.snapshotsToBlend.Count; i++) {
                                var aSnap = aEvent.snapshotsToBlend[i];
                                if (aSnap.snapshot == null) {
                                    continue;
                                }

                                if (theMixer == null) {
                                    theMixer = aSnap.snapshot.audioMixer;
                                } else if (theMixer != aSnap.snapshot.audioMixer) {
                                    Debug.LogError("Snapshot '" + aSnap.snapshot.name + "' isn't in the same Audio Mixer as the previous snapshot in Feedback. Please make sure all the Snapshots to blend are on the same mixer.");
                                    break;
                                }

                                snapshots.Add(aSnap.snapshot);
                                weights.Add(aSnap.weight);
                            }

                            if (snapshots.Count > 0) {
                                theMixer.updateMode = MasterAudio.Instance.mixerUpdateMode;
                                // ReSharper disable once PossibleNullReferenceException
                                theMixer.TransitionToSnapshots(snapshots.ToArray(), weights.ToArray(), aEvent.snapshotTransitionTime);
                            }

                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.PersistentSettingsControl:
                    switch (aEvent.currentPersistentSettingsCommand) {
                        case MasterAudio.PersistentSettingsCommand.SetBusVolume:
                            var busesForCommand = new List<string>();
                            if (!aEvent.allSoundTypesForBusCmd) {
                                busesForCommand.Add(aEvent.busName);
                            } else {
                                busesForCommand.AddRange(MasterAudio.RuntimeBusNames);
                            }

                            // ReSharper disable once ForCanBeConvertedToForeach
                            for (var i = 0; i < busesForCommand.Count; i++) {
                                var aBusName = busesForCommand[i];
                                var tgtVol = aEvent.volume;
                                PersistentAudioSettings.SetBusVolume(aBusName, tgtVol);
                            }
                            break;
                        case MasterAudio.PersistentSettingsCommand.SetGroupVolume:
                            var groupsForCommand = new List<string>();
                            if (!aEvent.allSoundTypesForGroupCmd) {
                                groupsForCommand.Add(aEvent.soundType);
                            } else {
                                groupsForCommand.AddRange(MasterAudio.RuntimeSoundGroupNames);
                            }

                            // ReSharper disable once ForCanBeConvertedToForeach
                            for (var i = 0; i < groupsForCommand.Count; i++) {
                                var aGroupName = groupsForCommand[i];
                                var tgtVol = aEvent.volume;
                                PersistentAudioSettings.SetGroupVolume(aGroupName, tgtVol);
                            }
                            break;
                        case MasterAudio.PersistentSettingsCommand.SetMixerVolume:
                            var targetVol = aEvent.volume;
                            PersistentAudioSettings.MixerVolume = targetVol;
                            break;
                        case MasterAudio.PersistentSettingsCommand.SetMusicVolume:
                            var targVol = aEvent.volume;
                            PersistentAudioSettings.MusicVolume = targVol;
                            break;
                        case MasterAudio.PersistentSettingsCommand.MixerMuteToggle:
                            if (PersistentAudioSettings.MixerMuted.HasValue) {
                                PersistentAudioSettings.MixerMuted = !PersistentAudioSettings.MixerMuted.Value;
                            } else {
                                PersistentAudioSettings.MixerMuted = true;
                            }
                            break;
                        case MasterAudio.PersistentSettingsCommand.MusicMuteToggle:
                            if (PersistentAudioSettings.MusicMuted.HasValue) {
                                PersistentAudioSettings.MusicMuted = !PersistentAudioSettings.MusicMuted.Value;
                            } else {
                                PersistentAudioSettings.MusicMuted = true;
                            }
                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.ParameterCommandControl:
                    switch (aEvent.currentParameterCmdCommand) {
                        case MasterAudio.ParameterCmdCommand.InvokeParameterCommand:
                            if (aEvent.parameterCommandName == MasterAudio.NoGroupName) {
                                break;
                            }

#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.InvokeParameterCommand(TranformOfCaller, aEvent.parameterCommandName);
                            } else {
                                MasterAudio.InvokeParameterCommand(aEvent.parameterCommandName, TranformOfCaller);
                            }
#else
                            MasterAudio.InvokeParameterCommand(aEvent.parameterCommandName, TranformOfCaller);
#endif
                            break;
                        case MasterAudio.ParameterCmdCommand.StopAllOfParameterCommand:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.StopParameterCommandsByCommandName(TranformOfCaller, aEvent.parameterCommandName);
                            } else {
                                MasterAudio.StopParameterCommandsByCommandName(aEvent.parameterCommandName);
                            }
#else
                            MasterAudio.StopParameterCommandsByCommandName(aEvent.parameterCommandName);
#endif
                            break;
                        case MasterAudio.ParameterCmdCommand.StopParameterCommandsOfTransform:
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.StopParameterCommandsOfTransform(TranformOfCaller, aEvent.parameterCommandName);
                            } else {
                                MasterAudio.StopParameterCommandsOfTransform(aEvent.parameterCommandName, TranformOfCaller);
                            }
#else
                            MasterAudio.StopParameterCommandsOfTransform(aEvent.parameterCommandName, TranformOfCaller);
#endif
                            break;
                    }
                    break;
                case MasterAudio.EventSoundFunctionType.ParameterControl:
                    switch (aEvent.currentParameterCommand) {
                        case MasterAudio.RealTimeParameterCommand.None:
                            switch (aEvent.parameterCommandName) {
                                case MasterAudio.NoGroupName:
                                case MasterAudio.MagicParameterDistanceFromListener:
                                case MasterAudio.MagicParameterFramesSinceStart:
                                case MasterAudio.MagicParameterSecondsSinceStart:
                                    break;
                            }
#if MULTIPLAYER_ENABLED
                            if (willSendToAllPlayers) {
                                MasterAudioMultiplayerAdapter.UpdateRealTimeParameter(TranformOfCaller, aEvent.parameterCommandName, aEvent.parameterNewValue);
                            } else {
                                MasterAudio.UpdateRealTimeParameter(aEvent.parameterCommandName, aEvent.parameterNewValue);
                            }
#else
                            MasterAudio.UpdateRealTimeParameter(aEvent.parameterCommandName, aEvent.parameterNewValue);
#endif
                            break;
                    }
                    break;
            }
        }

        protected virtual string GetSubLabel()
        {
            var label = "";

            if (AudioEvent == null) return label;

            switch (AudioEvent.currentSoundFunctionType)
            {
                case MasterAudio.EventSoundFunctionType.PlaySound:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.soundType} : {(AudioEvent.variationName != "" ? AudioEvent.variationName : "Random" )}";
                    break;
                case MasterAudio.EventSoundFunctionType.BusControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentBusCommand} : {AudioEvent.busName}";
                    break;
                case MasterAudio.EventSoundFunctionType.GroupControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentSoundGroupCommand} : {AudioEvent.soundType}";
                    break;
                case MasterAudio.EventSoundFunctionType.GlobalControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentGlobalCommand}";
                    break;
                case MasterAudio.EventSoundFunctionType.ParameterCommandControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentParameterCmdCommand} : {AudioEvent.parameterCommandName}";
                    break;
                case MasterAudio.EventSoundFunctionType.CustomEventControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.theCustomEventName}";
                    break;
                case MasterAudio.EventSoundFunctionType.ParameterControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentParameterCommand} : {AudioEvent.parameterCommandName}";
                    break;
                case MasterAudio.EventSoundFunctionType.PersistentSettingsControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentPersistentSettingsCommand}";
                    break;
                case MasterAudio.EventSoundFunctionType.PlaylistControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentPlaylistCommand} : {AudioEvent.playlistControllerName} ";
                    break;
                case MasterAudio.EventSoundFunctionType.UnityMixerControl:
                    label += $"{AudioEvent.currentSoundFunctionType} : {AudioEvent.currentMixerCommand}";
                    break;
            }

            return label;

        }

        /// <summary>
        /// Returns the duration of the sound, or of the longest of the random sounds
        /// </summary>
        /// <returns></returns>
        protected virtual float GetDuration()
        {
            var maxLenght = 0f;
            if (MasterAudio.Instance && AudioEvent.currentSoundFunctionType == MasterAudio.EventSoundFunctionType.PlaySound && !string.IsNullOrEmpty(AudioEvent.soundType))
            {
                var child = MasterAudio.Instance.transform.Find(AudioEvent.soundType);
                if (child)
                {
                    var group = child.GetComponent<MasterAudioGroup>();
                    if (group)
                    {
                        foreach (var variation in group.groupVariations)
                        {
                            var curLenght = variation.VarAudio.clip.length;
                            if (curLenght > maxLenght) maxLenght = curLenght;
                        }
                    }
                }
            }

            if (AudioEvent.currentSoundFunctionType == MasterAudio.EventSoundFunctionType.PlaylistControl && AudioEvent.currentPlaylistCommand == MasterAudio.PlaylistCommand.FadeToVolume)
            {
                maxLenght = AudioEvent.fadeTime;
            }

            return maxLenght;
        }
    }
}
