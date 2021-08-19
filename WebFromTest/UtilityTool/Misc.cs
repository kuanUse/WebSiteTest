using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using static UtilityTool.SqlHelper;

namespace UtilityTool
{
    public class Misc : IRequiresSessionState
    {
        /// <summary>
        /// 建立Server暫存物件
        /// 過期時間預設30分鐘
        /// </summary>
        /// <param name="cacheKey">查詢Key</param>
        /// <param name="cacheObj"></param>
        /// <param name="cacheDurationSeconds">過期時間，預設30分鐘</param>
        public static void SetCacheObj(string cacheKey, object cacheObj, int cacheDurationSeconds = 1800)
        {

            if (!string.IsNullOrWhiteSpace(cacheKey) && cacheObj != null)
            {
                //if (cacheDurationSeconds <= 0)
                //{
                //    HttpRuntime.Cache.Insert(cacheKey, cacheObj);//Cache不過期
                //}
                HttpRuntime.Cache.Insert(
                         cacheKey,
                         cacheObj,
                         null,
                         System.Web.Caching.Cache.NoAbsoluteExpiration,
                         new TimeSpan(0, 0, cacheDurationSeconds),
                         System.Web.Caching.CacheItemPriority.NotRemovable,
                         null);
            }
        }
        public static object GetCacheObj(string cacheKey)
        {
            return HttpRuntime.Cache.Get(cacheKey);
        }
        public static void RemoveCacheObj(string cacheKey)
        {
            HttpRuntime.Cache.Remove(cacheKey);
        }
    }
}