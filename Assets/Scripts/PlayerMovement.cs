using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // Velocità di movimento
    public float rotationSpeed = 200f; // Velocità di rotazione
    public float gravity = -9.81f; // Gravità
    public CharacterController controller;

    private Vector3 velocity;

    void Start()
    {
        // Trova automaticamente il CharacterController se non è assegnato
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        // Input per il movimento avanti/indietro e laterale
        float moveForward = Input.GetAxis("Vertical"); // W/S o frecce su/giù
        float moveSide = Input.GetAxis("Horizontal"); // A/D o frecce sinistra/destra

        // Input per la rotazione sinistra/destra (usa l'asse orizzontale)
        float rotate = Input.GetAxis("Horizontal");

        // Movimento del personaggio avanti/indietro
        Vector3 move = transform.forward * moveForward;
        controller.Move(move * speed * Time.deltaTime);

        // Rotazione del personaggio
        transform.Rotate(0, rotate * rotationSpeed * Time.deltaTime, 0);

        // Applicazione della gravità
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Resetta la velocità verticale se a terra
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
