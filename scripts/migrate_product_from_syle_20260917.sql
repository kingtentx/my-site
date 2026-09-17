-- ============================================================================
-- 产品 / 产品分类迁移脚本
-- 源库 : slye_site（cimc-site-demo 项目 syle 分支）
--        分类 = tag      WHERE TagType = 2   （5 条）
--        产品 = album    WHERE TagType = 2   （81 条，其中 IsDelete=0 的 33 条）
-- 目标库: my_site
--        分类 = contentproductcategory（挂到产品根 Pid=13）
--        产品 = contentproduct
-- 幂等  : 分类按 (Pid=13, Name) 去重；产品按 (CategoryId, ProductName, CoverImage) 去重；
--         备份表「只建不覆盖」，已存在则跳过，重复执行不会冲掉首次快照。
-- 回滚  : contentproductcategory_bak_20260917 / contentproduct_bak_20260917
-- ============================================================================
SET NAMES utf8mb4;

-- ---------------------------------------------------------------------------
-- 0. 备份（只建不覆盖）
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS contentproductcategory_bak_20260917 AS
SELECT * FROM contentproductcategory;

CREATE TABLE IF NOT EXISTS contentproduct_bak_20260917 AS
SELECT * FROM contentproduct;

-- ---------------------------------------------------------------------------
-- 1. 源分类 -> 目标分类 的映射表（可重复执行，每次重建）
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS mig_syle_cat_map_20260917 (
    SrcTagId   INT NOT NULL PRIMARY KEY,
    SrcTagName VARCHAR(100),
    NewCatId   INT
);

-- ---------------------------------------------------------------------------
-- 2. 迁移产品分类：slye_site.tag(TagType=2) -> my_site.contentproductcategory(Pid=13)
--    Sort 按业务顺序重排：传统业务线1 / 特种业务线2 / 新业务线3 / 资质证书4 / 合作客户5
-- ---------------------------------------------------------------------------
INSERT INTO contentproductcategory
    (Pid, Name, Sort, IsActive, IsDelete, CreationTime, UpdateTime, CreationBy, UpdateBy)
SELECT
    13,
    s.TagName,
    CASE s.Id WHEN 10 THEN 1 WHEN 11 THEN 2 WHEN 12 THEN 3 WHEN 20 THEN 4 WHEN 25 THEN 5 ELSE 99 END,
    1,
    0,
    s.CreationTime,
    s.UpdateTime,
    IFNULL(s.CreationBy, 'syle-migration'),
    IFNULL(s.UpdateBy,   'syle-migration')
FROM slye_site.tag s
WHERE s.TagType = 2
  AND NOT EXISTS (
        SELECT 1 FROM contentproductcategory c
        WHERE c.Pid = 13 AND c.Name = s.TagName
  );

-- 重建映射
DELETE FROM mig_syle_cat_map_20260917;
INSERT INTO mig_syle_cat_map_20260917 (SrcTagId, SrcTagName, NewCatId)
SELECT s.Id, s.TagName, c.Id
FROM slye_site.tag s
JOIN contentproductcategory c ON c.Pid = 13 AND c.Name = s.TagName
WHERE s.TagType = 2;

-- ---------------------------------------------------------------------------
-- 3. 迁移产品：slye_site.album(TagType=2, IsDelete=0) -> my_site.contentproduct
--    图片目录统一落到 /upload/product/20260917/<原文件名>
--    album.Description -> Summary（摘要）
--    album.Detail      -> Description（详情富文本）
-- ---------------------------------------------------------------------------
INSERT INTO contentproduct
    (ProductName, CategoryId, CoverImage, ImageList, Summary, Description,
     Specification, Feature, Sort, IsRecommend, IsActive, ViewCount, IsDelete,
     CreationTime, UpdateTime, CreationBy, UpdateBy)
SELECT
    a.Title,
    m.NewCatId,
    CONCAT('/upload/product/20260917/', SUBSTRING_INDEX(a.ImageUrl, '/', -1)),
    NULL,
    a.Description,
    a.Detail,
    NULL,
    NULL,
    a.Sort,
    0,
    a.IsActive,
    0,
    0,
    a.CreationTime,
    a.UpdateTime,
    IFNULL(a.CreationBy, 'syle-migration'),
    IFNULL(a.UpdateBy,   'syle-migration')
FROM slye_site.album a
JOIN mig_syle_cat_map_20260917 m ON m.SrcTagId = a.TagId
WHERE a.TagType = 2
  AND a.IsDelete = 0
  AND NOT EXISTS (
        SELECT 1 FROM contentproduct p
        WHERE p.CategoryId  = m.NewCatId
          AND p.ProductName = a.Title
          AND p.CoverImage  = CONCAT('/upload/product/20260917/', SUBSTRING_INDEX(a.ImageUrl, '/', -1))
  );

-- ---------------------------------------------------------------------------
-- 4. 校验
-- ---------------------------------------------------------------------------
SELECT '===== 分类映射 =====' AS `检查项`;
SELECT m.SrcTagId, m.SrcTagName, m.NewCatId, c.Pid, c.Sort, c.IsDelete
FROM mig_syle_cat_map_20260917 m
LEFT JOIN contentproductcategory c ON c.Id = m.NewCatId
ORDER BY c.Sort;

SELECT '===== 各分类产品数 =====' AS `检查项`;
SELECT c.Id, c.Name, COUNT(p.Id) AS product_count
FROM contentproductcategory c
LEFT JOIN contentproduct p ON p.CategoryId = c.Id AND p.IsDelete = 0
WHERE c.Pid = 13 AND c.IsDelete = 0
GROUP BY c.Id, c.Name
ORDER BY c.Sort;

SELECT '===== 产品总数 / 孤儿 / 缺图 =====' AS `检查项`;
SELECT
    (SELECT COUNT(*) FROM contentproduct WHERE IsDelete = 0)                                   AS 产品总数,
    (SELECT COUNT(*) FROM contentproduct p
       WHERE p.IsDelete = 0
         AND NOT EXISTS (SELECT 1 FROM contentproductcategory c WHERE c.Id = p.CategoryId))     AS 孤儿产品,
    (SELECT COUNT(*) FROM contentproduct p
       WHERE p.IsDelete = 0 AND (p.CoverImage IS NULL OR p.CoverImage = ''))                    AS 缺封面图;
