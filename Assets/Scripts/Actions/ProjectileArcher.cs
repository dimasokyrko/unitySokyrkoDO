using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProjectileArcher : MonoBehaviour
{
    [SerializeField] private int _damage;

    private DateTime _lastEncounter;

    private void OnTriggerEnter2D(Collider2D info)
    {
        if ((DateTime.Now - _lastEncounter).TotalSeconds < 5f)
            return;

        _lastEncounter = DateTime.Now;
        PlayerController player = info.GetComponent<PlayerController>();
        if (player != null)
            player.TakeDamage(_damage);
        Destroy(gameObject);
    }
}
