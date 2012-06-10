using System;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class LegacyTaskXmlModify : LegacyTaskBase
    {
        /// <summary>
        ///
        /// </summary>
        public string XmlPathAndFileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NodeToChange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttributeKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttributeKeyValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttributeToChange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AttributeToChangeValue { get; set; }
    }
}
