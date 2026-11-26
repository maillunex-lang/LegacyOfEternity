using System;
using UnityEngine;

namespace LegacyOfEternity.Player
{
    /// <summary>Visual-only equipment slot. Future inventory systems can replace the model without changing combat.</summary>
    public sealed class WeaponEquipment : MonoBehaviour
    {
        [SerializeField] private Transform rightHandSocket;
        [SerializeField] private GameObject equippedWeapon;
        [SerializeField] private string equipmentId;
        public Transform RightHandSocket => rightHandSocket;
        public GameObject EquippedWeapon => equippedWeapon;
        [SerializeField] private Transform leftHandSocket;
        [SerializeField] private GameObject leftEquippedWeapon;
        [SerializeField] private string leftEquipmentId;
        public Transform LeftHandSocket => leftHandSocket;
        public GameObject LeftEquippedWeapon => leftEquippedWeapon;
        public string LeftEquipmentId => leftEquipmentId;
        public string EquipmentId => equipmentId;

        public void ConfigureLeft(Transform hand, GameObject weapon, string id)
        {
            if (hand == null || weapon == null || !hand.IsChildOf(transform))
                throw new ArgumentException("Second weapon socket and model must belong to this character visual");
            leftHandSocket = hand;
            leftEquippedWeapon = weapon;
            leftEquipmentId = id;
        }

        public GameObject EquipLeft(GameObject prefab, string id)
        {
            if (leftHandSocket == null || prefab == null)
                throw new InvalidOperationException("Left-hand equipment slot is not configured");
            if (leftEquippedWeapon != null) Destroy(leftEquippedWeapon);
            var instance = Instantiate(prefab, leftHandSocket, false);
            instance.name = "Equipped left " + id;
            leftEquippedWeapon = instance;
            leftEquipmentId = id;
            return instance;
        }

        public void Configure(Transform hand, GameObject weapon, string id)
        {
            if (hand == null || weapon == null || !hand.IsChildOf(transform))
                throw new ArgumentException("Weapon socket and model must belong to this character visual");
            rightHandSocket = hand;
            equippedWeapon = weapon;
            equipmentId = id;
        }

        public GameObject Equip(GameObject prefab, string id)
        {
            if (rightHandSocket == null || prefab == null)
                throw new InvalidOperationException("Weapon slot has no socket or prefab");
            if (equippedWeapon != null) Destroy(equippedWeapon);
            var instance = Instantiate(prefab, rightHandSocket, false);
            instance.name = "Equipped " + id;
            equippedWeapon = instance;
            equipmentId = id;
            return instance;
        }
    }
}
