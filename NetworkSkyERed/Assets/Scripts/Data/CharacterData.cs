using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    public CharacterView Prefab;
    public Character Character;
    
    [Min(1)]
    public int MaxHp = 3;

    [Min(1)]
    public float Speed = 4;
    
    [Min(1)]
    public int Damage = 4;
}