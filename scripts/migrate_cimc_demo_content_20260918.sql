-- 将 D:\MyProject\cimc-site-demo 对应的 slye_site 内容数据复制到当前 my_site。
-- 请先执行 EF 迁移 20260917160623_MigrateLegacyContentModules。
-- 当前开发库已于 2026-09-18 执行完成；本脚本用于重建开发环境时复现数据迁移。

START TRANSACTION;

DELETE FROM `my_site`.`Tag`;
INSERT INTO `my_site`.`Tag`
(`Id`,`TagName`,`TagName_EN`,`TagType`,`Sort`,`IsActive`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
SELECT `Id`,`TagName`,`TagName_EN`,`TagType`,`Sort`,`IsActive`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`
FROM `slye_site`.`Tag`;

DELETE FROM `my_site`.`Article`;
INSERT INTO `my_site`.`Article`
(`Id`,`Title`,`Title_EN`,`Keyword`,`Description`,`Description_EN`,`Detail`,`Detail_EN`,`Author`,`Source`,`SourceUrl`,`LinkUrl`,`ImageUrl`,`TagType`,`TagId`,`Sort`,`ViewCount`,`ShareCount`,`IsHot`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
SELECT `Id`,`Title`,`Title_EN`,`Keyword`,`Description`,`Description_EN`,`Detail`,`Detail_EN`,`Author`,`Source`,`SourceUrl`,`LinkUrl`,`ImageUrl`,`TagType`,`TagId`,`Sort`,`ViewCount`,`ShareCount`,`IsHot`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`
FROM `slye_site`.`Article`;

DELETE FROM `my_site`.`Album`;
INSERT INTO `my_site`.`Album`
(`Id`,`ImageUrl`,`LinkUrl`,`Title`,`Title_EN`,`Description`,`Description_EN`,`Detail`,`Detail_EN`,`Author`,`TagType`,`TagId`,`Sort`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
SELECT `Id`,`ImageUrl`,`LinkUrl`,`Title`,`Title_EN`,`Description`,`Description_EN`,`Detail`,`Detail_EN`,`Author`,`TagType`,`TagId`,`Sort`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`
FROM `slye_site`.`Album`;

DELETE FROM `my_site`.`Job`;
INSERT INTO `my_site`.`Job`
(`Id`,`JobName`,`JobName_EN`,`Author`,`Detail`,`Detail_EN`,`TagType`,`TagId`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`)
SELECT `Id`,`JobName`,`JobName_EN`,`Author`,`Detail`,`Detail_EN`,`TagType`,`TagId`,`IsActive`,`IsDelete`,`CreationTime`,`UpdateTime`,`CreationBy`,`UpdateBy`
FROM `slye_site`.`Job`;

COMMIT;
