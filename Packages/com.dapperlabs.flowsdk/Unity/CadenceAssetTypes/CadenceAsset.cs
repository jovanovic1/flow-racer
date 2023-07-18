using UnityEngine;

namespace DapperLabs.Flow.Sdk.Unity
{
    [CreateAssetMenu(fileName = "New Cadence Asset", menuName = "Scriptable Object/Cadence Asset")]
    public class CadenceAsset : ScriptableObject
    {
        public string text;
        
        public override string ToString()
        {
            return text;
        }
    }
}