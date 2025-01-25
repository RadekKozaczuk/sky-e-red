using UnityEngine;

public class CharacterData : ScriptableObject
{
    [SerializeField]
    public Character Character;
    
    [SerializeField]
    public int MaxHp = 3;

    // moving speed
    [SerializeField]
    public float Speed = 4;   
}