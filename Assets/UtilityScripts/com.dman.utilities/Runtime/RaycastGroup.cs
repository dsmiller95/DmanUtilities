using UnityEngine;

namespace Dman.Utilities
{
    public class RaycastGroup : ScriptableObject
    {
        public LayerMask layersToRaycastTo;
        public bool uiBlocksRay = true;
        
        private RaycastHit? _currentHit;
        private int lastFrameRaycasted;


        /// <summary>
        /// This value is cached on a per-frame basis. safe to access multiple times per frame and won't create extra raycasts
        /// </summary>
        public RaycastHit? CurrentlyHitObject
        {
            get
            {
                if(Time.frameCount > lastFrameRaycasted)
                {
                    this.CheckForRaycastHit();
                    lastFrameRaycasted = Time.frameCount;
                }
                return _currentHit;
            }
        }

        private void CheckForRaycastHit()
        {
            if (MouseOverHelpers.RaycastToObject(layersToRaycastTo, out var singleHit, uiBlocksRay))
            {
                _currentHit = singleHit;
            }
            else
            {
                _currentHit = null;
            }
        }
    }
}
