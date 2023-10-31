using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Hangman : MonoBehaviour
{
    [SerializeField] List<string> _availableWords = new List<string>();
    [SerializeField] TextMeshProUGUI _textField;
    [SerializeField] GameObject _hangman;
    [SerializeField] GameObject _rope;
    //[SerializeField] Vector3 _picnicPosition;
    [SerializeField] Vector3 _hangPosition;
    [SerializeField] float _moveTime;
    [SerializeField] GameObject[] _menus; 

    public enum Menus {Main,Options,Game, Pause }

    public Menus uImode;

    HingeJoint2D _head;
    Rigidbody2D _connectionPoint;
    public static Hangman GameController;

    GameObject[] _bodyParts = new GameObject[6];
    char[] _letters;
    List <char> _remainingLetters = new List<char>();
    int _hangmanCount = 0;
    char[] _allowedinputs = new char[] { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z' };

    private void Start()
    {
        if (GameController == null)
            GameController = this;
        else Destroy(this);

        Application.targetFrameRate = 60;

        _letters = _availableWords[UnityEngine.Random.Range(0,_availableWords.Count)].ToArray();
        _remainingLetters = _letters.ToList();
        UpdateText();

        _head = _hangman.transform.Find("Head").GetComponent<HingeJoint2D>();
        if (_head != null)
            _connectionPoint = _head.connectedBody;

        _rope.SetActive(false);
        foreach (Rigidbody2D rb in _hangman.GetComponentsInChildren<Rigidbody2D>(true))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        int i = 0;
        foreach (Transform t in _hangman.GetComponentsInChildren<Transform>(true))
        {
            //Debug.Log("found Image" + i);
            if (i < _bodyParts.Length &&t.gameObject!= _hangman && t.parent.gameObject == _hangman)
            {
                _bodyParts[i] = t.gameObject;
                t.gameObject.SetActive(false);
                i++;
            }
        }


    }

    public void ChangeUI(Menus newMode)
    {
        uImode = newMode;
        for (int i = 0; i < Enum.GetNames(typeof(Menus)).Length && i < _menus.Length;i++)
        {
            if (i == (int)newMode)
                _menus[i].SetActive(true);
            else
                _menus[i].SetActive(false);
        }
    }

    public async void Hang()
    {
        Vector3 _picnicPosition = _hangman.transform.position;
        float timer = Time.time;
        UIText.DisplayText("Oh No!");
        while (Time.time < timer + _moveTime)
        {
            _hangman.transform.position = Vector3.Lerp(_picnicPosition, _hangPosition, (Time.time - timer) / _moveTime);
            await Task.Delay(16);
        }
        _rope.SetActive(true);
        _head.connectedBody = _connectionPoint;
        foreach (Rigidbody2D rb in _hangman.GetComponentsInChildren<Rigidbody2D>())
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void Update()
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            char keyName = vKey.ToString().ToCharArray()[0];
            //Debug.Log("Checking " + keyName);
            if (Input.GetKeyDown(vKey) && _allowedinputs.Contains(keyName)&& vKey.ToString().Length<2)
            {
                TryLetter(keyName);
            }
        }
    }

    public bool TryLetter(char letter)
    {
        if (_remainingLetters.Count < 1 || _hangmanCount >= _bodyParts.Length)
            return false;

        Debug.Log("pressed " + letter);
        if (_remainingLetters.Contains(letter))
        {
            while (_remainingLetters.Contains(letter)) // -- in case of multiple instances of same letter
            {
                _remainingLetters.Remove(letter);
            }
            UpdateText();
            UpdateBody();
            if (_remainingLetters.Count == 0)
                Win();
            return true;
        }
        else if (!_letters.Contains(letter))
        {
            _hangmanCount++;
            UpdateText();
            UpdateBody();
            if (_hangmanCount >= _bodyParts.Length)
                Hang();
            return false;
        }
        UpdateText();
        UpdateBody();
        return true; // --> already tried correct letter
    }

    void Win()
    {
        UIText.DisplayText("You Win!");
    }

    private string UpdateText()
    {
        string output = "";
        for (int i = 0; i < _letters.Length; i++)
        {
            if (_remainingLetters.Contains(_letters[i]))
            {
                output += "_";

            }
            else
            {
                output += _letters[i];
            }
            output += " ";
        }
        //Debug.Log(output);
        _textField.text = output;
        return output;
    }
    private void UpdateBody()
    {
        for (int i = 0; i < _bodyParts.Length;i++)
        {
            if (_hangmanCount > i && _bodyParts[i].activeInHierarchy == false)
                _bodyParts[i].SetActive(true);
        }
    }
}
