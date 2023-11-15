using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour
{
    TextMeshProUGUI _textMesh;
    Button _button;
    [SerializeField] Color _correctColor;
    [SerializeField] Color _wrongColor;

    void OnEnable()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _button = GetComponent<Button>();
    }

    public void ButtonPressed()
    {

        ColorBlock c = _button.colors;
        if (Hangman.GameController.TryLetter(_textMesh.text.ToCharArray()[0]))
        {
            c.disabledColor = _correctColor;
        }
        else
        {
            c.disabledColor = _wrongColor;
        }
        _button.colors = c;
        _button.interactable = false;
    }
}
