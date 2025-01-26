using System.Collections.Generic;

public class PlayerModel
{
    public bool CanChangeCharacter => true; // todo: use later
    
    public Character CurrentCharacter => _characters[_currentCharacterIndex];
    
    /// <summary>
    /// Hp of the currently selected character.
    /// </summary>
    public int CurrentHp => _hpValues[_currentCharacterIndex];

    readonly List<Character> _characters;
    readonly List<int> _hpValues;
    int _currentCharacterIndex;

    public PlayerModel(List<Character> characters)
    {
        _characters = characters;
        _hpValues = new List<int>(characters.Count);
        _currentCharacterIndex = 0;
    }

    public void ChangeCharacter()
    {
        if (_currentCharacterIndex == _characters.Count - 1)
            _currentCharacterIndex = 0;
        else
            _currentCharacterIndex++;
    }
}
