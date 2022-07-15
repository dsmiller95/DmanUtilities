using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dman.Utilities
{
    public static class MouseOverHelpers
    {
        private static Func<Vector2> MousePosGetter;
        public static void ConfigureMouseHelper(Func<Vector2> mousePositionPixelCoordGetter = null)
        {
            if (mousePositionPixelCoordGetter == null)
            {
                mousePositionPixelCoordGetter = () => new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }

            MousePosGetter = mousePositionPixelCoordGetter;
        }

        /// <summary>
        /// return current mouse position. useful for 2D games.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetMousePos2D()
        {
            var ray = Camera.main.ScreenPointToRay(GetMousePos());
            var plane = new Plane(new Vector3(0, 0, 1), 0);

            if (plane.Raycast(ray, out var enter))
            {
                return ray.GetPoint(enter);
            }
            return default;
        }

        public static Ray GetRay()
        {
            return Camera.main.ScreenPointToRay(GetMousePos());
        }

        private static Vector2 GetMousePos()
        {
            if (MousePosGetter == null)
            {
                Debug.LogError("The mouse position helper must know where the mouse is");
                throw new NullReferenceException("Mouse over helper has not been configured");
            }
            return MousePosGetter();
        }

        /// <summary>
        /// Raycasts from the current mouse position to a game object.
        ///     over the UI
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="hit"></param>
        /// <param name="failOnUI">When true, the raycast hit will fail if it hits a UI element</param>
        /// <returns></returns>
        public static bool RaycastToObject(LayerMask mask, out RaycastHit hit, bool failOnUI = true)
        {
            hit = default;
            if (failOnUI && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }
            if (Camera.main == null)
            {
                Debug.LogWarning("No active camera");
                return false;
            }
            var ray = GetRay();
            if (Physics.Raycast(ray, out var innerHit, 100, mask))
            {
                hit = innerHit;
                return true;
            }
            return false;
        }

        public static RaycastHit[] RaycastAllToObject(LayerMask mask)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }
            var ray = Camera.main.ScreenPointToRay(GetMousePos());
            return Physics.RaycastAll(ray, 100, mask);
        }
    }
}
