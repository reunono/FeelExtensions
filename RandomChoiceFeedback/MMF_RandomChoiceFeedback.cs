using MoreMountains.Feedbacks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[FeedbackPath("Random/--- Random choice BEGIN ---")]
[FeedbackHelp("When this feedback starts to play, it will disable all the following feedbacks except randomly chosen one up to first \"Random choice END\" feedback or last feedback in list.\r\n" +
    "Ignores active/disabled state amd can mess up active/disable state of other feedbacks if \"Keep Playmode Changes\" enabled.\r\n" +
    "If \"Random choice BLOCK BEGIN\" blocks are present in between of start and end, then all feedbacks in only one randomly chosen block will be enabled." +
    "\"Random choice BEGIN\" and \"Random choice BLOCK BEGIN\" are starting point of block and contain field ProbabilityWeight which affects probability of block to be chosen. Higher value compared to others means higher probability to be chosen")]
public class MMF_RandomChoiceFeedback : MMF_RandomChoiceBlockBeginFeedback
{
    protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
    {
        // find self index and index of first END block after self
        var selfIdx = Owner.FeedbacksList.IndexOf(this);
        var choiceEndIdx = Owner.FeedbacksList.FindIndex(selfIdx, x => x is MMF_RandomChoiceEndFeedback);
        // if there is no END block, then we consider as END block index after the last one in FeedbacksList
        choiceEndIdx = choiceEndIdx < 0 ? Owner.FeedbacksList.Count : choiceEndIdx;

        // find indexes of separators between selfIdx and choiceEndIdx, including selfIdx and choiceEndIdx themselves
        var separatorIdxs = new List<int>();
        for(var i = selfIdx; i < choiceEndIdx; i++)
        {
            if (Owner.FeedbacksList[i] is MMF_RandomChoiceBlockBeginFeedback)
                separatorIdxs.Add(i);
        }
        separatorIdxs.Add(choiceEndIdx);

        // if there are just 2 separators - means only BEGIN and END, then do not use blocks and choose one random feedback to be enabled
        if (separatorIdxs.Count == 2)
        {
            for (var i = selfIdx + 1; i < choiceEndIdx; i++)
                Owner.FeedbacksList[i].Active = false;

            if (selfIdx + 1 != choiceEndIdx)
                Owner.FeedbacksList[Random.Range(selfIdx + 1, choiceEndIdx)].Active = true;
        }
        else
        {
            // get feedbacks, grouped by blocks. Disable all of them as well. Also collect values of ProbabilityWeight for blocks
            var blocksCount = separatorIdxs.Count - 1;
            var feedbacksInBlocks = new List<MMF_Feedback>[blocksCount];
            var blockProbabilityWeights = new float[blocksCount];
            for (var block = 0; block < blocksCount; block++)
            {
                blockProbabilityWeights[block] = (Owner.FeedbacksList[separatorIdxs[block]] as MMF_RandomChoiceBlockBeginFeedback).ProbabilityWeight;

                feedbacksInBlocks[block] = new List<MMF_Feedback>();
                for (var i = separatorIdxs[block] + 1; i < separatorIdxs[block + 1]; i++)
                {
                    feedbacksInBlocks[block].Add(Owner.FeedbacksList[i]);
                    Owner.FeedbacksList[i].Active = false;
                }
            }

            // choose block based on probability weights and activate all feedbacks inside
            var totalProbabilityWeight = Mathf.Max(blockProbabilityWeights.Sum(), 1e-10f);
            var randomChoice = Random.value;
            var accumulatedWeight = 0f;
            for (var i = 0; i < blocksCount; i++)
            {
                accumulatedWeight += blockProbabilityWeights[i] / totalProbabilityWeight;
                if(randomChoice < accumulatedWeight)
                {
                    foreach (var feedback in feedbacksInBlocks[i])
                        feedback.Active = true;
                    break;
                }
            }
        }
    }
}
