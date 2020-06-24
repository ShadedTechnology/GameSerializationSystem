using System;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSerialization
{
    public interface IDataSerializer
    {
        void LoadDataFromSerializer(SerializationInfo serializationInfo);
        void SaveDataToSerializer(SerializationInfo serializationInfo);
    }

    public class GameDataSerializer : IDataSerializer
    {
        private SerializableObjectsContainer serializableObjects;

        public GameDataSerializer(SerializableObjectsContainer serializableObjects)
        {
            this.serializableObjects = serializableObjects;
        }

        private void LoadDataToGame(SerializationInfo info)
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

        public void SaveDataFromGame(SerializationInfo info)
        {
            info.AddValue("SceneManager_ActiveSceneId", SceneManager.GetActiveScene().buildIndex);
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

        public void LoadDataFromSerializer(SerializationInfo serializationInfo)
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

        public void SaveDataToSerializer(SerializationInfo serializationInfo)
        {
            SaveDataFromGame(serializationInfo);
        }
    }
}
