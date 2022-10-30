using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.Utilities
{
    public static class MeshFramingUtilities
    {
        public static Vector3 GetAdjustmentToFrameRectsInPerspective(
            Camera framingCamera,
            (float distance, Rect rect) framedScreenSpace,
            Rect framingRectScreenSpace)
        {
            var centerOffsetScreenSpace = WorldSpaceTranslateVector(framedScreenSpace.rect.center, framingRectScreenSpace.center, framingCamera, framedScreenSpace.distance);
            var adjustmentToCenterInScreen = -centerOffsetScreenSpace;

            var scaleAdjustment = GetScaleAdjustmentToFit(framingRectScreenSpace, framedScreenSpace.rect);

            // assuming perspective projection, size of object on screen scales inversly with distance from camera
            var targetDistance = framedScreenSpace.distance / scaleAdjustment;
            var distanceToApproach = framedScreenSpace.distance - targetDistance;
            var zoomRay = framingCamera.ScreenPointToRay(framedScreenSpace.rect.center);
            var adjustmentToZoomToFrame = zoomRay.GetPoint(distanceToApproach) - zoomRay.origin;

            return adjustmentToCenterInScreen + adjustmentToZoomToFrame;
        }


        public static Rect MultiplyRectSize(Rect initialRect, float extensionFactor)
        {
            var trueCenter = initialRect.center;
            initialRect.size *= (extensionFactor * 0.9f + 0.1f);
            initialRect.center = trueCenter;

            return initialRect;
        }

        public static Rect ScreenSpaceBounds(RectTransform rectTransform)
        {
            Vector3[] uiRectWorldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(uiRectWorldCorners);
            return EnclosingRect(uiRectWorldCorners
                .Select(x => new Vector2(x.x, x.y))
                .ToList());
        }
        public static Rect ScreenSpaceBoundsWorldSpaceCanvas(RectTransform rectTransform, Camera framingCamera)
        {
            Vector3[] uiRectWorldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(uiRectWorldCorners);
            return EnclosingRect(uiRectWorldCorners
                .Select(x => RectTransformUtility.WorldToScreenPoint(framingCamera, x))
                .Select(x => new Vector2(x.x, x.y))
                .ToList());
        }

        public static (float distance, Rect screenRect) ScreenSpaceBoundsFromWorldObject(RectTransform rect, Camera framingCamera)
        {
            var framedBounds = ScreenSpaceBoundsWorldSpaceCanvas(rect, framingCamera);
            var boundTransform = rect.transform;
            var boundsCenter = boundTransform.TransformPoint(framedBounds.center);

            var distFromCam = Vector3.Distance(boundsCenter, framingCamera.transform.position);

            return (distFromCam, framedBounds);
        }

        public static (float distance, Rect screenRect) ScreenSpaceBoundsFromWorldObject(MeshFilter meshFilter, Camera framingCamera)
        {
            var framedBounds = meshFilter.mesh.bounds;
            var boundTransform = meshFilter.transform;
            var boundsCenter = boundTransform.TransformPoint(framedBounds.center);

            var distFromCam = Vector3.Distance(boundsCenter, framingCamera.transform.position);

            var framedBoundsInScreenSpace = ScreenSpaceBounds(framedBounds, framingCamera, boundTransform);
            return (distFromCam, framedBoundsInScreenSpace);
        }

        public static Rect ScreenSpaceBounds(Bounds renderedBounds, Camera renderedCamera, Transform relativeTo = null)
        {
            var pointTransform = relativeTo == null ? Matrix4x4.identity : relativeTo.localToWorldMatrix;
            var boundCourners = new Vector3[]
            {
                new Vector3(renderedBounds.min.x, renderedBounds.min.y, renderedBounds.min.z),
                new Vector3(renderedBounds.min.x, renderedBounds.min.y, renderedBounds.max.z),
                new Vector3(renderedBounds.min.x, renderedBounds.max.y, renderedBounds.min.z),
                new Vector3(renderedBounds.min.x, renderedBounds.max.y, renderedBounds.max.z),
                new Vector3(renderedBounds.max.x, renderedBounds.min.y, renderedBounds.min.z),
                new Vector3(renderedBounds.max.x, renderedBounds.min.y, renderedBounds.max.z),
                new Vector3(renderedBounds.max.x, renderedBounds.max.y, renderedBounds.min.z),
                new Vector3(renderedBounds.max.x, renderedBounds.max.y, renderedBounds.max.z),
            }
            .Select(x => pointTransform.MultiplyPoint(x))
            .Select(x => RectTransformUtility.WorldToScreenPoint(renderedCamera, x)).ToList();

            return EnclosingRect(boundCourners);
        }

        public static Rect EnclosingRect(IList<Vector2> points)
        {
            var min = points[0];
            var max = points[0];

            for (int i = 0; i < points.Count; i++)
            {
                min.x = Mathf.Min(min.x, points[i].x);
                min.y = Mathf.Min(min.y, points[i].y);

                max.x = Mathf.Max(max.x, points[i].x);
                max.y = Mathf.Max(max.y, points[i].y);
            }

            return new Rect(min, max - min);
        }

        /// <summary>
        /// get a scale which will fit <paramref name="toFit"/> inside <paramref name="targetBox"/>.
        /// More precisely, if <paramref name="toFit"/> is scaled by the return value of this function,
        ///     then both width and heigh of that rect will be less than or equal to 
        ///     <paramref name="targetBox"/>'s width and height
        /// </summary>
        /// <param name="targetBox"></param>
        /// <param name="toFit"></param>
        /// <returns></returns>
        public static float GetScaleAdjustmentToFit(Rect targetBox, Rect toFit)
        {
            var scaleAdjustFromWidth = targetBox.width / toFit.width;
            var scaleAdjustFromHeight = targetBox.height / toFit.height;
            return Mathf.Min(scaleAdjustFromHeight, scaleAdjustFromWidth);
        }

        /// <summary>
        /// get a world-space translation which should move an object bound's center in screen-space
        /// </summary>
        /// <param name="screenSpaceOrigin"></param>
        /// <param name="screenSpaceDest"></param>
        /// <param name="camera"></param>
        /// <param name="distanceFromCamera"></param>
        /// <returns></returns>
        public static Vector3 WorldSpaceTranslateVector(
            Vector2 screenSpaceOrigin,
            Vector2 screenSpaceDest,
            Camera camera,
            float distanceFromCamera)
        {
            var desiredCenter = camera.ScreenPointToRay(screenSpaceDest).GetPoint(distanceFromCamera);
            var currentCenter = camera.ScreenPointToRay(screenSpaceOrigin).GetPoint(distanceFromCamera);

            return desiredCenter - currentCenter;
        }

        public static IEnumerable<Vector2> GetCorners(Rect bounds)
        {
            yield return new Vector2(bounds.min.x, bounds.min.y);
            yield return new Vector2(bounds.max.x, bounds.min.y);
            yield return new Vector2(bounds.max.x, bounds.max.y);
            yield return new Vector2(bounds.min.x, bounds.max.y);
        }
    }
}
