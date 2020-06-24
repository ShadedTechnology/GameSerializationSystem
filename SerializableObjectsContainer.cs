using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

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

        public static FieldSerializationInfo FieldToSerializationInfo(FieldInfo field, string id, MonoBehaviour component)
        {
            string fieldId = id + field.Name;
            Func<object> getSerializableObject;
            Func<SerializationInfo, object> getOriginalObject;
            if (field.FieldType.IsSerializable)
            {
                getSerializableObject = () => { return field.GetValue(component); };
                getOriginalObject = (info) => { return info.GetValue(fieldId, field.FieldType); };
            }
            else if (SerializationHelper.serializationDictionary.TryGetValue(field.FieldType, out ITypeSerializer serializer))
            {
                getSerializableObject = () => { return serializer.ToSerializable(field.GetValue(component)); };
                getOriginalObject = (info) => { return serializer.FromSerializable(info.GetValue(fieldId, serializer.GetSerializationType())); };
            }
            else
            {
                Debug.LogError("Serialization error: " + id + " > Field \"" + field.Name + "\" cannot be serialized or deserialized");
                return null;
            }

            return new FieldSerializationInfo
            {
                id = fieldId,
                fieldInfo = field,
                component = component,
                getSerializableObject = getSerializableObject,
                getOriginalObject = getOriginalObject
            };
        }

        public static PropertySerializationInfo PropToSerializationInfo(PropertyInfo prop, string id, MonoBehaviour component)
        {
            string propId = id + prop.Name;
            Func<object> getSerializableObject;
            Func<SerializationInfo, object> getOriginalObject;
            if (prop.PropertyType.IsSerializable)
            {
                getSerializableObject = () => { return prop.GetValue(component); };
                getOriginalObject = (info) => { return info.GetValue(propId, prop.PropertyType); };
            }
            else if (SerializationHelper.serializationDictionary.TryGetValue(prop.PropertyType, out ITypeSerializer serializer))
            {
                getSerializableObject = () => { return serializer.ToSerializable(prop.GetValue(component)); };
                getOriginalObject = (info) => { return serializer.FromSerializable(info.GetValue(propId, serializer.GetSerializationType())); };
            }
            else
            {
                Debug.LogError("Serialization error: " + id + " > Property \"" + prop.Name + "\" cannot be serialized or deserialized");
                return null;
            }

            return new PropertySerializationInfo
            {
                id = propId,
                propInfo = prop,
                component = component,
                getSerializableObject = getSerializableObject,
                getOriginalObject = getOriginalObject
            };
        }

        public SerializableObjectsContainer FindAllSerializableObjects()
        {
            foreach (MonoBehaviour component in GameObject.FindObjectsOfType<MonoBehaviour>())
            {
                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.IsDefined(typeof(SaveableField), false)).ToArray();
                var props = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(typeof(SaveableProperty), false)).ToArray();
                string id = SerializationHelper.GetComponentPath(component);

                if (component is IOnSaveGameMethod)
                {
                    OnSaveGameMethods.Add((IOnSaveGameMethod)component);
                }
                if (component is IOnLoadGameMethod)
                {
                    OnLoadGameMethods.Add((IOnLoadGameMethod)component);
                }
                
                foreach (FieldInfo field in fields)
                {
                    FieldSerializationInfo fieldSerializationInfo = FieldToSerializationInfo(field, id, component);
                    if (null != fieldSerializationInfo)
                    {
                        SerializableFields.Add(fieldSerializationInfo);
                    }
                }

                foreach (PropertyInfo prop in props)
                {
                    PropertySerializationInfo propertySerializationInfo = PropToSerializationInfo(prop, id, component);
                    if (null != propertySerializationInfo)
                    {
                        SerializableProps.Add(propertySerializationInfo);
                    }
                }
            }
            return this;
        }
    }
}
