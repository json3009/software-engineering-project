using CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _placeholder;
        [SerializeField] private Button _removeBTN;

        private InventoryObject _item;
        private IInventoryUI _uiSystem;

        public InventoryObject Item
        {
            get => _item;
            set => _item = value;
        }

        public IInventoryUI UISystem
        {
            get => _uiSystem;
            set => _uiSystem = value;
        }


        public void SetSlot(InventoryObject obj)
        {
            _item = obj;
            _icon.sprite = obj.Icon;
            _icon.enabled = true;
            
            if(_placeholder)
                _placeholder.SetActive(false);

            if(_removeBTN)
                _removeBTN.interactable = true;
        }

        public void ResetSlot()
        {
            _item = null;
            _icon.sprite = null;
            _icon.enabled = false;      
            
            if(_placeholder)
                _placeholder.SetActive(true);
            
            if(_removeBTN)
                _removeBTN.interactable = false;
        }

        public void OnDeleteBTN()
        {
            if(_item)
                UISystem.Inventory.Drop(_item, UISystem.Character.transform.position, UISystem.Character.gameObject);
        }

        public void OnUseBTN()
        {

            if (_item)
                UISystem.Inventory.UseItem(_item, UISystem.Character);
        }
    }
}