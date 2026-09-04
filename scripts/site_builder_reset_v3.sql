-- ============================================================================
-- my-site Site Builder V3 数据清理与后台菜单重建脚本
-- Branch: agent/site-builder-enhancements
-- Database: MySQL 8.x
--
-- 作用：
--   1. 彻底删除 Menu 表历史记录并按当前项目菜单重新插入；
--   2. 清理旧 Site_Footer 角色权限；
--   3. 清空重写前的页面装修、页面版本、前台导航、旧 Footer 配置；
--   4. 保留文章、产品、招聘、素材、用户、角色、站点基础配置。
--
-- 执行前：停止 Web 应用并备份数据库。
-- ============================================================================

SET NAMES utf8mb4;
SELECT DATABASE() AS CurrentDatabase;
SET FOREIGN_KEY_CHECKS = 0;
START TRANSACTION;

-- ============================================================================
-- 1. 清理 Site Builder 重写前数据
-- ============================================================================
DELETE FROM `WebsitePageVersion`;
DELETE FROM `WebsitePage`;
DELETE FROM `WebsiteNavigation`;
DELETE FROM `WebsiteFooter`;

-- 重置自增，方便确认脚本确实执行到了当前数据库。
ALTER TABLE `WebsitePageVersion` AUTO_INCREMENT = 1;
ALTER TABLE `WebsitePage` AUTO_INCREMENT = 1;
ALTER TABLE `WebsiteNavigation` AUTO_INCREMENT = 1;
ALTER TABLE `WebsiteFooter` AUTO_INCREMENT = 1;

-- ============================================================================
-- 2. 清理废弃权限
-- RoleMenu 存的是 Permission 字符串，不是 MenuId。
-- ============================================================================
DELETE FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%';

-- ============================================================================
-- 3. 彻底重建 Menu 表
-- 这里不是按条件删除，而是删除 Menu 全表后重新插入当前版本的 17 条菜单。
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
('全局区域设计','/globalregion/index','layui-icon-component',2,@site_id,0,'Website_Page','Design,Publish',13,1,0,NOW(),NOW(),@seed_by,@seed_by),
('菜单管理','/navigation/index','layui-icon-nav',2,@site_id,0,'Site_Navigation','Add,Edit,Delete',14,1,0,NOW(),NOW(),@seed_by,@seed_by);

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

COMMIT;
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- 4. 执行结果验证
-- ============================================================================
SELECT COUNT(*) AS MenuCount FROM `Menu`;

SELECT
    `Id`,`Title`,`Path`,`Icon`,`MenuType`,`Pid`,`Spread`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`,`CreationBy`
FROM `Menu`
ORDER BY `Id`;

SELECT COUNT(*) AS WebsitePageCount FROM `WebsitePage`;
SELECT COUNT(*) AS WebsitePageVersionCount FROM `WebsitePageVersion`;
SELECT COUNT(*) AS WebsiteNavigationCount FROM `WebsiteNavigation`;
SELECT COUNT(*) AS WebsiteFooterCount FROM `WebsiteFooter`;

SELECT `Id`,`RoleId`,`Permission`
FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%';

-- 预期：
-- MenuCount = 17
-- WebsitePageCount = 0
-- WebsitePageVersionCount = 0
-- WebsiteNavigationCount = 0
-- WebsiteFooterCount = 0
-- Site_Footer 权限查询 = 0 行
-- ============================================================================
