using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalMover : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private float _speed;
    private int _direction = 1;
    void Start()
    {
    }

    void Update()
    {
        if (IsGroundEnding())
            _direction *= -1;
        transform.Translate(_speed * _direction * Time.deltaTime, 0, 0);
    }

    private bool IsGroundEnding()
    {
        return !Physics2D.OverlapPoint(_startPoint.position, _whatIsGround);
    }
}
