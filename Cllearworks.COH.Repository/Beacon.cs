//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cllearworks.COH.Repository
{
    using System;
    using System.Collections.Generic;
    
    public partial class Beacon
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Beacon()
        {
            this.Tracks = new HashSet<Track>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string UUID { get; set; }
        public Nullable<int> Major { get; set; }
        public Nullable<int> Minor { get; set; }
        public Nullable<int> BeaconType { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int PlaceId { get; set; }
        public int DepartmentId { get; set; }
    
        public virtual Department Department { get; set; }
        public virtual Place Place { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Track> Tracks { get; set; }
    }
}