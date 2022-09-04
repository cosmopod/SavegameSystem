using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, ISerializable
{
    public int hP;
    public string type;

    private void Start()
    {
        SaveGameManager.Instance.RegisterPersistence(this);
    }

    public string GetId()
    {
        return name;
    }

    public JObject Serialize()
    {
        string jsonString = JsonUtility.ToJson(this);
        return JObject.Parse(jsonString);
    }

    public void Deserialize(string jsonString)
    {
        JsonUtility.FromJsonOverwrite(jsonString, this);
    }

    private void OnDestroy()
    {
        SaveGameManager.Instance.UnregisterPersistence(this);
    }
}
