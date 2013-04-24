using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class CustomVariable : EntityBase
    {
        private string _key;
        private string _value;

        [DataMember]
        public string Key
        {
            get { return this._key; }

            set
            {
                this._key = value;
                NotifyPropertyChanged(() => this.Key);
            }
        }

        [DataMember]
        public string Value
        {
            get { return this._value; }

            set
            {
                this._value = value;
                NotifyPropertyChanged(() => this.Value);
            }
        }

        [DataMember]
        public bool ValueIsEncrypted { get; set; }

        public override string ToString()
        {
            return this.Key + ": " + this.Value;
        }
    }
}
