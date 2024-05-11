using EggLink.DanhengServer.Configuration;
using EggLink.DanhengServer.Database.Account;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Database.Mission;
using EggLink.DanhengServer.Util;
using Microsoft.Data.Sqlite;
using SqlSugar;
using System.Reflection;

namespace EggLink.DanhengServer.Database
{
    public class DatabaseHelper
    {
        public Logger logger = new("Database");
        public static SqlSugarScope? sqlSugarScope;
        public static DatabaseHelper? Instance;
        private static readonly Dictionary<int, object> _lock = [];

        public DatabaseHelper()
        {
            Instance = this;
        }

        public void Initialize()
        {
            logger.Info("Initializing database...");
            var config = ConfigManager.Config;
            DbType type;
            string connectionString;
            switch (config.Database.DatabaseType)
            {
                case "sqlite":
                    type = DbType.Sqlite;
                    var f = new FileInfo(config.Path.DatabasePath + "/" + config.Database.DatabaseName);
                    if (!f.Exists && f.Directory != null)
                    {
                        f.Directory.Create();
                    }
                    connectionString = $"Data Source={f.FullName};";
                    break;
                case "mysql":
                    type = DbType.MySql;
                    connectionString = $"server={config.Database.MySqlHost};Port={config.Database.MySqlPort};Database={config.Database.MySqlDatabase};Uid={config.Database.MySqlUser};Pwd={config.Database.MySqlPassword};";
                    break;
                default:
                    return;
            }

            sqlSugarScope = new(new ConnectionConfig()
            {
                ConnectionString = connectionString,
                DbType = type,
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new()
                {
                    SerializeService = new CustomSerializeService()
                }
            });
            switch (config.Database.DatabaseType)
            {
                case "sqlite":
                    InitializeSqlite();  // for all database types
                    break;
                case "mysql":
                    InitializeMysql();
                    break;
                default:
                    logger.Error("Unsupported database type");
                    break;
            }
        }

        public void UpgradeDatabase()
        {
            logger.Info("Upgrading database...");

            foreach (var instance in GetAllInstance<MissionData>()!)
            {
                instance.MoveFromOld();
            }

            foreach (var instance in GetAllInstance<InventoryData>()!)
            {
                UpdateInstance(instance);
            }
        }

        public void MoveFromSqlite()
        {
            logger.Info("Moving from sqlite...");

            var config = ConfigManager.Config;
            var f = new FileInfo(config.Path.DatabasePath + "/" + config.Database.DatabaseName);
            var sqliteScope = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = $"Data Source={f.FullName};",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                ConfigureExternalServices = new()
                {
                    SerializeService = new CustomSerializeService()
                },
            });

            var baseType = typeof(BaseDatabaseDataHelper);
            var assembly = typeof(BaseDatabaseDataHelper).Assembly;
            var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
            foreach (var type in types)
            {
                typeof(DatabaseHelper).GetMethod("MoveSqliteTable")?.MakeGenericMethod(type).Invoke(null, [sqliteScope]);
            }

            // exit the program
            Environment.Exit(0);
        }

        public static void MoveSqliteTable<T>(SqlSugarScope scope) where T : class, new()
        {
            try
            {
                var list = scope.Queryable<T>().ToList();
                foreach (var instance in list!)
                {
                    sqlSugarScope?.Insertable(instance).ExecuteCommand();
                }
            }
            catch (Exception e)
            {
                Logger.GetByClassName().Error("An error occurred while moving the table", e);
            }
        }

        public object GetLock(int uid)
        {
            if (!_lock.TryGetValue(uid, out object? value))
            {
                value = new();
                _lock[uid] = value;
            }
            return value;
        }

        public static void InitializeSqlite()
        {
            var baseType = typeof(BaseDatabaseDataHelper);
            var assembly = typeof(BaseDatabaseDataHelper).Assembly;
            var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
            foreach (var type in types)
            {
                typeof(DatabaseHelper).GetMethod("InitializeSqliteTable")?.MakeGenericMethod(type).Invoke(null, null);
            }
        }
        
        public static void InitializeMysql()
        {
            sqlSugarScope?.DbMaintenance.CreateDatabase();
            InitializeSqlite();
        }

        public static void InitializeSqliteTable<T>() where T : class, new()
        {
            try
            {
                sqlSugarScope?.Queryable<T>().ToList();
            } catch
            {
                sqlSugarScope?.CodeFirst.InitTables<T>();
            }
        }

        public T? GetInstance<T>(long uid) where T : class, new()
        {
            try
            {
                lock (GetLock((int)uid))
                {
                    return sqlSugarScope?.Queryable<T>().Where(it => (it as BaseDatabaseDataHelper)!.Uid == uid).First();
                }
            }
            catch (Exception e)
            {
                logger.Error("Unsupported type", e);
                return null;
            }
        }

        public T GetInstanceOrCreateNew<T>(int uid) where T : class, new()
        {
            var instance = GetInstance<T>(uid);
            if (instance == null)
            {
                instance = new();
                (instance as BaseDatabaseDataHelper)!.Uid = uid;
                SaveInstance(instance);
            }
            return instance;
        }

        public List<T>? GetAllInstance<T>() where T : class, new()
        {
            try
            {
                return sqlSugarScope?.Queryable<T>().ToList();
            } catch(Exception e)
            {
                logger.Error("Unsupported type", e);
                return null;
            }
        }

        public void SaveInstance<T>(T instance) where T : class, new()
        {
            lock (GetLock((instance as BaseDatabaseDataHelper)!.Uid))
            {
                sqlSugarScope?.Insertable(instance).ExecuteCommand();
            }
        }

        public void UpdateInstance<T>(T instance) where T : class, new()
        {
            lock (GetLock((instance as BaseDatabaseDataHelper)!.Uid))
            {
                sqlSugarScope?.Updateable(instance).ExecuteCommand();
            }
        }

        public void DeleteInstance<T>(T instance) where T : class, new()
        {
            lock (GetLock((instance as BaseDatabaseDataHelper)!.Uid))
            {
                sqlSugarScope?.Deleteable(instance).ExecuteCommand();
            }
        }
    }
}
