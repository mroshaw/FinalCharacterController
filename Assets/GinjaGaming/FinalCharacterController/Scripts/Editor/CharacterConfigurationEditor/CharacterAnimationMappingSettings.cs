using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GinjaGaming.FinalCharacterController.Editor.CharacterConfigurationEditor
{
    [CreateAssetMenu(fileName = "Data", menuName = "Final Character Controller/AnimationMappingSettings", order = 1)]
    public class CharacterAnimationMappingSettings : ScriptableObject
    {
        [Header("Animator Settings")] public AnimatorController referenceController;
        [Header("Animation Targets")] public AnimTarget[] animTargets;

        [Serializable]
        public class AnimTarget
        {
            [Header("Animation")]
            public string animationHeader;
            public string animationName;
            public string stateName;
            public string stateMachineName;
            public string layerName;

            [Header("Blend Tree")]
            public string blendTreeName;
            public int blendTreeIndex;

            public string AnimLabel => $"{animationHeader}/{animationName}";
        }

        public AnimatorController DuplicateController(string targetFileName)
        {
            if (!referenceController)
            {
                return null;
            }

            string sourcePath = AssetDatabase.GetAssetPath(referenceController as Object);
            string sourcePathFolder = Path.GetDirectoryName(sourcePath);
            string targetPath = $"{sourcePathFolder}/{targetFileName}.asset";

            if (!File.Exists(targetPath))
            {
                AssetDatabase.CopyAsset(sourcePath, targetPath);
            }
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(targetPath);
        }
    }
}