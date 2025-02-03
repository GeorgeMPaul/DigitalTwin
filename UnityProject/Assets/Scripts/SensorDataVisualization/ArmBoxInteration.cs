using UnityEngine;

public class ArmBoxInteraction : MonoBehaviour
{
    // Reference to the box's Rigidbody
    public Rigidbody boxRigidbody;
    public GameObject picker;
    // Flag to check if the box is attached
    private bool isBoxAttached = false;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Object Collided`");
        if (collision.gameObject.CompareTag("Box") && !isBoxAttached)
        {
            Debug.Log("Object Collided");
            if (boxRigidbody != null)
            {
                // Make the box a child of the robotic arm to make it stick
                collision.gameObject.transform.SetParent(this.transform);

                // Optionally reset box position relative to the arm
                collision.gameObject.transform.localPosition = picker.transform.position;
                collision.gameObject.transform.localRotation = Quaternion.identity;

                // Mark that the box is attached
                isBoxAttached = true;
            }
        }
    }
}
