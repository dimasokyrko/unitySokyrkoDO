using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private ServiceManager _serviceManager;
    [SerializeField] private int _maxHP;
    private int _currentHP;
    [SerializeField] private int _maxMP;
    private int _currentMP;

    [SerializeField] Slider _hpSlider;
    [SerializeField] Slider _mpSlider;

    Movement_Controller _playerMovement;

    private bool _canBeDamaged = true;
    private float _immortalityTime = 0;
    void Start()
    {
        _playerMovement = GetComponent<Movement_Controller>();
        _playerMovement.OnGetHurt += OnGetHurt;
        _currentHP = _maxHP;
        _currentMP = _maxMP;
        _hpSlider.maxValue = _maxHP;
        _hpSlider.value = _maxHP;
        _mpSlider.maxValue = _maxMP;
        _mpSlider.value = _maxMP;
        _serviceManager = ServiceManager.Instanse;
    }

    private void FixedUpdate()
    {
        if (_immortalityTime > 0)
            _immortalityTime -= Time.deltaTime;
        else
            _canBeDamaged = true;
    }

    public void TakeDamage(int damage, DamageType type = DamageType.Casual, Transform enemy = null)
    {
        if (!_canBeDamaged)
            return;

        _currentHP -= damage;
        if (_currentHP <= 0)
        {
            OnDeath();
        }

        switch (type)
        {
            case DamageType.Casual:
                _playerMovement.GetHit();
                break;
            case DamageType.PowerAttack:
                _playerMovement.GetHurt(enemy.position);
                break;
        }
        _hpSlider.value = _currentHP;
    }

    private void OnGetHurt(bool canBeDamaged)
    {
        if (_immortalityTime > 0)
            return;
        _canBeDamaged = canBeDamaged;
    }

    public void Immortality(float immortalityTime)
    {
        _canBeDamaged = false;
        _immortalityTime = immortalityTime;
    }

    public void RestoreHP(int hp)
    {
        _currentHP += hp;
        if (_currentHP > _maxHP)
        {
            _currentHP = _maxHP;
        }
        _hpSlider.value = _currentHP;
    }

    public void RestoreMP(int mp)
    {
        _currentMP += mp;
        if (_currentMP > _maxMP)
        {
            _currentMP = _maxMP;
        }
        _mpSlider.value = _currentMP;
    }

    public bool ChangeMP(int value)
    {
        if (value < 0 && _currentMP < Mathf.Abs(value))
            return false;

        _currentMP += value;
        if (_currentMP > _maxMP)
            _currentMP = _maxMP;
        _mpSlider.value = _currentMP;
        return true;
    }

    public void OnDeath()
    {
        _serviceManager.Restart();
    }
}