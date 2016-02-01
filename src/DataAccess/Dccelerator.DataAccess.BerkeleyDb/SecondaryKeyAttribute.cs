using System;


namespace Dccelerator.DataAccess.BerkeleyDb {

    /// <summary>
    /// Means what marked property will be used as for searching, and for that property should be generated an secondary index.
    /// </summary>
    public class SecondaryKeyAttribute : Attribute {

        /// <summary>
        /// Reletionship of key and entity.
        /// </summary>
        public Relationship Relationship { get; set; }


        /// <summary>
        /// Name of secondary key. It may be just marked property name.
        /// </summary>
        public string SecondaryKeyName { get; set; }


        public SecondaryKeyAttribute(Relationship relationship) {
            Relationship = relationship;
        }
    }
}