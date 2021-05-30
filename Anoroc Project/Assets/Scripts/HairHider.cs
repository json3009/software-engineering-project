using System;
using Scripts.BodySystem;
using CharacterSystem;
using EquipmentSystem;
using PlayFab.MultiplayerModels;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(Character))]
[ExecuteInEditMode]
public class HairHider : MonoBehaviour
{
    private const string HAIR_GUID = "0199589f-40d6-4b18-b03a-2f809df4d0de";
    private const string HELMET_GUID = "4630e305-2b3b-4bdc-be08-0f6c05045087";

    private const string SKIN_LAYER_ID = "6b72a5bd-9d21-481e-89f8-4e76872f27bf";
    private const string EQUIPMENT_LAYER_ID = "9893a39d-b622-4f95-a992-a1367b9f5a96";
    
    private Character _character;

    private BodyPartFlag _headSlot;
    private BodyPartFlag _hairSlot;
    
    private void OnEnable()
    {
        _character = GetComponent<Character>();

        _hairSlot = _character.Definition.GetPartByID(Guid.Parse(HAIR_GUID));
        _headSlot = _character.Definition.GetPartByID(Guid.Parse(HELMET_GUID));
        
        _character.Equipment.OnEquipmentChange += EquipmentOnOnEquipmentChange;
    }

    private void OnDestroy()
    {
        _character.Equipment.OnEquipmentChange -= EquipmentOnOnEquipmentChange;
    }

    private void EquipmentOnOnEquipmentChange(BodyPartFlag slot, SerializableGUID layerID, EquipmentItem equipment)
    {
        if(!slot.Equals(_hairSlot) && !slot.Equals(_headSlot))
            return;

        var hair = _character.Equipment.GetEquipment(_hairSlot, Guid.Parse(SKIN_LAYER_ID));
        var helmet = _character.Equipment.GetEquipment(_headSlot, Guid.Parse(EQUIPMENT_LAYER_ID));
        if (hair == null) return;
        
        if(helmet != null)
            _character.HideEquipment(_hairSlot, Guid.Parse(SKIN_LAYER_ID));
        else
            _character.ShowEquipment(_hairSlot, Guid.Parse(SKIN_LAYER_ID));
    }
}