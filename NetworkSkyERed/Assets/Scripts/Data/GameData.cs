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

    /// <summary>
    /// Some models need vertical adjustment so that their feet touch the floor.
    /// </summary>
    [Space(20f)]
    public float DefaultPositionYOffset = -1f;
    public float DefaultSpawnRotation = 180;
}
