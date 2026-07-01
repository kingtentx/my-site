using System.Collections.Generic;

namespace CimcSite.Web.Models
{
    public class TreeModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int Sort { get; set; }

        public List<TreeModel> Children { get; set; }

    }
}
