using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.Data;
using System.Web;
using System.Web.SessionState;

namespace UtilityTool
{
    public class ADVerify : IRequiresSessionState
    {
        public static readonly string UserInfoSessionKey = "userInfoSessionKey";
        public class UserInfo
        {
            public string ADUserKey { get; set; }
            public string ADEmployeeID { get; set; }
            public string FullName { get; set; }
            public string SamAccountName { get; set; }
            public string Domain { get; set; }
            public string Mail { get; set; }
        }
        /// <summary>
        /// 驗證間接人員帳號密碼是否正確
        /// </summary>
        /// <param name="strAccount"></param>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        public bool ChkADUserVerify(string strAccount, string strPassword)
        {
            var isExists = false;
            try
            {
                //先抓domain
                string strDomain = GetUserDomain(strAccount);
                if (!string.IsNullOrEmpty(strDomain))
                {
                    //檢查AD帳密是否正確
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, strDomain))
                    {
                        isExists = pc.ValidateCredentials(strAccount, strPassword);

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            return isExists;
        }
        public string GetUserDomain(string strAccount)
        {
            string strDomin = string.Empty;

            DataSet ds = new DataSet();
            string strSql = @"
                            Select U.DOMAIN,U.SAMACCOUNTNAME
                              From DW_FND_SQ_AD_USER U
                             Where UPPER(U.SAMACCOUNTNAME) = UPPER(:SAMACCOUNTNAME)
                                   And U.ACTIVE_FLAG = 'Y'";
            var domainDt = SqlHelper.doSql(strSql, new Dictionary<string, object>() {
                { "SAMACCOUNTNAME",strAccount}
            }, "WH01", DbCategory.Oracle, true);
            if (domainDt.AsEnumerable().Any())
            {
                strDomin = Convert.ToString(domainDt.Rows[0][0]);
            }
            return strDomin;

        }
        /// <summary>
        /// 從Session取得使用者資訊
        /// </summary>
        /// <returns></returns>
        public UserInfo GetUserInfo()
        {
            try
            {
                var userCacheKey = HttpContext.Current.Session[UserInfoSessionKey];
                if (userCacheKey == null || string.IsNullOrWhiteSpace(Convert.ToString(userCacheKey)))
                {
                    return null;
                }
                var UserInfoObj = Misc.GetCacheObj(Convert.ToString(userCacheKey));

                if (UserInfoObj == null)
                {
                    return null;
                }
                return (UserInfo)UserInfoObj;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 從DW_FND_SQ_AD_USER取得使用者資訊
        /// </summary>
        /// <returns></returns>
        public UserInfo GetUserInfo(string strAccount)
        {
            try
            {
                var userInfo = new UserInfo();
                DataSet ds = new DataSet();
                string strSql = @"
                            Select U.AD_USER_KEY
                                  ,U.FULL_NAME
                                  ,U.EMPLOYEE_ID
                                  ,U.DOMAIN
                                  ,U.SAMACCOUNTNAME
                                  ,U.MAIL
                              From DW_FND_SQ_AD_USER U
                             Where UPPER(U.SAMACCOUNTNAME) = UPPER(:SAMACCOUNTNAME)
                                   And U.ACTIVE_FLAG = 'Y'";
                var userDt = SqlHelper.doSql(strSql, new Dictionary<string, object>() {
                { "SAMACCOUNTNAME",strAccount}
            }, "WH01", DbCategory.Oracle, true);

                //找不到使用者資訊時回傳null
                if (!userDt.AsEnumerable().Any())
                {
                    return null;
                }
                var dr = userDt.Rows[0];
                userInfo.ADUserKey = Convert.ToString(dr["AD_USER_KEY"]);
                userInfo.FullName = Convert.ToString(dr["FULL_NAME"]);
                userInfo.ADEmployeeID = Convert.ToString(dr["EMPLOYEE_ID"]);
                userInfo.Domain = Convert.ToString(dr["DOMAIN"]);
                userInfo.SamAccountName = Convert.ToString(dr["SAMACCOUNTNAME"]);
                userInfo.Mail = Convert.ToString(dr["MAIL"]);

                var userInfoCacheKey = Guid.NewGuid().ToString("N");
                Misc.SetCacheObj(userInfoCacheKey, userInfo);
                HttpContext.Current.Session[UserInfoSessionKey] = userInfoCacheKey;
                return userInfo;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 驗證直接人員帳號密碼是否正確
        /// </summary>
        /// <param name="strAccount"></param>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        public bool ChkDLUserVerify(string strAccount, string strPassword)
        {
            //直接人員
            bool isExists = false;
            try
            {
                string strSql = @"
                            Select V.EMPNO
                              From APP_EMP_DL_OUT_V V
                             Where V.EMPNO = :EMPNO
                                   And V.PWD = :PWD";
                var dt = SqlHelper.doSql(strSql, new Dictionary<string, object>() {
                        { "EMPNO",strAccount},
                        { "PWD",strPassword},
                    }, "Door_APP", DbCategory.Oracle, true);
                if (dt.AsEnumerable().Any())
                {
                    isExists = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            return isExists;
        }
        public void UserLogout()
        {
            try
            {
                var userCacheKey = HttpContext.Current.Session[UserInfoSessionKey];
                if (userCacheKey == null || string.IsNullOrWhiteSpace(Convert.ToString(userCacheKey)))
                {
                    return;
                }
                Misc.RemoveCacheObj(Convert.ToString(userCacheKey));
                HttpContext.Current.Session.Remove(UserInfoSessionKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }

        }

        /// <summary>
        /// 使用where條件搜尋相關使用者，無條件時不執行搜尋
        /// </summary>
        /// <param name="whereSql"></param>
        /// <param name="requestParams"></param>
        /// <returns></returns>
        public List<UserInfo> SearchUserInfo(string whereSql, Dictionary<string, object> requestParams = null)
        {
            try
            {
                //沒有條件則不執行搜尋
                if (string.IsNullOrWhiteSpace(whereSql))
                {
                    return new List<UserInfo>();
                }

                DataSet ds = new DataSet();
                string strSql = string.Format(@"
                            Select U.AD_USER_KEY
                                  ,U.FULL_NAME
                                  ,U.EMPLOYEE_ID
                                  ,U.DOMAIN
                                  ,U.SAMACCOUNTNAME
                                  ,U.MAIL
                              From DW_FND_SQ_AD_USER U
                             Where U.ACTIVE_FLAG = 'Y' AND ({0})", whereSql);
                var userDt = SqlHelper.doSql(strSql, requestParams, "WH01", DbCategory.Oracle, true);

                //找不到使用者資訊時回傳空List
                if (!userDt.AsEnumerable().Any())
                {
                    return new List<UserInfo>();
                }
                var resultList = new List<UserInfo>();
                foreach (DataRow dr in userDt.Rows)
                {
                    var userInfo = new UserInfo();
                    userInfo.ADUserKey = Convert.ToString(dr["AD_USER_KEY"]);
                    userInfo.FullName = Convert.ToString(dr["FULL_NAME"]);
                    userInfo.ADEmployeeID = Convert.ToString(dr["EMPLOYEE_ID"]);
                    userInfo.Domain = Convert.ToString(dr["DOMAIN"]);
                    userInfo.SamAccountName = Convert.ToString(dr["SAMACCOUNTNAME"]);
                    userInfo.Mail = Convert.ToString(dr["MAIL"]);
                    resultList.Add(userInfo);
                }
                return resultList;
            }
            catch
            {
                return new List<UserInfo>();
            }
        }
    }
}