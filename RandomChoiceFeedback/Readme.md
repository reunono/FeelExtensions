# RandomChoiceFeedback
Allows you to add special feedbacks to randomly control other feedback's active state by choosing only 1 active from group or by activating single group of feedbacks out of multiple groups.
Random choice happens when feedback **Random choice BEGIN** starts playing. If put in a loop, it will perform random choice each loop.

Feedbacks are supposed to be used with **MMF Player** component and located in group *Random*, when adding new feedback.

## Feedbacks
### Random choice BEGIN
This is main feedback which controls all feedbacks after it until first occurrence of **Random choice END** feedback or until the end.
**Random choice BEGIN** feedback also acts as beginning of a group - similar to **Random choice BLOCK BEGIN** and also has field *ProbabilityWeight*.

### Random choice BLOCK BEGIN
Optional feedback.
If not present, then random choice will work in single feedback mode. If present, random choice will work in single group mode.
This feedback is supposed to be used between **Random choice BEGIN** and **Random choice END** feedbacks. It marks beginning of feedbacks group. All feedbacks until first occurrence of another **Random choice BLOCK BEGIN** feedback or  **Random choice END** will be considered as a part of this group.

### Random choice END
Optional feedback,
If not present, then end for feedbacks range will be the last feedback in the list.

## Modes
### Single chosen feedback
If you have only **Random choice BEGIN** and **Random choice END** (optional) blocks, then this mode will be enabled. In this mode, when **Random choice BEGIN** is played, all the feedbacks until **Random choice END** are disabled, except single one which is chosen by random. Each feedback has equal probability to be chosen.
### Single chosen feedback group
If you have at least one **Random choice BLOCK BEGIN** feedback, then this mode will be used. In this mode, when **Random choice BEGIN** is played, all the feedbacks until **Random choice END** are disabled, except of single group of feedbacks, chosen by random and considering *ProbabilityWeight* of the group.
Each group has *ProbabilityWeight* parameter with default value of 1.
For example:
- Block 1: `ProbabilityWeight=1`, chance to be chosen will be `1/(1+2+7) = 1/10 = 10%`
- Block 2: `ProbabilityWeight=2`, chance to be chosen will be `2/(1+2+7) = 2/10 = 20%`
- Block 3: `ProbabilityWeight=7`, chance to be chosen will be `7/(1+2+7) = 7/10 = 70%`

When values of `ProbabilityWeight` are the same for all groups, then all groups will have same chance to be chosen.

If feedback **Random choice BLOCK BEGIN** has another feedback **Random choice BLOCK BEGIN** or **Random choice END** right in front of it, then it will be still valid for random choice, but group will be empty. If empty group is chosen, then no feedbacks are enabled.

## Notes
- Initial active/disabled state of feedbacks is not considered.
- When feedbacks activated/disabled, they are not returning back to initial active/disabled state.
- If you enable **Keep Playmode Changes** while using random choice feedbacks, it will overwrite serialized values for `feedback.Active` field.