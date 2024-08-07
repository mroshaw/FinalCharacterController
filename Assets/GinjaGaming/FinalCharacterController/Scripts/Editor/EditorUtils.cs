using UnityEngine;

namespace GinjaGaming.Editor
{
    public static class EditorUtils
    {
        #region Class Methods

        public static void MoveComponentToTop(GameObject targetGameObject, Component component)
        {
            for(int currComponent = 1; currComponent < targetGameObject.GetComponents<Component>().Length; currComponent++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            }
        }
        #endregion
    }
}