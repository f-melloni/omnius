using System.Collections.Generic;

namespace FSS.Omnius.Modules.Entitron.Entity.Entitron
{
    public class AjaxTransferDbIndex : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Unique { get; set; }
        public List<string> ColumnNames { get; set; }

        public AjaxTransferDbIndex()
        {
            ColumnNames = new List<string>();
        }
    }
}