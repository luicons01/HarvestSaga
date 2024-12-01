using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickleCollision : MonoBehaviour
{
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
                Debug.Log($"Il falcetto ha colpito il grano! Raccolto: {harvestedAmount}");
            }
            else
            {
                Debug.LogError("Lo script Wheat non è presente sull'oggetto con tag 'Grano'!");
            }
        }
    }
}
