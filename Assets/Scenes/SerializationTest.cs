using UnityEngine;
#pragma warning disable CS0169 // Field is never used


public class SerializationTest : MonoBehaviour
{
    [SerializeField] private Vector2 testVector2;
    [SerializeField] private Vector3 testVector3;
    [SerializeField] private Vector4 testVector4;
    [SerializeField] private Quaternion testQuaternion;
    [SerializeField] private Matrix4x4 testMatrix4x4;
    [SerializeField] private Color testColor;
    [SerializeField] private Rect testRect;
    [SerializeField] private LayerMask testLayerMask;
    [SerializeField] private AnimationCurve testAnimationCurve;
}