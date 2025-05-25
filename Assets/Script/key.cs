using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class key : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player")) // pastikan Player punya tag "Player"
        {
            Destroy(gameObject); // Hilangkan kunci dari scene
        }
    }
}
