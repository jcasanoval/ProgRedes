using System;
using System.Collections.Generic;
using System.Text;

namespace Obligatorio.ServerClient
{
    public class User
    {

        private List<Photo> photos = new List<Photo>();
        public List<Photo> Photos { get { return photos; } }
        public String Name { get; set; }
        public String Password { get; set; }

        public void UploadPicture(string pictureName, string name)
        {
            Photo photo = new Photo();
            photo.FileName = pictureName;
            photo.Name = name;
            Photos.Add(photo);
        }

        public List<string> PictureList()
        {
            List<string> list = new List<string>();

            foreach (Photo photo in Photos)
            {
                list.Add(photo.Name);
            }

            return list;
        }

        public Photo GetPhoto(string name)
        {
            foreach (Photo photo in photos)
            {
                if (photo.Name == name)
                {
                    return photo;
                }
            }
            return null;
        }
    }
}
