using System;
using Actions;
using CharacterSystem;
using EventSystem;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.Attributes;

public class AutoOrient : MonoBehaviour
{
    private const float FROM_TO_SIDE = 0.7071f;
    private const float FROM_TO_VERTICAL = 0;

    [SerializeField] public Rigidbody2D _player;
    [SerializeField] public Character _character;
    [SerializeField] public Camera mainCamera;

    [SerializeField] private bool _followMouse;

    private void Start()
    {
        GlobalEventSystem.Instance.InputActions.Player.Look.Enable();
    }

    private void LateUpdate()
    {
        if (_followMouse)
        {
            Vector2 cursor = mainCamera.ScreenToWorldPoint(GlobalEventSystem.Instance.InputActions.Player.Look.ReadValue<Vector2>());
            Vector2 dir = (cursor - (Vector2)transform.position).normalized;

            float side = Vector2.Dot(dir, Vector2.left);
            float top = Vector2.Dot(dir, Vector2.up);

            Vector2 toGo = Vector2.zero;
            if (side > FROM_TO_SIDE)
                toGo = Vector2.left;
            else if(side < -FROM_TO_SIDE)
                toGo = Vector2.right;
            else if(top > FROM_TO_VERTICAL)
                toGo = Vector2.up;
            else if(top < -FROM_TO_VERTICAL)
                toGo = Vector2.down;
            
            RotateTo(toGo);
        }
        else
        {
            RotateTo(_player.velocity);
        }
    }
    
    private void RotateCharacter(bool left)
    {
        _character.transform.localRotation = Quaternion.Euler(0, left ? 180 : 0, 0);
    }

    private void RotateTo(Vector2 vector)
    {
        if(Math.Abs(vector.x) > Math.Abs(vector.y))
        {
            _character.SwitchSide(new Utilities.SerializableGUID() { Value = "fd326b7d-c939-43f9-bb7a-bece69a81e6d" });
            RotateCharacter(vector.x > 0);
        }
        else
        {
            if (vector.y > 0)
                _character.SwitchSide(new Utilities.SerializableGUID() { Value = "b7754afe-55f2-4fa4-9197-cbbab19c3445" });
            else if (vector.y < 0)
                _character.SwitchSide(new Utilities.SerializableGUID() { Value = "288bd822-cd91-4667-808a-995871ab1289" });
        }
    }
}