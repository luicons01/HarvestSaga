using System.Collections;
using UnityEngine;

/// <summary>
/// Gestisce la logica ed il controllo della UI
/// </summary>
public class GameManager : MonoBehaviour
{
    [Tooltip("Game ends when an agent collects this much nectar")]
    public int maxWheat = 12;

    [Tooltip("Game ends after this many seconds have elapsed")]
    public float timerAmount = 60f;

    [Tooltip("The UI Controller")]
    public UIController uiController;

    [Tooltip("The player hummingbird")]
    public FarmerAgent agent;

    [Tooltip("The flower area")]
    public HarvestArea harvestArea;

    [Tooltip("The main camera for the scene")]
    public Camera mainCamera;

    public SickleCollision sickleCollision;

    // Quando il timer è partito
    private float gameTimerStartTime;

    /// <summary>
    /// Tutti i possibili stati del Game
    /// </summary>
    public enum GameState
    {
        Default,
        MainMenu,
        Preparing,
        Playing,
        Gameover
    }

    /// <summary>
    /// Lo stato del Game attuale
    /// </summary>
    public GameState State { get; private set; } = GameState.Default;

    /// <summary>
    /// Ottiene il tempo rimanente nel Game
    /// </summary>
    public float TimeRemaining
    {
        get
        {
            if (State == GameState.Playing)
            {
                float timeRemaining = timerAmount - (Time.time - gameTimerStartTime);
                return Mathf.Max(0f, timeRemaining);
            }
            else
            {
                return 0f;
            }
        }
    }

    /// <summary>
    /// Gestisce il premere un pulsante nei vari stati
    /// </summary>
    public void ButtonClicked()
    {
        if (State == GameState.Gameover)
        {
            // In the Gameover state, button click should go to the main menu
            MainMenu();
        }
        else if (State == GameState.MainMenu)
        {
            // In the MainMenu state, button click should start the game
            StartCoroutine(StartGame());
        }
        else
        {
            Debug.LogWarning("Button clicked in unexpected state: " + State.ToString());
        }
    }

    /// <summary>
    /// Chiamata quando inizia il Game
    /// </summary>
    private void Start()
    {
        uiController.OnButtonClicked += ButtonClicked;

        // Avvio del Main Menu
        MainMenu();
    }

    /// <summary>
    /// Chiamata alla distruzione
    /// </summary>
    private void OnDestroy()
    {
        uiController.OnButtonClicked -= ButtonClicked;
    }

    /// <summary>
    /// Mostra il main menu
    /// </summary>
    private void MainMenu()
    {
        // Imposta lo stato a "Main Menu"
        State = GameState.MainMenu;

        // Aggiorna la UI
        uiController.ShowBanner("");
        uiController.ShowButton("Start");

        // Usa la camera principale e dissattiva quella dell'agente
        mainCamera.gameObject.SetActive(true);
        agent.agentCamera.gameObject.SetActive(false);

        // Reset dei wheats
        harvestArea.ResetWheats();

        // Reset dell'Agente
        agent.OnEpisodeBegin();

        // Freeze dell'Agente
        agent.FreezeAgent();
    }

    /// <summary>
    /// Inizia il Game con un countdown
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator StartGame()
    {
        // Imposta lo stato a "preparing"
        State = GameState.Preparing;

        // Aggiorna la UI (la nasconde)
        uiController.ShowBanner("");
        uiController.HideButton();

        // Usa la camera dell'Agente e disattiva la Main Camera
        mainCamera.gameObject.SetActive(false);
        agent.agentCamera.gameObject.SetActive(true);

        // Mostra il countdown
        uiController.ShowBanner("3");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("2");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("1");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("Go!");
        yield return new WaitForSeconds(1f);
        uiController.ShowBanner("");

        // Imposta lo stato a "playing"
        State = GameState.Playing;

        // Avvia il timer
        gameTimerStartTime = Time.time;

        // Unfreeze dell'Agente
        agent.UnfreezeAgent();
    }

    /// <summary>
    /// Termina il Game
    /// </summary>
    private void EndGame()
    {
        State = GameState.Gameover;
        agent.FreezeAgent();

        int harvestedWheat = sickleCollision.GetHarvestedWheatCount();

        if (harvestedWheat >= maxWheat)
        {
            uiController.ShowBanner("Agent wins!");
        }
        else
        {
            uiController.ShowBanner("Time's up!");
        }

        uiController.ShowButton("Main Menu");
    }


    /// <summary>
    /// Chiamata ogni frame
    /// </summary>
    private void Update()
        {
            if (State == GameState.Playing)
            {
                int harvestedWheat = sickleCollision.GetHarvestedWheatCount();

                if (TimeRemaining <= 0f || harvestedWheat >= maxWheat)
                {
                    EndGame();
                }

                uiController.SetTimer(TimeRemaining);
                uiController.SetWheatCount(harvestedWheat);
                uiController.SetWheatCount(harvestedWheat);

            }
            else if (State == GameState.Preparing || State == GameState.Gameover)
            {
                uiController.SetTimer(TimeRemaining);
            }
            else
            {
                uiController.SetTimer(-1f);
                uiController.SetWheatCount(0);
            }
        }
    }

