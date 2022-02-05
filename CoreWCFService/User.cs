using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CoreWCFService
{
    public class User
    {
        [Key]
        [StringLength(100)]
        public string Username { get; set; }
        public string Password { get; set; }
    }
}