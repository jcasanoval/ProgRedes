using System;
using System.Collections.Generic;
using System.Text;

namespace Obligatorio.ServerClient
{
    public class Photo
    {
        public List<Comment> Comments { get; set; }
        public String Name { get; set; }
        public string FileName { get; set; }
    }
}
