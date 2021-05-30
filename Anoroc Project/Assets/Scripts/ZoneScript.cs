using System;
using System.Collections;
using System.Collections.Generic;
using AISystem;
using CharacterSystem;
using EventSystem;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZoneScript : MonoBehaviour
{
    private bool _bossHasSpawned = false;

    [SerializeField] private Vector2 _npcReturnPosition;
    
    [SerializeField] private AudioClip _areaSoundTrack;
    [SerializeField] private EnemyScript[] _enemies;
    [SerializeField] private EnemyScript _enemyBoss;
    [SerializeField] private Character _player;
    
    private Collider2D _triggerCollider;

    private List<EnemyScript> _enemiesLeft;
    public AudioClip AreaSoundTrack => _areaSoundTrack;


    private void Start()
    {
        _enemyBoss.Zone = this;
        _enemyBoss.gameObject.SetActive(false);

        _enemiesLeft = new List<EnemyScript>(_enemies);

        foreach (var enemy in _enemiesLeft)
        {
            enemy.Zone = this;
            enemy.gameObject.SetActive(false);
        }
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        GlobalEventSystem.Instance.PlayerEnteredZone(this);

        foreach (var enemy in _enemiesLeft)
            enemy.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GlobalEventSystem.Instance.PlayerExitedZone(this);
    }

    public void EnemyDied(EnemyScript enemyScript)
    {
        enemyScript.Controller.TargetPosition = _npcReturnPosition;
        _enemiesLeft.Remove(enemyScript);

        if (_enemiesLeft.Count <= 0 && !_bossHasSpawned)
        {
            _bossHasSpawned = true;
            _enemyBoss.gameObject.SetActive(true);
        }
    }
}
