using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerData
{
    public Character CurrentCharacter => _characters[_currentCharacterIndex];

    readonly List<Character> _characters;
    int _currentCharacterIndex;

    public PlayerData(List<Character> characters)
    {
        _characters = characters;
        _currentCharacterIndex = 0;
    }

    public void NextCharacter()
    {
        Assert.IsTrue(_characters.Count > 1, "It is impossible to change characters if there is only one left.");

        if (_currentCharacterIndex == _characters.Count - 1)
            _currentCharacterIndex = 0;
        else
            _currentCharacterIndex++;
    }
    
    public void SetMovementVector(Vector2 move)
    {
        /*_moveDirection = new Vector3(move.x, 0f, move.y);

        transform.position += _moveDirection * (Time.deltaTime * Speed);
        
        float angle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);

        _moveDirection.x = 0;
        _moveDirection.z = 0;*/
    }

    public void Attack()
    {
        
    }
    
    public void ChangeCharacter()
    {
        
    }
}
