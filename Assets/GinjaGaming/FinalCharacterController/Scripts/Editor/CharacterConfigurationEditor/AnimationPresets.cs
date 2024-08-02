using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CreateAssetMenu(fileName = "AnimationPreset", menuName = "Final Character Controller/Animation Presets", order = 1)]
    public class AnimationPresets : ScriptableObject
    {
        [Header("Animation Targets")] public AnimationMappings animMappings;
        [Header("Animation Mapping")] public AnimPreset[] animPresets;

        public void UpdateMappings()
        {
            List<AnimPreset> tempAnimMappingList = animPresets.ToList();

            foreach (AnimationMappings.AnimMapping currAnimTarget in animMappings.animMappings)
            {
                Debug.Log($"Looking for: {currAnimTarget.animationHeader}/{currAnimTarget.animationName}");
                if (!DoesTargetExist(currAnimTarget))
                {
                    tempAnimMappingList.Add(new AnimPreset(currAnimTarget));
                }
            }

            animPresets = tempAnimMappingList.ToArray();
        }

        private bool DoesTargetExist(AnimationMappings.AnimMapping animMapping)
        {
            foreach (AnimPreset currAnimMapping in animPresets)
            {
                if (currAnimMapping.AnimMapping == animMapping)
                {
                    return true;
                }
            }
            return false;
        }

        public void InitSettings()
        {
            if (!animMappings || animMappings.animMappings.Length == 0)
            {
                return;
            }

            int numMappings = animMappings.animMappings.Length;
            animPresets = new AnimPreset[numMappings];

            for (int currMappingIndex = 0; currMappingIndex < numMappings; currMappingIndex++)
            {
                animPresets[currMappingIndex] = new AnimPreset(animMappings.animMappings[currMappingIndex]);
            }
        }

        public void UpdateAllAnims(AnimatorController animatorController)
        {
            foreach (AnimPreset currAnimMapping in animPresets)
            {
                currAnimMapping.UpdateInController(animatorController);
            }
        }

        public bool Validate(out List<string> validationErrors)
        {
            bool validationResult = true;

            validationErrors = new();
            foreach (AnimPreset currAnimPreset in animPresets)
            {
                if (!currAnimPreset.animClip)
                {
                    validationErrors.Add($"Animation clip is missing: {currAnimPreset.AnimMapping.AnimLabel}");
                    validationResult = false;
                }

                if (currAnimPreset.animSpeed == 0)
                {
                    validationErrors.Add($"Animation has 0 speed: {currAnimPreset.AnimMapping.AnimLabel}");
                    validationResult = false;
                }
            }
            return validationResult;

        }

        [Serializable]
        public class AnimPreset
        {
            public AnimationClip animClip;
            public bool mirrorAnimation;
            public float animSpeed;
            [SerializeField] private AnimationMappings.AnimMapping animMapping;

            public AnimationMappings.AnimMapping AnimMapping => animMapping;

            public AnimPreset(AnimationMappings.AnimMapping mapping, AnimationClip clip)
            {
                animClip = clip;
                animMapping = mapping;
                mirrorAnimation = false;
                animSpeed = 1.0f;
            }

            public AnimPreset(AnimationMappings.AnimMapping mapping)
            {
                animClip = null;
                animMapping = mapping;
                mirrorAnimation = false;
                animSpeed = 1.0f;
            }
            public void UpdateInController(AnimatorController animatorController)
            {
                if (animMapping == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: AnimTarget is null!");
                    return;
                }

                if (animClip == null)
                {
                    Debug.LogError($"CharacterAnimationSettings: AnimClip is empty on: {animMapping.AnimLabel}");
                    return;
                }
                animMapping.ApplyAnimSettings(animatorController, animClip, mirrorAnimation, animSpeed);
            }
        }
    }
}