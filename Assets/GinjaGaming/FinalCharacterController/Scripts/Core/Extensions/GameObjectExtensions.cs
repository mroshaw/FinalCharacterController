using UnityEngine;

namespace GinjaGaming.FinalCharacterController.Core.Extensions
{
    public static class GameObjectExtensions
    {
        #region Class Methods
        /// <summary>
        /// Wraps up Get and Add to either return a component if it exists, otherwise create and return a new instance
        /// </summary>
        private static Component EnsureComponent(this GameObject gameObject, System.Type componentType)
        {
            var component = gameObject.GetComponent(componentType);
            if (component)
            {
                return component;
            }
            component = gameObject.AddComponent(componentType);
            return component;
        }

        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component => EnsureComponent(gameObject, typeof (T)) as T;

        public static T EnsureComponent<T>(this Component existingComponent) where T : Component => EnsureComponent(existingComponent.gameObject, typeof (T)) as T;

        #endregion
    }
}