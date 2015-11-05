﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpServer;
using HttpServer.Exceptions;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS.Music;
using Newtonsoft.Json;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.MAS.Music
{
  internal class GetMusicTracksBasicForAlbum : IRequestMicroModuleHandler
  {
    public dynamic Process(IHttpRequest request)
    {
      HttpParam httpParam = request.Param;
      string id = httpParam["id"].Value;
      if (id == null)
        throw new BadRequestException("GetMusicTracksBasicForAlbum: no id is null");

      // decode the ID
      id = (new UTF8Encoding()).GetString(Convert.FromBase64String(id));

      // Get all episodes for this
      ISet<Guid> necessaryMIATypes = new HashSet<Guid>();
      necessaryMIATypes.Add(MediaAspect.ASPECT_ID);
      necessaryMIATypes.Add(AudioAspect.ASPECT_ID);
      necessaryMIATypes.Add(ImporterAspect.ASPECT_ID);

      IFilter searchFilter = new RelationalFilter(AudioAspect.ATTR_ALBUM, RelationalOperator.EQ, id);
      MediaItemQuery searchQuery = new MediaItemQuery(necessaryMIATypes, null, searchFilter);

      IList<MediaItem> tracks = ServiceRegistration.Get<IMediaLibrary>().Search(searchQuery, false);

      if (tracks.Count == 0)
        throw new BadRequestException("No Tracks found");

      var output = new List<WebMusicTrackBasic>();

      foreach (var item in tracks)
      {
        MediaItemAspect audioAspects = item.Aspects[AudioAspect.ASPECT_ID];

        WebMusicTrackBasic webMusicTrackBasic = new WebMusicTrackBasic();
        webMusicTrackBasic.Album = (string)audioAspects[AudioAspect.ATTR_ALBUM];
        var albumArtists = (HashSet<object>)audioAspects[AudioAspect.ATTR_ALBUMARTISTS];
        if (albumArtists != null)
          webMusicTrackBasic.AlbumArtist = String.Join(", ", albumArtists.Cast<string>().ToArray());
        //webMusicTrackBasic.AlbumArtistId;
        // TODO: We have to wait for the MIA Rework, until than the ID is just the name as bas64
        webMusicTrackBasic.AlbumId = Convert.ToBase64String((new UTF8Encoding()).GetBytes((string)audioAspects[AudioAspect.ATTR_ALBUM]));
        var trackArtists = (HashSet<object>)audioAspects[AudioAspect.ATTR_ARTISTS];
        if (albumArtists != null)
          webMusicTrackBasic.Artist = trackArtists.Cast<string>().ToList();
        //webMusicTrackBasic.ArtistId;
        webMusicTrackBasic.DiscNumber = audioAspects[AudioAspect.ATTR_DISCID] != null ? (int)audioAspects[AudioAspect.ATTR_DISCID] : 0;
        webMusicTrackBasic.Duration = Convert.ToInt32((long)audioAspects[AudioAspect.ATTR_DURATION]);
        var trackGenres = (HashSet<object>)audioAspects[AudioAspect.ATTR_GENRES];
        if (trackGenres != null)
          webMusicTrackBasic.Genres = trackGenres.Cast<string>().ToList();
        //webMusicTrackBasic.Rating = Convert.ToSingle((double)movieAspects[AudioAspect.]);
        webMusicTrackBasic.TrackNumber = (int)audioAspects[AudioAspect.ATTR_TRACK];
        webMusicTrackBasic.Type = WebMediaType.MusicTrack;
        //webMusicTrackBasic.Year;
        //webMusicTrackBasic.Artwork;
        webMusicTrackBasic.DateAdded = (DateTime)item.Aspects[ImporterAspect.ASPECT_ID][ImporterAspect.ATTR_DATEADDED];
        webMusicTrackBasic.Id = item.MediaItemId.ToString();
        webMusicTrackBasic.PID = 0;
        //webMusicTrackBasic.Path;
        webMusicTrackBasic.Title = (string)item.Aspects[MediaAspect.ASPECT_ID][MediaAspect.ATTR_TITLE];

        output.Add(webMusicTrackBasic);
      }

      // sort
      string sort = httpParam["sort"].Value;
      string order = httpParam["order"].Value;
      if (sort != null && order != null)
      {
        WebSortField webSortField = (WebSortField)JsonConvert.DeserializeObject(sort, typeof(WebSortField));
        WebSortOrder webSortOrder = (WebSortOrder)JsonConvert.DeserializeObject(order, typeof(WebSortOrder));

        output = output.SortWebMusicTrackBasic(webSortField, webSortOrder).ToList();
      }

      return output;
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}