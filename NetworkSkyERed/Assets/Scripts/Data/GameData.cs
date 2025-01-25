using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Data/GameData")]
public class GameData : ScriptableObject
{
    public Character[] FirstPlayerStartCharacters;
    public Character[] SecondPlayerStartCharacters;
}
