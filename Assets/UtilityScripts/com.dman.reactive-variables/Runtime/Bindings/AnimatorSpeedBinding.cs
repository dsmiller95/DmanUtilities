using UnityEngine;

using UniRx;
namespace Dman.ReactiveVariables.Bindings
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorSpeedBinding : MonoBehaviour
    {
        public FloatReference floatVariable;
        public float speedMultiplier = 1;
        private void Awake()
        {
            floatVariable.ValueChanges.TakeUntilDestroy(this)
                .Subscribe(next =>
                {
                    GetComponent<Animator>().speed = next * speedMultiplier;
                }).AddTo(this);
        }
    }
}
