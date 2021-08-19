using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace MvcTest.Models
{
    public class FirstCode
    {
        //使用檔案當成資料來源的玩家資料物件
        //先不牽扯資料庫可以更單純一點
        [Required]
        [Display(Name = "代號")]
        [RegularExpression("[A-Za-z0-9]{3,8}",
            ErrorMessage = "限定為3-8個英文或數字")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "分數")]
        [Range(0, 65535,
            ErrorMessage = "範圍: 0 - 65535")]
        public int Score { get; set; }

        #region 儲存相關函數

        private static string storageFolder = HttpContext.Current.Server.MapPath(@"~/Score");

        private static string getFilePath(string name)
        {
            return Path.Combine(storageFolder, name + ".txt");
        }

        private string FilePath
        {
            get { return getFilePath(Name); }
        }

        #endregion 儲存相關函數

        #region 模擬 新增/修改/刪除/查詢 功能

        //儲存
        public void Save()
        {
            if (!Directory.Exists(storageFolder))
            {
                Directory.CreateDirectory(storageFolder);
            }
            using (var tw = new StreamWriter(FilePath, true))
            {
                tw.WriteLine(Score.ToString());
            }
        }

        //讀取
        public static FirstCode Read(string name)
        {
            string file = getFilePath(name);
            if (File.Exists(file))
                return new FirstCode()
                {
                    Name = name,
                    Score = int.Parse(File.ReadAllText(file))
                };
            else
                return null;
        }

        //刪除
        public void Delete()
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }

        //清單
        public static List<string> GetPlayerNames()
        {
            //列舉所有檔案名稱(亦即玩家名稱)，傳回字串陣列
            return
                Directory.GetFiles(storageFolder, "*.txt")
                .Select(o => Path.GetFileNameWithoutExtension(o)).ToList();
        }

        #endregion 模擬 新增/修改/刪除/查詢 功能
    }
}