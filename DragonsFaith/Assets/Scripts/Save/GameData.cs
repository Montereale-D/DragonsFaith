using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Unity.Netcode;
using UnityEngine;

namespace Save
{
    [Serializable]
    public class GameData : INetworkSerializable
    {
        public PlayerData HostData => hostData;

        public PlayerData ClientData => clientData;

        [Serializable]
        public enum PlayerType
        {
            Host,
            Client
        }

        [Serializable]
        public enum InventoryType
        {
            Inventory,
            Equipment
        }

        [Serializable]
        public struct PlayerData : INetworkSerializable
        {
            [SerializeField] public PlayerType playerType;
            [SerializeField] public List<ItemData> itemDataList;
            [SerializeField] public int health;
            [SerializeField] public int maxHealth;
            [SerializeField] public int mana;
            [SerializeField] public int maxMana;

            public PlayerData(PlayerType playerType, List<ItemData> itemDataList)
            {
                this.playerType = playerType;
                this.itemDataList = itemDataList == null ? new List<ItemData>() : new List<ItemData>(itemDataList);
                health = maxHealth = 100;
                mana = maxMana = 100;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref playerType);

                // List
                var length = 0;
                var array = Array.Empty<ItemData>();

                if (!serializer.IsReader)
                {
                    array = itemDataList.ToArray();
                    length = itemDataList.Count;
                }

                serializer.SerializeValue(ref length);

                if (serializer.IsReader)
                {
                    array = new ItemData[length];
                }

                for (var n = 0; n < length; ++n)
                {
                    array[n].NetworkSerialize(serializer);
                }

                if (serializer.IsReader)
                {
                    itemDataList = array.ToList();
                }

                //bars
                serializer.SerializeValue(ref health);
                serializer.SerializeValue(ref maxHealth);
                serializer.SerializeValue(ref mana);
                serializer.SerializeValue(ref maxMana);
            }

            public override string ToString()
            {
                return "Type: " + playerType + " Health: " + health + "/" + maxHealth + " Mana: " + mana + "/" + maxMana
                       + "\n Items: " + PrintItemList(itemDataList);
            }

            private string PrintItemList(IEnumerable<ItemData> list)
            {
                return list.Aggregate("", (current, itemData) => current + (itemData + " "));
            }
        }

        [Serializable]
        public struct ItemData : INetworkSerializable
        {
            public string itemId;
            public int slotNumber;
            public int quantity;
            [SerializeField] public InventoryType inventoryType;

            public ItemData(string itemId, InventoryType inventoryType, int slotNumber, int quantity)
            {
                this.itemId = itemId;
                this.slotNumber = slotNumber;
                this.quantity = quantity;
                this.inventoryType = inventoryType;
            }


            public override string ToString()
            {
                return "Item: " + itemId + " x" + quantity;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref itemId);
                serializer.SerializeValue(ref inventoryType);
                serializer.SerializeValue(ref slotNumber);
                serializer.SerializeValue(ref quantity);
            }
        }

        [SerializeField] private PlayerData hostData;
        [SerializeField] private PlayerData clientData;

        public GameData()
        {
            hostData = new PlayerData(PlayerType.Host, new List<ItemData>());
            clientData = new PlayerData(PlayerType.Client, new List<ItemData>());
        }

        /// <summary>
        /// Add an item to the tmp file data
        /// </summary>
        public void AddItemData(PlayerType player, Item item, InventoryType inventoryType, int slotNumber, int quantity)
        {
            if (player == PlayerType.Host)
                hostData.itemDataList.Add(new ItemData(item.id, inventoryType, slotNumber, quantity));
            else
                clientData.itemDataList.Add(new ItemData(item.id, inventoryType, slotNumber, quantity));
        }

        /// <summary>
        /// Overwrite inventory tmp file data
        /// </summary>
        public void UpdateInventoryData(PlayerType player, IEnumerable<ItemData> inventory)
        {
            if (player == PlayerType.Host)
                hostData.itemDataList = new List<ItemData>(inventory);
            else
                clientData.itemDataList = new List<ItemData>(inventory);
        }

        /// <summary>
        /// Clean up inventory tmp file data
        /// </summary>
        public void CleanupItemData(PlayerType player)
        {
            if (player == PlayerType.Host)
                hostData.itemDataList = new List<ItemData>();
            else
                clientData.itemDataList = new List<ItemData>();
        }

        /// <summary>
        /// Get all items from tmp file data
        /// </summary>
        public List<ItemData> GetAllItemsData(PlayerType player)
        {
            return player == PlayerType.Host ? hostData.itemDataList : clientData.itemDataList;
        }

        public void UpdateBarsData(PlayerType player, int health, int maxHealth, int mana, int maxMana)
        {
            Debug.Log("UpdateBarsData " + player);
            
            if (player == PlayerType.Host)
            {
                hostData.health = health;
                hostData.maxHealth = maxHealth;
                hostData.mana = mana;
                hostData.maxMana = maxMana;
                
                Debug.Log(hostData.ToString());
            }
            else
            {
                clientData.health = health;
                clientData.maxHealth = maxHealth;
                clientData.mana = mana;
                clientData.maxMana = maxMana;
            }
        }

        public void GetBarsData(PlayerType player, out int health, out int maxHealth, out int mana, out int maxMana)
        {
            if (player == PlayerType.Host)
            {
                health = hostData.health;
                maxHealth = hostData.maxHealth;
                mana = hostData.mana;
                maxMana = hostData.maxMana;
            }
            else
            {
                health = clientData.health;
                maxHealth = clientData.maxHealth;
                mana = clientData.mana;
                maxMana = clientData.maxMana;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            hostData.NetworkSerialize(serializer);
            clientData.NetworkSerialize(serializer);
        }

        public override string ToString()
        {
            return "Host data: " + hostData + "\nClient data: " + clientData;
        }

        public static PlayerType GetPlayerType(bool isHost)
        {
            return isHost ? PlayerType.Host : PlayerType.Client;
        }
    }
}