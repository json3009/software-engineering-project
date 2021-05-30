using OdinSerializer;
using System.Collections;
using System.Collections.Generic;
using CharacterSystem;
using CombatSystem.SpellSystem;
using StatSystem;
using StatSystem.StatModifiers;
using UnityEngine;

public class SpellTest : MonoBehaviour
{

    public GameObject target;

    public Spell spell;
    public Character character;
    

    // Start is called before the first frame update
    void Start()
    {
        if (spell)
        {
            StatData statData = new StatData();
            statData.AddNewAttribute(spell.System.Traits.GetStatTypeByID("_level"), out IStatAttribute levelAttr);
            statData.AddNewAttribute(spell.System.Traits.GetStatTypeByID("_availableMana"), out IStatAttribute availableManaAttr);
            statData.AddNewAttribute(spell.System.Traits.GetStatTypeByID("_targetPos"), out IStatAttribute targetAttr);

            levelAttr.AddModifier(new IntModifier(2));
            targetAttr.AddModifier(new PositionModifier(target));
            availableManaAttr.AddModifier(new FloatModifier(100f, FloatModifier.FloatModifierType.Flat));

            Vector2 pos = transform.position;
            spell.Cast(character, statData, pos);
        }
        //sourceAttr.BaseValue = this.transform.position;

        /*SpellBehaviour t = archetypeToTest.Cast(spellArchetypeData);
        t.transform.position = transform.position;

        Debug.Log($"Before: {availableManaAttr.GetValue<float>()}");
        t.OnImpact += () =>
        {
            Debug.Log($"After: {t.DataInput.GetAttribute("_availableMana").GetValue<float>()}");
            Debug.Log($"Distance: {1000 - Vector3.Distance(new Vector3(1000, 6), t.transform.position)}");

            SpellBehaviour i = onCollisionArchetype.Cast(t.DataInput);
            i.transform.position = t.transform.position;
        };*/
        

        /*system.RootModule = (SpellModule) system.RootModule.Clone();
        system.RootModule.ManaCost = 50;

        var t = system.GetSaveData(DataFormat.JSON, out unityObjectReferences);
        Debug.Log(System.Text.Encoding.UTF8.GetString(t));

        system = SpellSystem.LoadSaveData(t, DataFormat.JSON, unityObjectReferences);

        Debug.Log(system);

        var e = GetSaveData(DataFormat.JSON);
        Debug.Log(System.Text.Encoding.UTF8.GetString(e));

        Debug.Log(LoadSaveData(e, DataFormat.JSON));*/

        /*foreach (var item in system.RootModule.)
        {

        }*/

        //var s = SerializationUtility.SerializeValue(spellArchetypeData, DataFormat.JSON);
        //Debug.Log(System.Text.Encoding.UTF8.GetString(s));

    }

    public byte[] GetSaveData(DataFormat format)
    {
        return SerializationUtility.SerializeValue(this, format);
    }

    public static SpellTest LoadSaveData(byte[] data, DataFormat format)
    {
        return SerializationUtility.DeserializeValue<SpellTest>(data, format);
    }
}
