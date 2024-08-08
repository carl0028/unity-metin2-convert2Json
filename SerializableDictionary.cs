using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// serialize dictionary; at first dictionary cannot be serialize in c#, so following must be need for resolving this
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    // Custom serialization method
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    // Custom deserialization method
    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<TKey, TValue>();
        if (keys.Count != values.Count)
        {
            Debug.LogError("The number of keys and values in SerializableDictionary are not the same!");
            return;
        }
        for (int i = 0; i < keys.Count; i++)
        {
            dictionary[keys[i]] = values[i];
        }
    }

    // Accessor for the dictionary
    public Dictionary<TKey, TValue> ToDictionary()
    {
        return dictionary;
    }
}