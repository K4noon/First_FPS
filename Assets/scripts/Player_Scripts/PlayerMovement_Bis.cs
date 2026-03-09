using UnityEngine;
using System.Collections;

public class PlayerMovement_Bis : MonoBehaviour
{

    public float curSpeed = 0f;
    float nAcceleration = 10f;
    float speedMaxWalk = 3.5f;
    public float speedMaxSprint = 10f;
    bool bMoving = false;
    float speed = 3.5f;
    public float Jump = 7f;
    public float jumpForce = 7f;
    Rigidbody rb;
    public bool isGrounded;

    private Collider playerCollider;

    public float obstacleForwardImpulse = 5f;
    public float obstacleUpImpulse = 5f;
    public float obstacleIgnoreCollisionTime = 0.5f;

    // Durée pour réduire la vitesse de sprint vers la vitesse de marche lors d'un saut
    public float jumpSprintReductionDuration = 0.7f;
    private bool reducingSprintOnJump = false;
    private float jumpSprintTimer = 0f;
    private float jumpSprintStartSpeed = 0f;



    [Tooltip("Animator du joueur (laissez vide pour récupération automatique dans les enfants)")]
    [SerializeField] private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Assurer la référence au collider du joueur (attendu : CapsuleCollider)
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        rb = GetComponent<Rigidbody>();
        if (Input.GetAxis("Jump") != 0 && isGrounded) //si appuie sur espace et touche le sol
        {
            Jump = Input.GetAxis("Jump") * jumpForce;
            if (rb != null)
            {
                rb.AddForce(Vector3.up * Jump, ForceMode.Impulse);
            }

            // Si le joueur saute alors qu'il est en sprint (LeftShift enfoncé) et que sa vitesse est supérieure à la marche,
            // on démarre la réduction progressive de la vitesse pour atteindre speedMaxWalk en jumpSprintReductionDuration secondes.
            if (Input.GetKey(KeyCode.LeftShift) && curSpeed > speedMaxWalk)
            {
                reducingSprintOnJump = true;
                jumpSprintTimer = 0f;
                jumpSprintStartSpeed = curSpeed;
            }

            isGrounded = false;
        }
        bMoving = false;
        // Récupération des entrées WASD (compatible avec GetKey comme avant)
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.D))
        {
            horizontal += 1f;
            bMoving = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            horizontal -= 1f;
            bMoving = true;
        }

        float vertical = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            vertical += 1f;
            bMoving = true;
        }

        if (Input.GetKey(KeyCode.S))
        {
            vertical -= 1f;
            bMoving = true;
        }

        Vector3 input = new Vector3(horizontal, 0f, vertical);

        bool sprintActive = Input.GetKey(KeyCode.LeftShift);

        if (bMoving)
        {
            if (curSpeed <= speedMaxWalk) curSpeed += 1 * Time.deltaTime * nAcceleration;

            if (sprintActive && curSpeed <= speedMaxSprint) curSpeed += 1 * Time.deltaTime * nAcceleration;
            else if (!sprintActive && curSpeed > speedMaxWalk) curSpeed -= 1 * Time.deltaTime * nAcceleration;

        }
        else
        {
            if (curSpeed >= 0f) curSpeed -= 1 * Time.deltaTime * nAcceleration;
        }

        // Si la réduction de sprint à cause du saut est active et que le joueur est en l'air, on applique la réduction progressive.
        if (reducingSprintOnJump)
        {
            if (!isGrounded)
            {
                jumpSprintTimer += Time.deltaTime;
                float t = Mathf.Clamp01(jumpSprintTimer / jumpSprintReductionDuration);
                curSpeed = Mathf.Lerp(jumpSprintStartSpeed, speedMaxWalk, t);
                if (t >= 1f)
                {
                    reducingSprintOnJump = false;
                }
            }
            else
            {
                // Si le joueur a atterri, on stoppe la réduction
                reducingSprintOnJump = false;
            }
        }

        transform.Translate(input * curSpeed * Time.deltaTime, Space.Self);

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
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject);
        // collision.gameObject

        // Assurer que playerCollider est défini
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider>();
        }

        // Vérifier si le contact implique le CapsuleCollider du joueur
        bool contactUsesPlayerCapsule = false;
        foreach (var cp in collision.contacts)
        {
            if (cp.thisCollider is CapsuleCollider)
            {
                contactUsesPlayerCapsule = true;
                break;
            }
        }

        // Si la collision est avec un BoxCollider et que l'objet a le tag "Obstacle" ou "Isfloor",
        // et que le contact provient du CapsuleCollider du joueur, on considère que le joueur est au sol.
        if (contactUsesPlayerCapsule && collision.collider is BoxCollider &&
            (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("IsFloor")))
        {
            isGrounded = true;
        }

        if (collision.collider.name == "Terrain")
        {
            //Debug.Log("entre");
            isGrounded = true;
        }
        // Si on touche un GameObject ayant le tag "obstacle" et que c'est un BoxCollider
        // L'effet n'est appliqué que si speed est strictement supérieur à 7f
        if (collision.gameObject.CompareTag("Obstacle") && collision.collider is BoxCollider && curSpeed > 7f)
        {
            // Assurer que rb et playerCollider sont présents
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (playerCollider == null) playerCollider = GetComponent<Collider>();



            // Ignorer temporairement la collision pour éviter d'accrocher l'obstacle
            if (playerCollider != null && collision.collider != null)
            {
                Debug.Log("Collision avec obstacle à haute vitesse, application de l'impulsion et ignore collision temporaire");
                Physics.IgnoreCollision(playerCollider, collision.collider, true);
                StartCoroutine(ReenableCollisionAfter(collision.collider, obstacleIgnoreCollisionTime));
                rb.AddForce(Jump * transform.forward * obstacleForwardImpulse + Vector3.up * obstacleUpImpulse, ForceMode.Impulse);
                

            }
        }

        if(collision.gameObject.name == "Elevator")
        {
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Elevator")
        {
            transform.parent = null;
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
}

