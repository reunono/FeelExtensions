using System;
using System.Collections;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;

/// <summary>
/// This feedback will let you pilot a DOTweenPath
/// </summary>
[AddComponentMenu("")]
[FeedbackHelp("This feedback will let you pilot a DOTweenPath")]
[FeedbackPath("DOTween/DOTween Path")]
public class DOTweenPathMMFeedback : MMFeedback
{
    public enum Modes { DOPlay, DOPlayBackwards, DOPlayForward, DOPause, DOTogglePause, DORewind, DORestart, DOComplete, DOKill }

    [Header("DOTWeen Path")]

    public DOTweenPath TargetDOTweenPath;

    public Modes Mode = Modes.DOPlay;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        switch (Mode)
        {
            case Modes.DOPlay:
                TargetDOTweenPath.DOPlay();
                break;
            case Modes.DOPlayBackwards:
                TargetDOTweenPath.DOPlayBackwards();
                break;
            case Modes.DOPlayForward:
                TargetDOTweenPath.DOPlayForward();
                break;
            case Modes.DOPause:
                TargetDOTweenPath.DOPause();
                break;
            case Modes.DOTogglePause:
                TargetDOTweenPath.DOTogglePause();
                break;
            case Modes.DORewind:
                TargetDOTweenPath.DORewind();
                break;
            case Modes.DORestart:
                TargetDOTweenPath.DORestart();
                break;
            case Modes.DOComplete:
                TargetDOTweenPath.DOComplete();
                break;
            case Modes.DOKill:
                TargetDOTweenPath.DOKill();
                break;
        }

    }
}
