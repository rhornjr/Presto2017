
namespace PrestoCommon.Entities
{
    /// <summary>
    /// Custom Variable
    /// </summary>
    public class CustomVariable : EntityBase
    {
        private string _key;
        private string _value;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get { return this._key; }

            set
            {
                this._key = value;
                NotifyPropertyChanged(() => this.Key);
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return this._value; }

            set
            {
                this._value = value;
                NotifyPropertyChanged(() => this.Value);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Key + ": " + this.Value;
        }
    }
}
