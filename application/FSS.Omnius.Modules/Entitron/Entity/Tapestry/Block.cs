using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FSS.Omnius.Modules.Entitron.Entity.Tapestry
{
    using Mozaic;

    [Table("Tapestry_Blocks")]
    public partial class Block : IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Block()
        {
            SourceTo_ActionRules = new HashSet<ActionRule>();
            TargetTo_ActionRules = new HashSet<ActionRule>();
            InitForWorkFlow = new HashSet<WorkFlow>();
            ResourceMappingPairs = new HashSet<ResourceMappingPair>();
            VirtualBlocks = new HashSet<Block>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Index("blockUniqueness", Order = 1, IsUnique = true)]
        public string Name { get; set; }

        [StringLength(50)]
        public string ModelName { get; set; }

        [Required]
        [StringLength(50)]
        public string DisplayName { get; set; }

        public int? IsVirtualForBlockId { get; set; }
        public virtual Block IsVirtualForBlock { get; set; }

        public bool IsInMenu { get; set; }

        [Index("blockUniqueness", Order = 2, IsUnique = true)]
        public int WorkFlowId { get; set; }

        public int? MozaicPageId { get; set; }
        public virtual Page MozaicPage { get; set; }

        public int? EditorPageId { get; set; }
        public int? BootstrapPageId { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActionRule> SourceTo_ActionRules { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ActionRule> TargetTo_ActionRules { get; set; }
        
        public virtual ICollection<ResourceMappingPair> ResourceMappingPairs { get; set; }

        public virtual ICollection<Block> VirtualBlocks { get; set; }

        public virtual WorkFlow WorkFlow { get; set; }
        
        public virtual ICollection<WorkFlow> InitForWorkFlow { get; set; }

        public virtual string RoleWhitelist { get; set; }
    }
}
