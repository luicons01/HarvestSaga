using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestArea : MonoBehaviour
{

    // Il diametro dell'area dove l'agente e le piante di grano pososno essere usati per osesrvare la distanza relativa fra agente e grano 
    public const float AreaDiameter = 30f;

    //Un dizionario di lookup per looking up un grano dal suo collider 
    private Dictionary<Collider, Wheat> wheatColliderDictionary;

    /// <summary>
    /// La lista di tutti i wheat nell'area 
    /// </summary>
    public List<Wheat> Wheats { get; private set; }

    /// <summary>
    /// Reset i wheats
    /// </summary>
    public void ResetWheats()
    {
        // Reset ogni wheat
        foreach (Wheat wheat in Wheats)
        {
            wheat.ResetWheat();
        }
    }
  
    // Recupera l'oggetto Wheat associato a un determinato Collider che rappresenta il grano. 
    public Wheat GetWheatFromCollider(Collider collider)
    {
    return wheatColliderDictionary[collider];
    }

    /// <summary>
    /// Chiamato quando la scena inizia
    /// </summary>
    private void Awake()
    {
        // Inizializza lista e dizionario
        wheatColliderDictionary = new Dictionary<Collider, Wheat>();
        Wheats = new List<Wheat>();

        // Trova tutti gli oggetti di grano nella scena
        foreach (Wheat wheat in FindObjectsOfType<Wheat>())
        {
            // Aggiungi il grano alla lista Wheats
            Wheats.Add(wheat);

            // Aggiungi il collider al dizionario di lookup
            Collider wheatCollider = wheat.GetComponent<Collider>();
            if (wheatCollider != null)
            {
                wheatColliderDictionary[wheatCollider] = wheat;
            }
        }
        //DebugHarvestArea();
    }


    private void DebugHarvestArea()
    {
        Debug.Log($"Numero di grani trovati: {Wheats.Count}");
        foreach (var wheat in Wheats)
        {
            Debug.Log($"Grano trovato: {wheat.name}");
        }

        // Debug per verificare il numero di collider registrati
        Debug.Log($"Numero di collider registrati: {wheatColliderDictionary.Count}");
    }




}
