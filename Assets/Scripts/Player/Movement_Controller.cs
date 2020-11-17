using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
[RequireComponent(typeof(PlayerController))]
public class Movement_Controller : MonoBehaviour
{
    public event Action<bool> OnGetHurt = delegate { };

    private Rigidbody2D _playerRB;
    private Animator _playerAnimator;
    private PlayerController _playerController;

    [Header("Horizontal movement")]
    [SerializeField] private float _speed;
    [Range(0, 1)]
    [SerializeField] private float _crouchSpeedReduce;

    private bool _faceRight = true;
    private bool _canMove = true;

    [Header("Jump")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _radius;
    [SerializeField] private bool _airControll;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _whatIsGround;
    private bool _grounded;
    private bool _canDoubleJump;

    [Header("Crouch")]
    [SerializeField] private Transform _cellCheck;
    [SerializeField] private Collider2D _headCollider;
    private bool _canStand;

    [Header("Casting")]
    [SerializeField] private GameObject _fireBall;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireBallSpeed;
    [SerializeField] private int _castCost;
    [SerializeField] private float _castCooldown;
    [SerializeField] Slider _fireballCooldownSlider;
    private float _timeToCastCooldown;
    private bool _isCasting;

    [Header("SwordAttack")]
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private int _damage;
    [SerializeField] private float _swordAttackRange;
    [SerializeField] private LayerMask _enemies;
    private bool _isStriking;

    [Header("StrikeAttack")]
    [SerializeField] private float _chargeTime;
    [SerializeField] private float _strikeAttackSpeed;
    [SerializeField] private Collider2D _strikeAttackCollider;
    [SerializeField] private int _strikeAttackDamage;
    [SerializeField] private int _strikeAttackCost;
    [SerializeField] private float _pushForce;
    [SerializeField] private float _strikeCooldown;
    [SerializeField] private Slider _strikeAttackSlider;
    private float _timeToStrikeCooldown;

    [Header("CreatePlatform")]
    [SerializeField] private GameObject _creatingPlatform;
    [SerializeField] private Transform _creatingPoint;
    [SerializeField] private int _createCost;
    [SerializeField] private float _lifetime;
    [SerializeField] private float _createCooldown;
    [SerializeField] Slider _createCooldownSlider;
    private float _timeToCreateCooldown;
    private bool _isCreating;

    private float _lastHurtTime;
    private int _playerObject, _collideObject;

    private List<EnemyControllerBase> _damagedEnemies = new List<EnemyControllerBase>();

    public float ChargeTime => _chargeTime;
    
    private void Start()
    {
        _playerRB = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();
        _playerObject = LayerMask.NameToLayer("Player");
        _collideObject = LayerMask.NameToLayer("Platforms");
        _timeToCreateCooldown = 0f;
        _timeToCastCooldown = 0f;
        _timeToStrikeCooldown = 0f;
        _fireballCooldownSlider.value = _castCooldown;
        _createCooldownSlider.value = _createCooldown;
        _strikeAttackSlider.value = _strikeCooldown;
    }

    private void FixedUpdate()
    {
        _grounded = Physics2D.OverlapCircle(_groundCheck.position, _radius, _whatIsGround);

        if (_playerAnimator.GetBool("Hurt") && Time.time - _lastHurtTime > 0.5f)
            EndHurt();
        if (_playerAnimator.GetBool("Hit") && _grounded && Time.time - _lastHurtTime > 0.5f)
            EndHit();

        if (_playerRB.velocity.y > 0)
            Physics2D.IgnoreLayerCollision(_playerObject, _collideObject, true);
        else
            Physics2D.IgnoreLayerCollision(_playerObject, _collideObject, false);

        if(_timeToCreateCooldown > 0)
        {
            _timeToCreateCooldown -= Time.deltaTime;
            _createCooldownSlider.value = _createCooldown - _timeToCreateCooldown;
        }
        if(_timeToCastCooldown > 0)
        {
            _timeToCastCooldown -= Time.deltaTime;
            _fireballCooldownSlider.value = _castCooldown - _timeToCastCooldown;
        }
        if(_timeToStrikeCooldown > 0)
        {
            _timeToStrikeCooldown -= Time.deltaTime;
            _strikeAttackSlider.value = _strikeCooldown - _timeToStrikeCooldown;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_groundCheck.position, _radius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_cellCheck.position, _radius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(_attackPoint.position, _swordAttackRange);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MovingPlatform")
        {
            this.transform.parent = collision.transform;
            return;
        }

        if (!_strikeAttackCollider.enabled)
        {
            return;
        }

        EnemyControllerBase enemy = collision.collider.GetComponent<EnemyControllerBase>();
        if (enemy == null || _damagedEnemies.Contains(enemy))
            return;

        enemy.TakeDamage(_strikeAttackDamage, DamageType.PowerAttack);
        _damagedEnemies.Add(enemy);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "MovingPlatform")
        {
            this.transform.parent = null;
        }
    }
    
    public void Move(float move, bool jump, bool crouch/*, bool fall*/)
    {
        if (!_canMove)
            return;

        float speedModificator = _headCollider.enabled ? 1 : _crouchSpeedReduce;

        if ((_grounded || _airControll))
            _playerRB.velocity = new Vector2(_speed * move * speedModificator, _playerRB.velocity.y);

        if (move > 0 && !_faceRight)
        {
            Flip();
        }
        else if (move < 0 && _faceRight)
        {
            Flip();
        }

        if (jump)
        {
            if (_grounded)
            {
                _playerRB.velocity = new Vector2(_playerRB.velocity.x, _jumpForce);
                _canDoubleJump = true;
            }
            else if (_canDoubleJump)
            {
                _playerRB.velocity = new Vector2(_playerRB.velocity.x, _jumpForce);
                _canDoubleJump = false;
            }
        }
        /* #region Falling
        if(fall)
        {
            StartCoroutine("FallOff");
        }

        #endregion*/
        _canStand = !Physics2D.OverlapCircle(_cellCheck.position, _radius, _whatIsGround);
        if (crouch)
        {
            _headCollider.enabled = false;
        }
        else if (!crouch && _canStand)
        {
            _headCollider.enabled = true;
        }
        _playerAnimator.SetFloat("Speed", Mathf.Abs(move));
        _playerAnimator.SetBool("Jump", !_grounded);
        _playerAnimator.SetBool("Crouch", !_headCollider.enabled);
    }

    /*private IEnumerator FallOff()
    {
        Physics2D.IgnoreLayerCollision(_playerObject, _collideObject, true);
        yield return new WaitForSeconds(2f);
        Physics2D.IgnoreLayerCollision(_playerObject, _collideObject, false);
    }*/

    public void StartCreating()
    {
        if (CanCreate())
        {
            if (_isCreating || !_playerController.ChangeMP(-_castCost))
                return;
            _isCreating = true;
            _timeToCreateCooldown = _createCooldown;
            _createCooldownSlider.value = 0;
            _playerAnimator.SetBool("Creating", true);
        }
    }

    public bool CanCreate()
    {
        if (_timeToCreateCooldown <= 0f)
            return true;
        return false;
    }
    private void Creating()
    {
        GameObject platform = Instantiate(_creatingPlatform, _creatingPoint.position, Quaternion.identity);
        Destroy(platform, _lifetime);
    }
    private void EndCreating()
    {
        _isCreating = false;
        _playerAnimator.SetBool("Creating", false);
    }

    public void StartCasting()
    {
        if (CanCast())
        {
            if (_isCasting || !_playerController.ChangeMP(-_castCost))
                return;
            _isCasting = true;
            _timeToCastCooldown = _castCooldown;
            _fireballCooldownSlider.value = 0;
            _playerAnimator.SetBool("Casting", true);
        }
    }

    private bool CanCast()
    {
        if(_timeToCastCooldown <= 0f)
        {
            return true;
        }
        return false;
    }

    private void CastFire()
    {
        GameObject fireBall = Instantiate(_fireBall, _firePoint.position, Quaternion.identity);
        fireBall.GetComponent<Rigidbody2D>().velocity = transform.right * _fireBallSpeed;
        fireBall.GetComponent<SpriteRenderer>().flipX = !_faceRight;
        Destroy(fireBall, 5f);
    }
    private void EndCasting()
    {
        _isCasting = false;
        _playerAnimator.SetBool("Casting", false);
    }

    public void StartAttack(float holdTime)
    {
        if (_isStriking || _playerRB.velocity != Vector2.zero)
            return;

        _canMove = false;
        if (holdTime >= _chargeTime)
        {
            if (CanAttack())
            {
                if (!_playerController.ChangeMP(-_strikeAttackCost))
                    return;
                _timeToStrikeCooldown = _strikeCooldown;
                _strikeAttackSlider.value = 0;
                _playerAnimator.SetBool("StrikeAttack", true);
            }
        }
        else
        {
            _playerAnimator.SetBool("SwordAttack", true);
        }

        _isStriking = true;
    }

    private bool CanAttack()
    {
        if (_timeToStrikeCooldown <= 0f)
        {
            return true;
        }
        return false;
    }

    public void GetHurt(Vector2 position)
    {
        _lastHurtTime = Time.time;
        _canMove = false;
        OnGetHurt(false);
        Vector2 pushDirection = new Vector2();
        pushDirection.x = position.x > transform.position.x ? -1 : 1;
        pushDirection.y = 1;
        _playerAnimator.SetBool("Hurt", true);
        _playerRB.AddForce(pushDirection * _pushForce, ForceMode2D.Impulse);
    }

    public void GetHit()
    {
        _lastHurtTime = Time.time;
        _canMove = false;
        _playerAnimator.SetBool("Hit", true);
    }

    private void ResetPlayer()
    {
        _playerAnimator.SetBool("SwordAttack", false);
        _playerAnimator.SetBool("StrikeAttack", false);
        _playerAnimator.SetBool("Casting", false);
        _playerAnimator.SetBool("Creating", false);
        _playerAnimator.SetBool("Hurt", false);
        _playerAnimator.SetBool("Hit", false);
        _isCasting = false;
        _isCreating = false;
        _isStriking = false;
        _canMove = true;
    }

    private void Flip()
    {
        _faceRight = !_faceRight;
        transform.Rotate(0, 180, 0);
    }

    private void EndHurt()
    {
        ResetPlayer();
        OnGetHurt(true);
    }

    private void EndHit()
    {
        ResetPlayer();
    }

    private void StartStrikeAttack()
    {
        _playerRB.velocity = transform.right * _strikeAttackSpeed;
        _strikeAttackCollider.enabled = true;
    }

    private void DisableStrikeAttack()
    {
        _playerRB.velocity = Vector2.zero;
        _strikeAttackCollider.enabled = false;
        _damagedEnemies.Clear();
    }

    private void EndStrikeAttack()
    {
        _playerAnimator.SetBool("StrikeAttack", false);
        _canMove = true;
        _isStriking = false;
    }

    private void SwordAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(_attackPoint.position, _swordAttackRange, _enemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyControllerBase enemy = enemies[i].GetComponent<EnemyControllerBase>();
            if (enemy != null)
                enemy.TakeDamage(_damage);
        }
    }

    private void EndSwordAttack()
    {
        _playerAnimator.SetBool("SwordAttack", false);
        _isStriking = false;
        _canMove = true;
    }
}