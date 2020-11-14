using System;
using System.Collections.Generic;
using System.Text;

namespace Obligatorio.ServerClient
{
    public class Photo
    {
        private List<Comment> comments = new List<Comment>();
        public List<Comment> Comments { get { return comments; } }
        public String Name { get; set; }
        public string FileName { get; set; }

        public List<string> CommentList()
        {
            List<string> list = new List<string>();
            foreach (Comment comment in comments)
            {
                list.Add("-" + comment.User.Name + "\n" + comment.Text);
            }
            return list;
        }
    }
}
