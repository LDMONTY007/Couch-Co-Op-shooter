using UnityEngine;

[CreateAssetMenu(fileName = "PrefabData", menuName = "Prefab Data")]
public class PrefabData : ScriptableObject
{
    public string key;
    public GameObject prefab;
}