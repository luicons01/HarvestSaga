using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FarmerAgent : Agent
{
    [Tooltip("Force to apply when moving")]
    public float moveForce = 1f;

    [Tooltip("The agent's camera")] 
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")] 
    public bool trainingMode;

    private CharacterController characterController;

    // L'area di mietitura 
    private HarvestArea harvestArea;
    
    // Massima distanza tra l'agente e un collider
    private const float AgentRadius = 0.1f;

    /// <summary>
    /// La quantità di grano raccolta
    /// </summary>
    public int wheatCollected = 0;

    // Il grano più vicino all'agente
    private Wheat nearestWheat;
    
    // Whether the agent is frozen (intentionally not flying)
    private bool frozen = false;

    public SickleCollision sickleCollision;

    public int wheatObtained;

     public Animator animator; // Riferimento all'Animator

    private Vector3 velocity; 

    private Vector3 currentMovement; 

    public float speed = 5f;  // Velocità di movimento

    public float gravity = -9.8f; // Gravità
 
    public float yawSpeed = 100f; // Velocità di rotazione

    /// <summary>
    /// Inizializza l'agente
    /// </summary>
    public override void Initialize()
    {
        // Ottieni il riferimento al CharacterController
        characterController = GetComponent<CharacterController>();
        harvestArea = GetComponentInParent<HarvestArea>();
        wheatObtained = sickleCollision.GetHarvestedWheatCount();

        // Se non si è in training mode si può giocare per sempre
        if (!trainingMode) MaxStep = 0;

        Debug.Log($"Training Mode: {trainingMode}");

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
        velocity = Vector3.zero; // Resetta la velocità verticale
        currentMovement = Vector3.zero; // Resetta qualsiasi vettore di movimento utilizzato

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

                // Calcola il vettore verso il grano, ignorando l'altezza (asse Y)
                Vector3 toWheat = randomWheat.WheatCenterPosition - potentialPosition;
                toWheat.y = 0; // Ignora la componente verticale

                // Crea una rotazione che guarda il grano sul piano orizzontale
                potentialRotation = Quaternion.LookRotation(toWheat.normalized, Vector3.up);
            }
            else
            {
                // Altezza
                float height = 0.2f;

                // Scegli un raggio casuale partendo dal centro dell'area
                float radius = UnityEngine.Random.Range(2f, 7f);

                // Scegli una direzione casuale che si basa sull'asse delle y
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // Combina i tre elementi precedenti per ottenere la posizione casuale
                potentialPosition = harvestArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;
            }

            // Controllo per vedere se l'agent collide con qualcosa
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.10f);

            // Posizione sicura trovata se non c'è sovrapposizione
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
    /// continuousActions[i] rappresentano:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// 
    /// discreteActions rappreseta l'azione di mietitura
    /// </summary>
    /// <param name="actions">L'azione da fare</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Ottieni le azioni continue
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;
        
        // Calcola il vettore di movimento in avanti rispetto alla rotazione attuale
        Vector3 move = transform.forward * continuousActions[1];
        // Applica il movimento al CharacterController
        characterController.Move(move * speed * Time.deltaTime);
        
        // Ottieni la rotazione attuale
        Vector3 rotationVector = transform.rotation.eulerAngles;
        float rotazione = continuousActions[0];

        float yaw = rotationVector.y + rotazione * Time.fixedDeltaTime * yawSpeed;
        // Applica la nuova rotazione
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Gestione della gravità
        if (!characterController.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Mantiene il personaggio ancorato al terreno
        }

        characterController.Move(velocity * Time.deltaTime);

        if (actions.DiscreteActions[0] == 1)
        {
            TriggerHarvest();
        }
    }

    //metodo che attiva l'animazione e notifica il falcetto per gestire la mietitura
    private void TriggerHarvest()
    {
        // Attiva l'animazione tramite il riferimento
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
    }


    /// <summary>
    /// Raccogliere osservazioni vettoriali dall'ambiente
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Se nearestWheat è nullo, osserva un array vuoto e ritorna in anticipo
        if (nearestWheat == null)
        {
            sensor.AddObservation(new float[9]);
            return;
        }
        
        // Osserva la rotazione locale dell'agente (4 osservazioni)
        sensor.AddObservation(transform.localRotation.normalized);

        // Ottieni un vettore dall'agente al grano più vicino
        Vector3 toWheat = nearestWheat.WheatCenterPosition - transform.position;
 
        // Osserva un vettore normalizzato che punta al grano più vicino (3 osservazioni)
        sensor.AddObservation(toWheat.normalized);

        // Osserva un prodotto scalare che indica se l'agente è rivolto verso il grano (1 osservazione)
        // (+1 significa che l'agente punta direttamente al grano, -1 significa direttamente lontano)
        sensor.AddObservation(Vector3.Dot(transform.forward.normalized, -nearestWheat.WheatUpVector.normalized));

        // Osservare la distanza relativa dall'agente al grano (1 osservazione)
        sensor.AddObservation(toWheat.magnitude / HarvestArea.AreaDiameter);
    }

/// <summary>
    /// When Behavior Type is set to "Heuristic Only" on the agent's Behavior Parameters,
    /// this function will be called. Its return values will be fed into
    /// <see cref="OnActionReceived(ActionBuffers)"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">The output action buffer</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Debug.Log("Heuristic method called");
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // Movimento
        continuousActions[0] = Input.GetAxis("Horizontal"); // Rotazione su Y
        continuousActions[1] = Input.GetAxis("Vertical");   // Movimento su Z

        //Debug.Log($"Heuristic Continuous Actions: {continuousActions[0]}, {continuousActions[1]}");
       
        // Mietitura (premi Spazio)
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    /// <summary>
    /// Evita che l'agente si muova o compia azioni
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze non sono supportati nel training");
        frozen = true;
        
        // Ferma il movimento dell'agente
        velocity = Vector3.zero;
        currentMovement = Vector3.zero;
    }

    /// <summary>
    /// Riprende il movimento dell'agente
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze sono sono supportati nel training");
        frozen = false;
    }

    /// <summary>
    /// Chiamata quando il collider dell'agente collide con un altro collider
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterOrStay(other);
    }

    /// <summary>
    /// Chiamata quando il collider dell'agente si trova in un trigger collider
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        TriggerEnterOrStay(other);
    }

    /// <summary>
    /// Gestisce le due funzioni scritte appena sopra 
    /// </summary>
    private void TriggerEnterOrStay(Collider collider)
    {
        // Check if agent is colliding with nectar
        if (collider.CompareTag("Grano"))
        {
            Vector3 closestPointToAgent = collider.ClosestPoint(transform.position);

            // Check if the closest collision point is close to the beak tip
            // Note: a collision with anything but the beak tip should not count
            if (Vector3.Distance(transform.position, closestPointToAgent) < AgentRadius)
            {
                // Look up the flower for this nectar collider
                Wheat wheat = harvestArea.GetWheatFromCollider(collider);

                // Ottieni l'istanza di SickleCollision
                SickleCollision sickle = GetComponentInChildren<SickleCollision>();

                // Tieni traccia del grano raccolto    
                Debug.Log("WheatCollected è:" + sickle.GetHarvestedWheatCount());

                if (trainingMode)
                {
                    // Calculate reward for getting nectar
                    //float bonus = .02f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestWheat.WheatUpVector.normalized));
                    AddReward(.01f);
                }

                // If flower is empty, update the nearest flower PER SICUREZZA TENERE D'OCCHIO DURANTE IL TRAINIG DIO CAAAAAAAAAAAAA
                if (wheat == null)
                {
                    UpdateNearestWheat();
                }
            }
        }
    }


    /// <summary>
    /// Chiamata quando l'agente collide con un oggetto solido
    /// </summary>
    /// <param name="collision">Le informazioni sulla collisione</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("Boundary"))
        {
            // Collisione con un oggetto Boundary, dai ricompensa negativa
            AddReward(-.5f);
        }
    }

    /// <summary>
    /// Chiamata ad ogni frame
    /// </summary>
    private void Update()
    {
        // Mostra una linea dall'agente al grano più vicino
        if (nearestWheat != null)
            Debug.DrawLine(transform.position, nearestWheat.WheatCenterPosition, Color.green);
    }


/*  PER IL VIDEOGIOCO
    private void FixedUpdate()
    {
        // Avoids scenario where nearest flower nectar is stolen by opponent and not updated
        if (nearestFlower != null && !nearestFlower.HasNectar)
            UpdateNearestFlower();
    }
*/



}
