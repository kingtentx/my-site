-- ============================================================================
-- ContentProductCategory（文章/产品/招聘 统一内容分类树）重复数据清理
-- Database : my_site (MySQL 8.0.38)
-- Branch   : agent/site-builder-enhancements
-- 执行前   : 建议停止 Web 应用；脚本会先自动备份到 contentproductcategory_bak_20260914
--
-- 背景：ContentCategoryHelper.EnsureRoots 是「查一下没有就插一条」的惰性种子逻辑，
--       没有任何锁或唯一约束。并发请求同时进来时会各自插入一份，造成
--       「文章 / 产品 / 招聘」三个内置大类各自被写入 3 份，子分类也跟着重复 3 份。
--       本库中三份的时间戳相差仅 2~3 毫秒，即为并发竞态的确凿证据。
--
-- 清理口径（同父级同名 = 重复，保留最小 Id）：
--   1. 先记录原始重复映射，把「将被删除分类」的子节点改挂到保留分类；
--   2. 再重算映射，把 contentproduct / contentjob 的分类引用改挂到保留分类；
--   3. 最后物理删除重复行。
--   全程在一个事务内完成，末尾 COMMIT；执行干跑时去掉最后一行 COMMIT 即可回滚。
--
-- 可重复执行：清理完成后再次运行，映射为空，产生 0 行变更。
-- ============================================================================

SET NAMES utf8mb4;

-- ---------------------------------------------------------------------------
-- 0. 备份（DDL 会隐式提交，必须在事务外执行）
--
--    幂等保留：备份表已存在时**不再覆盖**。
--    否则脚本第二次执行会把「清理前的 52 行」覆盖成「清理后的 27 行」，
--    回滚依据就永久丢失了。需要重新取一份快照时，先手动改名或 DROP 旧备份表。
-- ---------------------------------------------------------------------------
SET @bak_exists := (
    SELECT COUNT(*) FROM information_schema.TABLES
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'contentproductcategory_bak_20260914'
);
SET @sql := IF(@bak_exists = 0,
    'CREATE TABLE `contentproductcategory_bak_20260914` AS SELECT * FROM `contentproductcategory`',
    'DO 0');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT IF(@bak_exists = 0, '备份完成（新建）', '备份已存在，本次跳过（保留原始快照）') AS Step,
       COUNT(*) AS BackupRows
FROM `contentproductcategory_bak_20260914`;

-- ---------------------------------------------------------------------------
-- 1. 清理前统计
-- ---------------------------------------------------------------------------
SELECT '清理前总行数' AS Metric, COUNT(*) AS Value FROM `contentproductcategory`
UNION ALL SELECT '清理前重复分组数',
    (SELECT COUNT(*) FROM (
        SELECT Pid, Name FROM `contentproductcategory`
        GROUP BY Pid, Name HAVING COUNT(*) > 1
    ) t)
UNION ALL SELECT '清理前冗余行数',
    (SELECT COUNT(*) FROM `contentproductcategory` c
      WHERE c.Id <> (SELECT MIN(x.Id) FROM `contentproductcategory` x
                      WHERE x.Pid = c.Pid AND x.Name = c.Name));

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 2. 原始重复映射：DropId -> KeepId
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS `tmp_cat_map1`;
CREATE TEMPORARY TABLE `tmp_cat_map1` AS
SELECT c.Id AS DropId,
       (SELECT MIN(x.Id) FROM `contentproductcategory` x
         WHERE x.Pid = c.Pid AND x.Name = c.Name) AS KeepId
FROM `contentproductcategory` c
WHERE c.Id <> (SELECT MIN(x.Id) FROM `contentproductcategory` x
                WHERE x.Pid = c.Pid AND x.Name = c.Name);

SELECT '步骤2 待删除行数' AS Step, COUNT(*) AS Value FROM `tmp_cat_map1`;

-- ---------------------------------------------------------------------------
-- 3. 子节点改挂：把挂在将被删除分类下的子分类，改挂到保留分类
--    （本库涉及 23 行：智能硬件/软件应用/云服务/数字化咨询、公司新闻…、技术研发…、默认产品）
-- ---------------------------------------------------------------------------
UPDATE `contentproductcategory` ch
JOIN `tmp_cat_map1` m ON ch.Pid = m.DropId
SET ch.Pid = m.KeepId,
    ch.UpdateBy = 'dedupe',
    ch.UpdateTime = NOW(6);

SELECT ROW_COUNT() AS `步骤3 改挂子节点行数`;

-- ---------------------------------------------------------------------------
-- 4. 改挂后重算最终删除映射（改挂可能让子分类在保留大类下重新构成重名）
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS `tmp_cat_map2`;
CREATE TEMPORARY TABLE `tmp_cat_map2` AS
SELECT c.Id AS DropId,
       (SELECT MIN(x.Id) FROM `contentproductcategory` x
         WHERE x.Pid = c.Pid AND x.Name = c.Name) AS KeepId
FROM `contentproductcategory` c
WHERE c.Id <> (SELECT MIN(x.Id) FROM `contentproductcategory` x
                WHERE x.Pid = c.Pid AND x.Name = c.Name);

SELECT '步骤4 最终待删除行数' AS Step, COUNT(*) AS Value FROM `tmp_cat_map2`;

-- ---------------------------------------------------------------------------
-- 5. 内容引用改挂到保留分类（产品、招聘）
-- ---------------------------------------------------------------------------
UPDATE `contentproduct` p
JOIN `tmp_cat_map2` m ON p.CategoryId = m.DropId
SET p.CategoryId = m.KeepId;

SELECT ROW_COUNT() AS `步骤5 产品改挂行数`;

UPDATE `contentjob` j
JOIN `tmp_cat_map2` m ON j.CategoryId = m.DropId
SET j.CategoryId = m.KeepId;

SELECT ROW_COUNT() AS `步骤5 招聘改挂行数`;

-- ---------------------------------------------------------------------------
-- 6. 物理删除重复行
-- ---------------------------------------------------------------------------
DELETE c FROM `contentproductcategory` c
JOIN `tmp_cat_map2` m ON c.Id = m.DropId;

SELECT ROW_COUNT() AS `步骤6 物理删除行数`;

-- ---------------------------------------------------------------------------
-- 7. 清理后校验
-- ---------------------------------------------------------------------------
SELECT '清理后总行数' AS Metric, COUNT(*) AS Value FROM `contentproductcategory`
UNION ALL SELECT '剩余重复分组数',
    (SELECT COUNT(*) FROM (
        SELECT Pid, Name FROM `contentproductcategory`
        GROUP BY Pid, Name HAVING COUNT(*) > 1
    ) t)
UNION ALL SELECT '孤儿节点数（Pid 指向不存在的分类）',
    (SELECT COUNT(*) FROM `contentproductcategory` c
      WHERE c.Pid <> 0
        AND NOT EXISTS (SELECT 1 FROM `contentproductcategory` p WHERE p.Id = c.Pid))
UNION ALL SELECT '悬空产品引用数',
    (SELECT COUNT(*) FROM `contentproduct` p
      WHERE p.CategoryId <> 0
        AND NOT EXISTS (SELECT 1 FROM `contentproductcategory` c WHERE c.Id = p.CategoryId))
UNION ALL SELECT '悬空招聘引用数',
    (SELECT COUNT(*) FROM `contentjob` j
      WHERE j.CategoryId <> 0
        AND NOT EXISTS (SELECT 1 FROM `contentproductcategory` c WHERE c.Id = j.CategoryId));

-- 保留分类树结构速览（大类 + 直属子分类数）
SELECT c.Id, c.Pid, c.Name, c.Sort,
       (SELECT COUNT(*) FROM `contentproductcategory` ch WHERE ch.Pid = c.Id) AS DirectChildren
FROM `contentproductcategory` c
WHERE c.Pid = 0 OR c.Pid IN (SELECT Id FROM (SELECT Id FROM `contentproductcategory` WHERE Pid = 0) t)
ORDER BY c.Pid, c.Sort, c.Id;

-- ---------------------------------------------------------------------------
-- 8. 提交（干跑时删除本行，连接断开会自动回滚）
-- ---------------------------------------------------------------------------
COMMIT;

-- ============================================================================
-- 附：可选的数据库级防重（建议在代码侧修复生效后再考虑）
-- MySQL 8 唯一索引不忽略 NULL，可用生成列实现「未删除才唯一」：
--
-- ALTER TABLE `contentproductcategory`
--   ADD COLUMN `ActiveKey` VARCHAR(160)
--     GENERATED ALWAYS AS (IF(`IsDelete`, NULL, CONCAT(`Pid`, ':', `Name`))) STORED,
--   ADD UNIQUE INDEX `UX_ContentProductCategory_Active` (`ActiveKey`);
--
-- 注意：EF Core 模型未声明该生成列，插入重复数据时会抛唯一键冲突（DbUpdateException）。
--       这是期望行为——把「静默产生重复」变成「显式失败」。
-- ============================================================================
