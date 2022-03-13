using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using slnShopping.Models;
using System.Web.Security;
using PagedList;
using PagedList.Mvc;


namespace slnShopping.Controllers
{
    public class HomeController : Controller
    {
        //建立可存取資料庫的類別物件
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        //設定一頁可以顯示幾筆商品
        int pageSize = 6;
        //Get:Home/Index
        public ActionResult Index(int page=1)
        {
            
            int currentPage = page < 1 ? 1 : page;
            var product = db.tProduct.OrderByDescending(m =>m.fId).ToList();
            //設定商品要顯示幾頁，每頁6筆商品
            var result = product.ToPagedList(currentPage, pageSize);
            return View(result);
        }
        //Get:Home/Login
        public ActionResult Login()
        {
            return View();
        }
        //Post:Home/Login
        [HttpPost]
        public ActionResult Login(string fUserId,string fPwd)
        {
            //輸入帳號和密碼與資料庫的資料比對後指定給mumber
            var member = db.tMember.Where(m => m.fUserId == fUserId && m.fPwd == fPwd).FirstOrDefault();
            //若 member 是null，表示會員未註冊或帳號或密碼錯誤
            if (member == null)
            {
                ViewBag.Message = "登入失敗";
                return View();
            }
            //使用Session 變數紀錄歡迎詞
            Session["WelCome"] = member.fName + "歡迎光臨!";
            //使用者帳號密碼通過驗證
            FormsAuthentication.RedirectFromLoginPage(fUserId, true);
            
            return RedirectToAction("Index","Member");
        }
        //Get:Home/Register
        public ActionResult Register()
        {
            return View();
        }
        //Post:Home/Register
        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            //若帳號沒有通過驗證就顯示目前的頁面
            if(ModelState.IsValid == false)
            {
                return View();
            }
            //把取得會員輸入的帳號與資料庫比對指定給 mumber
            var member = db.tMember.Where(m => m.fUserId == pMember.fUserId).FirstOrDefault();
            //若 member 是null ，表示會員未註冊
            if(member == null)
            {
                //將會員紀錄新增到tMember資料表
                db.tMember.Add(pMember);
                //儲存資料
                db.SaveChanges();
                //會員註冊成功跳到登入畫面
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號已有人使用，註冊失敗";
            return View();
        }
    }
}