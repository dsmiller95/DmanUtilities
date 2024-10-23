using System;
using NUnit.Framework;
using UnityEngine;
using static Dman.Foundation.Tests.SaveDataTestUtils;

namespace Dman.Foundation.Tests
{
    [Serializable]
    public struct AllUnityPrimitives
    {
        public Vector2 testVector2;
        public Vector3 testVector3;
        public Vector4 testVector4;
        public Vector2Int testVector2Int;
        public Vector3Int testVector3Int;
        public Quaternion testQuaternion;
        public Matrix4x4 testMatrix4x4;
        public Color testColor;
        public Color32 testColor32;
        public LayerMask testLayerMask;
        
        // TODO: handle these types, currently not serialized
        public Rect testRect;
        public AnimationCurve testAnimationCurve;
        public Gradient testGradient;
    }
    
    public class TestUnityPrimitivesRoundTrip
    {
        [Test]
        public void WhenSavedPrimitiveTypes_SavesJson()
        {
            // arrange
            var savedData = new AllUnityPrimitives
            {
                testVector2 = new Vector2(1.1f, 1.2f),
                testVector3 = new Vector3(2.1f, 2.2f, 2.3f),
                testVector4 = new Vector4(3.1f, 3.2f, 3.3f, 3.4f),
                testVector2Int = new Vector2Int(8, 10),
                testVector3Int = new Vector3Int(19, 77, 7),
                testQuaternion = new Quaternion(4.1f, 4.2f, 4.3f, 4.4f),
                testMatrix4x4 = new Matrix4x4(Vector4.one, Vector4.zero, new Vector4(1,1,0,0), new Vector4(1,0,1,0)),
                testColor = new Color(5.1f, 5.2f, 5.3f, 5.4f),
                testColor32 = new Color32(100, 120, 130, 150),
                testRect = new Rect(6.6f, 6.7f, 6.8f, 6.9f),
                testLayerMask = (LayerMask)0b00100110,
                testAnimationCurve = AnimationCurve.EaseInOut(0, 3, 2, 9),
                testGradient = new Gradient(),
            };
            var expectedSavedString = @"
{
  ""unityPrimitives"": {
    ""testVector2"": {""x"":1.100000023841858,""y"":1.2000000476837159},
    ""testVector3"": {""x"":2.0999999046325685,""y"":2.200000047683716,""z"":2.299999952316284},
    ""testVector4"": {""x"":3.0999999046325685,""y"":3.200000047683716,""z"":3.299999952316284,""w"":3.4000000953674318},
    ""testVector2Int"": {""x"":8,""y"":10},
    ""testVector3Int"": {""x"":19,""y"":77,""z"":7},
    ""testQuaternion"": {""x"":4.099999904632568,""y"":4.199999809265137,""z"":4.300000190734863,""w"":4.400000095367432},
    ""testMatrix4x4"": {""m00"":1.0,""m10"":1.0,""m20"":1.0,""m30"":1.0,""m01"":0.0,""m11"":0.0,""m21"":0.0,""m31"":0.0,""m02"":1.0,""m12"":1.0,""m22"":0.0,""m32"":0.0,""m03"":1.0,""m13"":0.0,""m23"":1.0,""m33"":0.0},
    ""testColor"": {""r"":5.099999904632568,""g"":5.199999809265137,""b"":5.300000190734863,""a"":5.400000095367432},
    ""testColor32"": {""r"":100,""g"":120,""b"":130,""a"":150},
    ""testLayerMask"": {
      ""value"": 38
    },
    ""testRect"": {},
    ""testAnimationCurve"": {},
    ""testGradient"": {}
  }
}
".Trim();
            // act
            string serializedString = SerializeToString(
                "test",
                assertInternalRoundTrip: false,
                ("unityPrimitives", savedData));
            
            // assert
            AssertDeserializeWithoutError(
                "test",
                serializedString,
                ("unityPrimitives", savedData));
            AssertMultilineStringEqual(expectedSavedString, serializedString);
        }
    }
}
