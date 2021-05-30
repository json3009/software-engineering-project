using System;
using CharacterSystem;
using CombatSystem.SpellSystem;
using EventSystem;
using UISystem;
using UnityEngine;

public class LevelUpEventHandler : MonoBehaviour
{
     [SerializeField] private GameObject _levelUpObject;
     [SerializeField] private Character _player;

     private void Start()
     {
          GlobalEventSystem.Instance.OnPlayerArchetypeLevelUp += InstanceOnOnPlayerArchetypeLevelUp;
     }

     private void InstanceOnOnPlayerArchetypeLevelUp(SpellArchetype levelupSpell, int lvl)
     {
          GameObject newLvl = GameObject.Instantiate(_levelUpObject, transform.position, Quaternion.identity);
          LevelUpUIHandler handler = newLvl.GetComponent<LevelUpUIHandler>();

          handler.Text = $"{levelupSpell.Name}\nLevel Up ({lvl})";
     }
}