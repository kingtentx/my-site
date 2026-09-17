-- ============================================================================
-- 产品详情归位：参数表 HTML 从 Description(产品详情) 移到 Specification(产品参数)
-- 背景：源库 album.Detail 内容形如 <div class="product-param-table">...，
--       迁移时按源站渲染方式放进了 Description；现按语义改挂到「产品参数」区块。
-- 幂等：仅处理 Specification 为空且 Description 非空的行，重复执行无副作用。
-- 回滚：contentproduct_bak_before_spec_20260917（首次执行时的快照，只建不覆盖）
-- ============================================================================
SET NAMES utf8mb4;

-- 快照（只建不覆盖）
CREATE TABLE IF NOT EXISTS contentproduct_bak_before_spec_20260917 AS
SELECT * FROM contentproduct;

UPDATE contentproduct
SET Specification = Description,
    Description   = NULL,
    UpdateBy      = 'syle-migration',
    UpdateTime    = NOW()
WHERE IsDelete = 0
  AND (Specification IS NULL OR Specification = '')
  AND Description IS NOT NULL
  AND Description <> '';

-- 校验
SELECT '===== 更新后字段分布 =====' AS `检查项`;
SELECT
    COUNT(*)                                                             AS 产品总数,
    SUM(CASE WHEN Specification IS NOT NULL AND Specification <> '' THEN 1 ELSE 0 END) AS 有产品参数,
    SUM(CASE WHEN Description   IS NOT NULL AND Description   <> '' THEN 1 ELSE 0 END) AS 有产品详情,
    SUM(CASE WHEN Summary       IS NOT NULL AND Summary       <> '' THEN 1 ELSE 0 END) AS 有摘要
FROM contentproduct
WHERE IsDelete = 0;

SELECT '===== 各分类明细 =====' AS `检查项`;
SELECT c.Name 分类,
       COUNT(p.Id)                                                                    AS 产品数,
       SUM(CASE WHEN p.Specification IS NOT NULL AND p.Specification <> '' THEN 1 ELSE 0 END) AS 有参数表
FROM contentproductcategory c
LEFT JOIN contentproduct p ON p.CategoryId = c.Id AND p.IsDelete = 0
WHERE c.Pid = 13 AND c.IsDelete = 0
GROUP BY c.Name, c.Sort
ORDER BY c.Sort;
