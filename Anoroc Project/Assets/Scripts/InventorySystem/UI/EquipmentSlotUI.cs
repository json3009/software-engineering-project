using EquipmentSystem;
using TMPro;
using UnityEngine;
using Utilities;

namespace InventorySystem.UI
{
    public class EquipmentSlotUI : InventorySlot
    {
        [SerializeField] private TextMeshProUGUI _buffs;
        
        public new void SetSlot(InventoryObject obj)
        {
            if(!(obj is InventoryEquipment equipmentItem)) return;
            
            base.SetSlot(obj);
            _buffs.SetText("No Modifiers");
            if(equipmentItem.CharacterData == null && equipmentItem.SpellData == null)
                return;

            var resultString = "";
            if (equipmentItem.CharacterData != null)
            {
                foreach (var dataWrapper in equipmentItem.CharacterData.Modifiers)
                {
                    if (dataWrapper.Attribute.Modifiers.Length <= 0) continue;

                    resultString += $"{dataWrapper.Type.Name}: ";
                    for (var index = 0; index < dataWrapper.Attribute.Modifiers.Length; index++)
                    {
                        var appliedMod = dataWrapper.Attribute.Modifiers[index];
                        if (index > 0)
                            resultString += $"<alpha=#00>{dataWrapper.Type.Name}: <alpha=#FF>";
                        resultString += $"<color=#009e00>{appliedMod}</color>\n";
                    }
                }
            }
            
            if (equipmentItem.SpellData != null)
            {
                foreach (var dataWrapper in equipmentItem.SpellData.Modifiers)
                {
                    if (dataWrapper.Attribute.Modifiers.Length <= 0) continue;

                    resultString += $"{dataWrapper.Type.Name}: ";
                    for (var index = 0; index < dataWrapper.Attribute.Modifiers.Length; index++)
                    {
                        var appliedMod = dataWrapper.Attribute.Modifiers[index];
                        if (index > 0)
                            resultString += $"<alpha=#00>{dataWrapper.Type.Name}: <alpha=#FF>";
                        resultString += $"<color=#009e00>{appliedMod}</color>\n";
                    }
                }
            }

            _buffs.SetText(resultString.IsEmpty() ? "No Modifiers": resultString);
        }

        public new void ResetSlot()
        {
            base.ResetSlot();
            _buffs.SetText(""); 
        }
    }
}