using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dataAccess
{
    public class Movie
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Director { get; set; }
    }
}
