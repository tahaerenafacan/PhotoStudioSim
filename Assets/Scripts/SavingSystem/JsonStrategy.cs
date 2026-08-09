using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SyntaxSultan.SavingSystem
{
    [CreateAssetMenu(menuName = "PSSGame/SavingStrategies/Json", fileName ="JsonStrategy")]
    public class JsonStrategy : SavingStrategy
    {

        public override void SaveToFile(string saveFile, JObject state)
        {
            string path = GetPathFromSaveFile(saveFile);
            Debug.Log($"Saving to {path} ");
            using (var textWriter = File.CreateText(path))
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    writer.Formatting = Formatting.Indented; 
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, state);
                }
            }
        }

        public override JObject LoadFromFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path))
            {
                return new JObject();
            }
            
            using (var textReader = File.OpenText(path))
            {
                using (var reader = new JsonTextReader(textReader))
                {
                    reader.FloatParseHandling = FloatParseHandling.Double;

                    return JObject.Load(reader);
                }
            }
        }

        public override string GetExtension()
        {
            return ".json";
        }
    }
}