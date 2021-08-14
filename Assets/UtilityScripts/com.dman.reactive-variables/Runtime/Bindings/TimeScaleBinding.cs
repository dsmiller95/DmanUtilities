using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using UniRx;
namespace Dman.ReactiveVariables.Bindings
{
    public class TimeScaleBinding : MonoBehaviour
    {
        public FloatReference floatVariable;
        public float speedMultiplier = 1;
        private void Awake()
        {
            floatVariable.ValueChanges.TakeUntilDestroy(this)
                .Subscribe(next =>
                {
                    Time.timeScale = next * speedMultiplier;
                }).AddTo(this);
        }
    }
}
