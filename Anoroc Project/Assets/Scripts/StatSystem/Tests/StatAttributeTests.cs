using NUnit.Framework;
using OdinSerializer;
using StatSystem.StatAttributes;
using StatSystem.StatModifiers;
using static StatSystem.StatModifiers.FloatModifier;


namespace StatSystem.Tests
{
    public class StatAttributeTests
    {
        [Test(Description = "FloatAttribute Constructor correctly sets Base Value")]
        public void StatAttribute_Constructor_can_set_base_value()
        {
            // Arrange
            FloatAttribute damage;

            // Act
            damage = new FloatAttribute(10);

            // Assert
            Assert.AreEqual(10, damage.BaseValue);
        }

        [Test(Description = "FloatAttribute Constructor correctly adds provided Modifiers")]
        public void StatAttribute_Constructor_can_add_Modifiers()
        {
            // Arrange
            FloatAttribute damage;
            FloatModifier mod1 = new FloatModifier(3, FloatModifierType.Flat);
            FloatModifier mod2 = new FloatModifier(9, FloatModifierType.Flat);
            FloatModifier mod3 = new FloatModifier(1.5f, FloatModifierType.Percent);

            // Act
            damage = new FloatAttribute(10,
                mod1,
                mod2,
                mod3
            );

            // Assert
            Assert.Contains(mod1, damage.Modifiers);
            Assert.Contains(mod2, damage.Modifiers);
            Assert.Contains(mod3, damage.Modifiers);
        }

        [Test(Description = "FloatAttribute Merge can correctly merge two attributes into one")]
        public void StatAttribute_Merge_can_merge_two_attributes()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10, 
                new FloatModifier(3, FloatModifierType.Flat),
                new FloatModifier(9, FloatModifierType.Flat),
                new FloatModifier(0.5f, FloatModifierType.Percent)
            );

            FloatModifier mod1 = new FloatModifier(5, FloatModifierType.Flat);
            FloatAttribute damageExtra = new FloatAttribute(2,
                mod1,
                new FloatModifier(0.9f, FloatModifierType.Multiply)
            );

            // Act
            damage.Merge(damageExtra);

            // Assert
            Assert.Contains(mod1, damage.Modifiers);
        }

        [Test(Description = "FloatAttribute Merge triggers a recalculation")]
        public void StatAttribute_Merge_triggers_recalculation()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10,
                new FloatModifier(3, FloatModifierType.Flat),
                new FloatModifier(9, FloatModifierType.Flat),
                new FloatModifier(0.5f, FloatModifierType.Percent)
            );

            FloatAttribute damageExtra = new FloatAttribute(2,
                new FloatModifier(5, FloatModifierType.Flat),
                new FloatModifier(0.9f, FloatModifierType.Multiply)
            );

            float oldVal = damage.Value;

            // Act
            damage.Merge(damageExtra);
            float newVal = damage.Value;

            // Assert
            Assert.AreNotEqual(newVal, oldVal);
        }

        [Test(Description = "FloatAttribute Merge sorts all modifiers after merge is finished")]
        public void StatAttribute_Merge_modifiers_get_sorted()
        {
            // Arrange
            FloatModifier mod1 = new FloatModifier(3, FloatModifierType.Flat);
            FloatModifier mod2 = new FloatModifier(0.5f, FloatModifierType.Percent);
            FloatModifier mod3 = new FloatModifier(9, FloatModifierType.Flat);
            FloatAttribute damage = new FloatAttribute(10, mod1, mod2, mod3);

            FloatModifier mod4 = new FloatModifier(3, FloatModifierType.Flat);
            FloatAttribute damageExtra = new FloatAttribute(2, mod4);

            // Act
            damage.Merge(damageExtra);

            // Assert
            Assert.AreSame(mod1, damage.Modifiers[0]);
            Assert.AreSame(mod3, damage.Modifiers[1]);
            Assert.AreSame(mod4, damage.Modifiers[2]);
            Assert.AreSame(mod2, damage.Modifiers[3]);
        }


        [Test(Description = "FloatAttribute AddModifier correctly adds provided Modifiers")]
        public void StatAttribute_AddModifier_can_add_Modifiers()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);
            FloatModifier mod1 = new FloatModifier(3, FloatModifierType.Flat);
            FloatModifier mod2 = new FloatModifier(9, FloatModifierType.Flat);
            FloatModifier mod3 = new FloatModifier(0.5f, FloatModifierType.Percent);

            // Act
            damage.AddModifier(mod1, mod2, mod3);

            // Assert
            Assert.Contains(mod1, damage.Modifiers);
            Assert.Contains(mod2, damage.Modifiers);
            Assert.Contains(mod3, damage.Modifiers);
        }

        [Test(Description = "FloatAttribute AddModifier can not add Modifier when null")]
        public void StatAttribute_AddModifier_can_not_add_Modifiers_when_null()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);

            // Act
            damage.AddModifier(new FloatModifier(3, FloatModifierType.Flat), null);

            // Assert
            Assert.AreEqual(1, damage.Modifiers.Length);
        }

        [Test(Description = "FloatAttribute AddModifier triggers a recalculation")]
        public void StatAttribute_AddModifier_triggers_recalculation()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);
            float preValue = damage.Value;

            // Act
            damage.AddModifier(new FloatModifier(3, FloatModifierType.Flat), null);
            float newValue = damage.Value;

            // Assert
            Assert.AreNotEqual(newValue, preValue);
        }

        [Test(Description = "FloatAttribute AddModifier sorts all modifiers after insertion")]
        public void StatAttribute_AddModifier_modifiers_get_sorted()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);
            FloatModifier mod1 = new FloatModifier(3, FloatModifierType.Flat);
            FloatModifier mod2 = new FloatModifier(0.7f, FloatModifierType.Multiply);
            FloatModifier mod3 = new FloatModifier(0.5f, FloatModifierType.Percent);
            FloatModifier mod4 = new FloatModifier(9, FloatModifierType.Flat);

            // Act
            damage.AddModifier(mod1, mod2, mod3, mod4);

            // Assert
            Assert.AreSame(mod1, damage.Modifiers[0]);
            Assert.AreSame(mod4, damage.Modifiers[1]);
            Assert.AreSame(mod3, damage.Modifiers[2]);
            Assert.AreSame(mod2, damage.Modifiers[3]);
        }

        [Test(Description = "FloatAttribute RemoveModifier can remove a given modifier")]
        public void StatAttribute_RemoveModifier_can_remove_modifier()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);
            FloatModifier mod1 = new FloatModifier(3, FloatModifierType.Flat);

            damage.AddModifier(mod1);

            // Act
            bool res = damage.RemoveModifier(mod1);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual(0, damage.Modifiers.Length);
        }

        [Test(Description = "FloatAttribute RemoveModifier can not remove an nonexistent modifier")]
        public void StatAttribute_RemoveModifier_can_not_remove_nonexistent_modifier()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);

            // Act
            bool res = damage.RemoveModifier(new FloatModifier(1, FloatModifierType.Flat));

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "FloatAttribute RemoveModifier triggers recalculation")]
        public void StatAttribute_RemoveModifier_triggers_recalculation()
        {
            // Arrange
            FloatModifier mod1 = new FloatModifier(5, FloatModifierType.Flat);
            FloatAttribute damage = new FloatAttribute(10, mod1);
            float preValue = damage.Value;

            // Act
            damage.RemoveModifier(mod1);
            float newValue = damage.Value;

            // Assert
            Assert.AreNotEqual(newValue, preValue);
        }

        /*
        [Test(Description = "FloatAttribute RemoveModifiers can remove all modifiers by source")]
        public void StatAttribute_RemoveModifiers_can_remove_modifier_by_source()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);

            FloatModifier mod1 = new FloatModifier(9, FloatModifierType.Constant, damage);
            FloatModifier mod2 = new FloatModifier(3, FloatModifierType.Flat, damage);
            FloatModifier mod3 = new FloatModifier(6, FloatModifierType.Flat, damage);
            FloatModifier mod4 = new FloatModifier(0.3f, FloatModifierType.Percent, null);

            damage.AddModifier(mod1, mod2, mod3, mod4);

            // Act
            bool res = damage.RemoveModifiers(damage);

            // Assert
            Assert.IsTrue(res);
            Assert.AreEqual(1, damage.Modifiers.Length);
        }*/

        [Test(Description = "FloatAttribute RemoveModifiers returns false if source is null")]
        public void StatAttribute_RemoveModifiers_returns_false_when_source_null()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);

            // Act
            bool res = damage.RemoveModifiers(null);

            // Assert
            Assert.IsFalse(res);
        }

        /*[Test(Description = "FloatAttribute RemoveModifiers triggers recalculation")]
        public void StatAttribute_RemoveModifiers_triggers_recalculation()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);

            FloatModifier mod1 = new FloatModifier(9, FloatModifierType.Constant, damage);
            FloatModifier mod2 = new FloatModifier(3, FloatModifierType.Flat, damage);
            FloatModifier mod3 = new FloatModifier(6, FloatModifierType.Flat, damage);
            FloatModifier mod4 = new FloatModifier(0.3f, FloatModifierType.Percent, null);

            damage.AddModifier(mod1, mod2, mod3, mod4);
            float preValue = damage.Value;

            // Act
            damage.RemoveModifiers(damage);
            float newValue = damage.Value;

            // Assert
            Assert.AreNotEqual(newValue, preValue);
        }*/

        [Test(Description = "FloatAttribute set Base Value triggers recalculation")]
        public void StatAttribute_BaseValue_triggers_recalculation()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(10);
            float preValue = damage.Value;

            // Act
            damage.BaseValue = 15;
            float newValue = damage.Value;

            // Assert
            Assert.AreNotEqual(newValue, preValue);
        }

        [Test(Description = "FloatAttribute RecalculateValue calculates correct value")]
        public void StatAttribute_RecalculateValue_calculates_correct_value()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(5);

            //Act
            damage.AddModifier(
                new FloatModifier(9, FloatModifierType.Constant),
                new FloatModifier(3, FloatModifierType.Flat),
                new FloatModifier(-5, FloatModifierType.Flat),
                new FloatModifier(4, FloatModifierType.Flat),
                new FloatModifier(0.3f, FloatModifierType.Percent),
                new FloatModifier(0.1f, FloatModifierType.Percent),
                new FloatModifier(-0.2f, FloatModifierType.Percent),
                new FloatModifier(1.2f, FloatModifierType.Multiply),
                new FloatModifier(2f, FloatModifierType.Multiply)
            );

            // 9 + 3 - 5 + 4 = 11
            // 0.3 + 0.1 - 0.2 = 0.2
            // 11 * (1 + 0.2) = 13.2
            // 13.2 * (1.2) * (2) = 31.68

            // Assert
            Assert.AreEqual(31.68f, damage.Value);
        }

        [Test(Description = "FloatAttribute RecalculateValue calculates correct negative [percentage] value")]
        public void StatAttribute_RecalculateValue_calculates_correct_negative_value()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(5);

            //Act
            damage.AddModifier(
                new FloatModifier(-0.3f, FloatModifierType.Percent)
            );
            // 5 * (1 - 0.3) = 6.3

            // Assert
            Assert.AreEqual(3.5f, damage.Value);
        }

        [Test(Description = "FloatAttribute RequestRecalculation triggers a recalculation")]
        public void StatAttribute_RequestRecalculation_triggers_recalculation()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(5, 
                new FloatModifier(-0.3f, FloatModifierType.Percent)
            );

            float oldVal = damage.Value;

            var valField = typeof(FloatAttribute).GetField("_baseValue", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance
            );

            valField.SetValue(damage, 15);

            //Act
            damage.RequestRecalculation();
            float newVal = damage.Value;

            // Assert
            Assert.AreNotEqual(newVal, oldVal);
        }

        [Test(Description = "StatAttribute gets serialized correctly")]
        public void StatAttribute_Serialization_Test()
        {
            // Arrange
            FloatAttribute damage = new FloatAttribute(5);

            //Act
            damage.AddModifier(
                new FloatModifier(9, FloatModifierType.Constant),
                new FloatModifier(3, FloatModifierType.Flat),
                new FloatModifier(-5, FloatModifierType.Flat),
                new FloatModifier(4, FloatModifierType.Flat),
                new FloatModifier(0.3f, FloatModifierType.Percent),
                new FloatModifier(0.1f, FloatModifierType.Percent),
                new FloatModifier(-0.2f, FloatModifierType.Percent),
                new FloatModifier(0.5f, FloatModifierType.Multiply),
                new FloatModifier(0.7f, FloatModifierType.Multiply)
            );

            //Act
            byte[] serializedString = SerializationUtility.SerializeValue(damage, DataFormat.JSON);
            FloatAttribute deserializedAttr = SerializationUtility.DeserializeValue<FloatAttribute>(serializedString, DataFormat.JSON);

            // Assert
            for(int i = 0; i < deserializedAttr.Modifiers.Length; i++)
            {
                Assert.AreEqual(damage.Modifiers[i], deserializedAttr.Modifiers[i]);
            }
        }
    }
}
