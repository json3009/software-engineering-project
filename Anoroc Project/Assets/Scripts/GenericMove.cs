using UnityEngine;

using Actions;
using System;
using CharacterSystem;

public class GenericMove : MonoBehaviour
{
    [SerializeField] private MainActions _controls;

    [NonSerialized] private Vector2 _velocity;
    [NonSerialized] private Vector2 _totalVelocity;
    private Rigidbody2D player;

    public int speed = 5;

    private void OnEnable()
    {
        _controls = new MainActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        _controls.Player.Move.performed += Move_performed;
        _controls.Player.Move.canceled += Move_canceled;
        _controls.Player.Move.Enable();
        
        _controls.Player.Fire.Enable();

        player = GetComponent<Rigidbody2D>();
    }

    private void OnDestroy()
    {
        _controls.Player.Move.performed -= Move_performed;
        _controls.Player.Move.canceled -= Move_canceled;
    }

    private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _velocity = Vector2.zero;
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _velocity = obj.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        player.velocity = (_velocity * speed);
    }
    

}
