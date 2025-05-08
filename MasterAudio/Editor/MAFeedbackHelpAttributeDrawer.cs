using System.Collections.Generic;
using DarkTonic.MasterAudio;
using DarkTonic.MasterAudio.EditorScripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [CustomPropertyDrawer(typeof(MAFeedbackHelpAttribute))]
    public class MAFeedbackHelpAttributeDrawer : PropertyDrawer
    {
        private List<string> _groupNames;
        private List<string> _busNames;
        private List<string> _playlistNames;

        private List<string> _playlistControllerNames =
            new List<string> { MasterAudio.DynamicGroupName, MasterAudio.NoGroupName };

        private List<string> _customEventNames;
        private List<string> _parameterCmdNames;
        private List<string> _parameterNames;
        private bool _maInScene;
        private MasterAudio _ma;
        private bool _isDirty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _ma = MasterAudio.Instance;
            _maInScene = _ma != null;

            if (_maInScene)
            {
                DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.LogoTexture);
            }

            _isDirty = false;

            if (_maInScene)
            {
                // ReSharper disable once PossibleNullReferenceException
                _groupNames = _ma.GroupNames;
                _busNames = _ma.BusNames;
                _playlistNames = _ma.PlaylistNames;
                _customEventNames = _ma.CustomEventNames;
                _parameterCmdNames = _ma.ParameterCommandNames;
                _parameterNames = _ma.ParameterNames;

                var magn = Object.FindObjectsByType<PlaylistController>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                foreach (var c in magn)
                {
                    _playlistControllerNames.Add(c.name);
                }
            }


            var showVolumeSlider = true;
            var aEvent = property.boxedValue as FeedbackMAEvent;
            ;

            EditorGUI.indentLevel = 1;

            DTGUIHelper.StartGroupHeader();

            EditorGUILayout.BeginHorizontal();

            var newExpanded = DTGUIHelper.Foldout(aEvent.isExpanded, "Action");
            if (newExpanded != aEvent.isExpanded)
            {
                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, property.serializedObject.targetObject,
                    "toggle expand Action");
                SerializedProperty prop = property.FindPropertyRelative("isExpanded");
                prop.boolValue = newExpanded;
                prop.serializedObject.ApplyModifiedProperties();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Space(4);
            DTGUIHelper.AddHelpIconNoStyle("https://www.dtdevtools.com/docs/masteraudio/EventSounds.htm#Actions");

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;

            if (aEvent.isExpanded)
            {
                EditorGUI.indentLevel = 0;

                var newSoundType =
                    (MasterAudio.EventSoundFunctionType)EditorGUILayout.EnumPopup("Action Type",
                        aEvent.currentSoundFunctionType);
                if (newSoundType != aEvent.currentSoundFunctionType)
                {
                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, property.serializedObject.targetObject,
                        "change Action Type");
                    SerializedProperty prop = property.FindPropertyRelative("currentSoundFunctionType");
                    prop.enumValueIndex = (int)newSoundType;
                    prop.serializedObject.ApplyModifiedProperties();
                }

                switch (aEvent.currentSoundFunctionType)
                {
                    case MasterAudio.EventSoundFunctionType.PlaySound:
                        if (_maInScene)
                        {
                            var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                            int? groupIndex = null;

                            EditorGUI.indentLevel = 1;

                            var noGroup = false;
                            var noMatch = false;

                            if (existingIndex >= 1)
                            {
                                EditorGUILayout.BeginHorizontal();
                                groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                                if (existingIndex == 1)
                                {
                                    noGroup = true;
                                }

                                var isUsingVideoPlayersGroup = false;

                                if (_groupNames[groupIndex.Value] == MasterAudio.VideoPlayerSoundGroupName)
                                {
                                    isUsingVideoPlayersGroup = true;
                                }

                                if (groupIndex > MasterAudio.HardCodedBusOptions - 1)
                                {
                                    var button = DTGUIHelper.AddSettingsButton("Sound Group");
                                    switch (button)
                                    {
                                        case DTGUIHelper.DTFunctionButtons.Go:
                                            var grp = _groupNames[existingIndex];
                                            var trs = MasterAudio.FindGroupTransform(grp);
                                            if (trs != null)
                                            {
                                                Selection.activeObject = trs;
                                            }

                                            break;
                                    }

                                    var buttonPress = DTGUIHelper.AddDynamicVariationButtons();
                                    var sType = _groupNames[existingIndex];

                                    switch (buttonPress)
                                    {
                                        case DTGUIHelper.DTFunctionButtons.Play:
                                            DTGUIHelper.PreviewSoundGroup(sType);
                                            break;
                                        case DTGUIHelper.DTFunctionButtons.Stop:
                                            DTGUIHelper.StopPreview(sType);
                                            break;
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                                if (isUsingVideoPlayersGroup)
                                {
                                    DTGUIHelper.ShowRedError(MasterAudio.VideoPlayersSoundGroupSelectedError);
                                }
                            }
                            else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                            {
                                groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, _groupNames.ToArray());
                            }
                            else
                            {
                                // non-match
                                noMatch = true;
                                var newSound = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                if (newSound != aEvent.soundType)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Sound Group");
                                    SerializedProperty prop = property.FindPropertyRelative("soundType");
                                    prop.stringValue = newSound;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newIndex = EditorGUILayout.Popup("All Sound Groups", -1, _groupNames.ToArray());
                                if (newIndex >= 0)
                                {
                                    groupIndex = newIndex;
                                }
                            }

                            if (noGroup)
                            {
                                DTGUIHelper.ShowRedError("No Sound Group specified. Action will do nothing.");
                            }
                            else if (noMatch)
                            {
                                DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
                            }

                            if (groupIndex.HasValue)
                            {
                                if (existingIndex != groupIndex.Value)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Sound Group");
                                }

                                SerializedProperty prop = property.FindPropertyRelative("soundType");

                                switch (groupIndex.Value)
                                {
                                    case -1:
                                        prop.stringValue = MasterAudio.NoGroupName;
                                        break;
                                    default:
                                        prop.stringValue = _groupNames[groupIndex.Value];
                                        break;
                                }

                                prop.serializedObject.ApplyModifiedProperties();
                            }
                        }
                        else
                        {
                            var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                            if (newSType != aEvent.soundType)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject,
                                    "change Sound Group");
                                SerializedProperty prop = property.FindPropertyRelative("soundType");
                                prop.stringValue = newSType;
                                prop.serializedObject.ApplyModifiedProperties();
                            }
                        }

                        var newVarType =
                            (EventSounds.VariationType)EditorGUILayout.EnumPopup("Variation Mode",
                                aEvent.variationType);
                        if (newVarType != aEvent.variationType)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "change Variation Mode");
                            SerializedProperty prop = property.FindPropertyRelative("variationType");
                            prop.enumValueIndex = (int)newVarType;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.variationType == EventSounds.VariationType.PlaySpecific)
                        {
                            var newVarName = EditorGUILayout.TextField("Variation Name", aEvent.variationName);
                            if (newVarName != aEvent.variationName)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject,
                                    "change Variation Name");
                                SerializedProperty prop = property.FindPropertyRelative("variationName");
                                prop.stringValue = newVarName;
                                prop.serializedObject.ApplyModifiedProperties();
                            }

                            if (string.IsNullOrEmpty(aEvent.variationName))
                            {
                                DTGUIHelper.ShowRedError("Variation Name is empty. No sound will play.");
                            }
                        }

                        if (showVolumeSlider)
                        {
                            var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                            if (newVol != aEvent.volume)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject, "change Volume");
                                SerializedProperty prop = property.FindPropertyRelative("volume");
                                prop.floatValue = newVol;
                                prop.serializedObject.ApplyModifiedProperties();
                            }
                        }

                        var newFixedPitch = EditorGUILayout.Toggle("Override Pitch", aEvent.useFixedPitch);
                        if (newFixedPitch != aEvent.useFixedPitch)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "toggle Override Pitch");
                            SerializedProperty prop = property.FindPropertyRelative("useFixedPitch");
                            prop.boolValue = newFixedPitch;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.useFixedPitch)
                        {
                            EditorGUI.indentLevel = 1;
                            var newPitch = DTGUIHelper.DisplayPitchField(aEvent.pitch);
                            if (newPitch != aEvent.pitch)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject, "change Pitch");
                                SerializedProperty prop = property.FindPropertyRelative("pitch");
                                prop.floatValue = newPitch;
                                prop.serializedObject.ApplyModifiedProperties();
                            }
                        }

                        EditorGUI.indentLevel = 1;

                        var aud = aEvent.GetNamedOrFirstAudioSource();

                        if (aud != null)
                        {
                            var newShowgiz = EditorGUILayout.Toggle("Adjust Audio Range", aEvent.showSphereGizmo);
                            if (newShowgiz != aEvent.showSphereGizmo)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject,
                                    "toggle Adjust Audio Range");
                                SerializedProperty prop = property.FindPropertyRelative("showSphereGizmo");
                                prop.boolValue = newShowgiz;
                                prop.serializedObject.ApplyModifiedProperties();
                            }

                            if (aEvent.showSphereGizmo)
                            {
                                var newMin = EditorGUILayout.Slider("Min Distance", aud.minDistance, .1f,
                                    aud.maxDistance);
                                if (newMin != aud.minDistance)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, aud,
                                        "change Min Distance");

                                    switch (aEvent.variationType)
                                    {
                                        case EventSounds.VariationType.PlayRandom:
                                            var sources = aEvent.GetAllVariationAudioSources();
                                            if (sources != null)
                                            {
                                                for (var i = 0; i < sources.Count; i++)
                                                {
                                                    var src = sources[i];
                                                    src.minDistance = newMin;
                                                    EditorUtility.SetDirty(src);
                                                }
                                            }

                                            break;
                                        case EventSounds.VariationType.PlaySpecific:
                                            aud.minDistance = newMin;
                                            EditorUtility.SetDirty(aud);
                                            break;
                                    }
                                }

                                var newMax = EditorGUILayout.Slider("Max Distance", aud.maxDistance, .1f, 1000000f);
                                if (newMax != aud.maxDistance)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty, aud,
                                        "change Max Distance");

                                    switch (aEvent.variationType)
                                    {
                                        case EventSounds.VariationType.PlayRandom:
                                            var sources = aEvent.GetAllVariationAudioSources();
                                            if (sources != null)
                                            {
                                                for (var i = 0; i < sources.Count; i++)
                                                {
                                                    var src = sources[i];
                                                    src.maxDistance = newMax;
                                                    EditorUtility.SetDirty(src);
                                                }
                                            }

                                            break;
                                        case EventSounds.VariationType.PlaySpecific:
                                            aud.maxDistance = newMax;
                                            EditorUtility.SetDirty(aud);
                                            break;
                                    }
                                }

                                switch (aEvent.variationType)
                                {
                                    case EventSounds.VariationType.PlayRandom:
                                        DTGUIHelper.ShowLargeBarAlert(
                                            "Adjusting the Max Distance field will change the Max Distance on the Audio Source of every Variation in the selected Sound Group.");
                                        break;
                                    case EventSounds.VariationType.PlaySpecific:
                                        DTGUIHelper.ShowLargeBarAlert(
                                            "Adjusting the Max Distance field will change the Max Distance on the Audio Source for the selected Variation in the selected Sound Group.");
                                        break;
                                }

                                DTGUIHelper.ShowColorWarning(
                                    "You can also bulk apply Max Distance and other Audio Source properties with Audio Source Templates using the Master Audio Mixer.");
                            }
                        }

                        var newGlide =
                            (EventSounds.GlidePitchType)EditorGUILayout.EnumPopup("Glide By Pitch Type",
                                aEvent.glidePitchType);
                        if (newGlide != aEvent.glidePitchType)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject,
                                "toggle Glide By Pitch Type");
                            SerializedProperty prop = property.FindPropertyRelative("glidePitchType");
                            prop.enumValueIndex = (int)newGlide;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.glidePitchType != EventSounds.GlidePitchType.None)
                        {
                            EditorGUI.indentLevel = 2;
                            var fieldLabel = "Target Pitch";
                            switch (aEvent.glidePitchType)
                            {
                                case EventSounds.GlidePitchType.RaisePitch:
                                    fieldLabel = "Raise Pitch By";
                                    break;
                                case EventSounds.GlidePitchType.LowerPitch:
                                    fieldLabel = "Lower Pitch By";
                                    break;
                            }

                            var newTargetPitch = DTGUIHelper.DisplayPitchField(aEvent.targetGlidePitch, fieldLabel);
                            if (newTargetPitch != aEvent.targetGlidePitch)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject,
                                    "change " + fieldLabel);
                                SerializedProperty prop = property.FindPropertyRelative("targetGlidePitch");
                                prop.floatValue = newTargetPitch;
                                prop.serializedObject.ApplyModifiedProperties();
                            }

                            var newGlideTime = EditorGUILayout.Slider("Glide Time", aEvent.pitchGlideTime, 0f, 100f);
                            if (newGlideTime != aEvent.pitchGlideTime)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject, "change Glide Time");
                                SerializedProperty prop = property.FindPropertyRelative("pitchGlideTime");
                                prop.floatValue = newGlideTime;
                                prop.serializedObject.ApplyModifiedProperties();
                            }

                            if (_maInScene)
                            {
                                var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                int? customEventIndex = null;

                                EditorGUI.indentLevel = 2;

                                var noEvent = false;
                                var noMatch = false;

                                if (existingIndex >= 1)
                                {
                                    customEventIndex = EditorGUILayout.Popup("Finished Custom Event", existingIndex,
                                        _customEventNames.ToArray());
                                    if (existingIndex == 1)
                                    {
                                        noEvent = true;
                                    }
                                }
                                else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                {
                                    customEventIndex = EditorGUILayout.Popup("Finished Custom Event", existingIndex,
                                        _customEventNames.ToArray());
                                }
                                else
                                {
                                    // non-match
                                    noMatch = true;
                                    var newEventName = EditorGUILayout.TextField("Finished Custom Event",
                                        aEvent.theCustomEventName);
                                    if (newEventName != aEvent.theCustomEventName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Finished Custom Event");
                                        SerializedProperty prop = property.FindPropertyRelative("theCustomEventName");
                                        prop.stringValue = newEventName;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    var newIndex = EditorGUILayout.Popup("All Custom Events", -1,
                                        _customEventNames.ToArray());
                                    if (newIndex >= 0)
                                    {
                                        customEventIndex = newIndex;
                                    }
                                }

                                if (noEvent)
                                {
                                    DTGUIHelper.ShowRedError(
                                        "No Custom Event specified. This section will do nothing.");
                                }
                                else if (noMatch)
                                {
                                    DTGUIHelper.ShowRedError("Custom Event found no match. Type in or choose one.");
                                }

                                if (customEventIndex.HasValue)
                                {
                                    if (existingIndex != customEventIndex.Value)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Custom Event");
                                    }

                                    SerializedProperty prop = property.FindPropertyRelative("theCustomEventName");
                                    switch (customEventIndex.Value)
                                    {
                                        case -1:
                                            prop.stringValue = MasterAudio.NoGroupName;
                                            break;
                                        default:
                                            prop.stringValue = _customEventNames[customEventIndex.Value];
                                            break;
                                    }


                                    prop.serializedObject.ApplyModifiedProperties();
                                }
                            }
                            else
                            {
                                var newCustomEvent = EditorGUILayout.TextField("Finished Custom Event",
                                    aEvent.theCustomEventName);
                                if (newCustomEvent != aEvent.theCustomEventName)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "Finished Custom Event");
                                    SerializedProperty prop = property.FindPropertyRelative("theCustomEventName");
                                    prop.stringValue = newCustomEvent;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }
                            }
                        }

                        EditorGUI.indentLevel = 1;

                        var newDelay = EditorGUILayout.Slider("Delay Sound (sec)", aEvent.delaySound, 0f, 10f);
                        if (newDelay != aEvent.delaySound)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "change Delay Sound");
                            SerializedProperty prop = property.FindPropertyRelative("delaySound");
                            prop.floatValue = newDelay;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.PlaylistControl:
                        EditorGUI.indentLevel = 1;
                        var newPlaylistCmd =
                            (MasterAudio.PlaylistCommand)EditorGUILayout.EnumPopup("Playlist Command",
                                aEvent.currentPlaylistCommand);
                        if (newPlaylistCmd != aEvent.currentPlaylistCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject,
                                "change Playlist Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentPlaylistCommand");
                            prop.enumValueIndex = (int)newPlaylistCmd;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.currentPlaylistCommand != MasterAudio.PlaylistCommand.None)
                        {
                            // show Playlist Controller dropdown
                            if (EventSounds.PlaylistCommandsWithAll.Contains(aEvent.currentPlaylistCommand))
                            {
                                var newAllControllers = EditorGUILayout.Toggle("All Playlist Controllers?",
                                    aEvent.allPlaylistControllersForGroupCmd);
                                if (newAllControllers != aEvent.allPlaylistControllersForGroupCmd)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle All Playlist Controllers");
                                    SerializedProperty prop =
                                        property.FindPropertyRelative("allPlaylistControllersForGroupCmd");
                                    prop.boolValue = newAllControllers;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }
                            }

                            if (!aEvent.allPlaylistControllersForGroupCmd)
                            {
                                if (_playlistControllerNames.Count > 0)
                                {
                                    var existingIndex = _playlistControllerNames.IndexOf(aEvent.playlistControllerName);

                                    int? playlistControllerIndex = null;

                                    var noPC = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        playlistControllerIndex = EditorGUILayout.Popup("Playlist Controller",
                                            existingIndex, _playlistControllerNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noPC = true;
                                        }
                                    }
                                    else if (existingIndex == -1 &&
                                             aEvent.playlistControllerName == MasterAudio.NoGroupName)
                                    {
                                        playlistControllerIndex = EditorGUILayout.Popup("Playlist Controller",
                                            existingIndex, _playlistControllerNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        noMatch = true;

                                        var newPlaylistController = EditorGUILayout.TextField("Playlist Controller",
                                            aEvent.playlistControllerName);
                                        if (newPlaylistController != aEvent.playlistControllerName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Playlist Controller");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("playlistControllerName");
                                            prop.stringValue = newPlaylistController;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Playlist Controllers", -1,
                                            _playlistControllerNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            playlistControllerIndex = newIndex;
                                        }
                                    }

                                    if (noPC)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "No Playlist Controller specified. Action will do nothing.");
                                    }
                                    else if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "Playlist Controller found no match. Type in or choose one.");
                                    }

                                    if (playlistControllerIndex.HasValue)
                                    {
                                        if (existingIndex != playlistControllerIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Playlist Controller");
                                        }

                                        SerializedProperty prop =
                                            property.FindPropertyRelative("playlistControllerName");

                                        switch (playlistControllerIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue =
                                                    _playlistControllerNames[playlistControllerIndex.Value];
                                                break;
                                        }


                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newPlaylistControllerName = EditorGUILayout.TextField("Playlist Controller",
                                        aEvent.playlistControllerName);
                                    if (newPlaylistControllerName != aEvent.playlistControllerName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Playlist Controller");
                                        SerializedProperty prop =
                                            property.FindPropertyRelative("playlistControllerName");
                                        prop.stringValue = newPlaylistControllerName;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                            }
                        }

                        switch (aEvent.currentPlaylistCommand)
                        {
                            case MasterAudio.PlaylistCommand.None:
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                break;
                            case MasterAudio.PlaylistCommand.StopLoopingCurrentSong:
                                break;
                            case MasterAudio.PlaylistCommand.AddSongToQueue:
                                var newClip = EditorGUILayout.TextField("Song Name", aEvent.clipName);
                                if (newClip != aEvent.clipName)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Song Name");
                                    SerializedProperty prop = property.FindPropertyRelative("clipName");
                                    prop.stringValue = newClip;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (string.IsNullOrEmpty(aEvent.clipName))
                                {
                                    DTGUIHelper.ShowRedError("Song name is empty. Action will do nothing.");
                                }

                                break;
                            case MasterAudio.PlaylistCommand.ChangePlaylist:
                            case MasterAudio.PlaylistCommand.Start:
                                // show playlist name dropdown
                                if (_maInScene)
                                {
                                    var existingIndex = _playlistNames.IndexOf(aEvent.playlistName);

                                    int? playlistIndex = null;

                                    var noPl = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        playlistIndex = EditorGUILayout.Popup("Playlist Name", existingIndex,
                                            _playlistNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noPl = true;
                                        }
                                    }
                                    else if (existingIndex == -1 && aEvent.playlistName == MasterAudio.NoGroupName)
                                    {
                                        playlistIndex = EditorGUILayout.Popup("Playlist Name", existingIndex,
                                            _playlistNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        noMatch = true;

                                        var newPlaylist =
                                            EditorGUILayout.TextField("Playlist Name", aEvent.playlistName);
                                        if (newPlaylist != aEvent.playlistName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Playlist Name");
                                            SerializedProperty prop = property.FindPropertyRelative("playlistName");
                                            prop.stringValue = newPlaylist;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Playlists", -1,
                                            _playlistNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            playlistIndex = newIndex;
                                        }
                                    }

                                    if (noPl)
                                    {
                                        DTGUIHelper.ShowRedError("No Playlist Name specified. Action will do nothing.");
                                    }
                                    else if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "Playlist Name found no match. Type in or choose one.");
                                    }

                                    if (playlistIndex.HasValue)
                                    {
                                        if (existingIndex != playlistIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Playlist Name");
                                        }

                                        SerializedProperty prop = property.FindPropertyRelative("playlistName");

                                        switch (playlistIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue = _playlistNames[playlistIndex.Value];
                                                break;
                                        }

                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newPlaylistName =
                                        EditorGUILayout.TextField("Playlist Name", aEvent.playlistName);
                                    if (newPlaylistName != aEvent.playlistName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Playlist Name");
                                        SerializedProperty prop = property.FindPropertyRelative("playlistName");
                                        prop.stringValue = newPlaylistName;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                if (aEvent.currentPlaylistCommand == MasterAudio.PlaylistCommand.ChangePlaylist)
                                {
                                    var newStartPlaylist =
                                        EditorGUILayout.Toggle("Start Playlist?", aEvent.startPlaylist);
                                    if (newStartPlaylist != aEvent.startPlaylist)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "toggle Start Playlist");
                                        SerializedProperty prop = property.FindPropertyRelative("startPlaylist");
                                        prop.boolValue = newStartPlaylist;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.PlaylistCommand.FadeToVolume:
                                if (showVolumeSlider)
                                {
                                    var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Target Volume");
                                    if (newFadeVol != aEvent.fadeVolume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Target Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("fadeVolume");
                                        prop.floatValue = newFadeVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                var newFadeTime = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeTime != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTime;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.PlaylistCommand.PlaySong:
                                var newSong = EditorGUILayout.TextField("Song Name", aEvent.clipName);
                                if (newSong != aEvent.clipName)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Song Name");
                                    SerializedProperty prop = property.FindPropertyRelative("clipName");
                                    prop.stringValue = newSong;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (string.IsNullOrEmpty(aEvent.clipName))
                                {
                                    DTGUIHelper.ShowRedError("Song name is empty. Action will do nothing.");
                                }

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.GroupControl:
                        EditorGUI.indentLevel = 1;

                        var newGroupCmd =
                            (MasterAudio.SoundGroupCommand)EditorGUILayout.EnumPopup("Group Command",
                                aEvent.currentSoundGroupCommand);
                        if (newGroupCmd != aEvent.currentSoundGroupCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "change Group Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentSoundGroupCommand");
                            prop.enumValueIndex = (int)newGroupCmd;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (!MasterAudio.GroupCommandsWithNoGroupSelector.Contains(aEvent.currentSoundGroupCommand))
                        {
                            if (!MasterAudio.GroupCommandsWithNoAllGroupSelector.Contains(
                                    aEvent.currentSoundGroupCommand))
                            {
                                var newAllTypes = EditorGUILayout.Toggle("Do For Every Group?",
                                    aEvent.allSoundTypesForGroupCmd);
                                if (newAllTypes != aEvent.allSoundTypesForGroupCmd)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Do For Every Group?");
                                    SerializedProperty prop = property.FindPropertyRelative("allSoundTypesForGroupCmd");
                                    prop.boolValue = newAllTypes;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }
                            }

                            if (!aEvent.allSoundTypesForGroupCmd)
                            {
                                if (_maInScene)
                                {
                                    var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                                    int? groupIndex = null;

                                    var noGroup = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex,
                                            _groupNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noGroup = true;
                                        }
                                    }
                                    else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                    {
                                        groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex,
                                            _groupNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                        if (newSType != aEvent.soundType)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Sound Group");
                                            SerializedProperty prop = property.FindPropertyRelative("soundType");
                                            prop.stringValue = newSType;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Sound Groups", -1,
                                            _groupNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            groupIndex = newIndex;
                                        }

                                        noMatch = true;
                                    }

                                    if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
                                    }
                                    else if (noGroup)
                                    {
                                        DTGUIHelper.ShowRedError("No Sound Group specified. Action will do nothing.");
                                    }

                                    if (groupIndex.HasValue)
                                    {
                                        if (existingIndex != groupIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Sound Group");
                                        }

                                        SerializedProperty prop = property.FindPropertyRelative("soundType");

                                        switch (groupIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue = _groupNames[groupIndex.Value];
                                                break;
                                        }

                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newSoundT = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                    if (newSoundT != aEvent.soundType)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Sound Group");
                                        SerializedProperty prop = property.FindPropertyRelative("soundType");
                                        prop.stringValue = newSoundT;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                            }
                        }

                        switch (aEvent.currentSoundGroupCommand)
                        {
                            case MasterAudio.SoundGroupCommand.None:
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                break;
                            case MasterAudio.SoundGroupCommand.StopOldSoundGroupVoices:
                                var minAge = EditorGUILayout.Slider("Min. Age", aEvent.minAge, 0f, 100f);
                                if (minAge != aEvent.minAge)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Min Age");
                                    SerializedProperty prop = property.FindPropertyRelative("minAge");
                                    prop.floatValue = minAge;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutOldSoundGroupVoices:
                                var minAge2 = EditorGUILayout.Slider("Min. Age", aEvent.minAge, 0f, 100f);
                                if (minAge2 != aEvent.minAge)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Min Age");
                                    SerializedProperty prop = property.FindPropertyRelative("minAge");
                                    prop.floatValue = minAge2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newFadeTimeX = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeTimeX != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTimeX;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.ToggleSoundGroupOfTransform:
                            case MasterAudio.SoundGroupCommand.ToggleSoundGroup:
                                if (showVolumeSlider)
                                {
                                    var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                    if (newVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                var newFixedPitch2 = EditorGUILayout.Toggle("Override Pitch", aEvent.useFixedPitch);
                                if (newFixedPitch2 != aEvent.useFixedPitch)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Override Pitch");
                                    SerializedProperty prop = property.FindPropertyRelative("useFixedPitch");
                                    prop.boolValue = newFixedPitch2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.useFixedPitch)
                                {
                                    EditorGUI.indentLevel = 1;
                                    var newPitch = DTGUIHelper.DisplayPitchField(aEvent.pitch);
                                    if (newPitch != aEvent.pitch)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Pitch");
                                        SerializedProperty prop = property.FindPropertyRelative("pitch");
                                        prop.floatValue = newPitch;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                EditorGUI.indentLevel = 1;

                                var newDelay2 = EditorGUILayout.Slider("Delay Sound (sec)", aEvent.delaySound, 0f, 10f);
                                if (newDelay2 != aEvent.delaySound)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Delay Sound");
                                    SerializedProperty prop = property.FindPropertyRelative("delaySound");
                                    prop.floatValue = newDelay2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newFadeTime = EditorGUILayout.Slider("Fade Out Time If Playing", aEvent.fadeTime,
                                    0f, 10f);
                                if (newFadeTime != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Out Time If Playing");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTime;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.GlideByPitch:
                                var newGlide2 =
                                    (EventSounds.GlidePitchType)EditorGUILayout.EnumPopup("Glide By Pitch Type",
                                        aEvent.glidePitchType);
                                if (newGlide2 != aEvent.glidePitchType)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Glide By Pitch Type");
                                    SerializedProperty prop = property.FindPropertyRelative("glidePitchType");
                                    prop.enumValueIndex = (int)newGlide2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.glidePitchType != EventSounds.GlidePitchType.None)
                                {
                                    EditorGUI.indentLevel = 2;
                                    var fieldLabel = "Target Pitch";
                                    switch (aEvent.glidePitchType)
                                    {
                                        case EventSounds.GlidePitchType.RaisePitch:
                                            fieldLabel = "Raise Pitch By";
                                            break;
                                        case EventSounds.GlidePitchType.LowerPitch:
                                            fieldLabel = "Lower Pitch By";
                                            break;
                                    }

                                    var newTargetPitch =
                                        DTGUIHelper.DisplayPitchField(aEvent.targetGlidePitch, fieldLabel);
                                    if (newTargetPitch != aEvent.targetGlidePitch)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change " + fieldLabel);
                                        SerializedProperty prop = property.FindPropertyRelative("targetGlidePitch");
                                        prop.floatValue = newTargetPitch;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    var newGlideTime =
                                        EditorGUILayout.Slider("Glide Time", aEvent.pitchGlideTime, 0f, 100f);
                                    if (newGlideTime != aEvent.pitchGlideTime)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Glide Time");
                                        SerializedProperty prop = property.FindPropertyRelative("pitchGlideTime");
                                        prop.floatValue = newGlideTime;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    if (_maInScene)
                                    {
                                        var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                        int? customEventIndex = null;

                                        EditorGUI.indentLevel = 2;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noEvent = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Finished Custom Event",
                                                aEvent.theCustomEventName);
                                            if (newEventName != aEvent.theCustomEventName)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Finished Custom Event");
                                                SerializedProperty prop =
                                                    property.FindPropertyRelative("theCustomEventName");
                                                prop.stringValue = newEventName;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Custom Events", -1,
                                                _customEventNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                customEventIndex = newIndex;
                                            }
                                        }

                                        if (noEvent)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "No Custom Event specified. This section will do nothing.");
                                        }
                                        else if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "Custom Event found no match. Type in or choose one.");
                                        }

                                        if (customEventIndex.HasValue)
                                        {
                                            if (existingIndex != customEventIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Custom Event");
                                            }

                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");

                                            switch (customEventIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _customEventNames[customEventIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newCustomEvent = EditorGUILayout.TextField("Finished Custom Event",
                                            aEvent.theCustomEventName);
                                        if (newCustomEvent != aEvent.theCustomEventName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "Finished Custom Event");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");
                                            prop.stringValue = newCustomEvent;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }
                                else
                                {
                                    DTGUIHelper.ShowColorWarning(
                                        "Choosing 'None' for Glide By Pitch Type means this action will do nothing.");
                                }

                                EditorGUI.indentLevel = 1;

                                break;
                            case MasterAudio.SoundGroupCommand.FadeToVolume:
                                if (showVolumeSlider)
                                {
                                    var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Target Volume");
                                    if (newFadeVol != aEvent.fadeVolume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Target Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("fadeVolume");
                                        prop.floatValue = newFadeVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                var newFadeTime2 = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeTime2 != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTime2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newStop = EditorGUILayout.Toggle("Stop Group After Fade", aEvent.stopAfterFade);
                                if (newStop != aEvent.stopAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Stop Group After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("stopAfterFade");
                                    prop.boolValue = newStop;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newRestore = EditorGUILayout.Toggle("Restore Volume After Fade",
                                    aEvent.restoreVolumeAfterFade);
                                if (newRestore != aEvent.restoreVolumeAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Restore Volume After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("restoreVolumeAfterFade");
                                    prop.boolValue = newRestore;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newCust = EditorGUILayout.Toggle("Custom Event After Fade",
                                    aEvent.fireCustomEventAfterFade);
                                if (newCust != aEvent.fireCustomEventAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Custom Event After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("fireCustomEventAfterFade");
                                    prop.boolValue = newCust;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.fireCustomEventAfterFade)
                                {
                                    if (_maInScene)
                                    {
                                        var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                        int? customEventIndex = null;

                                        EditorGUI.indentLevel = 2;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noEvent = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Finished Custom Event",
                                                aEvent.theCustomEventName);
                                            if (newEventName != aEvent.theCustomEventName)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Finished Custom Event");
                                                SerializedProperty prop =
                                                    property.FindPropertyRelative("theCustomEventName");
                                                prop.stringValue = newEventName;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Custom Events", -1,
                                                _customEventNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                customEventIndex = newIndex;
                                            }
                                        }

                                        if (noEvent)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "No Custom Event specified. This section will do nothing.");
                                        }
                                        else if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "Custom Event found no match. Type in or choose one.");
                                        }

                                        if (customEventIndex.HasValue)
                                        {
                                            if (existingIndex != customEventIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Custom Event");
                                            }

                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");

                                            switch (customEventIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _customEventNames[customEventIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newCustomEvent = EditorGUILayout.TextField("Finished Custom Event",
                                            aEvent.theCustomEventName);
                                        if (newCustomEvent != aEvent.theCustomEventName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "Finished Custom Event");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");
                                            prop.stringValue = newCustomEvent;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutAllOfSound:
                                var newFadeT = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeT != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeT;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.FadeOutSoundGroupOfTransform:
                            case MasterAudio.SoundGroupCommand.FadeOutAllSoundsOfTransform:
                                var newFade = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFade != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFade;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.FadeSoundGroupOfTransformToVolume:
                                var newFade2 = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFade2 != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFade2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.RouteToBus:
                                if (_maInScene)
                                {
                                    var existingIndex = _busNames.IndexOf(aEvent.busName);

                                    int? busIndex = null;

                                    var noBus = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                            _busNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noBus = true;
                                        }
                                    }
                                    else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName)
                                    {
                                        busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                            _busNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                        if (newBusName != aEvent.busName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Bus Name");
                                            SerializedProperty prop = property.FindPropertyRelative("busName");
                                            prop.stringValue = newBusName;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Buses", -1,
                                            _busNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            busIndex = newIndex;
                                        }

                                        noMatch = true;
                                    }

                                    if (noBus)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "No Bus Name specified. Action will do nothing.");
                                    }
                                    else if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "Bus Name found no match. Type in or choose one.");
                                    }

                                    if (busIndex.HasValue)
                                    {
                                        if (existingIndex != busIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Bus");
                                        }

                                        SerializedProperty prop = property.FindPropertyRelative("busName");

                                        switch (busIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue = _busNames[busIndex.Value];
                                                break;
                                        }

                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                    if (newBusName != aEvent.busName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Bus Name");
                                        SerializedProperty prop = property.FindPropertyRelative("busName");
                                        prop.stringValue = newBusName;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.SoundGroupCommand.Mute:
                                break;
                            case MasterAudio.SoundGroupCommand.Pause:
                                break;
                            case MasterAudio.SoundGroupCommand.Solo:
                                break;
                            case MasterAudio.SoundGroupCommand.Unmute:
                                break;
                            case MasterAudio.SoundGroupCommand.Unpause:
                                break;
                            case MasterAudio.SoundGroupCommand.Unsolo:
                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.BusControl:
                        EditorGUI.indentLevel = 1;
                        var newBusCmd =
                            (MasterAudio.BusCommand)EditorGUILayout.EnumPopup("Bus Command", aEvent.currentBusCommand);
                        if (newBusCmd != aEvent.currentBusCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "change Bus Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentBusCommand");
                            prop.enumValueIndex = (int)newBusCmd;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.currentBusCommand != MasterAudio.BusCommand.None)
                        {
                            var newAllTypes =
                                EditorGUILayout.Toggle("Do For Every Bus?", aEvent.allSoundTypesForBusCmd);
                            if (newAllTypes != aEvent.allSoundTypesForBusCmd)
                            {
                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                    property.serializedObject.targetObject,
                                    "toggle Do For Every Bus?");
                                SerializedProperty prop = property.FindPropertyRelative("allSoundTypesForBusCmd");
                                prop.boolValue = newAllTypes;
                                prop.serializedObject.ApplyModifiedProperties();
                            }

                            if (!aEvent.allSoundTypesForBusCmd)
                            {
                                if (_maInScene)
                                {
                                    var existingIndex = _busNames.IndexOf(aEvent.busName);

                                    int? busIndex = null;

                                    var noBus = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                            _busNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noBus = true;
                                        }
                                    }
                                    else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName)
                                    {
                                        busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                            _busNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                        if (newBusName != aEvent.busName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Bus Name");
                                            SerializedProperty prop = property.FindPropertyRelative("busName");
                                            prop.stringValue = newBusName;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Buses", -1, _busNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            busIndex = newIndex;
                                        }

                                        noMatch = true;
                                    }

                                    if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError("Bus Name found no match. Type in or choose one.");
                                    }
                                    else if (noBus)
                                    {
                                        DTGUIHelper.ShowRedError("No Bus Name specified. Action will do nothing.");
                                    }

                                    if (busIndex.HasValue)
                                    {
                                        if (existingIndex != busIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Bus");
                                        }

                                        SerializedProperty prop = property.FindPropertyRelative("busName");

                                        switch (busIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue = _busNames[busIndex.Value];
                                                break;
                                        }

                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                    if (newBusName != aEvent.busName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Bus Name");
                                        SerializedProperty prop = property.FindPropertyRelative("busName");
                                        prop.stringValue = newBusName;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                            }
                        }

                        switch (aEvent.currentBusCommand)
                        {
                            case MasterAudio.BusCommand.None:
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                break;
                            case MasterAudio.BusCommand.GlideByPitch:
                                var newGlide2 =
                                    (EventSounds.GlidePitchType)EditorGUILayout.EnumPopup("Glide By Pitch Type",
                                        aEvent.glidePitchType);
                                if (newGlide2 != aEvent.glidePitchType)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Glide By Pitch Type");
                                    SerializedProperty prop = property.FindPropertyRelative("glidePitchType");
                                    prop.enumValueIndex = (int)newGlide2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.glidePitchType != EventSounds.GlidePitchType.None)
                                {
                                    EditorGUI.indentLevel = 2;
                                    var fieldLabel = "Target Pitch";
                                    switch (aEvent.glidePitchType)
                                    {
                                        case EventSounds.GlidePitchType.RaisePitch:
                                            fieldLabel = "Raise Pitch By";
                                            break;
                                        case EventSounds.GlidePitchType.LowerPitch:
                                            fieldLabel = "Lower Pitch By";
                                            break;
                                    }

                                    var newTargetPitch =
                                        DTGUIHelper.DisplayPitchField(aEvent.targetGlidePitch, fieldLabel);
                                    if (newTargetPitch != aEvent.targetGlidePitch)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change " + fieldLabel);
                                        SerializedProperty prop = property.FindPropertyRelative("targetGlidePitch");
                                        prop.floatValue = newTargetPitch;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    var newGlideTime =
                                        EditorGUILayout.Slider("Glide Time", aEvent.pitchGlideTime, 0f, 100f);
                                    if (newGlideTime != aEvent.pitchGlideTime)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Glide Time");
                                        SerializedProperty prop = property.FindPropertyRelative("pitchGlideTime");
                                        prop.floatValue = newGlideTime;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    if (_maInScene)
                                    {
                                        var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                        int? customEventIndex = null;

                                        EditorGUI.indentLevel = 2;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noEvent = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Finished Custom Event",
                                                aEvent.theCustomEventName);
                                            if (newEventName != aEvent.theCustomEventName)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Finished Custom Event");
                                                SerializedProperty prop =
                                                    property.FindPropertyRelative("theCustomEventName");
                                                prop.stringValue = newEventName;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Custom Events", -1,
                                                _customEventNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                customEventIndex = newIndex;
                                            }
                                        }

                                        if (noEvent)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "No Custom Event specified. This section will do nothing.");
                                        }
                                        else if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "Custom Event found no match. Type in or choose one.");
                                        }

                                        if (customEventIndex.HasValue)
                                        {
                                            if (existingIndex != customEventIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Custom Event");
                                            }

                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");

                                            switch (customEventIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _customEventNames[customEventIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newCustomEvent = EditorGUILayout.TextField("Finished Custom Event",
                                            aEvent.theCustomEventName);
                                        if (newCustomEvent != aEvent.theCustomEventName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "Finished Custom Event");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");
                                            prop.stringValue = newCustomEvent;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }
                                else
                                {
                                    DTGUIHelper.ShowColorWarning(
                                        "Choosing 'None' for Glide By Pitch Type means this action will do nothing.");
                                }

                                EditorGUI.indentLevel = 1;
                                break;
                            case MasterAudio.BusCommand.ChangePitch:
                                var newPitch = DTGUIHelper.DisplayPitchField(aEvent.pitch);
                                if (newPitch != aEvent.pitch)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject, "change Pitch");
                                    SerializedProperty prop = property.FindPropertyRelative("pitch");
                                    prop.floatValue = newPitch;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.BusCommand.FadeToVolume:
                                if (showVolumeSlider)
                                {
                                    var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.fadeVolume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Target Volume");
                                    if (newFadeVol != aEvent.fadeVolume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Target Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("fadeVolume");
                                        prop.floatValue = newFadeVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                var newFadeTime = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeTime != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTime;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newStop = EditorGUILayout.Toggle("Stop Bus After Fade", aEvent.stopAfterFade);
                                if (newStop != aEvent.stopAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Stop Bus After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("stopAfterFade");
                                    prop.boolValue = newStop;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newRestore = EditorGUILayout.Toggle("Restore Volume After Fade",
                                    aEvent.restoreVolumeAfterFade);
                                if (newRestore != aEvent.restoreVolumeAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Restore Volume After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("restoreVolumeAfterFade");
                                    prop.boolValue = newRestore;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newCust = EditorGUILayout.Toggle("Custom Event After Fade",
                                    aEvent.fireCustomEventAfterFade);
                                if (newCust != aEvent.fireCustomEventAfterFade)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Custom Event After Fade");
                                    SerializedProperty prop = property.FindPropertyRelative("fireCustomEventAfterFade");
                                    prop.boolValue = newCust;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.fireCustomEventAfterFade)
                                {
                                    if (_maInScene)
                                    {
                                        var existingIndex = _customEventNames.IndexOf(aEvent.theCustomEventName);

                                        int? customEventIndex = null;

                                        EditorGUI.indentLevel = 2;

                                        var noEvent = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noEvent = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                        {
                                            customEventIndex = EditorGUILayout.Popup("Finished Custom Event",
                                                existingIndex, _customEventNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            noMatch = true;
                                            var newEventName = EditorGUILayout.TextField("Finished Custom Event",
                                                aEvent.theCustomEventName);
                                            if (newEventName != aEvent.theCustomEventName)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Finished Custom Event");
                                                SerializedProperty prop =
                                                    property.FindPropertyRelative("theCustomEventName");
                                                prop.stringValue = newEventName;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Custom Events", -1,
                                                _customEventNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                customEventIndex = newIndex;
                                            }
                                        }

                                        if (noEvent)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "No Custom Event specified. This section will do nothing.");
                                        }
                                        else if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "Custom Event found no match. Type in or choose one.");
                                        }

                                        if (customEventIndex.HasValue)
                                        {
                                            if (existingIndex != customEventIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Custom Event");
                                            }

                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");

                                            switch (customEventIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _customEventNames[customEventIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newCustomEvent = EditorGUILayout.TextField("Finished Custom Event",
                                            aEvent.theCustomEventName);
                                        if (newCustomEvent != aEvent.theCustomEventName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "Finished Custom Event");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("theCustomEventName");
                                            prop.stringValue = newCustomEvent;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }

                                break;
                            case MasterAudio.BusCommand.Pause:
                                break;
                            case MasterAudio.BusCommand.Unpause:
                                break;
                            case MasterAudio.BusCommand.StopOldBusVoices:
                                var minAge = EditorGUILayout.Slider("Min. Age", aEvent.minAge, 0f, 100f);
                                if (minAge != aEvent.minAge)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Min Age");
                                    SerializedProperty prop = property.FindPropertyRelative("minAge");
                                    prop.floatValue = minAge;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                            case MasterAudio.BusCommand.FadeOutOldBusVoices:
                                var minAge2 = EditorGUILayout.Slider("Min. Age", aEvent.minAge, 0f, 100f);
                                if (minAge2 != aEvent.minAge)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Min Age");
                                    SerializedProperty prop = property.FindPropertyRelative("minAge");
                                    prop.floatValue = minAge2;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newFadeTimeX = EditorGUILayout.Slider("Fade Time", aEvent.fadeTime, 0f, 10f);
                                if (newFadeTimeX != aEvent.fadeTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Fade Time");
                                    SerializedProperty prop = property.FindPropertyRelative("fadeTime");
                                    prop.floatValue = newFadeTimeX;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.CustomEventControl:
                        DTGUIHelper.ShowRedError(
                            "Select another Action Type.");
                        break;
                    case MasterAudio.EventSoundFunctionType.GlobalControl:
                        EditorGUI.indentLevel = 1;
                        var newCmd =
                            (MasterAudio.GlobalCommand)EditorGUILayout.EnumPopup("Global Cmd",
                                aEvent.currentGlobalCommand);
                        if (newCmd != aEvent.currentGlobalCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject, "change Global Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentGlobalCommand");
                            prop.enumValueIndex = (int)newCmd;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        if (aEvent.currentGlobalCommand == MasterAudio.GlobalCommand.None)
                        {
                            DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                        }

                        switch (aEvent.currentGlobalCommand)
                        {
                            case MasterAudio.GlobalCommand.SetMasterMixerVolume:
                                if (showVolumeSlider)
                                {
                                    var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Master Mixer Volume");
                                    if (newFadeVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Master Mixer Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newFadeVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.GlobalCommand.SetMasterPlaylistVolume:
                                if (showVolumeSlider)
                                {
                                    var newFadeVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Master Playlist Volume");
                                    if (newFadeVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Master Playlist Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newFadeVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.UnityMixerControl:
                        var newMix =
                            (MasterAudio.UnityMixerCommand)EditorGUILayout.EnumPopup("Unity Mixer Cmd",
                                aEvent.currentMixerCommand);
                        if (newMix != aEvent.currentMixerCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject,
                                "change Unity Mixer Cmd");
                            SerializedProperty prop = property.FindPropertyRelative("currentMixerCommand");
                            prop.enumValueIndex = (int)newMix;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        EditorGUI.indentLevel = 1;

                        switch (aEvent.currentMixerCommand)
                        {
                            case MasterAudio.UnityMixerCommand.TransitionToSnapshot:
                                var newTime = EditorGUILayout.Slider("Transition Time", aEvent.snapshotTransitionTime,
                                    0, 100);
                                if (newTime != aEvent.snapshotTransitionTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Transition Time");
                                    SerializedProperty prop = property.FindPropertyRelative("snapshotTransitionTime");
                                    prop.floatValue = newTime;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                var newSnap = (AudioMixerSnapshot)EditorGUILayout.ObjectField("Snapshot",
                                    aEvent.snapshotToTransitionTo, typeof(AudioMixerSnapshot), false);
                                if (newSnap != aEvent.snapshotToTransitionTo)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Snapshot");
                                    SerializedProperty prop = property.FindPropertyRelative("snapshotToTransitionTo");
                                    prop.objectReferenceValue = newSnap;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.snapshotToTransitionTo == null)
                                {
                                    DTGUIHelper.ShowRedError("No snapshot selected. No transition will be made.");
                                }

                                break;
                            case MasterAudio.UnityMixerCommand.TransitionToSnapshotBlend:
                                newTime = EditorGUILayout.Slider("Transition Time", aEvent.snapshotTransitionTime, 0,
                                    100);
                                if (newTime != aEvent.snapshotTransitionTime)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "change Transition Time");
                                    SerializedProperty prop = property.FindPropertyRelative("snapshotTransitionTime");
                                    prop.floatValue = newTime;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (aEvent.snapshotsToBlend.Count == 0)
                                {
                                    DTGUIHelper.ShowRedError(
                                        "You have no snapshots to blend. This action will do nothing.");
                                }
                                else
                                {
                                    EditorGUILayout.Separator();
                                }

                                for (var i = 0; i < aEvent.snapshotsToBlend.Count; i++)
                                {
                                    var aSnap = aEvent.snapshotsToBlend[i];
                                    newSnap = (AudioMixerSnapshot)EditorGUILayout.ObjectField("Snapshot #" + (i + 1),
                                        aSnap.snapshot, typeof(AudioMixerSnapshot), false);
                                    if (newSnap != aSnap.snapshot)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Snapshot");
                                        SerializedProperty prop = property.FindPropertyRelative("snapshotsToBlend");
                                        prop.GetArrayElementAtIndex(i).FindPropertyRelative("snapshot")
                                            .objectReferenceValue = newSnap;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    if (aSnap.snapshot == null)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "No snapshot selected. This item will not be used for blending.");
                                        continue;
                                    }

                                    var newWeight = EditorGUILayout.Slider("Weight", aSnap.weight, 0f, 1f);
                                    if (newWeight != aSnap.weight)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Weight");
                                        SerializedProperty prop = property.FindPropertyRelative("snapshotsToBlend");
                                        prop.GetArrayElementAtIndex(i).FindPropertyRelative("weight").floatValue =
                                            newWeight;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }

                                    EditorGUILayout.Separator();
                                }

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(16);
                                GUI.contentColor = DTGUIHelper.BrightButtonColor;
                                if (GUILayout.Button(new GUIContent("Add Snapshot", "Click to add a Snapshot"),
                                        EditorStyles.toolbarButton, GUILayout.Width(85)))
                                {
                                    aEvent.snapshotsToBlend.Add(new FeedbackMAEvent.MA_SnapshotInfo(null, 1f));
                                }

                                if (aEvent.snapshotsToBlend.Count > 0)
                                {
                                    GUILayout.Space(6);
                                    GUI.contentColor = Color.red;
                                    if (DTGUIHelper.AddDeleteIcon("Snapshot", true))
                                    {
                                        aEvent.snapshotsToBlend.RemoveAt(aEvent.snapshotsToBlend.Count - 1);
                                    }
                                }

                                EditorGUILayout.EndHorizontal();
                                GUI.contentColor = Color.white;

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.PersistentSettingsControl:
                        EditorGUI.indentLevel = 1;

                        var newPersistentCmd =
                            (MasterAudio.PersistentSettingsCommand)EditorGUILayout.EnumPopup(
                                "Persistent Settings Command", aEvent.currentPersistentSettingsCommand);
                        if (newPersistentCmd != aEvent.currentPersistentSettingsCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject,
                                "change Persistent Settings Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentPersistentSettingsCommand");
                            prop.enumValueIndex = (int)newPersistentCmd;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        switch (aEvent.currentPersistentSettingsCommand)
                        {
                            case MasterAudio.PersistentSettingsCommand.None:
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                break;
                            case MasterAudio.PersistentSettingsCommand.SetBusVolume:

                                var newAllTypes =
                                    EditorGUILayout.Toggle("Do For Every Bus?", aEvent.allSoundTypesForBusCmd);
                                if (newAllTypes != aEvent.allSoundTypesForBusCmd)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Do For Every Bus?");
                                    SerializedProperty prop = property.FindPropertyRelative("allSoundTypesForBusCmd");
                                    prop.boolValue = newAllTypes;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (!aEvent.allSoundTypesForBusCmd)
                                {
                                    if (_maInScene)
                                    {
                                        var existingIndex = _busNames.IndexOf(aEvent.busName);

                                        int? busIndex = null;

                                        var noBus = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                                _busNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noBus = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.busName == MasterAudio.NoGroupName)
                                        {
                                            busIndex = EditorGUILayout.Popup("Bus Name", existingIndex,
                                                _busNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                            if (newBusName != aEvent.busName)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Bus Name");
                                                SerializedProperty prop = property.FindPropertyRelative("busName");
                                                prop.stringValue = newBusName;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Buses", -1, _busNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                busIndex = newIndex;
                                            }

                                            noMatch = true;
                                        }

                                        if (noBus)
                                        {
                                            DTGUIHelper.ShowRedError("No Bus Name specified. Action will do nothing.");
                                        }
                                        else if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError("Bus Name found no match. Type in or choose one.");
                                        }

                                        if (busIndex.HasValue)
                                        {
                                            if (existingIndex != busIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Bus");
                                            }

                                            SerializedProperty prop = property.FindPropertyRelative("busName");

                                            switch (busIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _busNames[busIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newBusName = EditorGUILayout.TextField("Bus Name", aEvent.busName);
                                        if (newBusName != aEvent.busName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Bus Name");
                                            SerializedProperty prop = property.FindPropertyRelative("busName");
                                            prop.stringValue = newBusName;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }

                                if (showVolumeSlider)
                                {
                                    var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                    if (newVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.PersistentSettingsCommand.SetGroupVolume:
                                var newAllGrps = EditorGUILayout.Toggle("Do For Every Group?",
                                    aEvent.allSoundTypesForGroupCmd);
                                if (newAllGrps != aEvent.allSoundTypesForGroupCmd)
                                {
                                    AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                        property.serializedObject.targetObject,
                                        "toggle Do For Every Group?");
                                    SerializedProperty prop = property.FindPropertyRelative("allSoundTypesForGroupCmd");
                                    prop.boolValue = newAllGrps;
                                    prop.serializedObject.ApplyModifiedProperties();
                                }

                                if (!aEvent.allSoundTypesForGroupCmd)
                                {
                                    if (_maInScene)
                                    {
                                        var existingIndex = _groupNames.IndexOf(aEvent.soundType);

                                        int? groupIndex = null;

                                        var noGroup = false;
                                        var noMatch = false;

                                        if (existingIndex >= 1)
                                        {
                                            groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex,
                                                _groupNames.ToArray());
                                            if (existingIndex == 1)
                                            {
                                                noGroup = true;
                                            }
                                        }
                                        else if (existingIndex == -1 && aEvent.soundType == MasterAudio.NoGroupName)
                                        {
                                            groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex,
                                                _groupNames.ToArray());
                                        }
                                        else
                                        {
                                            // non-match
                                            noMatch = true;

                                            var newSType = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                            if (newSType != aEvent.soundType)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Sound Group");
                                                SerializedProperty prop = property.FindPropertyRelative("soundType");
                                                prop.stringValue = newSType;
                                                prop.serializedObject.ApplyModifiedProperties();
                                            }

                                            var newIndex = EditorGUILayout.Popup("All Sound Groups", -1,
                                                _groupNames.ToArray());
                                            if (newIndex >= 0)
                                            {
                                                groupIndex = newIndex;
                                            }
                                        }

                                        if (noMatch)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "Sound Group found no match. Type in or choose one.");
                                        }
                                        else if (noGroup)
                                        {
                                            DTGUIHelper.ShowRedError(
                                                "No Sound Group specified. Action will do nothing.");
                                        }

                                        if (groupIndex.HasValue)
                                        {
                                            if (existingIndex != groupIndex.Value)
                                            {
                                                AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                    property.serializedObject.targetObject,
                                                    "change Sound Group");
                                            }

                                            SerializedProperty prop = property.FindPropertyRelative("soundType");

                                            switch (groupIndex.Value)
                                            {
                                                case -1:
                                                    prop.stringValue = MasterAudio.NoGroupName;
                                                    break;
                                                default:
                                                    prop.stringValue = _groupNames[groupIndex.Value];
                                                    break;
                                            }

                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                    else
                                    {
                                        var newSoundT = EditorGUILayout.TextField("Sound Group", aEvent.soundType);
                                        if (newSoundT != aEvent.soundType)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Sound Group");
                                            SerializedProperty prop = property.FindPropertyRelative("soundType");
                                            prop.stringValue = newSoundT;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }

                                if (showVolumeSlider)
                                {
                                    var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true);
                                    if (newVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.PersistentSettingsCommand.SetMixerVolume:
                                if (showVolumeSlider)
                                {
                                    var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Mixer Volume");
                                    if (newVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Mixer Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                            case MasterAudio.PersistentSettingsCommand.SetMusicVolume:
                                if (showVolumeSlider)
                                {
                                    var newVol = DTGUIHelper.DisplayVolumeField(aEvent.volume,
                                        DTGUIHelper.VolumeFieldType.None, MasterAudio.MixerWidthMode.Normal, 0f, true,
                                        "Music Volume");
                                    if (newVol != aEvent.volume)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Music Volume");
                                        SerializedProperty prop = property.FindPropertyRelative("volume");
                                        prop.floatValue = newVol;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.ParameterCommandControl:
                        EditorGUI.indentLevel = 1;

                        var newParamCmdCommand =
                            (MasterAudio.ParameterCmdCommand)EditorGUILayout.EnumPopup("Parameter Cmd Command",
                                aEvent.currentParameterCmdCommand);
                        if (newParamCmdCommand != aEvent.currentParameterCmdCommand)
                        {
                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                property.serializedObject.targetObject,
                                "change Parameter Cmd Command");
                            SerializedProperty prop = property.FindPropertyRelative("currentParameterCmdCommand");
                            prop.enumValueIndex = (int)newParamCmdCommand;
                            prop.serializedObject.ApplyModifiedProperties();
                        }

                        switch (aEvent.currentParameterCmdCommand)
                        {
                            case MasterAudio.ParameterCmdCommand.None:
                                DTGUIHelper.ShowRedError("You have no command selected. Action will do nothing.");
                                break;
                            default:
                                if (_maInScene)
                                {
                                    var existingIndex = _parameterCmdNames.IndexOf(aEvent.parameterCommandName);

                                    int? paramCmdIndex = null;

                                    EditorGUI.indentLevel = 1;

                                    var noCmd = false;
                                    var noMatch = false;

                                    if (existingIndex >= 1)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        paramCmdIndex = EditorGUILayout.Popup("Parameter Command", existingIndex,
                                            _parameterCmdNames.ToArray());
                                        if (existingIndex == 1)
                                        {
                                            noCmd = true;
                                        }

                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else if (existingIndex == -1 &&
                                             aEvent.parameterCommandName == MasterAudio.NoGroupName)
                                    {
                                        paramCmdIndex = EditorGUILayout.Popup("Parameter Command", existingIndex,
                                            _parameterCmdNames.ToArray());
                                    }
                                    else
                                    {
                                        // non-match
                                        noMatch = true;
                                        var newCommand = EditorGUILayout.TextField("Parameter Command",
                                            aEvent.parameterCommandName);
                                        if (newCommand != aEvent.parameterCommandName)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Parameter Command");
                                            SerializedProperty prop =
                                                property.FindPropertyRelative("parameterCommandName");
                                            prop.stringValue = newCommand;
                                            prop.serializedObject.ApplyModifiedProperties();
                                        }

                                        var newIndex = EditorGUILayout.Popup("All Parameter Commands", -1,
                                            _parameterCmdNames.ToArray());
                                        if (newIndex >= 0)
                                        {
                                            paramCmdIndex = newIndex;
                                        }
                                    }

                                    if (noCmd)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "No Parameter Command specified. Action will do nothing.");
                                    }
                                    else if (noMatch)
                                    {
                                        DTGUIHelper.ShowRedError(
                                            "Parameter Command found no match. Type in or choose one.");
                                    }

                                    if (paramCmdIndex.HasValue)
                                    {
                                        if (existingIndex != paramCmdIndex.Value)
                                        {
                                            AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                                property.serializedObject.targetObject,
                                                "change Parameter Command");
                                        }

                                        SerializedProperty prop = property.FindPropertyRelative("parameterCommandName");

                                        switch (paramCmdIndex.Value)
                                        {
                                            case -1:
                                                prop.stringValue = MasterAudio.NoGroupName;
                                                break;
                                            default:
                                                prop.stringValue = _parameterCmdNames[paramCmdIndex.Value];
                                                break;
                                        }

                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }
                                else
                                {
                                    var newParamCmd = EditorGUILayout.TextField("Parameter Command",
                                        aEvent.parameterCommandName);
                                    if (newParamCmd != aEvent.parameterCommandName)
                                    {
                                        AudioUndoHelper.RecordObjectPropertyForUndo(ref _isDirty,
                                            property.serializedObject.targetObject,
                                            "change Parameter Command");
                                        SerializedProperty prop = property.FindPropertyRelative("parameterCommandName");
                                        prop.stringValue = newParamCmd;
                                        prop.serializedObject.ApplyModifiedProperties();
                                    }
                                }

                                break;
                        }

                        break;
                    case MasterAudio.EventSoundFunctionType.ParameterControl:
                        DTGUIHelper.ShowRedError(
                            "Select another Action Type.");
                        break;
                }

                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.EndVertical();
            if (GUI.changed || _isDirty)
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
    }
}
