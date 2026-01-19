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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject);
        // collision.gameObject
        if (collision.collider.name == "Terrain")
        {
            //Debug.Log("entre");
            isGrounded = true;
        }

        // Si on touche un GameObject nommé "Obstacle" et que c'est un BoxCollider
        // L'effet n'est appliqué que si speed est strictement supérieur à 7f
        if (collision.gameObject.name == "Obstacle" && collision.collider is BoxCollider && speed > 7f)
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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = SprintSpeed;
        }
        else
        {
            speed = 7f;
        }

    }
}

