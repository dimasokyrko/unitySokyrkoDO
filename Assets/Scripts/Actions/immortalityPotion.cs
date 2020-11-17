using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class immortalityPotion : MonoBehaviour
{
    [SerializeField] private float _immortalityTime;

    private void OnTriggerEnter2D(Collider2D info)
    {
        info.GetComponent<PlayerController>().Immortality(_immortalityTime);
        Destroy(gameObject);
    }
}
