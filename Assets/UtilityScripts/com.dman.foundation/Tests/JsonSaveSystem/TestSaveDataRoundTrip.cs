using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Dman.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Dman.Foundation.Tests.SaveDataTestUtils;

namespace Dman.Foundation.Tests
{
    public class Animal : IEquatable<Animal>
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public bool Equals(Animal other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Age == other.Age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Animal)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Age;
            }
        }
    }

    public class Dog : Animal, IEquatable<Dog>
    {
        public string TaggedName { get; set; }

        public bool Equals(Dog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && TaggedName == other.TaggedName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Dog)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (TaggedName != null ? TaggedName.GetHashCode() : 0);
            }
        }

    }

    [TypeConverter(typeof(EnumConverter))]
    public enum Personality
    {
        Friendly,
        Aggressive,
        Indifferent
    }
    public class Cat : Animal, IEquatable<Cat>
    {
        public Personality Personality { get; set; }
        
        public bool Equals(Cat other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Personality == other.Personality;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Cat)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int)Personality;
            }
        }

    }
    
    public class Zoo : IEquatable<Zoo>
    {
        public string Name { get; set; }
        public List<Animal> Animals { get; set; }
        
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
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Animals != null ? Animals.GetHashCode() : 0);
            }
        }
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
        public void WhenLoadsTypeDifferentThanSaved_LogsError()
        {
            // arrange
            var savedData = new Dog
            {
                Name = "Fido",
                Age = 3,
                TaggedName = "Fido the Third"
            };
            
            // act
            using var stringStore = new StringStorePersistText();
            var saveDataContextProvider = SaveDataContextProvider.CreateAndPersistTo(stringStore);
            var saveDataContext = saveDataContextProvider.GetContext("test");
            saveDataContext.Save("dogg", savedData);
            saveDataContextProvider.PersistContext("test");
            
            saveDataContextProvider.LoadContext("test");
            var didLoad = saveDataContext.TryLoad("dogg", out Cat _);
            
            // assert
            Assert.IsFalse(didLoad, "The load should fail");
            LogAssert.Expect(LogType.Error, new Regex(@"Failed to load data of type Dman\.Foundation\.Tests\.Cat for key dogg\. Raw json"));
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
            var expectedSavedString = @$"
{{
  ""zoo"": {{
    ""name"": ""Chaos zoo"",
    ""animals"": [
      {{
        ""$type"": ""{Namespace}.Dog, {Assembly}"",
        ""taggedName"": ""Fido the Third"",
        ""name"": ""Fido"",
        ""age"": 3
      }},
      {{
        ""$type"": ""{Namespace}.Cat, {Assembly}"",
        ""personality"": ""Indifferent"",
        ""name"": ""Mr. green"",
        ""age"": 6
      }},
      {{
        ""name"": ""Borg"",
        ""age"": 3000
      }}
    ]
  }}
}}
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
            using var stringStore = new StringStorePersistText();
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
