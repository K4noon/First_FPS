using Unity.VisualScripting;
using UnityEngine;

public class Obstacle_GroundDetection : MonoBehaviour
{
    [Tooltip("Nom du GameObject joueur contenant le CapsuleCollider et PlayerMovement")]
    [SerializeField] private string playerObjectName = "PlayerModel";

    [Tooltip("Tolérance verticale (en mètres) pour considérer le capsule collider 'au-dessus'")]
    [SerializeField] private float verticalTolerance = 0.05f;

    [Tooltip("Valeur de jumpForce appliquée quand le joueur est sur l'obstacle")]
    [SerializeField] private float forcedJumpForce = 4f;

    private BoxCollider boxCollider;
    private GameObject playerObject;
    private CapsuleCollider playerCapsule;
    private PlayerMovement playerMovement;

    private float originalJumpForce;
    private bool hasOriginalJumpForce = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"{nameof(Obstacle_GroundDetection)} nécessite un BoxCollider sur le même GameObject.");
            enabled = false;
            return;
        }

        playerObject = GameObject.Find(playerObjectName);
        if (playerObject == null)
        {
            Debug.LogWarning($"Impossible de trouver un GameObject nommé '{playerObjectName}'.");
            return;
        }

        playerCapsule = playerObject.GetComponentInChildren<CapsuleCollider>();
        if (playerCapsule == null)
        {
            Debug.LogWarning($"Aucun {nameof(CapsuleCollider)} trouvé dans '{playerObjectName}'.");
        }

        playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning($"Aucun composant {nameof(PlayerMovement)} trouvé sur '{playerObjectName}'.");
        }
    }

    // Vérifie continuellement pendant la collision si le bas du capsule est au-dessus de la face supérieure du box
    private void OnCollisionStay(Collision collision)
    {
        if (playerObject == null || playerCapsule == null || playerMovement == null)
            return;

        if (collision.gameObject != playerObject)
            return;

        Bounds boxBounds = boxCollider.bounds;
        float boxTopY = boxBounds.max.y;

        float capsuleBottomY = playerCapsule.bounds.min.y;
        Vector3 capsuleHorizontalCenter = new Vector3(playerCapsule.bounds.center.x, boxBounds.center.y, playerCapsule.bounds.center.z);

        bool aboveTop = capsuleBottomY >= boxTopY - verticalTolerance;
        bool withinX = capsuleHorizontalCenter.x >= boxBounds.min.x && capsuleHorizontalCenter.x <= boxBounds.max.x;
        bool withinZ = capsuleHorizontalCenter.z >= boxBounds.min.z && capsuleHorizontalCenter.z <= boxBounds.max.z;

        if (aboveTop && withinX && withinZ)
        {
            playerMovement.isGrounded = true;

            // Forcer jumpForce à la valeur souhaitée et mémoriser l'original pour restauration
            if (!hasOriginalJumpForce)
            {
                originalJumpForce = playerMovement.jumpForce;
                hasOriginalJumpForce = true;
            }

            if (playerMovement.jumpForce != forcedJumpForce)
                playerMovement.jumpForce = forcedJumpForce;
        }
    }

    // Réinitialise isGrounded et restaure jumpForce lors de la sortie de collision avec le joueur
    private void OnCollisionExit(Collision collision)
    {
        if (playerObject == null || playerMovement == null)
            return;

        if (collision.gameObject == playerObject)
        {
            playerMovement.isGrounded = false;

            if (hasOriginalJumpForce)
            {
                playerMovement.jumpForce = originalJumpForce;
                hasOriginalJumpForce = false;
            }
        }
    }
}
