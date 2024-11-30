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
            // Puoi aggiungere altro comportamento qui se necessario
        }
    }
}
