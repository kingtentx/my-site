-- ============================================================================
-- my-site Site Builder V2 强制清理脚本
-- Branch: agent/site-builder-enhancements
-- Database: MySQL 8.x
--
-- 执行顺序：
--   1) 先停止 Web 应用 / IIS 应用池 / Docker 容器
--   2) 确认已部署包含 SiteBuilderUpgradeInitializer 的最新代码
--   3) 在应用实际连接的数据库执行本脚本
--   4) 检查脚本末尾查询结果
--   5) 再启动应用
-- ============================================================================
SET NAMES utf8mb4;
SELECT DATABASE() AS `CurrentDatabase`;
SET @upgrade_operator = 'site-builder-upgrade';
SET @empty_builder_doc = '{"schemaVersion":1,"name":"首页","nodes":[],"settings":{}}';
SET FOREIGN_KEY_CHECKS = 0;
START TRANSACTION;

DELETE FROM `WebsitePageVersion`;
DELETE FROM `WebsitePage`;
DELETE FROM `WebsiteNavigation`;
DELETE FROM `WebsiteFooter`;

INSERT INTO `WebsitePage`
(
    `SiteId`, `ParentId`, `PageName`, `PageCode`, `PagePath`, `PageTitle`,
    `SeoKeywords`, `SeoDescription`, `ShowInNavigation`, `NavigationTitle`,
    `NavigationIcon`, `NavigationTarget`, `LayoutJson`, `ComponentJson`,
    `Status`, `IsHome`, `Sort`, `IsActive`, `PublishTime`, `IsDelete`,
    `CreationTime`, `UpdateTime`, `CreationBy`, `UpdateBy`
)
VALUES
(
    1, 0, '首页', 'home', '/', '首页', NULL, NULL, 0, NULL,
    NULL, 0, NULL, @empty_builder_doc, 0, 1, 1, 1, NULL, 0,
    NOW(), NOW(), @upgrade_operator, @upgrade_operator
);

SET @home_page_id = LAST_INSERT_ID();
INSERT INTO `WebsitePageVersion`
(
    `PageId`, `VersionNo`, `DraftJson`, `PublishJson`, `Status`, `PublishTime`,
    `CreateUserId`, `CreateUserName`, `CreationTime`, `CreationBy`
)
VALUES
(
    @home_page_id, 1, @empty_builder_doc, NULL, 0, NULL,
    0, @upgrade_operator, NOW(), @upgrade_operator
);

INSERT INTO `WebsiteNavigation`
(
    `Pid`, `Title`, `Path`, `Icon`, `Target`, `Sort`, `IsShow`, `IsActive`,
    `IsDelete`, `CreationTime`, `UpdateTime`, `CreationBy`, `UpdateBy`
)
VALUES
(
    0, '首页', '/', NULL, 0, 1, 1, 1,
    0, NOW(), NOW(), @upgrade_operator, @upgrade_operator
);

DELETE FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%';

SET @website_menu_id = (
    SELECT `Id` FROM `Menu`
    WHERE `Pid` = 0 AND `PermissionKey` = 'Site'
    ORDER BY `Id` LIMIT 1
);

INSERT INTO `Menu`
(
    `Title`, `Path`, `Icon`, `MenuType`, `Pid`, `Spread`,
    `PermissionKey`, `Buttons`, `Sort`, `IsShow`, `IsDelete`,
    `CreationTime`, `UpdateTime`, `CreationBy`, `UpdateBy`
)
SELECT
    '网站管理', NULL, 'layui-icon-website', 1, 0, 0,
    'Site', NULL, 10, 1, 0,
    NOW(), NOW(), @upgrade_operator, @upgrade_operator
WHERE @website_menu_id IS NULL;

SET @website_menu_id = (
    SELECT `Id` FROM `Menu`
    WHERE `Pid` = 0 AND `PermissionKey` = 'Site'
    ORDER BY `Id` LIMIT 1
);

UPDATE `Menu`
SET `Title`='网站管理', `Path`=NULL, `Icon`='layui-icon-website',
    `MenuType`=1, `Pid`=0, `Spread`=0, `PermissionKey`='Site',
    `Buttons`=NULL, `Sort`=10, `IsShow`=1, `IsDelete`=0,
    `UpdateTime`=NOW(), `UpdateBy`=@upgrade_operator
WHERE `Id`=@website_menu_id;

DELETE FROM `Menu`
WHERE `Pid` = 0 AND `PermissionKey` = 'Site' AND `Id` <> @website_menu_id;

DELETE FROM `Menu`
WHERE `Id` <> @website_menu_id
  AND (
       `PermissionKey` IN ('Site_Info', 'Website_Page', 'Site_Navigation', 'Site_Footer')
       OR LOWER(IFNULL(`Path`, '')) IN (
            '/siteconfig/index','/page/index','/globalregion/index','/navigation/index','/footer/index'
       )
       OR (`Pid` = @website_menu_id AND `Title` IN (
            '站点设置','页面管理','全局区域设计','导航管理','菜单管理','页脚设置'
       ))
  );

INSERT INTO `Menu`
(
    `Title`, `Path`, `Icon`, `MenuType`, `Pid`, `Spread`,
    `PermissionKey`, `Buttons`, `Sort`, `IsShow`, `IsDelete`,
    `CreationTime`, `UpdateTime`, `CreationBy`, `UpdateBy`
)
VALUES
('站点设置','/siteconfig/index','layui-icon-set',2,@website_menu_id,0,'Site_Info','Edit',11,1,0,NOW(),NOW(),@upgrade_operator,@upgrade_operator),
('页面管理','/page/index','layui-icon-template',2,@website_menu_id,0,'Website_Page','Add,Edit,Delete,Design,Publish',12,1,0,NOW(),NOW(),@upgrade_operator,@upgrade_operator),
('全局区域设计','/globalregion/index','layui-icon-component',2,@website_menu_id,0,'Website_Page','Design,Publish',13,1,0,NOW(),NOW(),@upgrade_operator,@upgrade_operator),
('菜单管理','/navigation/index','layui-icon-nav',2,@website_menu_id,0,'Site_Navigation','Add,Edit,Delete',14,1,0,NOW(),NOW(),@upgrade_operator,@upgrade_operator);

COMMIT;
SET FOREIGN_KEY_CHECKS = 1;

SELECT COUNT(*) AS `TotalPages`,
       SUM(CASE WHEN LEFT(LTRIM(IFNULL(`ComponentJson`,'')),1)='[' THEN 1 ELSE 0 END) AS `LegacyArrayPages`,
       SUM(CASE WHEN LEFT(LTRIM(IFNULL(`ComponentJson`,'')),1)='{' THEN 1 ELSE 0 END) AS `NewBuilderPages`
FROM `WebsitePage`;

SELECT `Id`,`PageName`,`PageCode`,`PagePath`,`IsHome`,`Status`,
       LEFT(LTRIM(IFNULL(`ComponentJson`,'')),1) AS `JsonRoot`
FROM `WebsitePage` ORDER BY `Sort`,`Id`;

SELECT COUNT(*) AS `LegacyFooterRows` FROM `WebsiteFooter`;
SELECT `Id`,`Pid`,`Title`,`Path`,`Sort`,`IsShow`,`CreationBy`
FROM `WebsiteNavigation` ORDER BY `Sort`,`Id`;
SELECT `Id`,`Pid`,`Title`,`Path`,`PermissionKey`,`Buttons`,`Sort`,`IsShow`,`IsDelete`
FROM `Menu`
WHERE `Id`=@website_menu_id OR `Pid`=@website_menu_id
ORDER BY `Pid`,`Sort`,`Id`;
SELECT `Id`,`Pid`,`Title`,`Path`,`PermissionKey`
FROM `Menu`
WHERE `PermissionKey`='Site_Footer'
   OR LOWER(IFNULL(`Path`,''))='/footer/index'
   OR `Title`='页脚设置';
