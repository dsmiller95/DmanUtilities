﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace Dman.Utilities
{
    [CreateAssetMenu(fileName = "RaycastGroup", menuName = "DmanUtilities/RaycastGroup", order = 10)]
    public class RaycastGroup : ScriptableObject
    {
        public LayerMask layersToRaycastTo;
        public bool uiBlocksRay = true;
        
        private RaycastHit? _currentHit;
        public bool hitUI { get; private set; }
        private int lastFrameRaycasted;


        /// <summary>
        /// This value is cached on a per-frame basis. safe to access multiple times per frame and won't create extra raycasts
        /// </summary>
        public RaycastHit? CurrentlyHitObject
        {
            get
            {
                // lastFrameRaycasted can be greater than if the scriptableobject has cached in edit mode
                if(Time.frameCount != lastFrameRaycasted)
                {
                    this.CheckForRaycastHit();
                    lastFrameRaycasted = Time.frameCount;
                }
                return _currentHit;
            }
        }

        public Ray CurrentRay
        {
            get
            {
                return MouseOverHelpers.GetRay();
            }
        }

        private void CheckForRaycastHit()
        {
            hitUI = EventSystem.current.IsPointerOverGameObject();
            if (uiBlocksRay && hitUI)
            {
                _currentHit = null;
                return;
            }
            if (MouseOverHelpers.RaycastToObject(layersToRaycastTo, out var singleHit, false))
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
