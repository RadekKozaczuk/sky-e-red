using UnityEngine;

public class GameData : ScriptableObject
{
    [SerializeField]
    Character[] _hostStartCharacters;
    
    [SerializeField]
    Character[] _clientStartCharacters;
}
