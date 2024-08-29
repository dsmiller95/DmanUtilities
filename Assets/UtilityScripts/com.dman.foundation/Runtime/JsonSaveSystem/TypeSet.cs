using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class TypeSet
    {
        private HashSet<Type> _types;

        private static TypeSet _unityPrimitiveTypeSet;

        public static TypeSet UnityPrimitiveTypeSet
        {
            get
            {
                if (_unityPrimitiveTypeSet != null) return _unityPrimitiveTypeSet;
                
                var unityPrimitives = new HashSet<Type>();
                unityPrimitives.Add(typeof(Vector2));
                unityPrimitives.Add(typeof(Vector3));
                unityPrimitives.Add(typeof(Vector4));
                unityPrimitives.Add(typeof(Quaternion));
                unityPrimitives.Add(typeof(Matrix4x4));
                unityPrimitives.Add(typeof(Color));
                unityPrimitives.Add(typeof(Color32));
                // unityPrimitives.Add(typeof(LayerMask));
                // TODO: handle these types, currently not serialized
                unityPrimitives.Add(typeof(Rect));
                unityPrimitives.Add(typeof(AnimationCurve));
                unityPrimitives.Add(typeof(Gradient));
                _unityPrimitiveTypeSet = new TypeSet()
                {
                    _types = unityPrimitives
                };
                return _unityPrimitiveTypeSet;
            }
        }

        public bool Contains(Type type)
        {
            return _types.Contains(type);
        }
    }
}