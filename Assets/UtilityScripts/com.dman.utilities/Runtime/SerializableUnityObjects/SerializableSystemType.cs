﻿using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    // Simple helper class that allows you to serialize System.Type objects.
    // Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
    // nice to see people using your stuff!)
    //
    // Written by Bryan Keiren (http://www.bryankeiren.com)
    // lifted from https://forum.unity.com/threads/serializable-system-type-get-it-while-its-hot.187557/
    // modified by Dan Miller

    [System.Serializable]
    public class SerializableSystemType
    {
        [SerializeField]
        private string m_AssemblyQualifiedName;

        public string AssemblyQualifiedName
        {
            get { return m_AssemblyQualifiedName; }
        }

        private System.Type m_SystemType;
        public System.Type SystemType
        {
            get
            {
                if (m_SystemType == null)
                {
                    GetSystemType();
                }
                return m_SystemType;
            }
        }

        private void GetSystemType()
        {
            m_SystemType = string.IsNullOrEmpty(m_AssemblyQualifiedName) ? null : System.Type.GetType(m_AssemblyQualifiedName);
        }

        public SerializableSystemType(System.Type _SystemType)
        {
            m_SystemType = _SystemType;
            m_AssemblyQualifiedName = _SystemType.AssemblyQualifiedName;
        }

        public override int GetHashCode()
        {
            return m_AssemblyQualifiedName.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            SerializableSystemType temp = obj as SerializableSystemType;
            if ((object)temp == null)
            {
                return false;
            }
            return this.Equals(temp);
        }

        public bool Equals(SerializableSystemType _Object)
        {
            if((object)_Object == null)
            {
                return false;
            }
            if(m_AssemblyQualifiedName == null && _Object.m_AssemblyQualifiedName == null)
            {
                return true;
            }
            if(m_AssemblyQualifiedName == null || _Object.m_AssemblyQualifiedName == null)
            {
                return false;
            }
            //return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
            return _Object.m_AssemblyQualifiedName.Equals(m_AssemblyQualifiedName);
        }

        public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }

    }
}
