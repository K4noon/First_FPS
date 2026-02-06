using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    public float SpeedMax = 14f;
    public float jumpForce = 7f;
    public float Jump;
    
    public bool isGrounded = false;
    public Rigidbody rb;

    // Nouveaux paramètres pour franchir un obstacle
    public float obstacleForwardImpulse = 5f;
    public float obstacleUpImpulse = 5f;
    public float obstacleIgnoreCollisionTime = 0.5f;

    private Collider playerCollider;

    [Tooltip("Animator du joueur (laissez vide pour récupération automatique dans les enfants)")]
    [SerializeField] private Animator animator;
    

    // Paramètres pour la transition de vitesse sprint/marche
    public float walkSpeed = 3.5f;
    public float SprintSpeed = 10f;

    [Header("Sprint transition")]
    [Tooltip("Durée pour atteindre la vitesse de sprint (secondes)")]
    [SerializeField] private float sprintAccelerationTime = 0.7f; // durée pour monter jusqu'à SprintSpeed (inchangée)
    [Tooltip("Durée pour redescendre vers walkSpeed (secondes) — plus lente que l'accélération")]
    [SerializeField] private float sprintDecelerationTime = 1.4f; // durée pour redescendre vers walkSpeed (plus lente)
    

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

        // S'assurer que la vitesse initiale correspond à la vitesse de marche
        speed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Récupération des entrées WASD (compatible avec GetKey comme avant)
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

        float vertical = 0f;
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;

        Vector3 input = new Vector3(horizontal, 0f, vertical);

        // Empêcher l'accumulation de vitesse en diagonale :
        // si deux touches sont pressées, normaliser le vecteur d'entrée pour que sa magnitude ne dépasse pas 1.
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        // Déplacement basé sur le vecteur d'entrée (local space) et la vitesse courante
        // On applique la translation locale pour conserver l'orientation du joueur
        transform.Translate(input * speed * Time.deltaTime, Space.Self);

        if (Input.GetAxis("Jump") != 0 && isGrounded) //si appuie sur espace et touche le sol
        {
            Jump = Input.GetAxis("Jump") * jumpForce;
            rb.AddForce(new Vector3(0, Jump, 0), ForceMode.Impulse); //ajoute une force vers le haut
            isGrounded = false;
        }

        bool sprintActive = Input.GetKey(KeyCode.LeftShift);

        // Ne pas augmenter la vitesse si le joueur est en l'air
        float targetSpeed;

        if (sprintActive && isGrounded)
        {
            targetSpeed = SprintSpeed;
        }
        else if (!sprintActive && isGrounded)
        {
            targetSpeed = walkSpeed;
        }
        else
        {
            targetSpeed = 0f; 
        }

            // Transition progressive de la vitesse avec taux asymétrique
            float maxSpeedDiff = Mathf.Abs(SprintSpeed - walkSpeed);

        if (maxSpeedDiff <= 0.0001f)
        {
            speed = targetSpeed;
        }
        else
        {
            // Choisir le temps selon qu'on accélère ou décélère
            bool accelerating = targetSpeed > speed;
            float timeToUse = accelerating ? sprintAccelerationTime : sprintDecelerationTime;

            if (timeToUse <= 0f)
            {
                speed = targetSpeed;
            }
            else
            {
                float speedChangeRate = maxSpeedDiff / timeToUse; // unité : unités de speed par seconde
                speed = Mathf.MoveTowards(speed, targetSpeed, speedChangeRate * Time.deltaTime);
            }
        }

        // Mettre à jour l'Animator : conserver les bools existants mais basés sur les touches
        if (animator != null) 
        {
            bool isWalking = Input.GetKey(KeyCode.W) && !sprintActive;
            animator.SetBool("IsWalking", isWalking);

            bool isWalkingB = Input.GetKey(KeyCode.S) && !sprintActive;
            animator.SetBool("IsWalkingB", isWalkingB);

            bool isWalkingR = Input.GetKey(KeyCode.D) && !sprintActive;
            animator.SetBool("IsWalkingR", isWalkingR);

            bool isWalkingL = Input.GetKey(KeyCode.A) && !sprintActive;
            animator.SetBool("IsWalkingL", isWalkingL);

            // IsSprinting true si LeftShift est maintenu et que le joueur avance vers l'avant (W).
            animator.SetBool("IsSprinting", sprintActive && Input.GetKey(KeyCode.W));

            animator.SetBool("IsJumping", Input.GetAxis("Jump") != 0);
        }
    }
}

