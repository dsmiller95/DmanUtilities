using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    [Serializable]
    class ValueSaveObject<T>
    {
        public string dataID;
        public T savedValue;
    }
    [Serializable]
    class VariableInstantiatorSaveObject
    {
        public ValueSaveObject<object>[] boolValues;
        public ValueSaveObject<object>[] floatValues;
    }

    public class VariableInstantiator : MonoBehaviour
    {
        public BooleanState[] booleanStateConfig;
        public FloatState[] floatStateConfig;

        public IDictionary<string, GenericVariable<bool>> instancedBooleans;
        public IDictionary<string, GenericVariable<float>> instancedFloats;

        private void Awake()
        {
            EnsureInstanced();
        }

        private void EnsureInstanced()
        {
            if (instancedBooleans == null || instancedFloats == null)
            {
                InstantiateVariables();
            }
        }

        private void InstantiateVariables()
        {
            instancedBooleans = booleanStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
            instancedFloats = floatStateConfig.ToDictionary(x => x.IdentifierInInstantiator, x => x.GenerateNewVariable());
        }

        public GenericVariable<bool> GetBooleanValue(string name)
        {
            EnsureInstanced();
            return GetValue(name, instancedBooleans);
        }
        public FloatVariable GetFloatValue(string name)
        {
            EnsureInstanced();
            return GetValue(name, instancedFloats) as FloatVariable;
        }

        private GenericVariable<T> GetValue<T>(string path, IDictionary<string, GenericVariable<T>> instancedValues)
        {
            if (instancedValues.TryGetValue(path, out var variable))
            {
                return variable;
            }
            return null;
        }

        public static string ConstantIdentifier()
        {
            return "Instantiator";
        }
        public string IdentifierInsideMember()
        {
            return ConstantIdentifier();
        }

        public object GetSaveObject()
        {
            return new VariableInstantiatorSaveObject
            {
                boolValues = SaveValues(booleanStateConfig, instancedBooleans),
                floatValues = SaveValues(floatStateConfig, instancedFloats),
            };
        }

        private ValueSaveObject<object>[] SaveValues<T>(GenericState<T>[] stateConfig, IDictionary<string, GenericVariable<T>> variables)
        {
            return stateConfig.Select(config =>
            {
                var variable = variables[config.IdentifierInInstantiator];
                return new ValueSaveObject<object>
                {
                    dataID = config.IdentifierInInstantiator,
                    savedValue = config.GetSaveObjectFromVariable(variable)
                };
            }).ToArray();
        }

        public void SetupFromSaveObject(object save)
        {
            EnsureInstanced();

            var saveData = save as VariableInstantiatorSaveObject;
            if (saveData != null)
            {
                LoadValues(booleanStateConfig, instancedBooleans, saveData.boolValues);
                LoadValues(floatStateConfig, instancedFloats, saveData.floatValues);
            }
        }
        private void LoadValues<T>(GenericState<T>[] stateConfig, IDictionary<string, GenericVariable<T>> variables, ValueSaveObject<object>[] savedValues)
        {
            var stateConfigDictionary = stateConfig.ToDictionary(x => x.IdentifierInInstantiator);
            foreach (var value in savedValues)
            {
                if (variables.TryGetValue(value.dataID, out var variable) && stateConfigDictionary.TryGetValue(value.dataID, out var state))
                {
                    state.SetSaveObjectIntoVariable(variable, value.savedValue);
                }
            }
        }

        public string GetCurrentInfo()
        {
            var info = new StringBuilder("Variables: \n");
            if (instancedBooleans != null)
            {
                foreach (var boolean in instancedBooleans)
                {
                    info.AppendLine($"{boolean.Key}: {boolean.Value.CurrentValue}");
                }
            }
            if (instancedFloats != null)
            {
                foreach (var floatPair in instancedFloats)
                {
                    info.AppendLine($"{floatPair.Key}: {floatPair.Value.CurrentValue:F1}");
                }
            }
            return info.ToString();
        }
    }
}
