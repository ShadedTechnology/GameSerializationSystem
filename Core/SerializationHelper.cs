using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

namespace GameSerialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class SaveableField : Attribute { }

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class SaveableProperty : Attribute { }

    public interface IOnSaveGameMethod
    {
        void OnSaveGameMethod(SerializationInfo info);
    }

    public interface IOnLoadGameMethod
    {
        void OnLoadGameMethod(SerializationInfo info);
    }

    static class TransformPathExtension
    {
        public static string GetPath(this Transform current)
        {
            if (current.parent == null)
                return "/" + current.name;
            return current.parent.GetPath() + "/" + current.name;
        }
    }

    public interface ITypeSerializer
    {
        object ToSerializable(object obj);
        object FromSerializable(object obj);
        Type GetSerializationType();
    }

    static class SerializationHelper
    {
        public static string GetComponentPath(MonoBehaviour component)
        {
            return component.transform.GetPath() + "/" + component.GetType().ToString();
        }

        public static void SaveObject(SerializationInfo info, string id, object obj, Type type)
        {
            info.AddValue(id + "@data_type", type);
            info.AddValue(id + "@data", obj);
        }

        public static object LoadObject(SerializationInfo info, string id)
        {
            Type type = info.GetValue(id + "@data_type", typeof(Type)) as Type;
            return info.GetValue(id + "@data", type);
        }

        public static void SaveObject(SerializationInfo info, MonoBehaviour component, object obj, Type type)
        {
            info.AddValue(GetComponentPath(component) + "@data_type", type);
            info.AddValue(GetComponentPath(component) + "@data", obj);
        }

        public static object LoadObject(SerializationInfo info, MonoBehaviour component)
        {
            Type type = info.GetValue(GetComponentPath(component) + "@data_type", typeof(Type)) as Type;
            return info.GetValue(GetComponentPath(component) + "@data", type);
        }

    }
}