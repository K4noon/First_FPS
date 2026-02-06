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
    object rb;
    public bool isGrounded;

    private Collider playerCollider;

    public float obstacleForwardImpulse = 5f;
    public float obstacleUpImpulse = 5f;
    public float obstacleIgnoreCollisionTime = 0.5f;



    [Tooltip("Animator du joueur (laissez vide pour récupération automatique dans les enfants)")]
    [SerializeField] private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        rb = GetComponent<Rigidbody>();
        if (Input.GetAxis("Jump") != 0 && isGrounded) //si appuie sur espace et touche le sol
        {
            Jump = Input.GetAxis("Jump") * jumpForce;
            object rigidbody = rb as Rigidbody;
            if (rigidbody != null)
            {
                (rigidbody as Rigidbody).AddForce(Vector3.up * Jump, ForceMode.Impulse);
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

       

            // Ignorer temporairement la collision pour éviter d'accrocher l'obstacle
            if (playerCollider != null && collision.collider != null)
            {
                Physics.IgnoreCollision(playerCollider, collision.collider, true);
                StartCoroutine(ReenableCollisionAfter(collision.collider, obstacleIgnoreCollisionTime));
                obstacleForwardImpulse = 5f;
                obstacleUpImpulse = 5f;
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
}

    
    

