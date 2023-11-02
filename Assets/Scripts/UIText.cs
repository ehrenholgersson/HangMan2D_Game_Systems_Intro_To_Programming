using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour
{
    float _alpha;
    TextMeshProUGUI _text;
    static UIText _main;

    private void Start()
    {
        
        if (_main == null)
        {
            _main = this;
            _text = GetComponent<TextMeshProUGUI>();
        }
        else Destroy(this);
        _alpha = 0;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0);
    }
    // Update is called once per frame
    void Update()
    {
        if (_alpha > 0)
        {
            _alpha -= 0.2f * Time.deltaTime;
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _alpha);
        }
    }

    void UpdateText(string newText)
    {
        _text.text = newText;
        _alpha = 1;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _alpha);
    }

    public static void DisplayText(string newText)
    {
        _main.UpdateText(newText);
    }
}
