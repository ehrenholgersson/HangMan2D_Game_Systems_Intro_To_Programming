using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;   
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public struct SaveState
{
    char[] _remainingLetters;
    int _hangmanCount;
    char[] _triedLetters;
    char[] _letters;

    public List<char> RemainingLetters { get => _remainingLetters.ToList(); }

    public List<char> TriedLetters { get => _triedLetters.ToList(); }
    public int HangmanCount { get => _hangmanCount; }
    public char[] Letters { get => _letters; }


    public SaveState(List<char> remainingLetters,List<char> triedLetters, int hangmanCount, char[] letters)
    {
        _remainingLetters = remainingLetters.ToArray();
        _hangmanCount = hangmanCount;
        _triedLetters = triedLetters.ToArray();
        _letters = letters;
    }
}

public class Hangman : MonoBehaviour
{
    [Header("Game Values")]

    [SerializeField] GameObject _hangman;
    [SerializeField] GameObject _hanglady;
    [SerializeField] GameObject _wine;
    [SerializeField] GameObject _rope;
    [SerializeField] Vector3 _startPosition;
    [SerializeField] Vector3 _hangPosition;
    [SerializeField] float _moveTime;

    [SerializeField] List<string> _availableWords = new List<string>();

    [Header("UI/Menus")]

    [SerializeField] GameObject[] _menus;
    [SerializeField] TextMeshProUGUI _wordText;
    [SerializeField] TextMeshProUGUI _resolutionText;
    [SerializeField] TextMeshProUGUI _fullScreenText;
    [SerializeField] Button _continueButton;
    [SerializeField] GameObject _letterButtons;

    [Header("Settings")]

    [SerializeField] Vector2[] _availableResolutions;
    string _savePath;

     enum Menus {Main,Options,Game, Pause }

     Menus _uiMode;

    HingeJoint2D _head;
    Rigidbody2D _connectionPoint;
    public static Hangman GameController;

    GameObject[] _bodyParts = new GameObject[6];
    char[] _letters;
    List<char> _triedLetters = new List<char>();
    List <char> _remainingLetters = new List<char>();
    int _hangmanCount = 0;
    char[] _allowedinputs = new char[] { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z' };



    private void Start()
    {
        if (GameController == null)
            GameController = this;
        else Destroy(this);

        Application.targetFrameRate = 60;

        _savePath = Application.persistentDataPath +"/";

        SetResolution();

        _hangman.transform.position = _startPosition;

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
            if (i < _bodyParts.Length && t.gameObject != _hangman && t.parent.gameObject == _hangman)
            {
                _bodyParts[i] = t.gameObject;
                t.gameObject.SetActive(false);
                i++;
            }
        }
        ChangeUI(Menus.Main);
    }

    void SetResolution()
    {
        bool fullScreen;
        if (PlayerPrefs.GetInt("FullScreen", 2) > 0)
            fullScreen = true;
        else
            fullScreen = false;

        Screen.SetResolution(PlayerPrefs.GetInt("ResolutionX", 1920), PlayerPrefs.GetInt("ResolutionY", 1080), fullScreen);
    }

    public void Quit()
    {
        if (_uiMode == Menus.Pause)
        {
            SaveGame();
        }

        Application.Quit();
    }

    public async void Hang()
    {
        Vector3 _picnicPosition = _hangman.transform.position;
        float timer = Time.time;
        UIText.DisplayText("Oh No!");
        _wine.transform.parent = _hangman.transform;
        _wine.transform.rotation = Quaternion.identity;
        _wine.transform.localPosition = new Vector3(0.888880551f, -0.440991908f, 0);
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
        StartCoroutine("EndGame");
    }

    private void Update()
    {
        foreach (KeyCode vKey in Enum.GetValues(typeof(KeyCode)))
        {
            char keyName = vKey.ToString().ToCharArray()[0];
            //Debug.Log("Checking " + keyName);
            if (Input.GetKeyDown(vKey) && _allowedinputs.Contains(keyName)&& vKey.ToString().Length<2)
            {
                PressButton(keyName);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)&& _uiMode == Menus.Game)
        {
            ChangeUI(Menus.Pause);
        }
        if (_uiMode == Menus.Options)
        {
            UpdateOptions();
        }
    }

    public bool TryLetter(char letter)
    {
        if (_remainingLetters.Count < 1 || _hangmanCount >= _bodyParts.Length)
            return false;

        Debug.Log("pressed " + letter);
        if (_remainingLetters.Contains(letter))
        {
            _triedLetters.Add(letter);
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
        else if (!_letters.Contains(letter)&&!_triedLetters.Contains(letter))
        {
            _triedLetters.Add(letter);
            _hangmanCount++;
            UpdateText();
            UpdateBody();
            if (_hangmanCount >= _bodyParts.Length)
                Hang();
            return false;
        }
        UpdateText();
        UpdateBody();
        return _letters.Contains(letter); // --> already tried letter
    }
    
    void SaveGame()
    {
        if (File.Exists(_savePath+"Hangman.save"))
        {
            File.Delete(_savePath + "Hangman.save");
        }
        if (_hangmanCount >= _bodyParts.Length || _remainingLetters.Count < 1)
        {
            return;
        }

        FileStream stream = new FileStream(_savePath + "Hangman.save", FileMode.Create);
        BinaryFormatter converter = new BinaryFormatter();
        converter.Serialize(stream, new SaveState(_remainingLetters,_triedLetters, _hangmanCount,_letters));
        stream.Close();
        Debug.Log("saved to " + _savePath + "Hangman.save");
    }
    bool LoadGame()
    {
        
        if (File.Exists(_savePath + "Hangman.save"))
        {
            SaveState Load = new SaveState();
            FileStream stream = new FileStream(_savePath + "Hangman.save", FileMode.Open);
            BinaryFormatter converter = new BinaryFormatter();
            Load = (SaveState)converter.Deserialize(stream);
            stream.Close();
            if (Load.HangmanCount >= _bodyParts.Length || Load.RemainingLetters.Count < 1) // if game was quit between win/loose condition and return to menu then the save is not valid
            {
                File.Delete(_savePath + "Hangman.save");
                SceneManager.LoadScene(0);
            }
            _remainingLetters = Load.RemainingLetters;
            _hangmanCount = Load.HangmanCount;
            _letters = Load.Letters;
            _triedLetters = Load.TriedLetters;
            Debug.Log("Loaded "+ _savePath + "Hangman.save");
            return true;
        }
        Debug.Log("Failed to load save");
        return false;
    }

    public void QuitToMenu()
    {
        SaveGame();
        SceneManager.LoadScene(0);
    }


    IEnumerator EndGame()
    {

        yield return new WaitForSeconds(6);

        if (File.Exists(_savePath + "Hangman.save"))
        {
            File.Delete(_savePath + "Hangman.save");
        }

        SceneManager.LoadScene(0);
    }

    void Win()
    {
        _hanglady.SetActive(true);
        foreach (GameObject gO in _bodyParts)
        {
            gO.SetActive(true);
        }
        UIText.DisplayText("You Win!");
        StartCoroutine("EndGame");
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
        _wordText.text = output;
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

    void UpdateButtons() // press any button that corresponds with a letter we have already tried (will trigger the button to disable and switch color but game should ignore any letters we have already tried)
    {
        foreach(Button button in _letterButtons.GetComponentsInChildren<Button>())
        {
            Debug.Log("Checking Button: "+button);
            TextMeshProUGUI txt = button.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                if (_triedLetters.Contains(txt.text.ToCharArray()[0]))
                {
                    button.gameObject.GetComponent<LetterButton>().ButtonPressed();
                }
            }
        }
    }
    void PressButton(char letter) // press a specific button
    {
        foreach (Button button in _letterButtons.GetComponentsInChildren<Button>())
        {
            Debug.Log("Checking Button: " + button);
            TextMeshProUGUI txt = button.GetComponentInChildren<TextMeshProUGUI>();
            if (txt!=null)
            {
                if (letter == txt.text.ToCharArray()[0])
                {
                    Debug.Log("Found Button");
                    button.gameObject.GetComponent<LetterButton>().ButtonPressed();
                    return;
                }
                else
                    Debug.Log(letter + " does not match "+ txt.text.ToCharArray()[0]);
            }
        }
    }

    #region MenuStuff
    void ChangeUI(Menus newMode)
    {
        if (_uiMode == Menus.Pause) // save game if leaving pause menu
        {
            SaveGame();
        }
        if (_uiMode == Menus.Options) // save prefs if leaving options menu
        {
            PlayerPrefs.Save();
        }
        _uiMode = newMode;
        for (int i = 0; i < Enum.GetNames(typeof(Menus)).Length && i < _menus.Length; i++)
        {
            if (i == (int)newMode)
                _menus[i].SetActive(true);
            else
                _menus[i].SetActive(false);
        }
        if (newMode == Menus.Main) // if entering main menu enable/disable continue button depending on existance of save 
        {
            if (File.Exists(_savePath + "Hangman.save"))
                _continueButton.gameObject.SetActive(true);
            else
                _continueButton.gameObject.SetActive(false);
            ReSetHangMan();
        }
    }

    public void ChangeUI(String modeString)
    {
        Menus newMode = new Menus();
        if (!Enum.TryParse<Menus>(modeString, true, out newMode))
            return;

        ChangeUI(newMode);
    }

    public void ReSetHangMan()
    {
        _hangman.transform.position = _startPosition;
        _hanglady.SetActive(false);
        _wine.transform.parent = null;
        _wine.transform.position = new Vector3(861.34f, 21.774f, 151.949f);
        _wine.transform.localRotation = Quaternion.Euler(0, 0, 343.93f);

        _rope.SetActive(false);
        foreach (Rigidbody2D rb in _hangman.GetComponentsInChildren<Rigidbody2D>(true))
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        int i = 0;
        foreach (Transform t in _hangman.GetComponentsInChildren<Transform>(true))
        {
            //Debug.Log("found Image" + i);
            if (i < _bodyParts.Length && t.gameObject != _hangman && t.parent.gameObject == _hangman)
            {
                _bodyParts[i] = t.gameObject;
                t.gameObject.SetActive(false);
                i++;
            }
        }
    }

    public void NewGame()
    {
        _letters = _availableWords[UnityEngine.Random.Range(0, _availableWords.Count)].ToArray();
        _remainingLetters = _letters.ToList();
        UpdateText();
        ReSetHangMan();
        ChangeUI(Menus.Game);
    }

    public void ContinueGame()
    {
        //NewGame();
        ReSetHangMan();
        ChangeUI(Menus.Game);
        LoadGame(); 
        UpdateText();// update  
        UpdateBody();// all the
        UpdateButtons();// things
    }
    void UpdateOptions()
    {
        if ( _availableResolutions.Contains(new Vector2 (PlayerPrefs.GetInt("ResolutionX",0), PlayerPrefs.GetInt("ResolutionY", 0))))
        {
            _resolutionText.text = "" + PlayerPrefs.GetInt("ResolutionX", 1920) + " X " + PlayerPrefs.GetInt("ResolutionY", 1080);
        }
        else
        {
            PlayerPrefs.SetInt("ResolutionX", (int)_availableResolutions[0].x);
            PlayerPrefs.SetInt("ResolutionX", (int)_availableResolutions[0].y);
            _resolutionText.text = "" + PlayerPrefs.GetInt("ResolutionX", 1920) + " X " + PlayerPrefs.GetInt("ResolutionY", 1080);
            SetResolution();
        }
        if (PlayerPrefs.GetInt("FullScreen",2)<2)
        {
            if (PlayerPrefs.GetInt("FullScreen", 2) < 1)
                _fullScreenText.text = "Windowed";
            else
                _fullScreenText.text = "FullScreen";

        }
        else
        {
            PlayerPrefs.SetInt("FullScreen", 1);
            SetResolution();
        }
    }

    public void NextResolution()
    {
        Vector2 currentResolution = new Vector2(PlayerPrefs.GetInt("ResolutionX", 0), PlayerPrefs.GetInt("ResolutionY", 0));
        if (_availableResolutions.Contains(currentResolution))
        {
            currentResolution = _availableResolutions[(Array.IndexOf(_availableResolutions,currentResolution)+1)%_availableResolutions.Length];
        }
        else
        {
            currentResolution = _availableResolutions[0];
        }

        PlayerPrefs.SetInt("ResolutionX", (int)currentResolution.x);
        PlayerPrefs.SetInt("ResolutionY", (int)currentResolution.y);
        _resolutionText.text = "" + PlayerPrefs.GetInt("ResolutionX", 1920) + " X " + PlayerPrefs.GetInt("ResolutionY", 1080);
        SetResolution();
    }
    public void ToggleFullScreen()
    {
        if (PlayerPrefs.GetInt("FullScreen", 2) < 1)
        {
            PlayerPrefs.SetInt("FullScreen", 1);
            _fullScreenText.text = "FullScreen";
        }
        else
        {
            PlayerPrefs.SetInt("FullScreen", 0);
            _fullScreenText.text = "Windowed";
        }
        SetResolution();
    }
    #endregion
}
