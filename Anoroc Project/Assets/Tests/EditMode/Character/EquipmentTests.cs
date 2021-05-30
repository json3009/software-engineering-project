using NUnit.Framework;
using UnityEngine;
using System;
using System.Linq;

public class EquipmentTests
{

    /*
    EquipmentSettings settings;

    BodyPartFlag HeadPart;
    BodyPartFlag BodyPart;
    BodyPartFlag LegPart;
    BodyPartFlag ArmPart;
    BodyPartFlag FootPart;

    EquipmentLayer baseLayer;
    EquipmentLayer skinLayer;
    EquipmentLayer equipmentLayer;

    [SetUp]
    public void Setup()
    {
        settings = ScriptableObject.CreateInstance<EquipmentSettings>();

        HeadPart = new BodyPartFlag() { name = "Head", id = Guid.NewGuid() };
        BodyPart = new BodyPartFlag() { name = "Body", id = Guid.NewGuid() };
        LegPart = new BodyPartFlag() { name = "Leg", id = Guid.NewGuid() };
        ArmPart = new BodyPartFlag() { name = "Arm", id = Guid.NewGuid() };
        FootPart = new BodyPartFlag() { name = "Foot", id = Guid.NewGuid() };

        baseLayer = new EquipmentLayer() { name = "Base" , id = Guid.NewGuid() };
        skinLayer = new EquipmentLayer() { name = "Skin", id = Guid.NewGuid() };
        equipmentLayer = new EquipmentLayer() { name = "Equipment", id = Guid.NewGuid() };
    }

    [Test, Description("Tests if a character can equip some equipment")]
    public void can_equip_equipment()
    {
        // Arrange
        EquipmentObject toEquip = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        toEquip.EquipmentSlot = HeadPart;
        toEquip.Layer = baseLayer;

        // Act
        theEquipmentSystem.Equip(toEquip);

        // Assert
        Assert.AreSame(toEquip, theEquipmentSystem.GetEquipment(toEquip.EquipmentSlot, baseLayer.id));
    }



    [Test, Description("Tests if a character can not equip a piece of equipment when the given spot is already used")]
    public void can_not_equip_equipment_when_space_already_occupied()
    {
        // Arrange
        EquipmentObject toEquip1 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentObject toEquip2 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentObject toEquip3 = ScriptableObject.CreateInstance<EquipmentObject>();

        EquipmentManager theEquipmentSystem = new EquipmentManager();

        toEquip1.EquipmentSlot = HeadPart;
        toEquip2.EquipmentSlot = BodyPart;
        toEquip3.EquipmentSlot = BodyPart;

        toEquip1.Layer = baseLayer;
        toEquip2.Layer = baseLayer;
        toEquip3.Layer = baseLayer;

        // Act
        theEquipmentSystem.Equip(toEquip1);
        bool r1 = theEquipmentSystem.Equip(toEquip2);
        bool r2 = theEquipmentSystem.Equip(toEquip3);

        // Assert
        Assert.IsFalse(r1, "Could equip Head even though Head space was already occupied");
        Assert.IsFalse(r2, "Could equip Helmet even though Helmet space was already occupied");
    }

    [Test, Description("Tests if a character can unEquip some equipment")]
    public void can_unEquip_equipment()
    {
        // Arrange
        EquipmentObject toEquip = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        toEquip.EquipmentSlot = HeadPart;
        toEquip.Layer = baseLayer;

        // Act
        theEquipmentSystem.Equip(toEquip);
        theEquipmentSystem.UnEquip(toEquip);

        // Assert
        Assert.IsNull(theEquipmentSystem.GetEquipment(toEquip.EquipmentSlot, baseLayer.id));
    }

    [Test, Description("Tests if 'GetAllUsedSlots' function returns the correct equiped slots")]
    public void get_all_equiped_slots()
    {
        // Arrange
        EquipmentObject toEquip1 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentObject toEquip2 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentObject toEquip3 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        toEquip1.EquipmentSlot = HeadPart;
        toEquip2.EquipmentSlot = BodyPart;
        toEquip3.EquipmentSlot = LegPart;

        toEquip1.Layer = baseLayer;
        toEquip2.Layer = baseLayer;
        toEquip3.Layer = baseLayer;

        // Act
        theEquipmentSystem.Equip(toEquip1);
        theEquipmentSystem.Equip(toEquip2);
        theEquipmentSystem.Equip(toEquip3);

        // Assert
        var slots = theEquipmentSystem.GetAllUsedSlots(baseLayer.id);
        Assert.Contains(toEquip1.EquipmentSlot, slots);
        Assert.Contains(toEquip2.EquipmentSlot, slots);
        Assert.Contains(toEquip3.EquipmentSlot, slots);
    }


    [Test, Description("Tests if an equip call raises the 'OnEquip' event")]
    public void equip_raises_OnEquip_event()
    {
        // Arrange
        EquipmentObject toEquip = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        bool eventWasRaised = false;

        toEquip.EquipmentSlot = HeadPart;
        toEquip.Layer = baseLayer;

        // Act
        theEquipmentSystem.OnEquip += (e, r, t) => eventWasRaised = true;
        theEquipmentSystem.Equip(toEquip);

        // Assert
        Assert.IsTrue(eventWasRaised, "No event was detected");
    }

    [Test, Description("Tests if an invalid equip call doesn't raise the 'OnEquip' event")]
    public void invalid_equip_does_not_raise_OnEquip_event()
    {
        // Arrange
        EquipmentObject toEquip1 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentObject toEquip2 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        bool eventWasRaised = false;

        toEquip1.EquipmentSlot = HeadPart;
        toEquip2.EquipmentSlot = HeadPart;
        toEquip1.Layer = baseLayer;
        toEquip2.Layer = baseLayer;

        theEquipmentSystem.Equip(toEquip1);

        // Act
        theEquipmentSystem.OnEquip += (e, r, t) => eventWasRaised = true;
        theEquipmentSystem.Equip(toEquip2);

        // Assert
        Assert.IsFalse(eventWasRaised, "Event was risen, should not have");
    }

    [Test, Description("Tests if an unEquip call raises the 'OnUnEquip' event")]
    public void unEquip_raises_OnUnEquip_event()
    {
        // Arrange
        EquipmentObject toEquip = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        bool eventWasRaised = false;

        toEquip.EquipmentSlot = HeadPart;
        toEquip.Layer = baseLayer;

        // Act
        theEquipmentSystem.OnUnEquip += (e, r, t) => eventWasRaised = true;
        theEquipmentSystem.Equip(toEquip);
        theEquipmentSystem.UnEquip(toEquip);

        // Assert
        Assert.IsTrue(eventWasRaised, "No event was detected");
    }

    [Test, Description("Tests if an invalid unEquip call doesn't raise the 'OnUnEquip' event")]
    public void invalid_unEquip_does_not_raise_OnUnEquip_event()
    {
        // Arrange
        EquipmentObject toUnEquip1 = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        bool eventWasRaised = false;

        toUnEquip1.EquipmentSlot = HeadPart;
        toUnEquip1.Layer = baseLayer;

        // Act
        theEquipmentSystem.OnUnEquip += (e, r, t) => eventWasRaised = true;
        theEquipmentSystem.UnEquip(toUnEquip1);

        // Assert
        Assert.IsFalse(eventWasRaised, "Event was risen, should not have");
    }

    [Test, Description("Tests if an equip/unEquip call raises the 'OnEquipmentChange' event")]
    public void equip_unEquip_raises_OnEquipmentChange_event()
    {
        // Arrange
        EquipmentObject toEquip = ScriptableObject.CreateInstance<EquipmentObject>();
        EquipmentManager theEquipmentSystem = new EquipmentManager();

        int eventWasRaised = 0;

        toEquip.EquipmentSlot = HeadPart;
        toEquip.Layer = baseLayer;

        // Act
        theEquipmentSystem.OnEquipmentChange += (e, r, t) => eventWasRaised++;
        theEquipmentSystem.Equip(toEquip);
        theEquipmentSystem.UnEquip(toEquip);

        // Assert
        Assert.AreEqual(2, eventWasRaised);
    }

    */

}
