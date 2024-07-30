using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CreateAssetMenu(fileName = "Data", menuName = "Final Character Controller/AnimationSettings", order = 1)]
    public class CharacterAnimationSettings : ScriptableObject
    {
        [Header("Animation Targets")] public CharacterAnimationMappingSettings mappingSettings;
        [Header("Animation Mapping")] public AnimMapping[] animMappings;

        public void UpdateMappings()
        {
            List<AnimMapping> tempAnimMappingList = animMappings.ToList();

            foreach (CharacterAnimationMappingSettings.AnimTarget currAnimTarget in mappingSettings.animTargets)
            {
                Debug.Log($"Looking for: {currAnimTarget.animationHeader}/{currAnimTarget.animationName}");
                if (!DoesTargetExist(currAnimTarget))
                {
                    tempAnimMappingList.Add(new AnimMapping(currAnimTarget));
                }
            }

            animMappings = tempAnimMappingList.ToArray();
        }

        private bool DoesTargetExist(CharacterAnimationMappingSettings.AnimTarget animTarget)
        {
            foreach (AnimMapping currAnimMapping in animMappings)
            {
                if (currAnimMapping.animMappingLabel == animTarget.AnimLabel)
                {
                    return true;
                }
            }
            return false;
        }

        public void InitSettings()
        {
            if (!mappingSettings || mappingSettings.animTargets.Length == 0)
            {
                return;
            }

            int numMappings = mappingSettings.animTargets.Length;
            animMappings = new AnimMapping[numMappings];

            for (int currMappingIndex = 0; currMappingIndex < numMappings; currMappingIndex++)
            {
                animMappings[currMappingIndex] = new AnimMapping(mappingSettings.animTargets[currMappingIndex]);
            }
        }

        public void UpdateAllAnims(AnimatorController animatorController)
        {
            foreach (AnimMapping currAnimMapping in animMappings)
            {
                currAnimMapping.UpdateInController(animatorController);
            }
        }

        [Serializable]
        public class AnimMapping
        {
            [Header("Animation")]
            public string animMappingLabel;
            public AnimationClip animClip;
            [SerializeField] private CharacterAnimationMappingSettings.AnimTarget animTarget;

            public AnimMapping(CharacterAnimationMappingSettings.AnimTarget target, AnimationClip clip)
            {
                animMappingLabel = $"{target.animationHeader}/{target.animationName}";
                animClip = clip;
                animTarget = target;
            }

            public AnimMapping(CharacterAnimationMappingSettings.AnimTarget target)
            {
                animMappingLabel = $"{target.animationHeader}/{target.animationName}";
                animClip = null;
                animTarget = target;
            }
            public void UpdateInController(AnimatorController animatorController)
            {
                if (animTarget == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: AnimTarget is null!");
                    return;
                }

                if (animClip == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: AnimClip is empty on: {animTarget.AnimLabel}");
                    return;
                }

                if (string.IsNullOrEmpty(animTarget.layerName))
                {
                    Debug.LogError($"CharacterAnimationSettings: LayerName is empty on: {animTarget.AnimLabel}");
                    return;
                }

                AnimatorControllerLayer layer = GetLayerByName(animatorController, animTarget.layerName);
                if (layer == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: Layer not found! {animTarget.layerName}!");
                    return;
                }

                AnimatorStateMachine stateMachine;

                if (string.IsNullOrEmpty(animTarget.stateMachineName))
                {
                    stateMachine = layer.stateMachine;
                }
                else
                {
                    stateMachine = GetStateMachineInLayerByName(layer, animTarget.stateMachineName);
                }

                AnimatorState animatorState = GetStateFromStateMachine(stateMachine, animTarget.stateName);

                if (string.IsNullOrEmpty(animTarget.blendTreeName))
                {
                    SetMotionInState(animatorState, animClip);
                    return;
                }

                BlendTree blendTree = GetBlendTreeFromBlendTreeByName(animatorState.motion as BlendTree, animTarget.blendTreeName);
                SetMotionInBlendTree(blendTree, animTarget.blendTreeIndex, animClip);
            }

            #region Animation controller helper methods

            private BlendTree GetBlendTreeFromBlendTreeByName(BlendTree blendTree, string blendTreeName)
            {
                BlendTree blendTreeResult = GetBlendTreeFromBlendTreeByNameRecursive(blendTree, blendTreeName);
                if (blendTreeResult == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: Cannot find BlendTree: {blendTreeName} in BlendTree: {blendTree.name}");
                }

                return blendTreeResult;
            }

            private BlendTree GetBlendTreeFromBlendTreeByNameRecursive(BlendTree blendTree, string blendTreeName)
            {
                // Check if the current blend tree's name matches the desired name
                if (blendTree.name == blendTreeName)
                {
                    return blendTree;
                }

                // Iterate through the child motions
                foreach (var childMotion in blendTree.children)
                {
                    // Check if the child motion is a BlendTree
                    if (childMotion.motion is BlendTree childBlendTree)
                    {
                        // Recursively search in the child BlendTree
                        BlendTree foundBlendTree = GetBlendTreeFromBlendTreeByNameRecursive(childBlendTree, blendTreeName);
                        if (foundBlendTree != null)
                        {
                            return foundBlendTree;
                        }
                    }
                }

                return null;
            }
            private AnimatorStateMachine GetStateMachineInLayerByName(AnimatorControllerLayer layer,
                string stateMachineName)
            {
                AnimatorStateMachine stateMachine = GetStateMachineInLayerByNameRecursive(layer.stateMachine, stateMachineName);
                if (stateMachine == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: Cannot find StateMachine: {stateMachineName} in Layer: {layer.name}");
                }
                return stateMachine;
            }

            private AnimatorStateMachine GetStateMachineInLayerByNameRecursive(AnimatorStateMachine stateMachine,
                string stateMachineName)
            {
                if (stateMachine.name == stateMachineName)
                {
                    return stateMachine;
                }

                // Search in child state machines
                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    AnimatorStateMachine stateMachineFound = GetStateMachineInLayerByNameRecursive(childStateMachine.stateMachine, stateMachineName);
                    if (stateMachineFound != null)
                    {
                        return stateMachineFound;
                    }
                }
                return null;
            }

            private AnimatorControllerLayer GetLayerByName(AnimatorController animatorController, string layerName)
            {
                foreach (AnimatorControllerLayer currLayer in animatorController.layers)
                {
                    if (currLayer.name == layerName)
                    {
                        return currLayer;
                    }
                }
                Debug.LogError($"CharacterAnimationSettings: Cannot find Layer: {layerName} in AnimationController: {animatorController.name}");
                return null;
            }

            private AnimatorState GetStateFromStateMachine(AnimatorStateMachine stateMachine, string stateName)
            {
                AnimatorState animatorState = GetStateFromStateMachineRecursive(stateMachine, stateName);
                if (animatorState == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: Cannot find State: {stateName} in StateMachine: {stateMachine.name}");
                }

                return animatorState;
            }

            private AnimatorState GetStateFromStateMachineRecursive(AnimatorStateMachine stateMachine, string stateName)
            {
                foreach (ChildAnimatorState currState in stateMachine.states)
                {
                    if (currState.state.name == stateName)
                    {
                        return currState.state;
                    }
                }

                // Recursively search in child state machines
                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    AnimatorState foundState = GetStateFromStateMachineRecursive(childStateMachine.stateMachine, stateName);
                    if (foundState != null)
                    {
                        return foundState;
                    }
                }

                return null;
            }

            private void SetMotionInBlendTree(BlendTree blendTree, int motionIndex, AnimationClip animClip)
            {
                ChildMotion[] newMotions = blendTree.children;
                newMotions[motionIndex].motion = animClip;
                blendTree.children = newMotions;
            }

            private void SetMotionInState(AnimatorState state, AnimationClip animClip)
            {
                if (state.motion is BlendTree)
                {

                }
                else
                {
                    state.motion = animClip;
                }
            }

            private void SaveAnimatorAsset(AnimatorController animatorController)
            {
                EditorUtility.SetDirty(animatorController);
                AssetDatabase.SaveAssets();
            }
            #endregion
        }
    }
}