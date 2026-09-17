-- ============================================================================
-- 产品图片入库（迁移脚本自动生成，Size 为实际文件字节数）
-- 幂等：按 Url 去重，重复执行不会产生重复行
-- ============================================================================
SET NAMES utf8mb4;
INSERT INTO images (FileName, Url, ExtensionName, Size, CreationBy, CreationTime)
SELECT t.FileName, t.Url, t.ExtensionName, t.Size, t.CreationBy, t.CreationTime FROM (
SELECT '产品-43968717' AS FileName, '/upload/product/20260917/43968717.png' AS Url, 'png' AS ExtensionName, 30568 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968718' AS FileName, '/upload/product/20260917/43968718.png' AS Url, 'png' AS ExtensionName, 8031 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968719' AS FileName, '/upload/product/20260917/43968719.png' AS Url, 'png' AS ExtensionName, 20556 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968720' AS FileName, '/upload/product/20260917/43968720.png' AS Url, 'png' AS ExtensionName, 7022 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968721' AS FileName, '/upload/product/20260917/43968721.png' AS Url, 'png' AS ExtensionName, 4719 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968723' AS FileName, '/upload/product/20260917/43968723.png' AS Url, 'png' AS ExtensionName, 14066 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968725' AS FileName, '/upload/product/20260917/43968725.png' AS Url, 'png' AS ExtensionName, 29345 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968726' AS FileName, '/upload/product/20260917/43968726.png' AS Url, 'png' AS ExtensionName, 10535 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968727' AS FileName, '/upload/product/20260917/43968727.png' AS Url, 'png' AS ExtensionName, 8835 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968729' AS FileName, '/upload/product/20260917/43968729.png' AS Url, 'png' AS ExtensionName, 8658 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968730' AS FileName, '/upload/product/20260917/43968730.png' AS Url, 'png' AS ExtensionName, 9598 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43968763' AS FileName, '/upload/product/20260917/43968763.png' AS Url, 'png' AS ExtensionName, 5836 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43971817' AS FileName, '/upload/product/20260917/43971817.png' AS Url, 'png' AS ExtensionName, 974360 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43974676' AS FileName, '/upload/product/20260917/43974676.png' AS Url, 'png' AS ExtensionName, 161729 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43974835' AS FileName, '/upload/product/20260917/43974835.png' AS Url, 'png' AS ExtensionName, 111496 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43974836' AS FileName, '/upload/product/20260917/43974836.png' AS Url, 'png' AS ExtensionName, 128162 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-43974844' AS FileName, '/upload/product/20260917/43974844.png' AS Url, 'png' AS ExtensionName, 124502 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44064956' AS FileName, '/upload/product/20260917/44064956.jpg' AS Url, 'jpg' AS ExtensionName, 954815 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44064957' AS FileName, '/upload/product/20260917/44064957.jpg' AS Url, 'jpg' AS ExtensionName, 958243 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44068097' AS FileName, '/upload/product/20260917/44068097.png' AS Url, 'png' AS ExtensionName, 106907 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44068098' AS FileName, '/upload/product/20260917/44068098.png' AS Url, 'png' AS ExtensionName, 148578 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44161415' AS FileName, '/upload/product/20260917/44161415.jpg' AS Url, 'jpg' AS ExtensionName, 726020 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44458197' AS FileName, '/upload/product/20260917/44458197.png' AS Url, 'png' AS ExtensionName, 848207 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44463396' AS FileName, '/upload/product/20260917/44463396.png' AS Url, 'png' AS ExtensionName, 1728098 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44463398' AS FileName, '/upload/product/20260917/44463398.jpg' AS Url, 'jpg' AS ExtensionName, 205780 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44463400' AS FileName, '/upload/product/20260917/44463400.jpg' AS Url, 'jpg' AS ExtensionName, 172996 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44656636' AS FileName, '/upload/product/20260917/44656636.png' AS Url, 'png' AS ExtensionName, 5056048 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-44869223' AS FileName, '/upload/product/20260917/44869223.png' AS Url, 'png' AS ExtensionName, 110820 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-45999504' AS FileName, '/upload/product/20260917/45999504.png' AS Url, 'png' AS ExtensionName, 171872 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-45999506' AS FileName, '/upload/product/20260917/45999506.png' AS Url, 'png' AS ExtensionName, 64250 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-49759069' AS FileName, '/upload/product/20260917/49759069.png' AS Url, 'png' AS ExtensionName, 297983 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-49764457' AS FileName, '/upload/product/20260917/49764457.png' AS Url, 'png' AS ExtensionName, 537795 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime UNION ALL
SELECT '产品-49764461' AS FileName, '/upload/product/20260917/49764461.png' AS Url, 'png' AS ExtensionName, 685766 AS Size, 'syle-migration' AS CreationBy, NOW() AS CreationTime
) t WHERE NOT EXISTS (SELECT 1 FROM images i WHERE i.Url = t.Url);
