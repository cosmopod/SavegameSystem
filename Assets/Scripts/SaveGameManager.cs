using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance { get; private set; }
    
    private List<ISerializable> savedObjects = new List<ISerializable>();

    public UnityEvent OnSave;

    public UnityEvent OnLoad;
    // Update is called once per frame

    private void Awake()
    {
        if (!Instance && Instance != this)
        {
            Instance = this;
        }
        
        DontDestroyOnLoad(this);
    }
    
    private void Update()
    {
        //save game
        if (Input.GetKeyDown(KeyCode.S))
        {
            JObject savedGameObject = new JObject();

            for (int i = 0; i < savedObjects.Count; i++)
            {
                var curObjects = savedObjects[i];
                JObject serializedEnemy = curObjects.Serialize();
                savedGameObject.Add(curObjects.GetId(), serializedEnemy);
            }
            
            SaveGame(savedGameObject.ToString());
        }

        //load game
        if (Input.GetKeyDown(KeyCode.L))
        {
            string loadedGame = LoadGame();
            JObject loadedJObject = JObject.Parse(loadedGame);

            for (int i = 0; i < savedObjects.Count; i++)
            {
                var curEnemy = savedObjects[i];
                string enemyJsonString = loadedJObject[curEnemy.GetId()].ToString();
                curEnemy.Deserialize(enemyJsonString);
            }
        }
    }

    public void RegisterPersistence(ISerializable serializableGameObject)
    {
        if (!savedObjects.Contains(serializableGameObject))
        {
            savedObjects.Add(serializableGameObject);
        }
    }

    public void UnregisterPersistence(ISerializable serializableGameObject)
    {
        if (savedObjects.Contains(serializableGameObject))
        {
            savedObjects.Remove(serializableGameObject);
        }
    }

    private void SaveGame(string jsonString)
    {
        string filePath = Application.persistentDataPath + "/saveGame.sav";
        Debug.Log("saving to " + filePath + "\n" + jsonString);

        byte[] saveGame = Encrypt(jsonString);
        File.WriteAllBytes(filePath, saveGame);
        OnSave?.Invoke();
    }

    private string LoadGame()
    {
        string filePath = Application.persistentDataPath + "/saveGame.sav";
        Debug.Log("loading from " + filePath);
        
        byte[] encryptedSaveGame = File.ReadAllBytes(filePath);
        return Decrypt(encryptedSaveGame);
        OnLoad?.Invoke();
    }

    private byte[] _key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

    private byte[] _initializationVector = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };


    byte[] Encrypt(string message)
    {
        AesManaged aesManaged = new AesManaged();
        ICryptoTransform encryptor = aesManaged.CreateEncryptor(_key, _initializationVector);

        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        StreamWriter streamWriter = new StreamWriter(cryptoStream);
        
        streamWriter.Write(message);

        streamWriter.Close();
        cryptoStream.Close();
        memoryStream.Close();

        return memoryStream.ToArray();
    }

    string Decrypt(byte[] message)
    {
        AesManaged aesManaged = new AesManaged();
        ICryptoTransform decrypter = aesManaged.CreateDecryptor(_key, _initializationVector);

        MemoryStream memoryStream = new MemoryStream(message);
        CryptoStream cryptoStream = new CryptoStream(memoryStream, decrypter, CryptoStreamMode.Read);
        StreamReader streamReader = new StreamReader(cryptoStream);

        var decryptedMessage = streamReader.ReadToEnd();
        
        streamReader.Close();
        cryptoStream.Close();
        memoryStream.Close();

        return decryptedMessage;
    }
}