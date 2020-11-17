using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorMP : MonoBehaviour
{
    [SerializeField] private int _restoreValue;
    private void OnTriggerEnter2D(Collider2D info)
    {
        info.GetComponent<PlayerController>().RestoreMP(_restoreValue);
        Destroy(gameObject);
    }
}
