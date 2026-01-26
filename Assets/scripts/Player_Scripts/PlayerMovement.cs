using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public float Jump;
    public float SprintSpeed = 10f;
    public bool isGrounded = false;
    public Rigidbody rb;

    // Nouveaux paramètres pour franchir un obstacle
    public float obstacleForwardImpulse = 5f;
    public float obstacleUpImpulse = 5f;
    public float obstacleIgnoreCollisionTime = 0.5f;

    private Collider playerCollider;

    [Tooltip("Animator du joueur (laissez vide pour récupération automatique dans les enfants)")]
    [SerializeField] private Animator animator;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject);
        // collision.gameObject
        if (collision.collider.name == "Terrain")
        {
            //Debug.Log("entre");
            isGrounded = true;
        }

        // Si on touche un GameObject ayant le tag "obstacle" et que c'est un BoxCollider
        // L'effet n'est appliqué que si speed est strictement supérieur à 7f
        if (collision.gameObject.CompareTag("Obstacle") && collision.collider is BoxCollider && speed > 7f)
        {
            // Assurer que rb et playerCollider sont présents
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (playerCollider == null) playerCollider = GetComponent<Collider>();

            if (rb != null)
            {
                // Remettre la vélocité verticale à zéro avant d'ajouter l'impulsion (optionnel mais stabilise le saut)
                Vector3 currentVel = rb.linearVelocity;
                currentVel.y = 0f;
                rb.linearVelocity = currentVel;

                // Appliquer impulsion combinée avant + haut
                Vector3 impulse = transform.forward * obstacleForwardImpulse + Vector3.up * obstacleUpImpulse;
                rb.AddForce(impulse, ForceMode.Impulse);
            }

            // Ignorer temporairement la collision pour éviter d'accrocher l'obstacle
            if (playerCollider != null && collision.collider != null)
            {
                Physics.IgnoreCollision(playerCollider, collision.collider, true);
                StartCoroutine(ReenableCollisionAfter(collision.collider, obstacleIgnoreCollisionTime));
            }
        }
    }

    private IEnumerator ReenableCollisionAfter(Collider other, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (other != null && playerCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, other, false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        if (Input.GetAxis("Jump") != 0 && isGrounded) //si appuie sur espace et touche le sol
        {
            Jump = Input.GetAxis("Jump") * jumpForce;
            rb.AddForce(new Vector3(0, Jump, 0), ForceMode.Impulse); //ajoute une force vers le haut
            isGrounded = false;

        }

        bool sprintActive = Input.GetKey(KeyCode.LeftShift);

        if (sprintActive)
        {
            speed = SprintSpeed;
        }
        else
        {
            speed = 7f;
        }

        // Mettre à jour l'Animator : IsWalking = true quand W est pressé et que le sprint n'est pas actif
        if (animator != null) 
        {
            bool isWalking = Input.GetKey(KeyCode.W) && !sprintActive;
            animator.SetBool("IsWalking", isWalking);
        }
        if (animator != null) 
        {
            bool isWalkingB = Input.GetKey(KeyCode.S) && !sprintActive;
            animator.SetBool("IsWalkingB", isWalkingB);
        }
        if (animator != null) 
        {
            bool isWalkingR = Input.GetKey(KeyCode.D) && !sprintActive;
            animator.SetBool("IsWalkingR", isWalkingR);
        }
        if (animator != null) 
        {
            bool isWalkingL = Input.GetKey(KeyCode.A) && !sprintActive;
            animator.SetBool("IsWalkingL", isWalkingL);
        }
        if (animator != null) 
        {
            animator.SetBool("IsSprinting", sprintActive && Input.GetKey(KeyCode.W));
        }
        if (animator != null) 
        {
            animator.SetBool("IsSprintingR", sprintActive && Input.GetKey(KeyCode.D));
        }
        if (animator != null)
        {
            animator.SetBool("isJumping", Input.GetAxis("Jump") != 0);
        }
    }
}

