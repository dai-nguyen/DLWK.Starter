using System.ComponentModel;

namespace ApplicationCore.Enums
{
    public enum UploadType : byte
    {        
        [Description(@"Images\ProfilePictures")]
        ProfilePicture,

        [Description(@"Documents")]
        Document
    }
}
