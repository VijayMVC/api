using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Models.Places
{
    public class PlaceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }        
        public int ClientId { get; set; }

        public bool IsActive { get; set; }
    }
}
