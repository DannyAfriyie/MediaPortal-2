﻿#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
    http://www.team-mediaportal.com
    This file is part of MediaPortal 2
    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Extensions.OnlineLibraries;
using MediaPortal.Common.MediaManagement.Helpers;

namespace MediaPortal.Extensions.MetadataExtractors.SeriesMetadataExtractor
{
  class SeriesBaseTryExtractRelationships
  {
    public static bool GetBaseInfo<T>(IDictionary<Guid, IList<MediaItemAspect>> aspects, out T info)
    {
      info = default(T);

      string tvDbIdStr = null;
      string imDbId = null;
      string tmDbIdStr = null;
      string tvMazeIdStr = null;
      int tvDbId;
      int movieDbId;
      int tvMazeId;
      bool tvDbExists = MediaItemAspect.TryGetExternalAttribute(aspects, ExternalIdentifierAspect.SOURCE_TVDB, ExternalIdentifierAspect.TYPE_SERIES, out tvDbIdStr);
      bool imDbExists = MediaItemAspect.TryGetExternalAttribute(aspects, ExternalIdentifierAspect.SOURCE_IMDB, ExternalIdentifierAspect.TYPE_SERIES, out imDbId);
      bool tmDbExists = MediaItemAspect.TryGetExternalAttribute(aspects, ExternalIdentifierAspect.SOURCE_TMDB, ExternalIdentifierAspect.TYPE_SERIES, out tmDbIdStr);
      bool tvMazeExists = MediaItemAspect.TryGetExternalAttribute(aspects, ExternalIdentifierAspect.SOURCE_TVMAZE, ExternalIdentifierAspect.TYPE_SERIES, out tvMazeIdStr);
      if (tvDbExists || imDbExists || tmDbExists || tvMazeExists)
      {
        Int32.TryParse(tvDbIdStr, out tvDbId);
        Int32.TryParse(tmDbIdStr, out movieDbId);
        Int32.TryParse(tvMazeIdStr, out tvMazeId);
      }
      else
        return false;

      int seasonNum;
      bool seasonFound = MediaItemAspect.TryGetAttribute(aspects, EpisodeAspect.ATTR_SEASON, out seasonNum);

      IEnumerable<int> episodes = null;
      SingleMediaItemAspect episodeAspect;
      if (MediaItemAspect.TryGetAspect(aspects, EpisodeAspect.Metadata, out episodeAspect))
      {
        episodes = episodeAspect.GetCollectionAttribute<int>(EpisodeAspect.ATTR_EPISODE);
      }

      if(typeof(T) == typeof(EpisodeInfo))
      {
        EpisodeInfo episodeInfo = (EpisodeInfo)(object)info;
        episodeInfo.TvdbId = tvDbId;
        episodeInfo.MovieDbId = movieDbId;
        episodeInfo.ImdbId = imDbId;
        episodeInfo.TvMazeId = tvMazeId;
        if(seasonFound) episodeInfo.SeasonNumber = seasonNum;
        if(episodes != null) episodeInfo.EpisodeNumbers.AddRange(episodes);
      }
      else if (typeof(T) == typeof(SeasonInfo))
      {
        SeasonInfo seasonInfo = (SeasonInfo)(object)info;
        seasonInfo.TvdbId = tvDbId;
        seasonInfo.MovieDbId = movieDbId;
        seasonInfo.ImdbId = imDbId;
        seasonInfo.TvMazeId = tvMazeId;
        if (seasonFound) seasonInfo.SeasonNumber = seasonNum;
      }
      else if (typeof(T) == typeof(SeriesInfo))
      {
        SeriesInfo seariesInfo = (SeriesInfo)(object)info;
        seariesInfo.TvdbId = tvDbId;
        seariesInfo.MovieDbId = movieDbId;
        seariesInfo.ImdbId = imDbId;
        seariesInfo.TvMazeId = tvMazeId;
      }
      return true;
    }

    public bool TryExtractRelationships(IDictionary<Guid, IList<MediaItemAspect>> aspects, out ICollection<IDictionary<Guid, IList<MediaItemAspect>>> extractedLinkedAspects, bool forceQuickMode)
    {
      extractedLinkedAspects = null;

      // Build the series MI

      SeriesInfo seriesInfo;
      if (!GetBaseInfo(aspects, out seriesInfo))
        return false;

      SeriesTheMovieDbMatcher.Instance.UpdateSeries(seriesInfo);
      SeriesTvMazeMatcher.Instance.UpdateSeries(seriesInfo);
      SeriesTvDbMatcher.Instance.UpdateSeries(seriesInfo);
      SeriesOmDbMatcher.Instance.UpdateSeries(seriesInfo);

      extractedLinkedAspects = new List<IDictionary<Guid, IList<MediaItemAspect>>>();
      IDictionary<Guid, IList<MediaItemAspect>> seriesAspects = new Dictionary<Guid, IList<MediaItemAspect>>();
      extractedLinkedAspects.Add(seriesAspects);

      seriesInfo.SetMetadata(seriesAspects);
      return true;
    }
  }
}