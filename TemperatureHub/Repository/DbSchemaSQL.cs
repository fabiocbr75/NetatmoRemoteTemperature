using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TemperatureHub.Repository
{
    public static class DbSchemaSQL
    {
        public const string SQLScript = @"
                CREATE TABLE IF NOT EXISTS DataSources (
                        DataSourceId   TEXT PRIMARY KEY NOT NULL,
                        DataSourceName TEXT NOT NULL COLLATE NOCASE,
                        DataSourceType INT NOT NULL,
                        ServerName     TEXT NOT NULL COLLATE NOCASE,
                        DatabaseName   TEXT NOT NULL COLLATE NOCASE,
                        UserName       TEXT NULL COLLATE NOCASE,
                        Password       TEXT NULL,
                        UseWindowsAuth INT NOT NULL,
                        Description    TEXT NOT NULL COLLATE NOCASE
                );
                
                CREATE UNIQUE INDEX IDX_DataSourceName ON DataSources (DataSourceName COLLATE NOCASE ASC);

                INSERT INTO DataSources (DataSourceId, DataSourceName, DataSourceType,  ServerName, DatabaseName, UserName,  Password,  UseWindowsAuth, Description) 
                       VALUES  ('00000000-0000-0000-0000-000000000000', 'n/a', -1, '', '', '', '', false, '');



                CREATE TABLE IF NOT EXISTS RepoItems (
                        ItemId       TEXT PRIMARY KEY NOT NULL,
                        CbId         TEXT NOT NULL,
                        CbType       TEXT NOT NULL,
                        Descriptor   TEXT NOT NULL,
                        ItemName     TEXT NOT NULL COLLATE NOCASE,
                        Description  TEXT NOT NULL COLLATE NOCASE,
                        TimestampUTC INT  NOT NULL,
                        FileContent  BLOB,
                        Author       TEXT NOT NULL COLLATE NOCASE,
                        Language     TEXT NOT NULL COLLATE NOCASE,
                        ParentItemId TEXT NOT NULL,
                        ItemType     INT  NOT NULL,
                        DataSourceId TEXT NOT NULL,
                        FOREIGN KEY(DataSourceId) REFERENCES DataSources(DataSourceId)
                );

                CREATE UNIQUE INDEX IDX_ITEMNAME_PARTENT ON RepoItems (ItemName COLLATE NOCASE ASC, ParentItemId COLLATE NOCASE ASC);               


                CREATE TABLE IF NOT EXISTS Roles (
                        RoleId          TEXT PRIMARY KEY NOT NULL,
                        RoleName        TEXT NOT NULL COLLATE NOCASE,                     
                        Description     TEXT NOT NULL COLLATE NOCASE,
                        CanManageRoles  INT NOT NULL,
                        CanViewReport   INT NOT NULL,
                        CanModifyReport INT NOT NULL,                     
                        CanManageDatasources INT NOT NULL,
                        CanViewDatasources INT NOT NULL
                );

                CREATE UNIQUE INDEX IDX_RoleName ON Roles (RoleName COLLATE NOCASE ASC);

                INSERT INTO Roles (RoleId, RoleName, Description, CanManageRoles, CanViewReport, CanModifyReport, CanManageDatasources, CanViewDatasources) 
                       VALUES  ('C72B1239-DACA-45B0-A5A9-F684D776722F', 'Report View', 'allows only to view existing reports', 0,1,0,0,1),
                               ('85B493AD-9251-4479-B3A5-EB9F161479D9', 'Report Authoring', 'allows full access to reports: create, edit, delete, design, as well as data sources configuration',0,1,1,0,1),
                               ('B2378A00-B242-428E-BDBA-3940751CAF75', 'Report Administrator', 'allows full access + capability of assigning roles to users/groups',1,1,1,1,1);


                CREATE TABLE IF NOT EXISTS Users (
                        UserId          TEXT PRIMARY KEY NOT NULL,
                        UserName        TEXT NOT NULL COLLATE NOCASE,
                        RoleId          TEXT NULL,
                        FOREIGN KEY(RoleId) REFERENCES Roles(RoleId)
                );

                CREATE UNIQUE INDEX IDX_UsersName ON Users (UserName COLLATE NOCASE ASC);

                INSERT INTO Users (UserId, UserName, RoleId) 
                       VALUES  ('C72B1239-DACA-45B0-A5A9-F684D776722F', 'BUILTIN\Administrators', 'B2378A00-B242-428E-BDBA-3940751CAF75');


                CREATE TABLE IF NOT EXISTS UserRoles (
                        UserRoleId  TEXT PRIMARY KEY NOT NULL,
                        RoleId      TEXT NOT NULL,
                        ItemId      TEXT NULL,
                        UserId      TEXT NOT NULL,
                        FOREIGN KEY(RoleId) REFERENCES Roles(RoleId),
                        FOREIGN KEY(ItemId) REFERENCES RepoItems(ItemId)
                        FOREIGN KEY(UserId) REFERENCES Users(UserId)
                );

                CREATE UNIQUE INDEX IDX_UserRoles ON UserRoles (UserId COLLATE NOCASE ASC,ItemId COLLATE NOCASE ASC);

                INSERT INTO UserRoles (UserRoleId, RoleId, UserId) 
                       VALUES  ('C72B1239-DACA-45B0-A5A9-F684D776722F', '85B493AD-9251-4479-B3A5-EB9F161479D9', 'C72B1239-DACA-45B0-A5A9-F684D776722F');

                ";
    }
}