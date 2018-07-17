-- This script migrates AudioItem aspect data from database version 2.1 to version 2.*. DO NOT MODIFY!

INSERT INTO M_AUDIOITEM(MEDIA_ITEM_ID,TRACKNAME,ALBUM,ISCOMPILATION,DURATION,LYRICS,ISCD,TRACK,NUMTRACKS,ENCODING,CONTENTGROUP,BITRATE,CHANNELS,SAMPLERATE,DISCID,NUMDISCS,TOTALRATING,RATINGCOUNT) SELECT MEDIA_ITEM_ID,TRACKNAME,ALBUM,ISCOMPILATION,DURATION,LYRICS,ISCD,TRACK,NUMTRACKS,ENCODING,'' CONTENTGROUP,BITRATE,CHANNELS,SAMPLERATE,DISCID,NUMDISCS,TOTALRATING,RATINGCOUNT FROM M_AUDIOITEM%SUFFIX%;
INSERT INTO V_AUDIOITEM_ALBUMARTISTS SELECT * FROM V_AUDIOITEM_ALBUMARTISTS%SUFFIX%;
INSERT INTO V_AUDIOITEM_ARTISTS SELECT * FROM V_AUDIOITEM_ARTISTS%SUFFIX%;
INSERT INTO V_AUDIOITEM_COMPOSERS SELECT * FROM V_AUDIOITEM_COMPOSERS%SUFFIX%;
INSERT INTO NM_AUDIOITEM_ALBUMARTISTS(MEDIA_ITEM_ID,VALUE_ID,VALUE_ORDER) SELECT MEDIA_ITEM_ID,VALUE_ID,0 VALUE_ORDER FROM NM_AUDIOITEM_ALBUMARTISTS%SUFFIX%;
INSERT INTO NM_AUDIOITEM_ARTISTS(MEDIA_ITEM_ID,VALUE_ID,VALUE_ORDER) SELECT MEDIA_ITEM_ID,VALUE_ID,0 VALUE_ORDER FROM NM_AUDIOITEM_ARTISTS%SUFFIX%;
INSERT INTO NM_AUDIOITEM_COMPOSERS(MEDIA_ITEM_ID,VALUE_ID,VALUE_ORDER) SELECT MEDIA_ITEM_ID,VALUE_ID,0 VALUE_ORDER FROM NM_AUDIOITEM_COMPOSERS%SUFFIX%;
