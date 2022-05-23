using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using TextSpeech;

[System.Serializable]
public class TicTacToe : MonoBehaviour
{
    public GameObject[] buttonPlaces; // stores button gameobjects
    GameObject currentHit; // stores details of the current hit square
    enum buttonStatus { playerOne, playerTwo, free }; // statuses of the squares
    enum gameStatus { playing, over }; // statuses of the game
    enum winStatus { won, notWon }; // win status
    enum playerTurn { playerOne, playerTwo }; // player turn status
    public AudioClip[] textToSpeech; // array to store game TTS audio
    public AudioSource audioSource; // used to play the audio in game
    public TextMesh winnerText; // displays the win status
    string btnName = ""; // used to detect what action to take on button press
    int noOfTurns = 0; // total number of turns taken
    const string LANG_CODE = "en-GB"; // country code for voice recognition
    playerTurn PlayerTurn; // stores player turn
    gameStatus GameStatus; // stores game status
    winStatus WinStatus; // stores win status
    buttonStatus[] buttonStatusArr = new buttonStatus[9]; // stores button status
    public Material playerOneMaterial;
    public Material playerTwoMaterial;
    public Material defaultMaterial;

    // checks for microphone permissions, if not, asks for it
    void CheckPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }
        
    // Start is called before the first frame update
    void Start()
    {
        PlayerTurn = playerTurn.playerOne;
        GameStatus = gameStatus.playing;
        for (int i = 0; i < buttonStatusArr.Length; i++)
        {
            buttonStatusArr[i] = buttonStatus.free;
        }
        winnerText = GameObject.Find("WinnerText").GetComponent<TextMesh>();
        btnName = "";
        winnerText.text = "";
        SpeechToText.instance.Setting(LANG_CODE);
        SpeechToText.instance.onResultCallback = OnFinalSpeechResult;
        SpeechToText.instance.onPartialResultsCallback = OnPartialSpeechResult;
        CheckPermission();
    }

    // resets the game
    void Reset()
    {
        GameStatus = gameStatus.playing;
        PlayerTurn = playerTurn.playerOne;
        for (int i = 0; i < buttonStatusArr.Length; i++)
        {
            buttonStatusArr[i] = buttonStatus.free;
        }
        for (int i = 0; i < buttonPlaces.Length - 2; i++)
        {
            buttonPlaces[i].GetComponent<Renderer>().material = defaultMaterial;

        }
        noOfTurns = 0;
        winnerText.text = "";
        audioSource.clip = textToSpeech[18];
        audioSource.Play();
    }

    // speech to text function
    void StartListening()
    {
        SpeechToText.instance.StartRecording();
    }

    // speech to text function
    void StopListening()
    {
        SpeechToText.instance.StopRecording();
    }

    // returns speech to text result
    void OnFinalSpeechResult(string result)
    {
        btnName = result;
    }

    // returns speech to text result
    void OnPartialSpeechResult(string result)
    {
        btnName = result;
    }

    // speech to text coroutine, listens for 2 seconds
    private IEnumerator Voice()
    {
        btnName = "";
        Handheld.Vibrate();
        StartListening();
        yield return new WaitForSeconds(2.0f);
        StopListening();
    }

    // detects which button was touched
    void OnButtonTouched()
    {
        if ((Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                btnName = hit.transform.name;
            }
        }
    }

    // upon game button hit or audio, takes the players turn
    void PlayerMove(int squareNumber, string btnName, int pOneAudio, int pTwoAudio)
    {
        if (buttonStatusArr[squareNumber - 1] == buttonStatus.free && GameStatus == gameStatus.playing)
        {
            currentHit = GameObject.Find(btnName);
            noOfTurns++;
            Handheld.Vibrate();
            if (PlayerTurn == playerTurn.playerOne)
            {
                currentHit.GetComponent<Renderer>().material = playerOneMaterial;
                buttonStatusArr[squareNumber - 1] = buttonStatus.playerOne;
                audioSource.clip = textToSpeech[pOneAudio];
                if (WinCheck(squareNumber) == winStatus.won)
                {
                    OnWin();
                }
                else if (DrawCheck())
                {
                    OnDraw();
                }
                else
                {
                    PlayerTurn = playerTurn.playerTwo;
                }
            }
            else
            {
                currentHit.GetComponent<Renderer>().material = playerTwoMaterial;
                buttonStatusArr[squareNumber - 1] = buttonStatus.playerTwo;
                audioSource.clip = textToSpeech[pTwoAudio];
                if (WinCheck(squareNumber) == winStatus.won)
                {
                    OnWin();
                }
                else if (DrawCheck())
                {
                    OnDraw();
                }
                else
                {
                    PlayerTurn = playerTurn.playerOne;
                }
            }
        }
        // if already chosen by player one, tell player
        else if (buttonStatusArr[squareNumber - 1] == buttonStatus.playerOne && GameStatus == gameStatus.playing)
        {
            audioSource.clip = textToSpeech[22];
        }
        // if already chosen by player one, tell player
        else if (buttonStatusArr[squareNumber - 1] == buttonStatus.playerTwo && GameStatus == gameStatus.playing)
        {
            audioSource.clip = textToSpeech[23];
        }

        if (GameStatus == gameStatus.playing)
        {
            audioSource.Play();
        }
    }

    // checks for three in a row
    bool ThreeInARowCheck(int adjaecentButtonOne, int adjaecentButtonTwo)
    {
        if (PlayerTurn == playerTurn.playerOne)
        {
            if (buttonStatusArr[adjaecentButtonOne - 1] == buttonStatus.playerOne && buttonStatusArr[adjaecentButtonTwo - 1] == buttonStatus.playerOne)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (buttonStatusArr[adjaecentButtonOne - 1] == buttonStatus.playerTwo && buttonStatusArr[adjaecentButtonTwo - 1] == buttonStatus.playerTwo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // checks if player has won
    winStatus WinCheck(int lastButtonPressed)
    {
        if (noOfTurns >= 5)
        {
            switch (lastButtonPressed)
            {
                case 1:
                    if (ThreeInARowCheck(2, 3) || ThreeInARowCheck(4, 7) || ThreeInARowCheck(5, 9))
                    {
                        return winStatus.won;
                    }
                    break;
                case 2:
                    if (ThreeInARowCheck(1, 3) || ThreeInARowCheck(5, 8))
                    {
                        return winStatus.won;
                    }
                    break;
                case 3:
                    if (ThreeInARowCheck(1, 2) || ThreeInARowCheck(6, 9) || ThreeInARowCheck(5, 7))
                    {
                        return winStatus.won;
                    }
                    break;
                case 4:
                    if (ThreeInARowCheck(5, 6) || ThreeInARowCheck(1, 7))
                    {
                        return winStatus.won;
                    }
                    break;
                case 5:
                    if (ThreeInARowCheck(2, 8) || ThreeInARowCheck(4, 6) || ThreeInARowCheck(1, 9) || ThreeInARowCheck(3, 7))
                    {
                        return winStatus.won;
                    }
                    break;
                case 6:
                    if (ThreeInARowCheck(3, 9) || ThreeInARowCheck(4, 5))
                    {
                        return winStatus.won;
                    }
                    break;
                case 7:
                    if (ThreeInARowCheck(1, 4) || ThreeInARowCheck(8, 9) || ThreeInARowCheck(3, 5))
                    {
                        return winStatus.won;
                    }
                    break;
                case 8:
                    if (ThreeInARowCheck(7, 9) || ThreeInARowCheck(2, 5))
                    {
                        return winStatus.won;
                    }
                    break;
                case 9:
                    if (ThreeInARowCheck(7, 8) || ThreeInARowCheck(3, 6) || ThreeInARowCheck(1, 5))
                    {
                        return winStatus.won;
                    }
                    break;
            }
        }
        return winStatus.notWon;
    }

    // checks if game is drawn
    bool DrawCheck()
    {
        if (noOfTurns >= 9 && GameStatus == gameStatus.playing)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // run when game is won
    void OnWin()
    {
        if (PlayerTurn == playerTurn.playerOne && GameStatus == gameStatus.playing)
        {
            winnerText.text = "Player 1 Wins!";
            audioSource.clip = textToSpeech[20];
        }
        else if (PlayerTurn == playerTurn.playerTwo && GameStatus == gameStatus.playing)
        {
            winnerText.text = "Player 2 Wins!";
            audioSource.clip = textToSpeech[21];
        }
        audioSource.Play();
        GameStatus = gameStatus.over;
    }

    // won when game is drawn
    void OnDraw()
    {
        winnerText.text = "It's a Draw!";
        audioSource.clip = textToSpeech[19];
        audioSource.Play();
        GameStatus = gameStatus.over;
    }
       
    // game loop, Update is called once per frame
    void Update()
    {
        OnButtonTouched();
        switch (btnName)
        {
            case "1":
                PlayerMove(1, "1", 0, 9);
                btnName = "";
                break;
            case "2":
                PlayerMove(2, "2", 1, 10);
                btnName = "";
                break;
            case "3":
                PlayerMove(3, "3", 2, 11);
                btnName = "";
                break;
            case "4":
                PlayerMove(4, "4", 3, 12);
                btnName = "";
                break;
            case "5":
                PlayerMove(5, "5", 4, 13);
                btnName = "";
                break;
            case "6":
                PlayerMove(6, "6", 5, 14);
                btnName = "";
                break;
            case "7":
                PlayerMove(7, "7", 6, 15);
                btnName = "";
                break;
            case "8":
                PlayerMove(8, "8", 7, 16);
                btnName = "";
                break;
            case "9":
                PlayerMove(9, "9", 8, 17);
                btnName = "";
                break;
            case "Reset":
                Reset();
                btnName = "";
                break;
            case "reset":
                Reset();
                btnName = "";
                break;
            case "Voice":
                StartCoroutine(Voice());
                break;
            default:
                break;
        }
    }
}