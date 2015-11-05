﻿using System;
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Extensions.MediaServer.Profiles;

namespace MediaPortal.Extensions.MediaServer.Objects.MediaLibrary
{
  public class MediaLibraryPlaylist : MediaLibraryContainer, IDirectoryPlaylistItem
  {
    public MediaLibraryPlaylist(MediaItem item, EndPointSettings client)
      : base(item, null, null, null, client)
    {
    }

    public IList<MediaItem> GetItems()
    {
      throw new NotImplementedException("Playlists don't work");
    }

    public IList<string> Artist
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public IList<string> Genre
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string LongDescription
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string StorageMedium
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string Description
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string Date
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public string Language
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }
  }
}
