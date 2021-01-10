using UnityEngine;
using UnityEngine.EventSystems;

namespace Dman.Utilities
{
    public static class MouseOverHelpers
    {
        /// <summary>
        /// return current mouse position. useful for 2D games.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetMousePos2D()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(new Vector3(0, 0, 1), 0);

            if (plane.Raycast(ray, out var enter))
            {
                return ray.GetPoint(enter);
            }
            return default;
        }

        public static bool RaycastToObject(LayerMask mask, out RaycastHit hit)
        {
            hit = default;
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var innerHit, 100, mask))
                {
                    hit = innerHit;
                    return true;
                }
            }
            return false;
        }

        public static RaycastHit[] RaycastAllToObject(LayerMask mask)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.RaycastAll(ray, 100, mask);
        }
    }
}
