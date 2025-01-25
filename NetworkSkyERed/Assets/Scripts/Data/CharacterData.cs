using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [SerializeField]
    public CharacterView Prefab;
    
    [Min(1)]
    [SerializeField]
    public int MaxHp = 3;

    [Min(1)]
    [SerializeField]
    public float Speed = 4;
    
    [Min(1)]
    [SerializeField]
    public int Damage = 4;   
}