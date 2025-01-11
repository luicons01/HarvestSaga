using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlla una semplice UI
/// </summary>
public class UIController : MonoBehaviour
{
    [Tooltip("The wheat count text")]
    public TextMeshProUGUI wheatCountText;

    [Tooltip("The timer text")]
    public TextMeshProUGUI timerText;

    [Tooltip("The banner text")]
    public TextMeshProUGUI bannerText;

    [Tooltip("The button")]
    public Button button;

    [Tooltip("The button text")]
    public TextMeshProUGUI buttonText;

    /// <summary>
    /// Delegate per il click di un pulsante
    /// </summary>
    public delegate void ButtonClick();

    /// <summary>
    /// Chiamato quando un pulsante è stato cliccato
    /// </summary>
    public ButtonClick OnButtonClicked;

    /// <summary>
    /// Risponde ai click di un pulsante
    /// </summary>
    public void ButtonClicked()
    {
        if (OnButtonClicked != null) OnButtonClicked();
    }

    /// <summary>
    /// Mostra il pulsante
    /// </summary>
    /// <param name="text">la stringa sul pulsante</param>
    public void ShowButton(string text)
    {
        buttonText.text = text;
        button.gameObject.SetActive(true);
    }

    /// <summary>
    /// Nasconde il pulsante
    /// </summary>
    public void HideButton()
    {
        button.gameObject.SetActive(false);
    }

    /// <summary>
    /// Mostra il banner text
    /// </summary>
    /// <param name="text">la stringa da mostrare</param>
    public void ShowBanner(string text)
    {
        bannerText.text = text;
        bannerText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Nasconde il banner text
    /// </summary>
    public void HideBanner()
    {
        bannerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Imposta il timer , se timeRemaining è negativo, nasconde il testo
    /// </summary>
    /// <param name="timeRemaining">il tempo rimanente in secondi</param>
    public void SetTimer(float timeRemaining)
    {
        if (timeRemaining > 0f)
            timerText.text = timeRemaining.ToString("00");
        else
            timerText.text = "";
    }

    /// <summary>
    /// Imposta la quantità di grano raccolto
    /// </summary>
    /// <param name="nectarAmount">Una quantità da 0 a 12</param>
    public void SetWheatCount(int wheatAmount)
    {
        wheatCountText.text = "Wheat: " + wheatAmount;
    }
}
