-- ============================================================================
-- my-site Site Builder V3 数据清理与后台菜单重建脚本
-- Branch: agent/site-builder-enhancements
-- Database: MySQL 8.x
--
-- 当前架构：
--   1. WebsitePage 页面树同时承担网站导航；
--   2. 不再使用独立 WebsiteNavigation；
--   3. Header/Footer 的布局、颜色、定位等统一由全局区域设计维护；
--   4. SiteConfig 只保留站点基础信息和整站启用状态；
--   5. 页面布局与组件统一存储在 ComponentJson 的 BuilderDocument 中。
--
-- 执行前：停止 Web 应用并备份数据库。
-- ============================================================================

SET NAMES utf8mb4;
SELECT DATABASE() AS CurrentDatabase;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================================
-- 1. 清理 Site Builder 重写前页面/版本数据
-- ============================================================================
DELETE FROM `WebsitePageVersion`;
DELETE FROM `WebsitePage`;
ALTER TABLE `WebsitePageVersion` AUTO_INCREMENT = 1;
ALTER TABLE `WebsitePage` AUTO_INCREMENT = 1;

-- 页面树已经取代 WebsiteNavigation，全局 Footer Builder 已经取代 WebsiteFooter。
DROP TABLE IF EXISTS `WebsiteNavigation`;
DROP TABLE IF EXISTS `WebsiteFooter`;

-- ============================================================================
-- 2. 删除旧装修字段及已经迁移到 Header Builder 的站点字段
-- 使用 information_schema 判断字段是否存在，脚本可重复执行。
-- ============================================================================
SET @db = DATABASE();

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsitePage' AND COLUMN_NAME='LayoutJson'),
    'ALTER TABLE `WebsitePage` DROP COLUMN `LayoutJson`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='HeaderBgColor'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `HeaderBgColor`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='HeaderTextColor'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `HeaderTextColor`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='HeaderActiveColor'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `HeaderActiveColor`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='HeaderFixedTop'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `HeaderFixedTop`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='Theme'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Theme`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME='WebsiteSiteConfig' AND COLUMN_NAME='Language'),
    'ALTER TABLE `WebsiteSiteConfig` DROP COLUMN `Language`',
    'SELECT 1'
); PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 3. 清理已经废弃的角色权限
-- ============================================================================
DELETE FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%'
   OR `Permission` = 'Site_Navigation'
   OR `Permission` LIKE 'Site_Navigation\_%';

-- ============================================================================
-- 4. 彻底重建 Menu 表
-- 网站管理只保留：站点设置、页面管理、全局区域设计。
-- 页面管理同时负责网站导航树。
-- ============================================================================
DELETE FROM `Menu`;
ALTER TABLE `Menu` AUTO_INCREMENT = 1;

SET @seed_by = 'site-builder-v3';

-- 首页
INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('首页','/admin/main','layui-icon-home',2,0,1,NULL,NULL,0,1,0,NOW(),NOW(),@seed_by,@seed_by);

-- 系统管理
INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('系统管理',NULL,'layui-icon-set',1,0,0,'System',NULL,90,1,0,NOW(),NOW(),@seed_by,@seed_by);
SET @system_id = LAST_INSERT_ID();

INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('角色管理','/role/index','layui-icon-user',2,@system_id,0,'System_Role','Add,Edit,Delete,Authorize',91,1,0,NOW(),NOW(),@seed_by,@seed_by),
('管理员','/manager/index','layui-icon-username',2,@system_id,0,'System_Admin','Add,Edit,Delete',92,1,0,NOW(),NOW(),@seed_by,@seed_by),
('菜单管理','/menu/index','layui-icon-align-left',2,@system_id,0,'System_Menu','Add,Edit,Delete',93,1,0,NOW(),NOW(),@seed_by,@seed_by),
('审计日志','/auditlog/index','layui-icon-survey',2,@system_id,0,'System_AuditLog','View,Delete',95,1,0,NOW(),NOW(),@seed_by,@seed_by);

-- 网站管理
INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('网站管理',NULL,'layui-icon-website',1,0,0,'Site',NULL,10,1,0,NOW(),NOW(),@seed_by,@seed_by);
SET @site_id = LAST_INSERT_ID();

INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('站点设置','/siteconfig/index','layui-icon-set',2,@site_id,0,'Site_Info','Edit',11,1,0,NOW(),NOW(),@seed_by,@seed_by),
('页面管理','/page/index','layui-icon-template',2,@site_id,0,'Website_Page','Add,Edit,Delete,Design,Publish',12,1,0,NOW(),NOW(),@seed_by,@seed_by),
('全局区域设计','/globalregion/index','layui-icon-component',2,@site_id,0,'Website_Page','Design,Publish',13,1,0,NOW(),NOW(),@seed_by,@seed_by);

-- 内容管理
INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('内容管理',NULL,'layui-icon-read',1,0,0,'Content',NULL,30,1,0,NOW(),NOW(),@seed_by,@seed_by);
SET @content_id = LAST_INSERT_ID();

INSERT INTO `Menu`
(`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
VALUES
('新闻管理','/article/index','layui-icon-list',2,@content_id,0,'Content_Article','Add,Edit,Delete',31,1,0,NOW(),NOW(),@seed_by,@seed_by),
('产品分类','/productcategory/index','layui-icon-cols',2,@content_id,0,'Content_ProductCategory','Add,Edit,Delete',32,1,0,NOW(),NOW(),@seed_by,@seed_by),
('产品管理','/product/index','layui-icon-component',2,@content_id,0,'Content_Product','Add,Edit,Delete',33,1,0,NOW(),NOW(),@seed_by,@seed_by),
('招聘管理','/job/index','layui-icon-friends',2,@content_id,0,'Content_Job','Add,Edit,Delete',34,1,0,NOW(),NOW(),@seed_by,@seed_by),
('素材管理','/images/index','layui-icon-picture',2,@content_id,0,'Content_Images','Add,Edit,Delete',35,1,0,NOW(),NOW(),@seed_by,@seed_by);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- 5. 执行结果验证
-- ============================================================================
SELECT COUNT(*) AS MenuCount FROM `Menu`;

SELECT
    `Id`,`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationBy`
FROM `Menu`
ORDER BY `Id`;

SELECT COUNT(*) AS WebsitePageCount FROM `WebsitePage`;
SELECT COUNT(*) AS WebsitePageVersionCount FROM `WebsitePageVersion`;
SELECT COUNT(*) AS LegacyNavigationTableCount
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WebsiteNavigation';
SELECT COUNT(*) AS LegacyFooterTableCount
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WebsiteFooter';
SELECT COUNT(*) AS LegacyLayoutJsonColumnCount
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'WebsitePage' AND COLUMN_NAME = 'LayoutJson';

SELECT `Id`,`RoleId`,`Permission`
FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%'
   OR `Permission` = 'Site_Navigation'
   OR `Permission` LIKE 'Site_Navigation\_%';

-- 预期：
-- MenuCount = 16
-- WebsitePageCount = 0
-- WebsitePageVersionCount = 0
-- LegacyNavigationTableCount = 0
-- LegacyFooterTableCount = 0
-- LegacyLayoutJsonColumnCount = 0
-- Site_Footer / Site_Navigation 权限查询 = 0 行
-- ============================================================================
