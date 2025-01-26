using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Data/GameData")]
public class GameData : ScriptableObject
{
    public Character[] FirstPlayerStartCharacters;
    public Character[] SecondPlayerStartCharacters;

    public float DefaultPositionYOffset = -0.7f;
    public float DefaultSpawnRotation = 180;
}
