using CharacterSystem;

namespace InventorySystem.UI
{
    public interface IInventoryUI
    {
        public InventoryManager Inventory { get; }
        public Character Character { get; }
    }
}