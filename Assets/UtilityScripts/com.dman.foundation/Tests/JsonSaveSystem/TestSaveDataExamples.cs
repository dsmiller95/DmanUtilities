using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using static Dman.Foundation.Tests.SaveDataTestUtils;

namespace Dman.Foundation.Tests
{
    [Serializable]
    public struct MovementInputParams
    {
        [SerializeField] public Vector2 axisInput;
        public float deltaTime;
    }

    public class MovementState
    {
        public Vector2 CurrentPosition { get; set; }
        public Vector2 CurrentVelocity { get; set; }
    }
    public interface IAffectMovement
    {
        public void AffectMovement(MovementState state, MovementInputParams input);
    }

    public class AddInputToVelocity : IAffectMovement
    {
        public float InputToVelocityMultiplier { get; set; }

        public void AffectMovement(MovementState state, MovementInputParams input)
        {
            state.CurrentVelocity += input.axisInput * InputToVelocityMultiplier * input.deltaTime;
        }
    }

    public class DampenVelocity : IAffectMovement
    {
        public float DampingFactor { get; set; }

        public void AffectMovement(MovementState state, MovementInputParams input)
        {
            state.CurrentVelocity /= DampingFactor;
        }
    }

    public class AddVelocityToPosition : IAffectMovement
    {
        public void AffectMovement(MovementState state, MovementInputParams input)
        {
            state.CurrentPosition += state.CurrentVelocity * input.deltaTime;
        }
    }
    
    public class MovementStrategy
    {
        public List<IAffectMovement> MovementAffectors { get; set; }
        public MovementInputParams Input { get; set; }
        public MovementState State { get; set; }
        
        public void ApplyAll()
        {
            foreach (var affector in MovementAffectors)
            {
                affector.AffectMovement(State, Input);
            }
        }
    }
    
    public class TestSaveDataExamples
    {
        [Test]
        public void WhenSavedMovementStrategy_SavesAllMetadata()
        {
            // arrange
            var savedData = new MovementStrategy
            {
                MovementAffectors = new List<IAffectMovement>
                {
                    new AddInputToVelocity
                    {
                        InputToVelocityMultiplier = 5
                    },
                    new DampenVelocity
                    {
                        DampingFactor = 1.1f
                    },
                    new AddVelocityToPosition()
                },
                Input = new MovementInputParams
                {
                    axisInput = new Vector2(1, 0),
                    deltaTime = 0.1f
                },
                State = new MovementState
                {
                    CurrentPosition = new Vector2(2, 3.2f),
                    CurrentVelocity = new Vector2(4, 5),
                },
            };
            var expectedSavedString = $@"
{{
  ""movementStrat"": {{
    ""movementAffectors"": [
      {{
        ""$type"": ""{Namespace}.AddInputToVelocity, {Assembly}"",
        ""inputToVelocityMultiplier"": 5.0
      }},
      {{
        ""$type"": ""{Namespace}.DampenVelocity, {Assembly}"",
        ""dampingFactor"": 1.1
      }},
      {{
        ""$type"": ""{Namespace}.AddVelocityToPosition, {Assembly}""
      }}
    ],
    ""input"": {{
      ""axisInput"": {{""x"":1.0,""y"":0.0}},
      ""deltaTime"": 0.1
    }},
    ""state"": {{
      ""currentPosition"": {{""x"":2.0,""y"":3.200000047683716}},
      ""currentVelocity"": {{""x"":4.0,""y"":5.0}}
    }}
  }}
}}
".Trim();
            // act
            string serializedString = SerializeToString(
                "test",
                assertInternalRoundTrip: false,
                ("movementStrat", savedData));

            // assert
            AssertMultilineStringEqual(expectedSavedString, serializedString);
        }
    }
}
