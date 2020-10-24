using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSerialization;
using System.Runtime.Serialization;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

public class SavingManager : MonoBehaviour
{
    private SerializableObjectsContainer serializableObjectContainer;
    private SceneDataSerializer sceneSerializer;
    
    private void Start()
    {
        serializableObjectContainer = GameDataCollector.FindAllSerializableObjects();
        sceneSerializer = new SceneDataSerializer(serializableObjectContainer);
    }

    public void SaveGame(string saveName)
    {
        SerializationHandler.SaveData(saveName, sceneSerializer);
    }

    public void LoadGame(string saveName)
    {
        SerializationHandler.LoadData(saveName, sceneSerializer);
    }
}
