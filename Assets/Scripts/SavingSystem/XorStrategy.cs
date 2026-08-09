using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace SyntaxSultan.SavingSystem
{
    [CreateAssetMenu(menuName = "PSSGame/SavingStrategies/Xor", fileName ="XorStrategy")]
    public class XorStrategy : SavingStrategy
    {
        [Header("Use Context Menu to generate a random key.")]
        [SerializeField] string key = "TheQuickBrownFoxJumpedOverTheLazyDog"; 
        
        public string EncryptDecrypt(string szPlainText, string szEncryptionKey)  
        {  
            StringBuilder szInputStringBuild = new StringBuilder(szPlainText);  
            StringBuilder szOutStringBuild = new StringBuilder(szPlainText.Length);  
            char Textch;  
            for (int iCount = 0; iCount < szPlainText.Length; iCount++)
            {
                int stringIndex = iCount % szEncryptionKey.Length;
                Textch = szInputStringBuild[iCount];  
                Textch = (char)(Textch ^ szEncryptionKey[stringIndex]);  
                szOutStringBuild.Append(Textch);  
            }  
            return szOutStringBuild.ToString();  
        } 
        
        public override void SaveToFile(string saveFile, JObject state)
        {
            string path = GetPathFromSaveFile(saveFile);
            Debug.Log($"Saving to {path} ");
            using (var textWriter = File.CreateText(path))
            {
                string json = state.ToString();
                string encoded = EncryptDecrypt(json, key);
                textWriter.Write(encoded);
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
                string encoded = textReader.ReadToEnd();
                string json = EncryptDecrypt(encoded, key);
                return (JObject)JToken.Parse(json);
            }
        }
        public override string GetExtension()
        {
            return ".xor";
        }
        
        #if UNITY_EDITOR

            [ContextMenu("Generate Random Key")]
            void GenerateKey()
            {
                SerializedObject serializedObject = new SerializedObject(this);
                SerializedProperty property = serializedObject.FindProperty("key");
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }
        
        #endif
    }
}
