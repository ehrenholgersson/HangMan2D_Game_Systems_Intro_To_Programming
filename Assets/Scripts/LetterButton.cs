using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour
{
    TextMeshProUGUI _textMesh;
    Button _button;
    [SerializeField] Color CorrectColor;
    [SerializeField] Color WrongColor;
    // Start is called before the first frame update
    void Start()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _button = GetComponent<Button>();
    }

    public void ButtonPressed()
    {

        ColorBlock c = _button.colors;
        if (Hangman.GameController.TryLetter(_textMesh.text.ToCharArray()[0]))
        {
            c.disabledColor = CorrectColor;
        }
        else
        {
            c.disabledColor = WrongColor;
        }
        _button.colors = c;
        _button.interactable = false;
    }
}
