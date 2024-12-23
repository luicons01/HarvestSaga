using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleCollision : MonoBehaviour
{

    // Contatore globale per il grano raccolto
    private int totalHarvestedWheat = 0;

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto colpito ha il tag "Grano"
        if (other.CompareTag("Grano"))
        {
            Debug.Log("Il falcetto ha colpito il grano!");

            // Ottieni il riferimento allo script Wheat
            Wheat wheat = other.GetComponent<Wheat>();
            
            if (wheat != null)
            {
                // Chiama la funzione Harvest
                int harvestedAmount = wheat.Harvest();
                totalHarvestedWheat += harvestedAmount; // Aggiorna il contatore
                Debug.Log($"Il falcetto ha colpito il grano!");
                Debug.Log($"Totale grano raccolto: {totalHarvestedWheat}");

                // Reset dopo 5 secondi (puoi cambiare il tempo o usare un altro trigger)
                // StartCoroutine(ResetAfterTime(wheat, 5f));
            }
            else
            {
                Debug.LogError("Lo script Wheat non è presente sull'oggetto con tag 'Grano'!");
            }
        }
    }

    public int GetHarvestedWheatCount()
    {
        return totalHarvestedWheat;
    }


    // Coroutines per il reset (la usiamo come debug)
    private IEnumerator ResetAfterTime(Wheat wheat, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Esegui il reset
        wheat.ResetWheat();
        Debug.Log("Grano e collider sono stati ripristinati!");
    }

}
