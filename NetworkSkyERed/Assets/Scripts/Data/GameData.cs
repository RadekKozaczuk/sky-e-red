using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Data/GameData")]
public class GameData : ScriptableObject
{
    [Space(20f)]
    public Vector2 FirstPlayerSpawnPosition;
    public Character[] FirstPlayerStartCharacters;
    
    [Space(20f)]
    public Vector2 SecondPlayerSpawnPosition;
    public Character[] SecondPlayerStartCharacters;

    [Space(20f)]
    public float DefaultPositionYOffset = -0.7f;
    public float DefaultSpawnRotation = 180;
}
