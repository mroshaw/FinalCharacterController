using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CreateAssetMenu(fileName = "AnimationMapping", menuName = "Final Character Controller/Animation Mapping", order = 1)]
    public class AnimationMappings : ScriptableObject
    {
        [Header("Animator Settings")] public AnimatorController referenceController;
        [Header("Animation Targets")] public AnimMapping[] animMappings;

        #region Public class methods
        public bool Validate(out List<string> validationErrors)
        {
            bool validationResult = true;

            validationErrors = new();
            foreach (AnimMapping currAnimMapping in animMappings)
            {
                if (string.IsNullOrEmpty(currAnimMapping.layerName))
                {
                    validationErrors.Add($"Animation Map is missing layer name: {currAnimMapping.AnimLabel}");
                    validationResult = false;
                }
                currAnimMapping.ApplyAnimSettings(referenceController, null, false, 0.0f, false);
            }

            return validationResult;
        }

        public AnimatorController DuplicateController(string targetFileName)
        {
            if (!referenceController)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(referenceController);
            string sourcePathFolder = Path.GetDirectoryName(sourcePath);
            string targetPath = $"{sourcePathFolder}/{targetFileName}.asset";

            if (!File.Exists(targetPath))
            {
                AssetDatabase.CopyAsset(sourcePath, targetPath);
            }
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(targetPath);
        }
        #endregion

        [Serializable]
        public class AnimMapping
        {
            public string animationHeader;
            public string animationName;
            public string stateName;
            public string stateMachineName;
            public string layerName;

            public string blendTreeName;
            public int blendTreeIndex;

            public string AnimLabel => $"{animationHeader}/{animationName}";

            public override bool Equals(object obj) => this.Equals(obj as AnimMapping);

            #region Equality overrides
            public bool Equals(AnimMapping animMapping)
            {
                if (animMapping is null)
                {
                    return false;
                }

                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, animMapping))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != animMapping.GetType())
                {
                    return false;
                }

                // Return true if the fields match.
                // Note that the base class is not invoked because it is
                // System.Object, which defines Equals as reference equality.
                return (animationHeader == animMapping.animationHeader) && (animationName == animMapping.animationName);
            }

            public override int GetHashCode() => (animationHeader, animationName).GetHashCode();

            public static bool operator ==(AnimMapping lhs, AnimMapping rhs)
            {
                if (lhs is null)
                {
                    if (rhs is null)
                    {
                        return true;
                    }

                    // Only the left side is null.
                    return false;
                }

                // Equals handles case of null on right side.
                return lhs.Equals(rhs);
            }

            public static bool operator !=(AnimMapping lhs, AnimMapping rhs) => !(lhs == rhs);
            #endregion
            #region Public class methods
            public void ApplyAnimSettings(AnimatorController animatorController,
                AnimationClip animClip, bool mirrorAnim, float animSpeed, bool applyChanges)
            {
                AnimatorState animationState = GetStateInController(animatorController);

                if (!string.IsNullOrEmpty(blendTreeName))
                {
                    BlendTree blendTree = GetBlendTreeInState(animationState);

                    ChildMotion[] newMotions = blendTree.children;
                    if (blendTreeIndex >= newMotions.Length)
                    {
                        Debug.LogError($"Blend Tree index is out of range! {AnimLabel}");
                    }

                    if (applyChanges)
                    {

                        newMotions[blendTreeIndex].motion = animClip;
                        newMotions[blendTreeIndex].mirror = mirrorAnim;
                        if (animSpeed > 0)
                        {
                            newMotions[blendTreeIndex].timeScale = animSpeed;
                        }
                        blendTree.children = newMotions;
                    }

                }
                else
                {
                    if (applyChanges)
                    {
                        animationState.mirror = mirrorAnim;
                        animationState.speed = animSpeed;
                        animationState.motion = animClip;
                    }
                }
            }
            #endregion
            #region Animation controller helper methods
            private AnimatorState GetStateInController(AnimatorController animatorController)
            {
                AnimatorControllerLayer layer = GetLayerByName(animatorController, layerName);
                var stateMachine = string.IsNullOrEmpty(stateMachineName) ? layer.stateMachine : GetStateMachineInLayerByName(layer, stateMachineName);
                return GetStateFromStateMachine(stateMachine, stateName);
            }

            private BlendTree GetBlendTreeInState(AnimatorState animatorState)
            {
                return GetBlendTreeFromBlendTreeByName(animatorState.motion as BlendTree, blendTreeName);
            }

            private BlendTree GetBlendTreeFromBlendTreeByName(BlendTree blendTree, string blendTreeToFindName)
            {
                BlendTree blendTreeResult = GetBlendTreeFromBlendTreeByNameRecursive(blendTree, blendTreeToFindName);
                if (blendTreeResult == null)
                {
                    Debug.LogError(
                        $"CharacterAnimationSettings: Cannot find BlendTree: {blendTreeToFindName} in BlendTree: {blendTree.name}");
                }

                return blendTreeResult;
            }

            private BlendTree GetBlendTreeFromBlendTreeByNameRecursive(BlendTree blendTree, string blendTreeToFindName)
            {
                // Check if the current blend tree's name matches the desired name
                if (blendTree.name == blendTreeToFindName)
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
                        BlendTree foundBlendTree =
                            GetBlendTreeFromBlendTreeByNameRecursive(childBlendTree, blendTreeToFindName);
                        if (foundBlendTree != null)
                        {
                            return foundBlendTree;
                        }
                    }
                }

                return null;
            }

            private AnimatorStateMachine GetStateMachineInLayerByName(AnimatorControllerLayer layer,
                string stateMachineToFindName)
            {
                AnimatorStateMachine stateMachine =
                    GetStateMachineInLayerByNameRecursive(layer.stateMachine, stateMachineToFindName);
                if (stateMachine == null)
                {
                    Debug.LogError(
                        $"CharacterAnimationSettings: Cannot find StateMachine: {stateMachineToFindName} in Layer: {layer.name}");
                }

                return stateMachine;
            }

            private AnimatorStateMachine GetStateMachineInLayerByNameRecursive(AnimatorStateMachine stateMachine,
                string stateMachineToFindName)
            {
                if (stateMachine.name == stateMachineToFindName)
                {
                    return stateMachine;
                }

                // Search in child state machines
                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    AnimatorStateMachine stateMachineFound =
                        GetStateMachineInLayerByNameRecursive(childStateMachine.stateMachine, stateMachineToFindName);
                    if (stateMachineFound != null)
                    {
                        return stateMachineFound;
                    }
                }

                return null;
            }

            private AnimatorControllerLayer GetLayerByName(AnimatorController animatorController, string layerToFindName)
            {
                foreach (AnimatorControllerLayer currLayer in animatorController.layers)
                {
                    if (currLayer.name == layerToFindName)
                    {
                        return currLayer;
                    }
                }

                Debug.LogError(
                    $"CharacterAnimationSettings: Cannot find Layer: {layerName} in AnimationController: {animatorController.name}");
                return null;
            }

            private AnimatorState GetStateFromStateMachine(AnimatorStateMachine stateMachine, string stateNameToFind)
            {
                AnimatorState animatorState = GetStateFromStateMachineRecursive(stateMachine, stateNameToFind);
                if (animatorState == null)
                {
                    Debug.LogError(
                        $"CharacterAnimationSettings: Cannot find State: {stateNameToFind} in StateMachine: {stateMachine.name}");
                }

                return animatorState;
            }

            private AnimatorState GetStateFromStateMachineRecursive(AnimatorStateMachine stateMachine, string stateNameToFind)
            {
                foreach (ChildAnimatorState currState in stateMachine.states)
                {
                    if (currState.state.name == stateNameToFind)
                    {
                        return currState.state;
                    }
                }

                // Recursively search in child state machines
                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    AnimatorState foundState =
                        GetStateFromStateMachineRecursive(childStateMachine.stateMachine, stateNameToFind);
                    if (foundState != null)
                    {
                        return foundState;
                    }
                }

                return null;
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