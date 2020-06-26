using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSerialization;
using System.Runtime.Serialization;
using System;
using UnityEngine.SceneManagement;

public class SavingManager : MonoBehaviour
{
    private SerializableObjectsContainer serializableObjectContainer;
    
    private void Start()
    {
        serializableObjectContainer = GameDataCollector.FindAllSerializableObjects();
    }

    public void SaveGame(string saveName)
    {
        SerializationHandler.SaveGame(saveName, new SceneDataSerializer(serializableObjectContainer));
    }

    public void LoadGame(string saveName)
    {
        SerializationHandler.LoadGame(saveName, new SceneDataSerializer(serializableObjectContainer));
    }
}
