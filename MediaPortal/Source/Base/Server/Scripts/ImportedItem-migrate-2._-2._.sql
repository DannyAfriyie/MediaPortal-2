-- This script migrates ImportedItem aspect data from database version 2.* to version 2.*. DO NOT MODIFY!

INSERT INTO M_IMPORTEDITEM SELECT * FROM M_IMPORTEDITEM%SUFFIX%;
UPDATE M_IMPORTEDITEM SET DIRTY=1 WHERE MEDIA_ITEM_ID IN (SELECT MEDIA_ITEM_ID FROM M_PROVIDERRESOURCE WHERE TYPE IN (0,1,3));
