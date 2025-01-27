using System.Collections.Generic;

class PlayerBuilder
{
	public PlayerModel Model => _model ??= new PlayerModel(_characters, _hpValues);

	// the object we're going to build
	PlayerModel _model;

	readonly List<Character> _characters = new();
	readonly List<int> _hpValues = new();
	readonly List<int> _damageValues = new();
	readonly List<float> _speedValues = new();
	
	public PlayerBuilder HasCharacter(Character character)
	{
		_characters.Add(character);
		
		if (_characters.Count - _hpValues.Count == 2)
			_hpValues.Add(0);
		
		if (_characters.Count - _damageValues.Count == 2)
			_damageValues.Add(0);
		
		if (_characters.Count - _speedValues.Count == 2)
			_speedValues.Add(0);
		
		return this;
	}
	
	public PlayerBuilder WithHp(int hp)
	{
		_hpValues.Add(hp);
		return this;
	}

	public PlayerBuilder WithDamage(int damage)
	{
		_damageValues.Add(damage);
		return this;
	}

	public PlayerBuilder WithSpeed(float speed)
	{
		_speedValues.Add(speed);
		return this;
	}
}
