using System;
using NUnit.Framework;
using UnityEngine;
using Utilities;

namespace Scripts.BodySystem.Tests
{
    [TestFixture(Author = "Marx Jason", TestOf = typeof(BodyDefinition))]
    public class BodyDefinitionTests
    {
        #region Generalized Tests

        [Test(Description = "Can add a new BodyPart")]
        public void can_add_bodyPart()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            BodyPartFlag item = new BodyPartFlag() { id = Guid.NewGuid() };
            def.RootBodyPart.AddChild(item);

            // Assert
            Assert.Contains(item, def.BodyParts);
        }

        [Test(Description = "Can not add a new BodyPart with a Empty ID")]
        public void can_not_add_bodyPart_empty_id()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            BodyPartFlag item = new BodyPartFlag() { };
            def.RootBodyPart.AddChild(item);

            // Assert
            if (def.RootBodyPart.Children.Contains(item))
                Assert.Fail($"Child was inserted even though item had no ID");
        }

        [Test(Description = "Can not add duplicate Body Part")]
        public void can_not_add_duplicate_bodyPart()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyPartFlag item1 = new BodyPartFlag() { id = guid };
            BodyPartFlag item2 = new BodyPartFlag() { id = guid };

            // Act
            def.RootBodyPart.AddChild(item1);
            bool insert2 = def.RootBodyPart.AddChild(item1);
            bool insert3 = def.RootBodyPart.AddChild(item2);

            // Assert
            Assert.IsFalse(insert2, "Could add duplicate item!");
            Assert.IsFalse(insert3, "Could add item with same ID!");
        }

        [Test(Description = "Can remove BodyPart")]
        public void can_remove_bodyPart()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            BodyPartFlag item = new BodyPartFlag() { id = Guid.NewGuid() };
            def.RootBodyPart.AddChild(item);

            // Act
            bool result = def.RootBodyPart.RemoveChild(item);

            // Assert
            Assert.IsTrue(result, "Item could not be removed");

            if (def.RootBodyPart.Children.Contains(item))
                Assert.Fail($"Item was not removed!");
        }

        [Test(Description = "can add multiple bodyParts to multiple parts`")]
        public void can_add_multiple_bodyParts()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            BodyPartFlag item1 = new BodyPartFlag() { id = Guid.NewGuid() };
            BodyPartFlag item2 = new BodyPartFlag() { id = Guid.NewGuid() };
            BodyPartFlag item3 = new BodyPartFlag() { id = Guid.NewGuid() };
            BodyPartFlag item4 = new BodyPartFlag() { id = Guid.NewGuid() };
            BodyPartFlag item5 = new BodyPartFlag() { id = Guid.NewGuid() };

            // Act
            def.RootBodyPart.AddChild(item1);
            def.RootBodyPart.AddChild(item2);

            item1.AddChild(item3);
            item2.AddChild(item5);

            item5.AddChild(item4);

            // Assert
            Assert.AreEqual(6, def.GetAllBodyParts().Count);
        }

        [Test(Description = "Can add a layer")]
        public void can_add_layer()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };

            // Act
            def.AddLayer(layer);

            // Assert
            Assert.IsNotNull(def.GetLayer(layer.id));
        }

        [Test(Description = "Can not add a layer with an empty id")]
        public void can_not_add_layer_empty_id()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer = new BodyLayer();

            // Act
            bool insert = def.AddLayer(layer);

            // Assert
            Assert.IsFalse(insert, "Layer was added even though layer had no ID");
        }

        [Test(Description = "Can not add a duplicate layer")]
        public void can_not_add_duplicate_layer()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer = new BodyLayer() { id = guid };
            BodyLayer layer2 = new BodyLayer() { id = guid };

            // Act
            def.AddLayer(layer);
            bool insert2 = def.AddLayer(layer);
            bool insert3 = def.AddLayer(layer2);

            // Assert
            Assert.IsFalse(insert2, "Could add duplicate item!");
            Assert.IsFalse(insert3, "Could add item with same ID!");
        }

        [Test(Description = "Can add a new Body Side")]
        public void can_add_side()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodySide side = new BodySide() { id = Guid.NewGuid() };

            // Act
            def.AddSide(side);

            // Assert
            Assert.IsNotNull(def.GetSide(side.id));
        }

        [Test(Description = "Can not add a side with an empty id")]
        public void can_not_add_side_empty_id()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodySide side = new BodySide();

            // Act
            bool insert = def.AddSide(side);

            // Assert
            Assert.IsFalse(insert, "Side was added even though side had no ID");
        }

        [Test(Description = "Can not add a duplicate Side")]
        public void can_not_add_duplicate_side()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodySide side = new BodySide() { id = guid };
            BodySide side2 = new BodySide() { id = guid };

            // Act
            def.AddSide(side);
            bool insert2 = def.AddSide(side);
            bool insert3 = def.AddSide(side2);

            // Assert
            Assert.IsFalse(insert2, "Could add duplicate item!");
            Assert.IsFalse(insert3, "Could add item with same ID!");
        }

        [Test(Description = "Body Definition is not valid when no body parts")]
        public void no_bodyParts_is_not_valid()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };
            BodySide side = new BodySide() { id = Guid.NewGuid() };

            def.AddLayer(layer);
            def.AddSide(side);

            // Act
            bool isValid = def.IsValid(out BodyDefinition.BodyDefinitionErrorCode[] errors);

            // Assert
            Assert.IsFalse(isValid, "Body Definition was valid even though no children were defined!");
            Assert.Contains(BodyDefinition.BodyDefinitionErrorCode.NoChildren, errors);
        }

        [Test(Description = "Body Definition is not valid when no layers")]
        public void no_layers_is_not_valid()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodySide side = new BodySide() { id = Guid.NewGuid() };
            BodyPartFlag bodyPart = new BodyPartFlag() { id = Guid.NewGuid() };

            def.RootBodyPart.AddChild(bodyPart);
            def.AddSide(side);

            // Act
            bool isValid = def.IsValid(out BodyDefinition.BodyDefinitionErrorCode[] errors);

            // Assert
            Assert.IsFalse(isValid, "Body Definition was valid even though no Layers were defined!");
            Assert.Contains(BodyDefinition.BodyDefinitionErrorCode.NoLayers, errors);
        }

        [Test(Description = "Body Definition is not valid when no sides")]
        public void no_side_is_not_valid()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyPartFlag bodyPart = new BodyPartFlag() { id = Guid.NewGuid() };
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };

            def.AddLayer(layer);
            def.RootBodyPart.AddChild(bodyPart);

            // Act
            bool isValid = def.IsValid(out BodyDefinition.BodyDefinitionErrorCode[] errors);

            // Assert
            Assert.IsFalse(isValid, "Body Definition was valid even though no Sides were defined!");
            Assert.Contains(BodyDefinition.BodyDefinitionErrorCode.NoSides, errors);
        }

        #endregion

        #region Specific Tests

        [Test(Description = "AddLayer can add a new layer")]
        public void AddLayer_can_add_layer()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddLayer(new BodyLayer() { id = guid });

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "AddLayer can not add a new layer when ID is NULL")]
        public void AddLayer_returns_false_if_id_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddLayer(new BodyLayer());

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "AddLayer can not add a new layer when ID is EMPTY")]
        public void AddLayer_returns_false_if_id_empty()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddLayer(new BodyLayer() { id = Guid.Empty });

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "AddLayer can not add a new layer when ID is already exists")]
        public void AddLayer_returns_false_if_id_already_exists()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            def.AddLayer(new BodyLayer() { id = guid });
            var res2 = def.AddLayer(new BodyLayer() { id = guid });

            // Assert
            Assert.IsFalse(res2);
        }

        [Test(Description = "AddLayer can not add a new layer when Layer is NULL")]
        public void AddLayer_returns_false_bodyLayer_is_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddLayer(null);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "AddSide can add a new Side")]
        public void AddSide_can_add_side()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddSide(new BodySide() { id = guid });

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "AddSide can not add a new side when ID is NULL")]
        public void AddSide_returns_false_if_id_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddSide(new BodySide());

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "AddSide can not add a new side when ID is EMPTY")]
        public void AddSide_returns_false_if_id_empty()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddSide(new BodySide() { id = Guid.Empty });

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "AddSide can not add a new side when ID already exists")]
        public void AddSide_returns_false_if_id_already_exists()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            def.AddSide(new BodySide() { id = guid });
            var res2 = def.AddSide(new BodySide() { id = guid });

            // Assert
            Assert.IsFalse(res2);
        }

        [Test(Description = "AddSide can not add a new side when Side is NULL")]
        public void AddSide_returns_false_if_bodySide_is_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.AddSide(null);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "GetPartByID returns body part ID")]
        public void GetPartByID_can_find_part()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            def.RootBodyPart.AddChild(new BodyPartFlag() { id = guid });

            // Act
            var obj = def.GetPartByID(guid);

            // Assert
            Assert.AreEqual((SerializableGUID)guid, obj.id);
        }

        [Test(Description = "GetPartByID returns body part [None] ID")]
        public void GetPartByID_returns_none_part_if_not_found()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetPartByID(guid);

            // Assert
            Assert.AreEqual(BodyPartFlag.None.id, obj.id);
        }

        [Test(Description = "GetLayer returns layer by ID")]
        public void GetLayer_returns_layer_by_id()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            def.AddLayer(new BodyLayer() { id = guid });

            // Act
            var obj = def.GetLayer(guid);

            // Assert
            Assert.AreEqual((SerializableGUID)guid, obj.id);
        }

        [Test(Description = "GetLayer returns null if ID was empty")]
        public void GetLayer_returns_null_if_id_empty()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetLayer(Guid.Empty);

            // Assert
            Assert.IsNull(obj);
        }

        [Test(Description = "GetLayer returns null if ID was null")]
        public void GetLayer_returns_null_if_id_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetLayer(default);

            // Assert
            Assert.IsNull(obj);
        }

        [Test(Description = "GetLayer returns bull if ID not found")]
        public void GetLayer_returns_null_if_none_found()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetLayer(Guid.NewGuid());

            // Assert
            Assert.IsNull(obj);
        }

        [Test(Description = "GetLayerIndex returns index of layer")]
        public void GetLayerIndex_returns_index()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer1 = new BodyLayer() { id = Guid.NewGuid() };
            BodyLayer layer2 = new BodyLayer() { id = Guid.NewGuid() };

            // Act
            def.AddLayer(layer1);
            def.AddLayer(layer2);

            // Assert
            Assert.AreEqual(1, def.GetLayerIndex(layer2));
        }

        [Test(Description = "GetLayerIndex returns -1 if layer not found")]
        public void GetLayerIndex_returns_negative1_if_not_found()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer1 = new BodyLayer() { id = Guid.NewGuid() };

            // Act
            def.AddLayer(layer1);

            // Assert
            Assert.AreEqual(-1, def.GetLayerIndex(Guid.NewGuid()));
        }

        [Test(Description = "GetLayerByIndex returns layer by index")]
        public void GetLayerByIndex_returns_bodyPart_by_index()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer1 = new BodyLayer() { id = Guid.NewGuid() };

            // Act
            def.AddLayer(layer1);

            // Assert
            Assert.AreEqual(layer1.id, def.GetLayerByIndex(0).id);
        }

        [Test(Description = "GetLayerByIndex returns null if index was not found")]
        public void GetLayerByIndex_returns_null_if_not_found()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Assert
            Assert.IsNull(def.GetLayerByIndex(3));
        }

        [Test(Description = "GetAllLayers returns all layers")]
        public void GetAllLayers_returns_all_layers()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodyLayer layer1 = new BodyLayer() { id = Guid.NewGuid() };
            BodyLayer layer2 = new BodyLayer() { id = Guid.NewGuid() };

            // Act
            def.AddLayer(layer1);
            def.AddLayer(layer2);

            // Assert
            Assert.AreEqual(2, def.GetAllLayers().Count);
        }

        [Test(Description = "GetSide returns side by ID")]
        public void GetSide_returns_side_by_id()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();
            BodySide side = new BodySide() { id = Guid.NewGuid() };
            BodySide side2 = new BodySide() { id = Guid.NewGuid() };

            // Act
            def.AddSide(side);
            def.AddSide(side2);

            // Assert
            Assert.AreEqual(side2.id, def.GetSide(side2.id).id);
        }

        [Test(Description = "GetSide returns null if ID was empty")]
        public void GetSide_returns_null_if_id_empty()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetSide(Guid.Empty);

            // Assert
            Assert.IsNull(obj);
        }

        [Test(Description = "GetSide returns null if ID was null")]
        public void GetSide_returns_null_if_id_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var obj = def.GetSide(default);

            // Assert
            Assert.IsNull(obj);
        }

        [Test(Description = "RemoveLayer can remove an existing Layer by ID")]
        public void RemoveLayer_can_remove_layer_by_id()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            def.AddLayer(new BodyLayer() { id = guid });

            // Act
            var res = def.RemoveLayer(guid);

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "RemoveLayer can remove an existing Layer")]
        public void RemoveLayer_can_remove_layer()
        {
            // Arrange
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            def.AddLayer(layer);

            // Act
            var res = def.RemoveLayer(layer);

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "RemoveLayer returns false if Layer ID doesn't exist")]
        public void RemoveLayer_returns_False_if_layer_unexistant_by_id()
        {
            // Arrange
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveLayer(layer.id);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "RemoveLayer returns false if Layer doesn't exist")]
        public void RemoveLayer_returns_False_if_layer_unexistant()
        {
            // Arrange
            BodyLayer layer = new BodyLayer() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveLayer(layer);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "RemoveLayer returns false if Layer is NULL")]
        public void RemoveLayer_returns_False_if_layer_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveLayer(null);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "RemoveSide can remove an existing Side by ID")]
        public void RemoveSide_can_remove_side_by_id()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            def.AddSide(new BodySide() { id = guid });

            // Act
            var res = def.RemoveSide(guid);

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "RemoveSide can remove an existing Side")]
        public void RemoveSide_can_remove_side()
        {
            // Arrange
            BodySide side = new BodySide() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            def.AddSide(side);

            // Act
            var res = def.RemoveSide(side);

            // Assert
            Assert.IsTrue(res);
        }

        [Test(Description = "RemoveSide returns false if Side ID doesn't exist")]
        public void RemoveSide_returns_False_if_side_unexistant_by_id()
        {
            // Arrange
            BodySide side = new BodySide() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveSide(side.id);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "RemoveSide returns false if Side doesn't exist")]
        public void RemoveSide_returns_False_if_side_unexistant()
        {
            // Arrange
            BodySide side = new BodySide() { id = Guid.NewGuid() };
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveSide(side);

            // Assert
            Assert.IsFalse(res);
        }

        [Test(Description = "RemoveSide returns false if Side is NULL")]
        public void RemoveSide_returns_False_if_side_null()
        {
            // Arrange
            BodyDefinition def = ScriptableObject.CreateInstance<BodyDefinition>();

            // Act
            var res = def.RemoveSide(null);

            // Assert
            Assert.IsFalse(res);
        }

        #endregion
    }
}
