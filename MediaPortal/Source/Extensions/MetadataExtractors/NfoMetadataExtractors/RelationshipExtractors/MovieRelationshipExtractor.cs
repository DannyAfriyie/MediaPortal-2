#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
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
using MediaPortal.Common.MediaManagement.MLQueries;

namespace MediaPortal.Extensions.MetadataExtractors.NfoMetadataExtractors
{
  class MovieRelationshipExtractor : IRelationshipExtractor
  {
    #region Constants

    /// <summary>
    /// GUID string for the movie relationship metadata extractor.
    /// </summary>
    public const string METADATAEXTRACTOR_ID_STR = "069D00E6-FD9B-42E4-AF84-5920714CB145";

    /// <summary>
    /// Series relationship metadata extractor GUID.
    /// </summary>
    public static Guid METADATAEXTRACTOR_ID = new Guid(METADATAEXTRACTOR_ID_STR);

    #endregion

    protected RelationshipExtractorMetadata _metadata;
    private IList<IRelationshipRoleExtractor> _extractors;

    public MovieRelationshipExtractor()
    {
      _metadata = new RelationshipExtractorMetadata(METADATAEXTRACTOR_ID, "NFO Movie relationship extractor");

      _extractors = new List<IRelationshipRoleExtractor>();

      _extractors.Add(new MovieCollectionRelationshipExtractor());
      _extractors.Add(new MovieActorRelationshipExtractor());
      _extractors.Add(new MovieDirectorRelationshipExtractor());
      _extractors.Add(new MovieCharacterRelationshipExtractor());
    }

    public IList<RelationshipHierarchy> Hierarchies
    {
      get { return null; }
    }

    public RelationshipExtractorMetadata Metadata
    {
      get { return _metadata; }
    }

    public IList<IRelationshipRoleExtractor> RoleExtractors
    {
      get { return _extractors; }
    }

    public IDictionary<IFilter, uint> GetLastChangedItemsFilters()
    {
      return null;
    }

    public void ResetLastChangedItems()
    {
    }
  }
}
