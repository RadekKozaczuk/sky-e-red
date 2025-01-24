using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class HpView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;

    public void SetHp(int hp)
    {
        Assert.IsTrue(hp >= 0, "Hp cannot be negative");

        _text.text = "HP: " + hp;
    }
}