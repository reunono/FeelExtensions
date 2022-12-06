using MoreMountains.Feedbacks;
using UnityEngine;

[FeedbackPath("Random/--- Random choice BLOCK BEGIN ---")]
[FeedbackHelp("Do not put it right after \"Random choice BEGIN\" unless you need empty block, since it's considered as beginning of block as well. If 2 block beginnigs put one after another, it will create empty block, which will have chance to be chosen - so, no feedbacks option.")]
public class MMF_RandomChoiceBlockBeginFeedback : MMF_Feedback
{
    [MMFInspectorGroup("Probability weight", true)]
    [Tooltip("If weight is higher, compared to other weights, then probability to choose this block is higher. If block B1 has weight 1 and B2 has weight 3, then B1 has 1/(1+3)=25% chance and B2 has 3/(1+3)=75% chance to be chosen")]
    public float ProbabilityWeight = 1;

    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
    {
    }
}
