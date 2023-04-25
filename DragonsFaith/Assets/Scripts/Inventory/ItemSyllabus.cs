using UnityEngine;
using System.Linq;

namespace Inventory
{
    /// <summary>
    /// Utility class used as a catalogue for items
    /// </summary>
    public class ItemSyllabus : MonoBehaviour
    {
        
         [SerializeField] private Item[] itemList;

        public static ItemSyllabus Instance { get; private set; }
        private void Awake() 
        { 
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 
        }
        
        /// <summary>
        /// Get an item
        /// </summary>
        public Item SearchItem(string idOrName)
        {
            var item = itemList.First(item => (item.id == idOrName) || (item.name == idOrName));
            if (item == null) Debug.LogError("Invalid name or id");
            return item;
        }
    }
}
