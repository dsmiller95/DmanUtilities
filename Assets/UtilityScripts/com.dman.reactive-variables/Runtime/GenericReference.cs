using System;
using UniRx;

namespace Dman.ReactiveVariables
{
    public enum ReferenceDataSource
    {
        CONSTANT,
        SINGLETON_VARIABLE,
        INSTANCER
    }

    public abstract class GenericReference<T>
    {
        /// <summary>
        /// Warning: only change this in the inspector. the ValueChanges subscribtion currently will break otherwise
        /// </summary>
        public ReferenceDataSource DataSource = ReferenceDataSource.CONSTANT;
        public T ConstantValue;
        public GenericVariable<T> Variable;

        public VariableInstantiator Instancer;
        public string NamePath;

        public GenericReference(T value)
        {
            DataSource = ReferenceDataSource.CONSTANT;
            ConstantValue = value;
        }

        public abstract GenericVariable<T> GetFromInstancer(VariableInstantiator instancer, string name);

        public T CurrentValue
        {
            get
            {
                switch (DataSource)
                {
                    case ReferenceDataSource.CONSTANT:
                        return ConstantValue;
                    case ReferenceDataSource.SINGLETON_VARIABLE:
                        return Variable.CurrentValue;
                    case ReferenceDataSource.INSTANCER:
                        return GetFromInstancer(Instancer, NamePath).CurrentValue;
                    default:
                        throw new Exception("Invalid data source");
                }
            }
            set
            {
                SetValue(value);
            }
        }

        public IObservable<T> ValueChanges
        {
            get
            {
                switch (DataSource)
                {
                    case ReferenceDataSource.CONSTANT:
                        return Observable.Return(ConstantValue);
                    case ReferenceDataSource.SINGLETON_VARIABLE:
                        return Variable.Value;
                    case ReferenceDataSource.INSTANCER:
                        return GetFromInstancer(Instancer, NamePath).Value;
                    default:
                        throw new Exception("Invalid data source");
                }
            }
        }

        public void SetValue(T v)
        {
            switch (DataSource)
            {
                case ReferenceDataSource.CONSTANT:
                    ConstantValue = v;
                    break;
                case ReferenceDataSource.SINGLETON_VARIABLE:
                    Variable.SetValue(v);
                    break;
                case ReferenceDataSource.INSTANCER:
                    GetFromInstancer(Instancer, NamePath).SetValue(v);
                    break;
                default:
                    throw new Exception("Invalid data source");
            }
        }

        public static implicit operator T(GenericReference<T> reference) => reference.CurrentValue;
    }
}
