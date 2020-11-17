using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Movement_Controller))]
public class InputController : MonoBehaviour
{
    Movement_Controller _playerMovement;
    DateTime _strikeClickTime;
    float _move;
    bool _jump;
    //bool _fall;
    bool _crouch;
    bool _canAtack;

    private void Start()
    {
        _playerMovement = GetComponent<Movement_Controller>();
    }

    void Update()
    {
        _move = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonUp("Jump"))
        {
            _jump = true;
        }

        /*_fall = Input.GetKey(KeyCode.DownArrow);*/

        _crouch = Input.GetKey(KeyCode.C);

        if (Input.GetKey(KeyCode.E))
            _playerMovement.StartCasting();

        if (Input.GetKey(KeyCode.R))
            _playerMovement.StartCreating();

        if (!IsPointerOverUI())
        {
            if (Input.GetButtonDown("Fire1"))
            {
                _strikeClickTime = DateTime.Now;
                _canAtack = true;
            }
            if (Input.GetButtonUp("Fire1"))
            {
                float holdTime = (float)(DateTime.Now - _strikeClickTime).TotalSeconds;
                if (_canAtack)
                    _playerMovement.StartAttack(holdTime);
                _canAtack = false;
            }
        }

        if ((DateTime.Now - _strikeClickTime).TotalSeconds >= _playerMovement.ChargeTime * 2 && _canAtack)
        {
            _playerMovement.StartAttack(_playerMovement.ChargeTime);
            _canAtack = false;
        }
    }

    private void FixedUpdate()
    {
        _playerMovement.Move(_move, _jump, _crouch /*, _fall*/);
        _jump = false;
    }

    private bool IsPointerOverUI() => EventSystem.current.IsPointerOverGameObject();
}
