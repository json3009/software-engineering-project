using System;
using CharacterSystem;
using CombatSystem.SpellSystem;
using EventSystem;
using StatSystem;
using StatSystem.StatAttributes;
using StatSystem.StatModifiers;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character))]
public class SpellCaster : MonoBehaviour
{
    [SerializeField] private Spell _spellToCast;
    [SerializeField] public Camera _mainCamera;
    
    private Character _character;
    
    private void Start()
    {
        _character = GetComponent<Character>();
        GlobalEventSystem.Instance.InputActions.Player.Enable();
        
        GlobalEventSystem.Instance.InputActions.Player.Fire.performed += FireOnPerformed;
    }

    private void OnDestroy()
    {
        GlobalEventSystem.Instance.InputActions.Player.Fire.performed -= FireOnPerformed;
    }

    private void FireOnPerformed(InputAction.CallbackContext obj)
    {
        var statData = new StatData();
        Vector2 cursor = _mainCamera.ScreenToWorldPoint(GlobalEventSystem.Instance.InputActions.Player.Look.ReadValue<Vector2>());
        
        //statData.AddNewAttribute(spell.System.Traits.GetStatTypeByID("_level"), out IStatAttribute levelAttr);
        statData.AddNewAttribute(_spellToCast.System.Traits.GetStatTypeByID("_availableMana"), out FloatAttribute availableManaAttr);
        statData.AddNewAttribute(_spellToCast.System.Traits.GetStatTypeByID("_targetPos"), out PositionAttribute targetAttr);

        //get cost 
        var manaCost = Math.Min(_character.Mana.Value, _spellToCast.GetMaxManaCost(_character, statData));
        _character.Mana.Value -= manaCost;
        
        //levelAttr.AddModifier(new IntModifier(2));
        targetAttr.AddModifier(new PositionModifier(cursor));
        availableManaAttr.AddModifier(new FloatModifier(manaCost, FloatModifier.FloatModifierType.Flat));

        _spellToCast.Cast(_character, statData);
    }
}