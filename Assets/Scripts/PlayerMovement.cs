using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator; // Riferimento all'Animator
    public float speed = 5f;  // Velocità di movimento
    public float gravity = -9.8f; // Gravità
    public float rotationSpeed = 0.5f; // Velocità di rotazione
    private CharacterController controller; // Riferimento al CharacterController
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>(); // Ottieni il CharacterController
    }

    void Update()
    {
        // Ottieni i movimenti orizzontali e verticali
        float moveY = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");
        // Calcola il vettore di movimento
        Vector3 move = transform.forward * moveY;
        // Applica il movimento orizzontale
        controller.Move(move * speed * Time.deltaTime);

        // Rotazione del personaggio
        transform.Rotate(0, rotate * rotationSpeed * Time.deltaTime, 0); 
        
        // Gestione della gravità
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Mantieni il personaggio ancorato al terreno
        }

        controller.Move(velocity * Time.deltaTime);

        // Gestione delle animazioni
        animator.SetBool("isWalking", move.magnitude > 0.1f);

        // Animazione attacco (quando premi la barra spaziatrice)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("attack");
        }
    }
}
