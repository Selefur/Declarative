//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Taro2._0_Lab6_
{
    using System;
    using System.Collections.Generic;
    
    public partial class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> IDQuestion { get; set; }
        public string Answer { get; set; }
    
        public virtual Question Question { get; set; }
    }
}
