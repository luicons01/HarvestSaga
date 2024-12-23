using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public class FarmerAgent : Agent
{
    [Tooltip("Force to apply when moving")]
    public float moveForce = 2f;

    [Tooltip("The agent's camera")] 
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")] 
    public bool trainingMode;
   
   // Il rigidbody dell'agente
    new private Rigidbody rigidbody;

    // L'area di mietitura 
    private HarvestArea harvestArea;

    // Il grano più vicino all'agente
    private Wheat nearestWheat;
    
    // Whether the agent is frozen (intentionally not flying)
    private bool frozen = false;

    public SickleCollision sickleCollision;

    public int wheatObtained;

  /*public override void CollectObservations(VectorSensor sensor)
    {
        // Aggiungi il conteggio delle piante raccolte alle osservazioni
        sensor.AddObservation(sickleCollision.GetHarvestedWheatCount());
    }
    */

    /// <summary>
    /// Inizializza l'agente
    /// </summary>
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        harvestArea = GetComponentInParent<HarvestArea>();
        wheatObtained = sickleCollision.GetHarvestedWheatCount();

        // Se non si è in training mode si può giocare per sempre
        if (!trainingMode) MaxStep = 0;
    }


    /// <summary>
    /// Resetta l'agente quando inizia un nuovo Episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // Resetta il grano solo nel training
            harvestArea.ResetWheats();
        }

        // Reset del numero di grano ottenuto
        wheatObtained = 0;

        // Si impostano le velocità a 0 prima che un Episode inizi
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Spawna difronte ad un fascio di grano
        bool inFrontOfWheat = true;
        if (trainingMode)
        {
            // Spawna difronte ad un fascio di grano con una probabilità del 50% durante il training
            inFrontOfWheat = UnityEngine.Random.value > .5f;
        }

        // Muovi l'agente a una posizione random
        MoveToSafeRandomPosition(inFrontOfWheat);

        // Ricalcola il grano più vicino dopo che l'agente si è mosso
        UpdateNearestWheat();
    }


    /// <summary>
    /// Muove l'agente in una posizione sicura
    /// se si trova vicino al grano, punterà verso il grano
    /// </summary>
    private void MoveToSafeRandomPosition(bool inFrontOfWheat)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100; // Evitiamo un loop infinito provando un massimo di 100 volte
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // Cicla finchè non si trova una posizione sicura o sono terminati i tentativi
        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            if (inFrontOfWheat)
            {
                
                // Scegli un grano casuale
                Wheat randomWheat = harvestArea.Wheats[UnityEngine.Random.Range(0, harvestArea.Wheats.Count)];

                // Posiziona 10 o 20cm difronte al grano
                float distanceFromWheat = UnityEngine.Random.Range(.1f, .2f);
                potentialPosition = randomWheat.transform.position + randomWheat.WheatUpVector * distanceFromWheat;

                
                //potentialPosition.y = 0f; RICORDATI DI QUESTA COSA SE L'AVATAR SVOLAZZA, FISSA L'ALTEZZA A QUELLA DEL TERRENO


                // punta l'agent davanti al grano
                Vector3 toWheat = randomWheat.WheatCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toWheat, Vector3.up);
            }
            else
            {
                // Altezza
                float height = 0f;

                // Scegli un raggio casuale partendo dal centro dell'area
                float radius = UnityEngine.Random.Range(2f, 7f);

                // Scegli una direzione casuale che si basa sull'asse delle y
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // Combina i tre elementi precedenti per ottenere la posizione casuale
                potentialPosition = harvestArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

            }

            // Controllo per vedere se l'agent collide con qualcosa
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.10f);

            // Posiione sicura trovata se non c'è sovrapposizione
            safePositionFound = colliders.Length == 0;
        }

        Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

        // Imposta posizione e rotazione
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }


    /// <summary>
    /// Aggiorna il grano più vicino all'agente
    /// </summary>
    private void UpdateNearestWheat()
    {
        foreach (Wheat wheat in harvestArea.Wheats)
        {
            if (nearestWheat == null)
            {
                // se non ci sono grani vicini viene settato questo come grano
                nearestWheat = wheat;
            }
            else
            {
                // Calcola la distanza da questo fiore a quello più vicino
                float distanceToWheat = Vector3.Distance(wheat.transform.position, transform.position);
                float distanceToCurrentNearestWheat = Vector3.Distance(nearestWheat.transform.position, transform.position);

                // se non c'è fiore più vicino o la distanza è minima assegna questo
                if (!nearestWheat || distanceToWheat < distanceToCurrentNearestWheat)
                {
                    nearestWheat = wheat;
                }
            }
        }
    }


    /// <summary>
    /// Viene chiamata quando è assegnata un azione che sia dall'utente o dalla rete neurale
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        if (frozen) return;

        // Calcola il vettore di movimento
        Vector3 move = new Vector3(vectorAction[0], vectorAction[1]);

        // Aggiunge forza in quella direzione per muoversi
        rigidbody.AddForce(move * moveForce);

        // Ricava la rotazione attuale
        Vector3 rotationVector = transform.rotation.eulerAngles;

    }


}
