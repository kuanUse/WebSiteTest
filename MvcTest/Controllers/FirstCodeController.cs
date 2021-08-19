using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcTest.Models;

namespace MvcTest.Controllers
{
    public class FirstCodeController : Controller
    {
        // GET: FirstCode
        public ActionResult Index()
        {
            return View(FirstCode.GetPlayerNames());
        }
        [HttpGet] // httq://server/home/create, 新增資料用
        public ActionResult Create()
        {
            //預設會對映到/Views/Home/Create.cshtml
            return View();
        }
        [HttpPost] // httq://server/home/create 送出表單時
        public ActionResult Create(FirstCode player)
        {
            //前方送來的資料會自動對應到Player上(很神奇吧?)
            if (ModelState.IsValid)
            {
                player.Save();
            }
            //轉到httq://server/home/index
            return RedirectToAction("Index");
        }
        //httq://server/home/details?name=xxx 顯示詳細資料
        //name參數可用來承接?name=xxx所傳入的值
        public ActionResult Details(string name)
        {
            return View(FirstCode.Read(name));
        }
        [HttpGet]
        //httq://server/home/edit?name=xxx
        //提供輸入欄位及Submit鈕
        public ActionResult Edit(string name)
        {
            return View(FirstCode.Read(name));
        }
        [HttpPost] //httq://server/home/edit?name=xxx 送出表單時
        public ActionResult Edit(string name, FormCollection form)
        {
            //TODO: 實務上應加入更新權限檢核，在此省略

            //先讀出資料
            FirstCode origPlayer = FirstCode.Read(name);
            //用前端傳回的資料更新Key以外的欄位
            if (
                origPlayer != null &&
                TryUpdateModel<FirstCode>(
                    origPlayer,
                    //列出要更新的屬性, Name不得更換，故只有Score
                    //【補充】demo有篇TryUpdateModel介紹: http://demo.tc/Post/655
                    new string[] { "Score" }, form)
               )
                //資料無誤的話才儲存
                origPlayer.Save();
            else //更新失敗時
            {
                ModelState.AddModelError("UpdateError", "更新失敗!");
                return View(origPlayer);
            }
            return RedirectToAction("Index");

        }
        [HttpGet]
        //httq://server/home/delete?name=xxx, 
        //顯示詳細資料(等同Details)，提供Submit鈕確認刪除
        public ActionResult Delete(string name)
        {
            return View(FirstCode.Read(name));
        }
        [HttpPost]//httq://server/home/delete?name=xxx, 送出表單時
        public ActionResult Delete(string name, FormCollection col)
        {
            FirstCode player = FirstCode.Read(name);
            if (player != null)
                player.Delete();
            return RedirectToAction("Index");
        }
    }
}