using UnityEngine;

public class key : MonoBehaviour
{
    public int keyValue = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            player.addKey(keyValue);
            Destroy(gameObject); // Hapus key dari scene
        }
    }
}
