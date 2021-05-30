using System;
using System.Collections;
using System.Collections.Generic;
using AISystem;
using CharacterSystem;
using DialogSystem;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] private AIStateController _controller;
    [SerializeField] private Character _enemyCharacter;
    [SerializeField] private Character _npcCharacter;

    [SerializeField] private AIState _enemyState;
    [SerializeField] private AIState _npcState;

    [SerializeField] private bool _hasDialog = false; 
    
    public ZoneScript Zone { get; set; }

    public AIStateController Controller => _controller;

    public Character EnemyCharacter => _enemyCharacter;

    private void OnEnable()
    {
        Controller.TransitionToState(_enemyState);

        EnemyCharacter.gameObject.SetActive(true);
        _npcCharacter.gameObject.SetActive(false);
        
        EnemyCharacter.OnDeath += EnemyDied;
    }

    private void OnDisable()
    {
        EnemyCharacter.OnDeath -= EnemyDied;
    }

    private void EnemyDied()
    {
        if (_hasDialog && TryGetComponent<DialogObject>(out var dialog))
            dialog.StartStory();
        
        
        Zone.EnemyDied(this);
        
        Controller.TransitionToState(_npcState);

        EnemyCharacter.gameObject.SetActive(false);
        _npcCharacter.gameObject.SetActive(true);
    }
}
