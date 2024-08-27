using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static Dman.SaveSystem.Tests.SaveDataTestUtils;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Dman.SaveSystem.Tests
{
    [Serializable]
    public class SerializableAnimal
    {

        [SerializeField] private string name;
        [SerializeField] private int age;

        // intentionally obfuscated constructor, ensures private fields cannot be populated from json via this constructor
        public SerializableAnimal(int choice)
        {
            switch (choice)
            {
                case 0:
                    name = "Borg";
                    age = 3000;
                    break;
                case 1:
                    name = "Fido";
                    age = 3;
                    break;
                case 2:
                    name = "Mr. green";
                    age = 6;
                    break;
                case 3:
                    name = "Eternal Crab";
                    age = 3;
                    break;
            }
        }

        public override string ToString()
        {
            return $"animal: {name}, {age}yrs";
        }

        protected bool Equals(SerializableAnimal other)
        {
            return name == other.name && age == other.age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableAnimal)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0) * 397) ^ age;
            }
        }
    }

    [Serializable]
    public class SerializableDog : SerializableAnimal
    {
        [SerializeField] private string taggedName;

        // intentionally obfuscated constructor, ensures private fields cannot be populated from json via this constructor
        public SerializableDog(int choice, string taggedName) : base(choice)
        {
            this.taggedName = taggedName;
        }
        
        public override string ToString()
        {
            return $"dog: {taggedName}, {base.ToString()}";
        }
        protected bool Equals(SerializableDog other)
        {
            return base.Equals(other) && taggedName == other.taggedName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableDog)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (taggedName != null ? taggedName.GetHashCode() : 0);
            }
        }

    }

    [Serializable]
    public class SerializableCat : SerializableAnimal
    {
        [SerializeField] private Personality personality;

        // intentionally obfuscated constructor, ensures private fields cannot be populated from json via this constructor
        public SerializableCat(int choice, Personality personality) : base(choice)
        {
            this.personality = personality;
        }
        public override string ToString()
        {
            return $"cat: {personality}, {base.ToString()}";
        }
        
        protected bool Equals(SerializableCat other)
        {
            return base.Equals(other) && personality == other.personality;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableCat)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int)personality;
            }
        }
    }
    
    [Serializable]
    public class PartiallySerializableAnimal
    {
        [SerializeField] private string name;
        private int age;

        // intentionally obfuscated constructor, ensures private fields cannot be populated from json via this constructor
        public PartiallySerializableAnimal(int choice)
        {
            switch (choice)
            {
                case 0:
                    name = "Borg";
                    age = 3000;
                    break;
                case 1:
                    name = "Fido";
                    age = 3;
                    break;
                case 2:
                    name = "Mr. green";
                    age = 6;
                    break;
                case 3:
                    name = "Eternal Crab";
                    age = 3;
                    break;
                case 4:
                    name = "Eternal Crab";
                    age = 0;
                    break;
            }
        }
        
        protected bool Equals(PartiallySerializableAnimal other)
        {
            return name == other.name && age == other.age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PartiallySerializableAnimal)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0) * 397) ^ age;
            }
        }

    }

    
    public class TestSaveDataWithSerializableAttributes
    {
        [Test]
        public void WhenSavedSerializableType_SavesPrivateSerializableFields()
        {
            // arrange
            var savedData = new SerializableDog(1, "Fido the Third");
            var expectedSavedString = @"
{
  ""dogg"": {
    ""taggedName"": ""Fido the Third"",
    ""name"": ""Fido"",
    ""age"": 3
  }
}
".Trim();
            // act
            var savedString = GetSerializedToAndAssertRoundTrip(("dogg", savedData));
            
            // assert
            AssertMultilineStringEqual(expectedSavedString, savedString);
        }
        
        [Test]
        public void WhenSavedPartiallySerializableType_SavesPrivateSerializableFields()
        {
            // arrange
            var savedData = new PartiallySerializableAnimal(3);
            var expectedSavedString = @"
{
  ""creb"": {
    ""name"": ""Eternal Crab""
  }
}
".Trim();
            // act
            var savedString = SerializeToString("test", assertInternalRoundTrip: false, ("creb", savedData));
            var loaded = TryLoad(savedString, "creb", out PartiallySerializableAnimal loadedData);
            
            // assert
            AssertMultilineStringEqual(expectedSavedString, savedString);
            Assert.IsTrue(loaded);
            var expectedLoadedData = new PartiallySerializableAnimal(4);
            Assert.AreEqual(expectedLoadedData, loadedData);
        }
        
        [Test]
        public void WhenLoadsTypeDifferentSerializableType_ThanSaved_Errors()
        {
            // arrange
            var savedData = new SerializableDog(1, "Fido the Third");
            
            // act
            using var stringStore = new StringStorePersistSaveData();
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            var saveDataContext = saveDataContextProvider.GetContext("test");
            saveDataContext.Save("dogg", savedData);
            saveDataContextProvider.PersistContext("test");
            
            var loadAction = new TestDelegate(() =>
            {
                saveDataContextProvider.LoadContext("test");
                saveDataContext.TryLoad("dogg", out Cat _);
            });
            
            // assert
            Assert.Throws<SaveDataException>(loadAction);
        }

        
        [Test]
        public void WhenSavedDifferentDerivedSerializableTypeDirectly_SavesNoMetadataInBoth()
        {
            // arrange
            var savedDog = new SerializableDog(1, "Fido the Third");
            var savedCat = new SerializableCat(2, Personality.Indifferent);
            var savedAnimal = new SerializableAnimal(0);
            var expectedSavedString = @"
{
  ""dogg"": {
    ""taggedName"": ""Fido the Third"",
    ""name"": ""Fido"",
    ""age"": 3
  },
  ""kitty"": {
    ""personality"": ""Indifferent"",
    ""name"": ""Mr. green"",
    ""age"": 6
  },
  ""???"": {
    ""name"": ""Borg"",
    ""age"": 3000
  }
}
".Trim();
            // act
            var savedString = GetSerializedToAndAssertRoundTrip(
                ("dogg", savedDog),
                ("kitty", savedCat),
                ("???", savedAnimal)
                );
            
            // assert
            AssertMultilineStringEqual(expectedSavedString, savedString);
        }
        
        
        [Test]
        public void WhenSavedListOfDifferentDerivedSerializableTypeDirectly_KeepsMetadata_SavesPrivateFields()
        {
            // arrange
            var savedData = new List<SerializableAnimal>
            {
                new SerializableDog(1, "Fido the Third"),
                new SerializableCat(2, Personality.Indifferent),
                new SerializableAnimal(0)
            };
            var expectedSavedString = @"
{
  ""zoo"": [
    {
      ""$type"": ""SaveSystem.Test.SerializableDog, SaveSystem.Test"",
      ""taggedName"": ""Fido the Third"",
      ""name"": ""Fido"",
      ""age"": 3
    },
    {
      ""$type"": ""SaveSystem.Test.SerializableCat, SaveSystem.Test"",
      ""personality"": ""Indifferent"",
      ""name"": ""Mr. green"",
      ""age"": 6
    },
    {
      ""name"": ""Borg"",
      ""age"": 3000
    }
  ]
}
".Trim();
            // act
            var savedString = GetSerializedToAndAssertRoundTrip(
                ("zoo", savedData)
            );
            
            // assert
            AssertMultilineStringEqual(expectedSavedString, savedString);
        }
        
        public class SerializableMonoBehavior : MonoBehaviour
        {
            public string data;
        }
        [Test]
        public void WhenSavedMonobehavior_ThrowsException()
        {
            // arrange
            var gameObject = new GameObject();
            var savedData = gameObject.AddComponent<SerializableMonoBehavior>();
            savedData.data = "hello";
            savedData.name = "can't save me";
            
            // act
            using var stringStore = new StringStorePersistSaveData();
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            var saveDataContext = saveDataContextProvider.GetContext("test");
            
            var loadAction = new TestDelegate(() =>
            {
                saveDataContext.Save("mono", savedData);
            });
            
            // assert
            Assert.Throws<SaveDataException>(loadAction);
        }
    }
}
