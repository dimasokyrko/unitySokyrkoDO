using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class EnemyBossController : EnemyArcher
{
    [Header("Strike")]
    [SerializeField] private Transform _strikePoint;
    [SerializeField] private int _damage;
    [SerializeField] private float _strikeRange;
    [SerializeField] private LayerMask _enemies;

    [Header("PowerStrike")]
    [SerializeField] private Collider2D _strikeCollider;
    [SerializeField] private int _powerStrikeDamage;
    [SerializeField] private float _powerStrikeRange;
    [SerializeField] private float _powerStrikeSpeed;

    [Header("Tramsition")]
    [SerializeField] private float _waitTime;

    private float _currentStrikeRange;
    private bool _fightStarted;

    private EnemyState _stateOnHold;
    private EnemyState[] _attackStates = { EnemyState.Attack};

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_currentState == EnemyState.Moving && _attacking)
        {
            TurnToPlayer();
            if (CanAttack())
            {
                ChangeState(_stateOnHold);
            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_strikePoint.position, new Vector3(_strikeRange, _strikeRange, 0));
    }

    public override void TakeDamage(int damage, DamageType type = DamageType.Casual, Transform palyer = null)
    {
        if (_currentState == EnemyState.PowerAttack && type != DamageType.Projectile || _currentState == EnemyState.Hurt)
            return;

        base.TakeDamage(damage, type, palyer);

    }

    protected override void ChangeState(EnemyState state)
    {
        base.ChangeState(state);

        switch (_currentState)
        {
            case EnemyState.PowerAttack:
            case EnemyState.Attack:
                _attacking = true;
                _currentStrikeRange = state == EnemyState.Attack ? _strikeRange : _powerStrikeRange;
                _enemyRb.velocity = Vector2.zero;
                if (!CanAttack())
                {
                    _stateOnHold = state;
                    _enemyAnimator.SetBool(_currentState.ToString(), false);
                    ChangeState(EnemyState.Moving);
                }
                break;
            case EnemyState.Hurt:
                _attacking = false;
                _enemyRb.velocity = Vector2.zero;
                StopAllCoroutines();
                break;

        }
    }

    protected override void DoStateAction()
    {
        base.DoStateAction();
        switch (_currentState)
        {
            case EnemyState.Attack:
                Strike();
                break;
            case EnemyState.PowerAttack:
                StrikeWithPower();
                break;
        }
    }

    protected override void EndState()
    {
        switch (_currentState)
        {
            case EnemyState.PowerAttack:
                EndPowerStrike();
                _attacking = false;
                break;
            case EnemyState.Attack:
                _attacking = false;
                break;
            case EnemyState.Hurt:
                _fightStarted = false;
                break;
        }

        base.EndState();

        if (_currentState == EnemyState.Shoot || _currentState == EnemyState.PowerAttack || _currentState == EnemyState.Attack || _currentState == EnemyState.Hurt)
        {
            StartCoroutine(BeginNewCircle());
        }
    }

    protected override void ResetState()
    {
        base.ResetState();
        _enemyAnimator.SetBool(EnemyState.PowerAttack.ToString(), false);
        _enemyAnimator.SetBool(EnemyState.Attack.ToString(), false);
    }

    protected override void ChekPlayerInRange()
    {
        if (_player == null || _isAngry)
            return;

        if (Vector2.Distance(transform.position, _player.transform.position) < _angerRange)
        {
            _isAngry = true;
            if (!_fightStarted)
            {
                StopAllCoroutines();
                StartCoroutine(BeginNewCircle());
            }
        }
        else
            _isAngry = false;
    }
    protected void ChooseNextAttackState()
    {
        int state = Random.Range(0, _attackStates.Length);
        ChangeState(_attackStates[state]);
    }

    protected void Strike()
    {
        Collider2D player = Physics2D.OverlapBox(_strikePoint.position, new Vector2(_strikeRange, _strikeRange), 0, _enemies);
        if (player != null)
        {
            player.GetComponent<PlayerController>().TakeDamage(_damage);
        }
    }

    protected void StrikeWithPower()
    {
        _strikeCollider.enabled = true;
        _enemyRb.velocity = transform.right * _powerStrikeSpeed;
    }

    protected void EndPowerStrike()
    {
        _strikeCollider.enabled = false;
        _enemyRb.velocity = Vector2.zero;
    }

    protected override void TryToDamage(Collider2D enemy)
    {
        if (_currentState == EnemyState.Idle || _currentState == EnemyState.Shoot || _currentState == EnemyState.Hurt)
            return;

        base.TryToDamage(enemy);
    }

    private bool CanAttack()
    {
        return Vector2.Distance(transform.position, _player.transform.position) < _currentStrikeRange;
    }

    private IEnumerator BeginNewCircle()
    {
        if (_currentState == EnemyState.Death)
            yield break;

        if (_fightStarted)
        {
            _isAngry = false;
            ChekPlayerInRange();
            if (!_isAngry)
            {
                _fightStarted = false;
                StartCoroutine(ScanForPlayer());
                yield break;
            }
            yield return new WaitForSeconds(_waitTime);
        }
        _fightStarted = true;
        TurnToPlayer();
        ChooseNextAttackState();
    }
}
