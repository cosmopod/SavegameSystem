using Newtonsoft.Json.Linq;

public interface ISerializable
{
    string GetId();
    JObject Serialize();
    void Deserialize(string jsonString);
}
