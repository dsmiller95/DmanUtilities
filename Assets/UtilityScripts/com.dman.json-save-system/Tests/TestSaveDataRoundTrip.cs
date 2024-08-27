using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using static SaveSystem.Test.SaveDataTestUtils;

namespace SaveSystem.Test
{
    public record Animal
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public record Dog : Animal
    {
        public string TaggedName { get; set; }
    }

    [TypeConverter(typeof(EnumConverter))]
    public enum Personality
    {
        Friendly,
        Aggressive,
        Indifferent
    }
    public record Cat : Animal
    {
        public Personality Personality { get; set; }
    }
    
    public class Zoo : IEquatable<Zoo>
    {
        public bool Equals(Zoo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Animals.SequenceEqual(other.Animals);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Zoo)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Animals);
        }

        public string Name { get; set; }
        public List<Animal> Animals { get; set; }
    }
    
    public class TestSaveDataRoundTrip
    {
        [Test]
        public void WhenSavedDerivedTypeDirectly_SavesNoMetadata()
        {
            // arrange
            var savedData = new Dog
            {
                Name = "Fido",
                Age = 3,
                TaggedName = "Fido the Third"
            };
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
        public void WhenLoadsTypeDifferentThanSaved_Errors()
        {
            // arrange
            var savedData = new Dog
            {
                Name = "Fido",
                Age = 3,
                TaggedName = "Fido the Third"
            };
            
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
        public void WhenSavedDifferentDerivedTypeDirectly_SavesNoMetadataInBoth()
        {
            // arrange
            var savedDog = new Dog
            {
                Name = "Fido",
                Age = 3,
                TaggedName = "Fido the Third"
            };
            var savedCat = new Cat
            {
                Name = "Mr. green",
                Age = 6,
                Personality = Personality.Indifferent
            };
            var savedAnimal = new Animal
            {
                Name = "Borg",
                Age = 3000
            };
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
        public void WhenSavedListOfDifferentDerivedTypeDirectly_SavesAllMetadataNeeded()
        {
            // arrange
            var savedData = new Zoo
            {
                Name = "Chaos zoo",
                Animals = new List<Animal>
                {
                    new Dog
                    {
                        Name = "Fido",
                        Age = 3,
                        TaggedName = "Fido the Third"
                    },
                    new Cat
                    {
                        Name = "Mr. green",
                        Age = 6,
                        Personality = Personality.Indifferent
                    },
                    new Animal
                    {
                        Name = "Borg",
                        Age = 3000
                    }
                }
            };
            var expectedSavedString = @"
{
  ""zoo"": {
    ""name"": ""Chaos zoo"",
    ""animals"": [
      {
        ""$type"": ""SaveSystem.Test.Dog, SaveSystem.Test"",
        ""taggedName"": ""Fido the Third"",
        ""name"": ""Fido"",
        ""age"": 3
      },
      {
        ""$type"": ""SaveSystem.Test.Cat, SaveSystem.Test"",
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
}
".Trim();
            // act
            var savedString = GetSerializedToAndAssertRoundTrip(
                ("zoo", savedData)
            );
            
            // assert
            AssertMultilineStringEqual(expectedSavedString, savedString);
        }
        
        [Test]
        public void WhenSaveContextDeleted_HandleRemainsValid()
        {
            // arrange
            var savedData = new Dog
            {
                Name = "Fido",
                Age = 3,
                TaggedName = "Fido the Third"
            };
            var savedDataTwo = new Cat
            {
                Name = "Mr. green",
                Age = 6,
                Personality = Personality.Indifferent
            };
            
            // act
            using var stringStore = new StringStorePersistSaveData();
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            var contextOne = saveDataContextProvider.GetContext("test");
            contextOne.Save("dogg", savedData);
            saveDataContextProvider.PersistContext("test");
            saveDataContextProvider.DeleteContext("test");
            saveDataContextProvider.LoadContext("test");
            
            var didLoad = contextOne.TryLoad("dogg", out Dog loadedDog);
            Assert.IsFalse(didLoad, "The original context should find the data deleted");

            var contextTwo = saveDataContextProvider.GetContext("test");
            contextTwo.Save("kitty", savedDataTwo);
            
            var didTwoLoad = contextTwo.TryLoad("kitty", out Cat loadedCat);
            Assert.IsTrue(didTwoLoad, "The new context should find the freshly saved data");
            Assert.AreEqual(savedDataTwo, loadedCat, "the new context should load the new data");
            
            var didOneLoad = contextOne.TryLoad("kitty", out loadedCat);
            Assert.IsTrue(didOneLoad, "The original context should find the new data");
            Assert.AreEqual(savedDataTwo, loadedCat, "the original context should load the new data");
        }
    }
}
