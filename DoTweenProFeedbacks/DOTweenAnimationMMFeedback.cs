using System;
using System.Collections;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using DG;
using DG.Tweening;

/// <summary>
/// This feedback will let you pilot a DoTweenAnimation
/// </summary>
[AddComponentMenu("")]
[FeedbackHelp("This feedback will let you pilot a DOTweenAnimation")]
[FeedbackPath("DOTween/DOTween Animation")]
public class DOTweenAnimationMMFeedback : MMFeedback
{
    public enum Modes { DOPlay, DOPlayBackwards, DOPlayForward, DOPause, DOTogglePause, DORewind, DORestart, DOComplete, DOKill }
    
    [Header("DOTWeen Animation")]
    
    public DOTweenAnimation TargetDOTweenAnimation;

    public Modes Mode = Modes.DOPlay;
    
    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
    {
        switch (Mode)
        {
            case Modes.DOPlay:
                TargetDOTweenAnimation.DOPlay();
                break;
            case Modes.DOPlayBackwards:
                TargetDOTweenAnimation.DOPlayBackwards();
                break;
            case Modes.DOPlayForward:
                TargetDOTweenAnimation.DOPlayForward();
                break;
            case Modes.DOPause:
                TargetDOTweenAnimation.DOPause();
                break;
            case Modes.DOTogglePause:
                TargetDOTweenAnimation.DOTogglePause();
                break;
            case Modes.DORewind:
                TargetDOTweenAnimation.DORewind();
                break;
            case Modes.DORestart:
                TargetDOTweenAnimation.DORestart();
                break;
            case Modes.DOComplete:
                TargetDOTweenAnimation.DOComplete();
                break;
            case Modes.DOKill:
                TargetDOTweenAnimation.DOKill();
                break;
        }

    }
}
