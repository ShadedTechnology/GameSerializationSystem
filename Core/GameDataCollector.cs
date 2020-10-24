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
    public class GameDataCollector
    {
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
            else if (SerializableTypes.serializationDictionary.TryGetValue(field.FieldType, out ITypeSerializer serializer))
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
            else if (SerializableTypes.serializationDictionary.TryGetValue(prop.PropertyType, out ITypeSerializer serializer))
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

        public static SerializableObjectsContainer FindAllSerializableObjects()
        {
            SerializableObjectsContainer container = new SerializableObjectsContainer();
            foreach (MonoBehaviour component in GameObject.FindObjectsOfType<MonoBehaviour>())
            {
                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.IsDefined(typeof(SaveableField), false)).ToArray();
                var props = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(typeof(SaveableProperty), false)).ToArray();
                string id = SerializationHelper.GetComponentPath(component);

                if (component is IOnSaveGameMethod)
                {
                    container.OnSaveGameMethods.Add((IOnSaveGameMethod)component);
                }
                if (component is IOnLoadGameMethod)
                {
                    container.OnLoadGameMethods.Add((IOnLoadGameMethod)component);
                }

                foreach (FieldInfo field in fields)
                {
                    FieldSerializationInfo fieldSerializationInfo = FieldToSerializationInfo(field, id, component);
                    if (null != fieldSerializationInfo)
                    {
                        container.SerializableFields.Add(fieldSerializationInfo);
                    }
                }

                foreach (PropertyInfo prop in props)
                {
                    PropertySerializationInfo propertySerializationInfo = PropToSerializationInfo(prop, id, component);
                    if (null != propertySerializationInfo)
                    {
                        container.SerializableProps.Add(propertySerializationInfo);
                    }
                }
            }
            return container;
        }
    }
}
