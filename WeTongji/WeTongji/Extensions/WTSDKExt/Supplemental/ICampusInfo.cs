using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;

namespace WeTongji.Api.Domain
{
    interface ICampusInfo
    {
        int Id { get; }
        String Title { get; }
        String Source { get;}
        String Summary { get; }
        String DisplayCreationTime { get; }
        Boolean IsIllustrated { get; }
        Boolean CampusInfoImageExists { get; }
        DateTime CreatedAt { get; }
        ImageSource CampusInfoImageBrush { get; }
        String CampusInfoImageUrl { get; }
        void SaveCampusInfoImage(Stream stream);
    }
}
