using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatingPlatformController : MonoBehaviour
{
    [SerializeField] private int _damage;
    private Animator _fireBallAnimator;

    private void OnTriggerEnter2D(Collider2D info)
    {
        EnemyControllerBase enemy = info.GetComponent<EnemyControllerBase>();
        if (enemy != null)
            enemy.TakeDamage(_damage, DamageType.Projectile);
        Destroy(gameObject);
    }
}
