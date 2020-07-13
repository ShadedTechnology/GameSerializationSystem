using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

namespace GameSerialization
{
    public interface IDataSerializer
    {
        void LoadDataFromSerializer(SerializationInfo serializationInfo);
        void SaveDataToSerializer(SerializationInfo serializationInfo);
    }

    public class GameDataSerializer : IDataSerializer
    {
        protected SerializableObjectsContainer serializableObjects;

        public GameDataSerializer(SerializableObjectsContainer serializableObjects)
        {
            this.serializableObjects = serializableObjects;
        }

        protected void LoadDataToGame(SerializationInfo info)
        {
            foreach (IOnLoadGameMethod onLoadGameMethod in serializableObjects.OnLoadGameMethods)
            {
                onLoadGameMethod.OnLoadGameMethod(info);
            }
            foreach (FieldSerializationInfo fieldSerializationInfo in serializableObjects.SerializableFields)
            {
                fieldSerializationInfo.fieldInfo.SetValue(fieldSerializationInfo.component, fieldSerializationInfo.getOriginalObject(info));
            }
            foreach (PropertySerializationInfo propSerializationInfo in serializableObjects.SerializableProps)
            {
                propSerializationInfo.propInfo.SetValue(propSerializationInfo.component, propSerializationInfo.getOriginalObject(info));
            }
        }

        protected void SaveDataFromGame(SerializationInfo info)
        {
            foreach (IOnSaveGameMethod onSaveGameMethod in serializableObjects.OnSaveGameMethods)
            {
                onSaveGameMethod.OnSaveGameMethod(info);
            }
            foreach (FieldSerializationInfo fieldSerializationInfo in serializableObjects.SerializableFields)
            {
                info.AddValue(fieldSerializationInfo.id, fieldSerializationInfo.getSerializableObject());
            }
            foreach (PropertySerializationInfo propSerializationInfo in serializableObjects.SerializableProps)
            {
                info.AddValue(propSerializationInfo.id, propSerializationInfo.getSerializableObject());
            }
        }

        public virtual void LoadDataFromSerializer(SerializationInfo serializationInfo)
        {
            LoadDataToGame(serializationInfo);
        }

        public virtual void SaveDataToSerializer(SerializationInfo serializationInfo)
        {
            SaveDataFromGame(serializationInfo);
        }
    }

    public class SceneDataSerializer : GameDataSerializer
    {
        public SceneDataSerializer(SerializableObjectsContainer serializableObjects) : base(serializableObjects) {}

        public override void LoadDataFromSerializer(SerializationInfo serializationInfo)
        {
            if (null == serializationInfo) return;

            Action<AsyncOperation> onSceneLoaded = (operation) =>
            {
                serializableObjects = GameDataCollector.FindAllSerializableObjects();
                LoadDataToGame(serializationInfo);
            };

            int sceneId = (int)serializationInfo.GetValue("SceneManager_ActiveSceneId", typeof(int));
            if (sceneId != SceneManager.GetActiveScene().buildIndex)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
                asyncLoad.completed += onSceneLoaded;
            }
            else
            {
                LoadDataToGame(serializationInfo);
            }
        }

        public override void SaveDataToSerializer(SerializationInfo serializationInfo)
        {
            serializationInfo.AddValue("SceneManager_ActiveSceneId", SceneManager.GetActiveScene().buildIndex);
            SaveDataFromGame(serializationInfo);
        }
    }
}
