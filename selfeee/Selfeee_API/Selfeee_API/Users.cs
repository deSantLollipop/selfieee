using System.Collections.Generic;

namespace Selfeee_API
{
    public class Users
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public byte[] ImageProfile { get; set; }
        public string ImagesPath { get; set; }
    }
}
