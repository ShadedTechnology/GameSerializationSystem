using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

namespace GameSerialization
{
    public class FieldSerializationInfo
    {
        public string id;
        public FieldInfo fieldInfo;
        public MonoBehaviour component;
        public Func<object> getSerializableObject;
        public Func<SerializationInfo, object> getOriginalObject;
    }

    public class PropertySerializationInfo
    {
        public string id;
        public PropertyInfo propInfo;
        public MonoBehaviour component;
        public Func<object> getSerializableObject;
        public Func<SerializationInfo, object> getOriginalObject;
    }

    public class SerializableObjectsContainer
    {
        public List<IOnSaveGameMethod> OnSaveGameMethods { get; set; } = new List<IOnSaveGameMethod>();
        public List<IOnLoadGameMethod> OnLoadGameMethods { get; set; } = new List<IOnLoadGameMethod>();
        public List<FieldSerializationInfo> SerializableFields { get; set; } = new List<FieldSerializationInfo>();
        public List<PropertySerializationInfo> SerializableProps { get; set; } = new List<PropertySerializationInfo>();
    }
}
