using System.Runtime.Serialization;

namespace RevolutionaryStuff.Core.ApplicationParts
{
    public interface IModifyable
    {
        bool IsReadOnly { get; }
        void MakeReadOnly();
    }

    [DataContract]
    public abstract class BaseModifyable : IModifyable
    {
        #region IModifyable Members

        [DataMember(EmitDefaultValue = false)]
        public bool IsReadOnly
        {
            get; private set;
        }

        public void MakeReadOnly()
        {
            if (IsReadOnly) return;
            OnMakeReadonly();
            IsReadOnly = true;
        }

        #endregion

        protected virtual void OnMakeReadonly() { }

        protected void CheckCanModify()
        {
            if (this.IsReadOnly) throw new ReadOnlyException(string.Format("cannot modify value, {0} is immutable", this.GetType().Name));
        }
    }

}
