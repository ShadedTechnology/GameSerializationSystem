using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// made by Michał Warzecha 28.08.2019

/// <summary>
/// Derive from this class and override it's methods to save/load your data
/// </summary>
public abstract class SaveableObject : MonoBehaviour
{
    /// <summary>
    /// Method should get data from game which you want to save,
    /// store it in class that derives from <see cref="DataToSave"/> class
    /// and return it's instance.
    /// </summary>
    /// <returns>Class with data you want to save.</returns>
    public abstract DataToSave GetDataToSave();

    /// <summary>
    /// This method should load data from <see cref="DataToSave"/> parameter to your in game classes
    /// </summary>
    /// <param name="data">Loaded data.</param>
    public abstract void SetLoadedData(DataToSave data);
}

/// <summary>
/// <para> !!! For this to work you have to write [<see cref="System.Serializable"/>] atribbute above your deriving class !!! </para>
/// Implement this interface to store in it your data you want to save.
/// </summary>
public interface DataToSave { }
