using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class UnitySerializationCompatibleContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            // Throw an exception if the type derives from UnityEngine.Object
            if (typeof(UnityEngine.Object).IsAssignableFrom(objectType) || typeof(ScriptableObject).IsAssignableFrom(objectType))
            {
                throw new InvalidOperationException($"Serialization of unity engine objects is not allowed. Tried to serialize {objectType.Name}");
            }
            
            return base.CreateContract(objectType);
        }
        
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            // when the default contract resolver sets IgnoreSerializableAttribute to false, then we will automatically include
            //  fields on classes with [Serializable] attribute.
            // all we have to do is filter out the fields which are not tagged with [SerializeField] inside classes tagged with [Serializable].
            var baseMembers = base.GetSerializableMembers(objectType);
            if (this.IgnoreSerializableAttribute) return baseMembers;
            var hasSerializableAttribute = objectType.GetCustomAttribute<SerializableAttribute>(inherit: false) != null;
            if (!hasSerializableAttribute) return baseMembers;
            return baseMembers.Where(member =>
            {
                if (member.MemberType == MemberTypes.Field && member is FieldInfo fieldInfo)
                {
                    return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null;
                }

                return false;
            }).ToList();
        }
    }
}