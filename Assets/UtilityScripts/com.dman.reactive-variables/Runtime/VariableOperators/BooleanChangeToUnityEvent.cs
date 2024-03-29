﻿using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BooleanChangeToUnityEvent : MonoBehaviour
    {
        public BooleanVariable booleanToWatch;
        public bool triggerOnAwake = false;

        public UnityEvent OnChangedToTrue;
        public UnityEvent OnChangedToFalse;


        private void Awake()
        {
            booleanToWatch.Value.TakeUntilDestroy(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current != pair.Previous)
                    {
                        if (pair.Current)
                        {
                            OnChangedToTrue?.Invoke();
                        }
                        else
                        {
                            OnChangedToFalse.Invoke();
                        }
                    }
                }).AddTo(this);

            if (triggerOnAwake)
            {
                if (booleanToWatch.CurrentValue)
                {
                    OnChangedToTrue.Invoke();
                }
                else
                {
                    OnChangedToFalse.Invoke();
                }
            }
        }
    }
}