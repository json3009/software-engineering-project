using System;
using Scripts.BodySystem;
using UnityEngine;
public class AnimationController : MonoBehaviour
{
    private const string HEAD_GUID = "31304dae-81ef-4b0f-8070-337b3198179b";
    private const string BODY_GUID = "5f67bfe0-294d-4ad8-9eab-b00b3d7afbfc";

    [SerializeField] private Rigidbody2D _rigidbody2D;
    
    [SerializeField] private BodyBase _front;
    [SerializeField] private BodyBase _side;
    [SerializeField] private BodyBase _back;
    
    [SerializeField] private RuntimeAnimatorController _frontHeadController;
    [SerializeField] private RuntimeAnimatorController _frontBodyController;
    [SerializeField] private RuntimeAnimatorController _sideHeadController;
    [SerializeField] private RuntimeAnimatorController _sideBodyController;
    [SerializeField] private RuntimeAnimatorController _backHeadController;
    [SerializeField] private RuntimeAnimatorController _backBodyController;

    private Animator _frontHeadAnimator;
    private Animator _frontTorsoAnimator;

    private Animator _sideHeadAnimator;
    private Animator _sideTorsoAnimator;
    
    private Animator _backHeadAnimator;
    private Animator _backTorsoAnimator;
    
    private static readonly int MOVEMENT_SPEED = Animator.StringToHash("MovementSpeed");

    private void Start()
    {
        if(!_front)
            throw new NullReferenceException("Body Base Front Reference is missing!");
        
        if(!_side)
            throw new NullReferenceException("Body Base Side Reference is missing!");
        
        if(!_back)
            throw new NullReferenceException("Body Base Back Reference is missing!");
            
        if (!_frontHeadController || !_frontBodyController)
            throw new NullReferenceException("Front Animator Controller is missing!");
        
        if (!_sideHeadController || !_sideBodyController)
            throw new NullReferenceException("Side Animator Controller is missing!");
        
        if (!_backHeadController || !_backBodyController)
            throw new NullReferenceException("Back Animator Controller is missing!");
        

        GetOrCreateAnimators(_front, out _frontHeadAnimator, out _frontTorsoAnimator);
        GetOrCreateAnimators(_side, out _sideHeadAnimator, out _sideTorsoAnimator);
        GetOrCreateAnimators(_back, out _backHeadAnimator, out _backTorsoAnimator);


        _frontHeadAnimator.runtimeAnimatorController = _frontHeadController;
        _frontTorsoAnimator.runtimeAnimatorController = _frontBodyController;
        
        _sideHeadAnimator.runtimeAnimatorController = _sideHeadController;
        _sideTorsoAnimator.runtimeAnimatorController = _sideBodyController;
        
        _backHeadAnimator.runtimeAnimatorController = _backHeadController;
        _backTorsoAnimator.runtimeAnimatorController = _backBodyController;
    }

    private void LateUpdate()
    {
        SetVariablesToTorso(_frontTorsoAnimator);
        SetVariablesToTorso(_sideTorsoAnimator);
        SetVariablesToTorso(_backTorsoAnimator);
    }

    
    private static void GetOrCreateAnimators(BodyBase @base ,out Animator head, out Animator body)
    {
        var headBase = @base.GetGameObjectAttributedToSlot(HEAD_GUID);
        var torsoBase = @base.GetGameObjectAttributedToSlot(BODY_GUID);

        if (!headBase.TryGetComponent<Animator>(out head))
            head = headBase.gameObject.AddComponent<Animator>();
        
        if (!torsoBase.TryGetComponent<Animator>(out body))
            body = torsoBase.gameObject.AddComponent<Animator>();
    }

    private void SetVariablesToTorso(Animator animator)
    {
        if(animator.gameObject.activeInHierarchy)
            animator.SetFloat(MOVEMENT_SPEED, _rigidbody2D.velocity.magnitude);
    }
}