using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using TemperatureHub.Helpers;

namespace TemperatureHub.Repository
{
    public class SQLiteFileRepository : ISQLiteFileRepository
    {
        private static ConcurrentDictionary<int, IDbConnection> _dbInstaces = new ConcurrentDictionary<int, IDbConnection>();
        private static ConcurrentQueue<Executor> _executorQueue = new ConcurrentQueue<Executor>();
        
        private readonly string _databasePath;

        public SQLiteFileRepository() { }

        public SQLiteFileRepository(IOptions<AppSettings> appSettings)
        {
            _databasePath = appSettings.Value.DbFullPath;
        }

        public static void CreateOrUpdateDb(string databasePath, string reportLanguage)
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get started");
            bool needsDatabaseInit = !File.Exists(databasePath);
            using (var db = new SQLiteConnection($"Data Source={databasePath};foreign keys = true;"))
            {
                db.Open();

                if (needsDatabaseInit)
                {
                    DropAndCreateTables(db);
                }
            }
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDb Get finished");
        }

        private IDbConnection GetDbInstance()
        {
            IDbConnection db = null;
            var currentThread = Thread.CurrentThread.ManagedThreadId;

            if (!_dbInstaces.TryGetValue(currentThread, out db))
            {
                db = new SQLiteConnection($"Data Source={_databasePath};foreign keys = true;");
                db.Open();

                _dbInstaces.AddOrUpdate(currentThread, db, (key, old) => db);
            }

            return db;
        }
        /// <summary>Initialiazes a new repository database file</summary>
        private static void DropAndCreateTables(IDbConnection db)
        {
            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get started");

            db.CreateCommand(DbSchemaSQL.SQLScript).ExecuteNonQuery();

            Logger.Info("SQLiteFileRepository", "DropAndCreateTables Get finished");
        }


        // See Interface
        public bool ContainsItem(string id)
        {
            Logger.Info("SQLiteFileRepository", "ContainsItem Get started");
            var ret = ExecuteOnThreadPool<bool>(() =>
            {
                using (var cmd = GetDbInstance().CreateCommand(@"SELECT COUNT(*) FROM RepoItems WHERE CbId = @CbId"))
                {
                    var resQry = cmd.SetParameter("CbId", id).ExecuteScalar();
                    var result = Convert.ToInt32(resQry, CultureInfo.InvariantCulture);
                    return (result == 1);
                }
            });
            Logger.Info("SQLiteFileRepository", "ContainsItem Get finished");
            return ret;
        }

        /*

        // See Interface
        public void CreateOrUpdateItem(RepositoryItem item, string importUserData, Stream sourceStream)
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateItem Get started");
            string currentUser = HttpContext.Current?.User?.Identity?.Name;

            if (string.IsNullOrEmpty(currentUser))
            {
                currentUser = "[Anonymous User]";
            }

            // Convert stream from List & Label to byte array to store it in the DB
            // Warning: sourceStream may be null! In that case, only the metadata should be changed in the database. See the documentation for IRepository.CreateOrUpdateItem() for details.
            byte[] fileContent = null;
            bool setMetadataOnly;
            if (sourceStream != null)
            {
                using (var memStream = new MemoryStream())
                {
                    sourceStream.CopyTo(memStream);
                    fileContent = memStream.ToArray();
                }
                setMetadataOnly = false;
            }
            else
            {
                setMetadataOnly = true;
            }

            CustomizedRepositoryItem itemToInsert;
            bool isUpdate = ContainsItem(item.InternalID);

            // Update of existing item?
            if (isUpdate)
            {
                // The 'item' parameter is always a new instance of the RepositoryItem class. List & Label does not know the custom properties
                // that we have added to the RepositoryItem in the 'CustomizedRepostoryItem' class, so when we want to update an existing item,
                // the already existing item must be updated manually. The 'Type' and 'InternalID' properties never change.

                itemToInsert = GetItemsFromDb(item.InternalID).First();
                itemToInsert.Descriptor = item.Descriptor;
                itemToInsert.LastModificationUTC = item.LastModificationUTC;
                itemToInsert.Author = currentUser;
                itemToInsert.ItemName = DecodeDescriptor(item.Descriptor);
            }
            else   // New Repository Item
            {
                var itemId = Guid.NewGuid();
                string tmpName = "TempName_" + itemId.ToString().ToUpperInvariant();
                itemToInsert = new CustomizedRepositoryItem(itemId, item.InternalID, item.Descriptor,
                                        item.Type, item.LastModificationUTC, currentUser, _reportLanguage,
                                        itemId, 0, default(Guid), "", tmpName, false);
            }

            // Get a suitable SQL query for INSERT / UPDATE and a call with/without the file content.
            // (If sourceStream is null, the file content must not be changed! In that case, only the metadata like descriptor, timestamp etc. should be modified.

            string sqlQuery;
            if (isUpdate)  // UPDATE
            {
                sqlQuery = @"UPDATE RepoItems 
                            SET Descriptor = @Descriptor, TimestampUTC = @TimestampUTC, Author = @Author, Language = @Language, ParentItemId = @ParentItemId, DataSourceId = @DataSourceId, Description = @Description, ItemName = @ItemName ";

                if (!setMetadataOnly)
                {
                    sqlQuery += @" ,FileContent = @FileContent";
                }

                sqlQuery += @" WHERE CbId = @CbId";
            }
            else    // INSERT
            {
                if (setMetadataOnly)
                {
                    sqlQuery = @"INSERT INTO RepoItems (ItemId, CbId,  CbType,  Descriptor,  TimestampUTC,  Author,  Language, ParentItemId, ItemType, DataSourceId, Description, ItemName)  
                                            VALUES  (@ItemId, @CbId, @CbType, @Descriptor, @TimestampUTC, @Author, @Language, @ParentItemId, @ItemType, @DataSourceId, @Description, @ItemName )";
                }
                else
                {
                    sqlQuery = @"INSERT INTO RepoItems (ItemId, CbId,  CbType,  Descriptor,  TimestampUTC,  Author,  FileContent, Language, ParentItemId, ItemType, DataSourceId, Description, ItemName) 
                                            VALUES  (@ItemId, @CbId, @CbType, @Descriptor, @TimestampUTC, @Author, @FileContent, @Language, @ParentItemId, @ItemType, @DataSourceId, @Description, @ItemName )";
                }
            }

            ExecuteOnThreadPool(() => {
                GetDbInstance().CreateCommand(sqlQuery)
                    .SetParameter("ItemId", itemToInsert.ItemId.ToString().ToUpperInvariant())
                    .SetParameter("ItemType", 0)
                    .SetParameter("ParentItemId", itemToInsert.ParentItemId.ToString().ToUpperInvariant())
                    .SetParameter("CbId", itemToInsert.InternalID ?? "")
                    .SetParameter("CbType", itemToInsert.Type ?? "")
                    .SetParameter("Descriptor", itemToInsert.Descriptor ?? "")
                    .SetParameter("FileContent", fileContent)
                    .SetParameter("TimestampUTC", itemToInsert.LastModificationUTC.ToBinary())  // Note that this is always UTC time (convert to local time for the UI)
                    .SetParameter("Author", itemToInsert.Author ?? "")
                    .SetParameter("Language", itemToInsert.Language ?? "")
                    .SetParameter("DataSourceId", itemToInsert.DataSourceId.ToString().ToUpperInvariant())
                    .SetParameter("Description", itemToInsert.Description ?? "")
                    .SetParameter("ItemName", itemToInsert.ItemName ?? "")
                    .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateItem Get finished");
        }

        // See Interface
        public void DeleteItem(string id)
        {
            Logger.Info("SQLiteFileRepository", "DeleteItem Get started");
            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand("DELETE FROM RepoItems WHERE CbId = @CbId")
                    .SetParameter("CbId", id)
                    .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "DeleteItem Get finished");
        }

        // See Interface
        public IEnumerable<RepositoryItem> GetAllItems()
        {
            Logger.Info("SQLiteFileRepository", "GetAllItems Get started");
            var ret = GetItemsFromDb().Where(x => x.ItemType == 0); //TODO FAKE .... IMPLEMENTS THE CORRECT QUERY ON SQLITE
            Logger.Info("SQLiteFileRepository", "GetAllItems Get finished");
            return ret;
        }

        // See Interface
        public RepositoryItem GetItem(string id)
        {
            Logger.Info("SQLiteFileRepository", "GetItem Get started");
            var ret = GetItemsFromDb(id).FirstOrDefault();
            Logger.Info("SQLiteFileRepository", "GetAllItems Get finished");
            return ret;
        }

        // See Interface
        public void LoadItem(string id, Stream destinationStream, CancellationToken cancelToken)
        {
            Logger.Info("SQLiteFileRepository", "LoadItem Get started");
            ExecuteOnThreadPool(() =>
            {
                byte[] content = (byte[])GetDbInstance().CreateCommand(
                 "SELECT FileContent FROM RepoItems WHERE CbId = @CbId")
                    .SetParameter("CbId", id).ExecuteScalar();

                destinationStream.Write(content, 0, content.Length);
            });
            Logger.Info("SQLiteFileRepository", "LoadItem Get finished");
        }

        // See Interface
        public bool LockItem(string id)
        {
            // If required, a repository item can be locked when it is loaded for editing. List & Label will call unlock when the designer is closed.
            // IMPORTANT: Always implement a fallback to release the locks (e.g. timeout). Especially when used with the Web Designer, UnlockItem() might not get called due to network problems.

            // Return true, if the lock was acquired or if no locking is implemented.
            // Return false, if the item is locked by an other user. The designer will show an error message and open the item in read-only mode.
            Logger.Info("SQLiteFileRepository", "LockItem");
            return true;
        }

        // See Interface
        public void UnlockItem(string id)
        {
            // If required, a repository item can be locked when it is loaded for editing. List & Label will call unlock when the designer is closed.
            // IMPORTANT: Always implement a fallback to release the locks (e.g. timeout). Especially when used with the Web Designer, UnlockItem() might not get called due to network problems.

            // Return true, if the lock was acquired or if no locking is implemented.
            // Return false, if the item is locked by an other user. The designer will show an error message and open the item in read-only mode.
            Logger.Info("SQLiteFileRepository", "UnlockItem");
        }

        #endregion

        public void DeleteItemById(Guid id)
        {
            Logger.Info("SQLiteFileRepository", "DeleteItemById Get started");
            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand("DELETE FROM RepoItems WHERE ItemId = @ItemId")
                .SetParameter("ItemId", id.ToString().ToUpperInvariant())
                .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "DeleteItemById Get finished");
        }

        public void Import(CustomizedRepositoryItem itemToInsert, byte[] fileContent)
        {
            Logger.Info("SQLiteFileRepository", "Import Get started");

            var sqlQuery = @"INSERT INTO RepoItems (ItemId, CbId,  CbType,  Descriptor,  TimestampUTC,  Author,  FileContent, Language, ParentItemId, ItemType, DataSourceId, Description, ItemName) 
                                              VALUES  (@ItemId, @CbId, @CbType, @Descriptor, @TimestampUTC, @Author, @FileContent, @Language, @ParentItemId, @ItemType, @DataSourceId, @Description, @ItemName )";

            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand(sqlQuery)
                    .SetParameter("ItemId", itemToInsert.ItemId.ToString().ToUpperInvariant())
                    .SetParameter("ItemType", 0)
                    .SetParameter("ParentItemId", itemToInsert.ParentItemId.ToString().ToUpperInvariant())
                    .SetParameter("CbId", itemToInsert.InternalID ?? "")
                    .SetParameter("CbType", itemToInsert.Type ?? "")
                    .SetParameter("Descriptor", itemToInsert.Descriptor ?? "")
                    .SetParameter("FileContent", fileContent)
                    .SetParameter("TimestampUTC", itemToInsert.LastModificationUTC.ToBinary())  // Note that this is always UTC time (convert to local time for the UI)
                    .SetParameter("Author", itemToInsert.Author ?? "")
                    .SetParameter("Language", itemToInsert.Language ?? "")
                    .SetParameter("DataSourceId", itemToInsert.DataSourceId.ToString().ToUpperInvariant())
                    .SetParameter("Description", itemToInsert.Description ?? "")
                    .SetParameter("ItemName", itemToInsert.ItemName ?? "")
                    .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "Import Get finished");
        }

        public DenormalizedCustomizedRepositoryItem GetItemById(Guid itemId)
        {
            Logger.Info("SQLiteFileRepository", "GetItemById Get started");
            var ret = GetItemsFromDb(itemId: itemId).FirstOrDefault();
            Logger.Info("SQLiteFileRepository", "GetItemById Get finished");
            return ret;
        }

        public DenormalizedCustomizedRepositoryItem GetItemByItemNameAndParentItem(string itemName, Guid parentItemId)
        {
            Logger.Info("SQLiteFileRepository", "GetItemByItemNameAndParentItem Get started");
            var ret = GetItemsFromDb().FirstOrDefault(x => x.ItemName == itemName && x.ParentItemId == parentItemId); //TODO FAKE .... IMPLEMENTS THE CORRECT QUERY ON SQLITE
            Logger.Info("SQLiteFileRepository", "GetItemByItemNameAndParentItem Get finished");
            return ret;
        }

        public IList<DenormalizedCustomizedRepositoryItem> GetAllItemsWithFolder(bool OnlyGreenFlag = false)
        {
            Logger.Info("SQLiteFileRepository", "GetAllItemsWithFolder Get started");
            var ret = GetItemsFromDb(OnlyGreenFlag: OnlyGreenFlag); //TODO FAKE .... IMPLEMENTS THE CORRECT QUERY ON SQLITE
            Logger.Info("SQLiteFileRepository", "GetAllItemsWithFolder Get finished");
            return ret;
        }

        public IList<DenormalizedCustomizedRepositoryItem> GetAllFolder()
        {
            Logger.Info("SQLiteFileRepository", "GetAllFolder Get started");
            var ret = GetItemsFromDb().Where(x => x.ItemType == 1).ToList(); //TODO FAKE .... IMPLEMENTS THE CORRECT QUERY ON SQLITE
            Logger.Info("SQLiteFileRepository", "GetAllFolder Get finished");
            return ret;
        }

        public bool ContainsFoderId(Guid itemId)
        {
            Logger.Info("SQLiteFileRepository", "ContainsFoderId Get started");
            var ret = ExecuteOnThreadPool<bool>(() =>
            {
                var resQry = GetDbInstance().CreateCommand(@"SELECT COUNT(*) FROM RepoItems WHERE ItemId = @ItemId")
                    .SetParameter("ItemId", itemId.ToString().ToUpperInvariant()).ExecuteScalar();
                var result = Convert.ToInt32(resQry, CultureInfo.InvariantCulture);

                return (result == 1);
            });
            Logger.Info("SQLiteFileRepository", "ContainsFoderId Get finished");
            return ret;
        }

        public string EncodeDescriptor(string itemName)
        {
            Logger.Info("SQLiteFileRepository", "EncodeDescriptor Get started");
            var ret = ReEncodeDescriptor(itemName, "");
            Logger.Info("SQLiteFileRepository", "EncodeDescriptor Get finished");
            return ret;
        }

        public string DecodeDescriptor(string descriptor)
        {
            Logger.Info("SQLiteFileRepository", "DecodeDescriptor Get started");
            var tmp = RepositoryItemDescriptor.LoadFromDescriptorString(descriptor);
            Logger.Info("SQLiteFileRepository", "DecodeDescriptor Get finished");
            return tmp.GetUIName(0);   
        }

        public string ReEncodeDescriptor(string itemName, string currentDescriptor)
        {
            Logger.Info("SQLiteFileRepository", "ReEncodeDescriptor Get started");
            var descriptor = RepositoryItemDescriptor.LoadFromDescriptorString(currentDescriptor);
            descriptor.SetUIName(0, itemName);   // 0 = default language
            Logger.Info("SQLiteFileRepository", "ReEncodeDescriptor Get finished");
            return descriptor.SerializeToString();
        }

        public void CreateOrUpdateFolder(Guid itemId, string itemName, Guid ParentItemId, string description)
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateFolder Get started");

            bool isUpdate = ContainsFoderId(itemId);
            string sqlQuery;
            string descriptor = EncodeDescriptor(itemName);
            if (isUpdate)
            {
                sqlQuery = @"UPDATE RepoItems 
                                 SET ParentItemId = @ParentItemId, Descriptor = @Descriptor, Description = @Description, ItemName = @ItemName
                                 WHERE ItemId = @ItemId";
            }
            else    // INSERT
            {
                sqlQuery = @"INSERT INTO RepoItems (ItemId, CbId,  CbType,  Descriptor,  TimestampUTC,  Author,  Language, ParentItemId, ItemType, DataSourceId, Description, ItemName) 
                                              VALUES  (@ItemId, @CbId, @CbType, @Descriptor, @TimestampUTC, @Author, @Language, @ParentItemId, 1, @DataSourceId, @Description, @ItemName)";
            }

            ExecuteOnThreadPool(() =>
            {
                using (var cmd = GetDbInstance().CreateCommand(sqlQuery))
                {
                    cmd
                    .SetParameter("ItemId", itemId.ToString().ToUpperInvariant())
                    .SetParameter("CbId", "")
                    .SetParameter("CbType", "")
                    .SetParameter("Descriptor", descriptor)
                    .SetParameter("FileContent", null)
                    .SetParameter("TimestampUTC", DateTime.UtcNow.ToBinary())  // Note that this is always UTC time (convert to local time for the UI)
                    .SetParameter("Author", "")
                    .SetParameter("Language", "en") // TODO check the way to remove this hardcoded
                    .SetParameter("ParentItemId", ParentItemId.ToString().ToUpperInvariant())
                    .SetParameter("DataSourceId", default(Guid).ToString().ToUpperInvariant())
                    .SetParameter("Description", description ?? "")
                    .SetParameter("ItemName", itemName ?? "")
                    .ExecuteNonQuery();
                }
            });
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateFolder Get finished");
        }

        public IEnumerable<RoleItem> GetRoleItems(bool withAdministrator = true)
        {
            Logger.Info("SQLiteFileRepository", "GetRoleItems Get started");
            var ret = ExecuteOnThreadPool<IEnumerable<RoleItem>>(() => {
                List<RoleItem> result = new List<RoleItem>();
                string Admin_RoleId = "B2378A00-B242-428E-BDBA-3940751CAF75";
                var query = "SELECT R.RoleId, R.RoleName, R.Description, " +
                    "R.CanManageRoles,R.CanViewReport, R.CanModifyReport, R.CanManageDatasources, R.CanViewDatasources " +
                    "FROM Roles R";
                if (!withAdministrator)
                {
                    query += " WHERE RoleId <> '" + Admin_RoleId + "'";
                }
                var cmd = GetDbInstance().CreateCommand(query);            

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new RoleItem(
                                Guid.Parse(reader.GetString(reader.GetOrdinal("RoleId"))),
                                reader.GetString(reader.GetOrdinal("RoleName")),
                                reader.GetString(reader.GetOrdinal("Description")),
                                reader.GetInt32(reader.GetOrdinal("CanManageRoles")),
                                reader.GetInt32(reader.GetOrdinal("CanViewReport")),
                                reader.GetInt32(reader.GetOrdinal("CanModifyReport")),
                                reader.GetInt32(reader.GetOrdinal("CanViewDatasources")),
                                reader.GetInt32(reader.GetOrdinal("CanManageDatasources"))
                            ));
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "GetRoleItems Get finished");
            return ret;
        }

        public IEnumerable<CustomizedUserRoleItem>GetUserRoleItems(String userName, List<String> groups, Guid itemId, Boolean findByUser)
        {
            Logger.Info("SQLiteFileRepository", "GetUserRoleItems Get started");
            List<CustomizedUserRoleItem> result = new List<CustomizedUserRoleItem>();
            StringBuilder query = new StringBuilder();
            query.Append("SELECT UR.UserRoleId, UR.RoleId, COALESCE(UR.ItemId, '" + Guid.Empty.ToString() + "') AS ItemId, U.UserId, U.UserName," +
                " R.RoleName, COALESCE(R2.CanManageRoles, 0 ) AS CanManageRoles , R.CanViewReport, R.CanModifyReport, " +
                " COALESCE(R2.CanViewDatasources, 0) as CanViewDatasources, " +
                " COALESCE(R2.CanManageDatasources, 0)  as CanManageDatasources " +
                " FROM UserRoles UR " +
                " JOIN Roles R on UR.RoleId = R.RoleId " +
                " JOIN Users U on UR.UserId = U.UserId " +
                " LEFT OUTER JOIN Roles R2 on U.RoleId = R2.RoleId ");

            Boolean rootItem = itemId.Equals(Guid.Empty);

            if (rootItem) {
                query.Append(" WHERE UR.ItemId IS NULL ");
            }else {
                query.Append(" WHERE UR.ItemId = @ItemId ");
            }

            if (findByUser)
            {
                if (groups.Count > 0)
                {
                    var glist = String.Join("','", groups);
                    query.Append(" AND (U.UserName = @UserName OR U.UserName IN ('");
                    query.Append(glist);
                    query.Append("'))");
                }
                else
                {
                    query.Append(" AND (U.UserName = @UserName) ");
                }
            }

            var ret = ExecuteOnThreadPool<IEnumerable<CustomizedUserRoleItem>>(() => {
                var cmd = GetDbInstance().CreateCommand(query.ToString());
                if (!rootItem) cmd.SetParameter("ItemId", itemId.ToString().ToUpperInvariant());
                if (findByUser) cmd.SetParameter("UserName", userName.ToUpperInvariant());
                //var cmd = _db.CreateCommand("Select * FROM UserRoles");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new CustomizedUserRoleItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("UserRoleId"))),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("RoleId"))),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("ItemId"))),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("UserId"))),
                            reader.GetString(reader.GetOrdinal("UserName")).ToUpperInvariant(),
                            reader.GetString(reader.GetOrdinal("RoleName")),
                            reader.GetInt32(reader.GetOrdinal("CanManageRoles")),
                            reader.GetInt32(reader.GetOrdinal("CanViewReport")),
                            reader.GetInt32(reader.GetOrdinal("CanModifyReport")),
                            reader.GetInt32(reader.GetOrdinal("CanManageDatasources")),
                            reader.GetInt32(reader.GetOrdinal("CanViewDatasources"))
                            ));
                    }
                }

                return result;
            });
            Logger.Info("SQLiteFileRepository", "GetUserRoleItems Get finished");
            return ret;
        }

        public UserRoleItem GetUserRoleFromDb(Guid userRoleId)
        {
            Logger.Info("SQLiteFileRepository", "GetUserRoleFromDb Get started");
            UserRoleItem result = null;
            var sqlQuery = @"SELECT UR.UserRoleId, UR.RoleId, UR.ItemId, UR.UserId FROM UserRoles UR Where UR.UserRoleId = @userRoleId";

            var ret = ExecuteOnThreadPool<UserRoleItem>(() => {
                var cmd = GetDbInstance().CreateCommand(sqlQuery);
                cmd.SetParameter("userRoleId", userRoleId.ToString().ToUpperInvariant());
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new UserRoleItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("UserRoleId"))),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("RoleId"))),
                            (reader.GetValue(reader.GetOrdinal("ItemId")).GetType().Equals(typeof(DBNull))) ?
                            Guid.Empty : Guid.Parse(reader.GetString(reader.GetOrdinal("ItemId"))),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("UserId")))
                        );                    
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "GetUserRoleFromDb Get finished");
            return ret;
        }


        public UserItem GetUserFromDb(String userName)
        {
            Logger.Info("SQLiteFileRepository", "GetUserFromDb Get started");

            UserItem result = null;
            var sqlQuery = @"SELECT U.UserId, U.UserName, COALESCE (U.RoleId, '00000000-0000-0000-0000-000000000000') AS RoleId
                            FROM Users U Where U.UserName = @userName";

            var ret = ExecuteOnThreadPool<UserItem>(() => {
                var cmd = GetDbInstance().CreateCommand(sqlQuery);
                cmd.SetParameter("userName", userName.ToString());
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new UserItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("UserId"))),
                            reader.GetString(reader.GetOrdinal("UserName")),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("RoleId")))
                        );
                    }
                }
                return result;
            });
            Logger.Info("SQLiteFileRepository", "GetUserFromDb Get finished");
            return ret;
        }

        public void CreateOrUpdateUserRole(Guid UserRoleId, Guid RoleId, Guid ItemId, String UserName, bool isAdministrator)
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateUserRole Get started");

            string sqlQuery;
            string userSqlQuery;

            string Admin_RoleId = "B2378A00-B242-428E-BDBA-3940751CAF75";
            bool UserRole_isUpdate = GetUserRoleFromDb(UserRoleId) != null;
            UserItem UserItem = GetUserFromDb(UserName);
          
            if (UserRole_isUpdate)
            { //UPDATE
                sqlQuery = @"UPDATE UserRoles 
                                 SET RoleId = @RoleId, 
                                     ItemId = @ItemId,
                                     UserId = @UserId                                    
                                 WHERE UserRoleId = @UserRoleId";

                userSqlQuery = @"Update Users 
                                SET RoleId = @RoleId
                                WHERE UserId = @UserId";
                                
            }
            else    // INSERT
            {
                
                userSqlQuery = (UserItem != null) 
                    ? @"Update Users SET RoleId = @RoleId
                                WHERE UserId = @UserId" :
                    @"INSERT INTO Users (UserId, UserName, RoleId)
                     VALUES (@UserId, @UserName, @RoleId) ";                              

                sqlQuery = (ItemId.Equals(Guid.Empty)) 
                            ? @"INSERT INTO UserRoles (UserRoleId, RoleId, UserId) 
                                              VALUES  (@UserRoleId, @RoleId, @UserId)"
                            : @"INSERT INTO UserRoles (UserRoleId, RoleId, ItemId, UserId) 
                                              VALUES  (@UserRoleId, @RoleId, @ItemId, @UserId)";           
          
            }

            ExecuteOnThreadPool(() => {
                String userId = (UserItem != null) ?
                                UserItem.UserId.ToString().ToUpperInvariant() :
                                Guid.NewGuid().ToString().ToUpperInvariant();
                GetDbInstance().CreateCommand(userSqlQuery)
                   .SetParameter("UserId", userId)
                   .SetParameter("UserName", UserName)
                   .SetParameter("RoleId", (isAdministrator) ? Admin_RoleId : null)
                   .ExecuteNonQuery();

                GetDbInstance().CreateCommand(sqlQuery)
                    .SetParameter("RoleId", RoleId.ToString().ToUpperInvariant())
                    .SetParameter("ItemId", (ItemId.Equals(Guid.Empty))? null :ItemId.ToString().ToUpperInvariant())
                    .SetParameter("UserId", userId)
                    .SetParameter("UserRoleId", UserRoleId.ToString().ToUpperInvariant())               
                    .ExecuteNonQuery();               
            });
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateUserRole Get finished");
        }

        public void DeleteUserRole(Guid UserRoleId)
        {
            Logger.Info("SQLiteFileRepository", "DeleteUserRole Get started");

            UserRoleItem user = GetUserRoleFromDb(UserRoleId);

            string query = @"DELETE FROM UserRoles WHERE UserRoleId = @UserRoleId";
            string query_user = @"DELETE FROM Users WHERE UserId = @UserId";
            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand(query)
                     .SetParameter("UserRoleId", UserRoleId.ToString().ToUpperInvariant())
                     .ExecuteNonQuery();

                GetDbInstance().CreateCommand(query_user)
                     .SetParameter("UserId", user.UserId.ToString().ToUpperInvariant())
                     .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "DeleteUserRole Get finished");
        }

        #region "Helpers"


        /// <summary>Reads one or all items (itemId = null) from the database.</summary>
        private IList<DenormalizedCustomizedRepositoryItem> GetItemsFromDb(string cbId = null, Guid itemId = default(Guid), bool OnlyGreenFlag = false)
        {
            Logger.Info("SQLiteFileRepository", "GetItemsFromDb Get started");

            var ret = ExecuteOnThreadPool<IList<DenormalizedCustomizedRepositoryItem>>(() => { 
                List <DenormalizedCustomizedRepositoryItem> result = new List<DenormalizedCustomizedRepositoryItem>();

                var cmd = GetDbInstance().CreateCommand("SELECT R.ItemId, R.CbId, R.CbType, R.Descriptor, R.ItemName, R.TimestampUTC, R.Author, LENGTH(R.FileContent) as IsEmpty, R.Language, R.ParentItemId, R.ItemType, R.DataSourceId, R.Description, D.DataSourceName FROM RepoItems R JOIN DataSources D ON R.DataSourceId = D.DataSourceId");

                // Define language for the reports
                cmd.CommandText += " WHERE (Language isnull OR Language=@Language)";
                cmd.SetParameter("Language", _reportLanguage);

                if (cbId != null)   // Optional: select a specific item by it`s ID
                {
                    cmd.CommandText += " AND (CbId = @CbId)";
                    cmd.SetParameter("CbId", cbId);
                }

                if (itemId != default(Guid))   // Optional: select a specific item by it`s ID
                {
                    cmd.CommandText += " AND (ItemId = @ItemId)";
                    cmd.SetParameter("ItemId", itemId.ToString().ToUpperInvariant());
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var isEmpty = true;
                        if (!reader.IsDBNull(reader.GetOrdinal("IsEmpty")))
                        {
                            isEmpty = reader.GetInt32(reader.GetOrdinal("IsEmpty")) == 0;
                        }

                        Guid dataSourceId = Guid.Parse(reader.GetString(reader.GetOrdinal("DataSourceId")));
                        bool isValid = (dataSourceId != Guid.Empty);
                        if (OnlyGreenFlag && reader.GetInt32(reader.GetOrdinal("ItemType")) == 0 && (!isValid || isEmpty)) { continue;  }
                        result.Add(new DenormalizedCustomizedRepositoryItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("ItemId"))),
                            reader.GetString(reader.GetOrdinal("CbId")),
                            reader.GetString(reader.GetOrdinal("Descriptor")),
                            reader.GetString(reader.GetOrdinal("CbType")),
                            DateTime.FromBinary(reader.GetInt64(reader.GetOrdinal("TimestampUTC"))),
                            reader.GetString(reader.GetOrdinal("Author")),
                            reader.GetString(reader.GetOrdinal("Language")),
                            Guid.Parse(reader.GetString(reader.GetOrdinal("ParentItemId"))),
                            reader.GetInt32(reader.GetOrdinal("ItemType")),
                            dataSourceId,
                            reader.GetString(reader.GetOrdinal("Description")),
                            reader.GetString(reader.GetOrdinal("DataSourceName")),
                            reader.GetString(reader.GetOrdinal("ItemName")),
                            isValid)
                        {
                            IsEmpty = isEmpty
                        });
                    }
                }

                return result;
            });
            Logger.Info("SQLiteFileRepository", "GetItemsFromDb Get finished");
            return ret;
        }

        public void SetItemMetadata(Guid itemId, string descriptor, Guid parentItemId, Guid dataSourceId, string description, string itemName)
        {
            Logger.Info("SQLiteFileRepository", "SetItemMetadata Get started");

            ExecuteOnThreadPool(() => { 
                GetDbInstance().CreateCommand(@"
                     UPDATE RepoItems 
                     SET Descriptor = @Descriptor,
                         ParentItemId = @ParentItemId,
                         DataSourceId = @DataSourceId,
                         Description = @Description,
                         ItemName = @ItemName
                     WHERE ItemId = @ItemId")
                        .SetParameter("Descriptor", descriptor ?? "")
                        .SetParameter("ItemId", itemId.ToString().ToUpperInvariant())
                        .SetParameter("ParentItemId", parentItemId.ToString().ToUpperInvariant())
                        .SetParameter("DataSourceId", dataSourceId.ToString().ToUpperInvariant())
                        .SetParameter("Description", description ?? "")
                        .SetParameter("ItemName", itemName ?? "")
                        .ExecuteNonQuery();
            });
            Logger.Info("SQLiteFileRepository", "SetItemMetadata Get ended");
        }

        private bool CheckUniqueNameInDestination(List<Guid> itemIdList, Guid parentItemId)
        {
            Logger.Info("SQLiteFileRepository", "CheckUniqueNameInDestination Get started");

            string itemIdListCommaSeparated = string.Join("','", itemIdList).ToUpperInvariant();
            string sql = @"SELECT ItemId 
                                 FROM RepoItems 
                                WHERE ParentItemId = @ParentItemId 
                                  AND ItemName in (
                               SELECT ItemName 
                                 FROM RepoItems 
                                WHERE ItemId IN (@ItemIdList)) LIMIT 1";
            sql = sql.Replace("@ItemIdList", $"'{itemIdListCommaSeparated}'");

            var ret = ExecuteOnThreadPool<bool>(() => { 
                var cmd = GetDbInstance().CreateCommand(sql);
                cmd.SetParameter("ParentItemId", parentItemId.ToString().ToUpperInvariant());

                bool existsWithSameName = false;
                using (var reader = cmd.ExecuteReader())
                {
                    existsWithSameName = reader.Read();
                }

                return existsWithSameName;
            });
            Logger.Info("SQLiteFileRepository", "CheckUniqueNameInDestination Get finished");
            return ret;
        }

        public bool CheckUniqueNameInDestinationByName(List<string> itemNameList, Guid parentItemId)
        {
            Logger.Info("SQLiteFileRepository", "CheckUniqueNameInDestinationByName Get started");

            string itemNameListCommaSeparated = string.Join("','", itemNameList);
            string sql = @"SELECT ItemId 
                             FROM RepoItems 
                            WHERE ParentItemId = @ParentItemId 
                              AND ItemName IN (@ItemNameList)
                            LIMIT 1";
            sql = sql.Replace("@ItemNameList", $"'{itemNameListCommaSeparated}'");

            var ret = ExecuteOnThreadPool<bool>(() =>
            {
                var cmd = GetDbInstance().CreateCommand(sql);
                cmd.SetParameter("ParentItemId", parentItemId.ToString().ToUpperInvariant());

                bool existsWithSameName = false;
                using (var reader = cmd.ExecuteReader())
                {
                    existsWithSameName = reader.Read();
                }

                return existsWithSameName;
            });
            Logger.Info("SQLiteFileRepository", "CheckUniqueNameInDestinationByName Get finished");
            return ret;
        }

        public bool CheckUniqueId(List<Guid> itemIdList)
        {
            Logger.Info("SQLiteFileRepository", "CheckUniqueId Get started");

            string itemIdListCommaSeparated = string.Join("','", itemIdList).ToUpperInvariant();
            string sql = @"SELECT ItemId 
                             FROM RepoItems 
                            WHERE ItemId IN (@ItemIdList)
                            LIMIT 1";
            sql = sql.Replace("@ItemIdList", $"'{itemIdListCommaSeparated}'");

            var ret = ExecuteOnThreadPool<bool>(() =>
            {
                var cmd = GetDbInstance().CreateCommand(sql);

                bool existsWithSameId = false;
                using (var reader = cmd.ExecuteReader())
                {
                    existsWithSameId = reader.Read();
                }

                return existsWithSameId;
            });
            Logger.Info("SQLiteFileRepository", "CheckUniqueId Get finished");
            return ret;
        }
        public void MoveToFolder(List<Guid> itemIdList, Guid parentItemId)
        {
            Logger.Info("SQLiteFileRepository", "MoveToFolder Get started");

            if (CheckUniqueNameInDestination(itemIdList, parentItemId))
            {
                throw new ApplicationException(ErrorCode.AlreadyExists.ToString());
            }

            string itemIdListCommaSeparated = string.Join("','", itemIdList).ToUpperInvariant();
            string sql = @"
                 UPDATE RepoItems 
                 SET ParentItemId = @ParentItemId
                 WHERE ItemId IN (@ItemIdList)";

            sql = sql.Replace("@ItemIdList", $"'{itemIdListCommaSeparated}'");

            ExecuteOnThreadPool(() =>
            {

                var ret = GetDbInstance().CreateCommand(sql)
                       .SetParameter("ParentItemId", parentItemId.ToString().ToUpperInvariant())
                       .ExecuteNonQuery();
            });

            Logger.Info("SQLiteFileRepository", "MoveToFolder Get finished");
        }
        public void ChangeDataSources(List<Guid> itemIdList, Guid datasourceId)
        {
            Logger.Info("SQLiteFileRepository", "ChangeDataSources Get started");

            string itemIdListCommaSeparated = string.Join("','", itemIdList).ToUpperInvariant();
            string sql = @"
                 UPDATE RepoItems 
                 SET DataSourceId = @DataSourceId
                 WHERE ItemId IN (@ItemIdList)";

            sql = sql.Replace("@ItemIdList", $"'{itemIdListCommaSeparated}'");

            ExecuteOnThreadPool(() =>
            {
                var ret = GetDbInstance().CreateCommand(sql)
                       .SetParameter("DataSourceId", datasourceId.ToString().ToUpperInvariant())
                       .ExecuteNonQuery();
            });

            Logger.Info("SQLiteFileRepository", "ChangeDataSources Get finished");
        }

        public void DeleteDatasource(Guid dataSourceId)
        {
            Logger.Info("SQLiteFileRepository", "DeleteDatasource Get started");

            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand("DELETE FROM DataSources WHERE DataSourceId = @Id")
                .SetParameter("Id", dataSourceId.ToString().ToUpperInvariant())
                .ExecuteNonQuery();
            });

            Logger.Info("SQLiteFileRepository", "DeleteDatasource Get finished");
        }
		
        public IEnumerable<DataSourceItem> GetDataSourcesFromDb()
        {
            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get started");
            var ret = GetDataSourcesFromDb(default(Guid));
            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get finished");

            return ret;
        }
		
        public IEnumerable<DataSourceItem> GetDataSourcesFromDb(Guid datasourceId)
        {
            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get started");

            var ret = ExecuteOnThreadPool<IEnumerable<DataSourceItem>>(() =>
            {
                List<DataSourceItem> result = new List<DataSourceItem>();

                var cmd = GetDbInstance().CreateCommand("SELECT DataSourceId, DataSourceType, DataSourceName, ServerName, DatabaseName, UserName, Password, UseWindowsAuth, Description FROM DataSources");

                if (datasourceId != default(Guid))
                {
                    cmd.CommandText += " WHERE (DataSourceId = @DataSourceId)";
                    cmd.SetParameter("DataSourceId", datasourceId.ToString().ToUpperInvariant());
                }

                cmd.CommandText += " ORDER BY DataSourceName";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool useWinAuth = reader.GetInt32(reader.GetOrdinal("UseWindowsAuth")) != 0;
                        string decryptPwd = string.Empty;
                        var encryptPwd = reader.GetString(reader.GetOrdinal("Password"));
                        if (!string.IsNullOrEmpty(encryptPwd))
                        {
                            decryptPwd = Unprotect(encryptPwd ?? "", "", DataProtectionScope.LocalMachine);
                        }

                        result.Add(new DataSourceItem(
                            Guid.Parse(reader.GetString(reader.GetOrdinal("DataSourceId"))),
                            reader.GetString(reader.GetOrdinal("DataSourceName")),
                            (DataSourceType)reader.GetInt32(reader.GetOrdinal("DataSourceType")),
                            reader.GetString(reader.GetOrdinal("ServerName")),
                            reader.GetString(reader.GetOrdinal("DatabaseName")),
                            reader.GetString(reader.GetOrdinal("UserName")),
                            decryptPwd,
                            useWinAuth,
                            reader.GetString(reader.GetOrdinal("Description"))
                            )
                        );
                    }
                }
                return result;
            });

            Logger.Info("SQLiteFileRepository", "GetDataSourcesFromDb Get finished");
            return ret;
        }
#pragma warning disable S107 // Methods should not have too many parameters
        public void CreateOrUpdateDataSource(Guid dataSourceId, string dataSourceName, DataSourceType dataSourceType, string serverName, string databaseName, string userName, string password, bool useWindowsAuth, string description)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDataSource Get started");

            if (!useWindowsAuth && (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userName)))
            {
                throw new ApplicationException("Invalid user/password. Empty user/passwords is not allowed!");
            }

            bool isUpdate = GetDataSourcesFromDb(dataSourceId).Any();

            string sqlQuery;
            if (isUpdate)
            { //TODO SECURITY... REMOVE CLEAR PASSWORD AND USE DPAPI
                sqlQuery = @"UPDATE DataSources 
                                 SET DataSourceName = @DataSourceName, 
                                     ServerName = @ServerName,
                                     DatabaseName = @DatabaseName,
                                     UserName = @UserName,
                                     Password = @Password,
                                     UseWindowsAuth = @UseWindowsAuth,
                                     Description = @Description
                                 WHERE DataSourceId = @DataSourceId";
            }
            else    // INSERT
            {
                sqlQuery = @"INSERT INTO DataSources (DataSourceId, DataSourceName, DataSourceType,  ServerName, DatabaseName, UserName, Password, UseWindowsAuth, Description) 
                                              VALUES  (@DataSourceId, @DataSourceName, @DataSourceType, @ServerName, @DatabaseName, @UserName, @Password, @UseWindowsAuth, @Description)";
            }
            string encryptPwd = string.Empty;
            if (!useWindowsAuth)
            {           
                encryptPwd = Protect(password, "", DataProtectionScope.LocalMachine);
            }

            ExecuteOnThreadPool(() =>
            {
                GetDbInstance().CreateCommand(sqlQuery)
                .SetParameter("DataSourceId", dataSourceId.ToString().ToUpperInvariant())
                .SetParameter("DataSourceName", dataSourceName ?? "")
                .SetParameter("DataSourceType", (int)dataSourceType)
                .SetParameter("ServerName", serverName ?? "")
                .SetParameter("DatabaseName", databaseName ?? "")
                .SetParameter("UserName", userName ?? "")
                .SetParameter("Password", encryptPwd)
                .SetParameter("UseWindowsAuth", useWindowsAuth ? 1 : 0)
                .SetParameter("Description", description ?? "")
                .ExecuteNonQuery();
            });

            Logger.Info("SQLiteFileRepository", "CreateOrUpdateDataSource Get finished");
        }

        #endregion

        // Don't use Dispose pattern for static members. Better specific CleanUp when Application shutdown.
        public static void CleanUp()
        {
            Logger.Info("SQLiteFileRepository", "CleanUp Get started");

            foreach (var item in _executorQueue)
            {
                Executor executor = null;
                if (_executorQueue.TryDequeue(out executor))
                {
                    executor.Run(() => {
                        IDbConnection db = null;
                        var currentThread = Thread.CurrentThread.ManagedThreadId;
                        _dbInstaces.TryRemove(currentThread, out db);
                        db?.Close();
                        db?.Dispose();
                    });
                    executor.Dispose();
                }
            }
            // It is needed to release file lock
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Logger.Info("SQLiteFileRepository", "CleanUp Get finished");
        }
        */
        private Executor GetExecutor()
        {
            Executor executor;
            if (!_executorQueue.TryDequeue(out executor))
            {
                executor = new Executor();
                executor.StartExecutionLoop();
            }

            return executor;
        }

        private void ExecuteOnThreadPool(Action action)
        {
            Executor executor = GetExecutor();

            try
            {
                executor.Run(() => { action(); });
                _executorQueue.Enqueue(executor);
            }
            catch (Exception ex)
            {
                Logger.Error("SQLiteFileRepository", "ExecuteOnThreadPool Action Error. " + ex.Message);
            }
        }

        private T ExecuteOnThreadPool<T>(Func<T> func)
        {
            T ret = default(T);
            Executor executor = GetExecutor();

            try
            {
                executor.Run(() => { ret = func(); });
                _executorQueue.Enqueue(executor);
            }
            catch (Exception ex)
            {
                Logger.Error("SQLiteFileRepository", "ExecuteOnThreadPool Func Error. " + ex.Message);
            }
            return ret;
        }
    }
}

