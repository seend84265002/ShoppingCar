using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Antlr.Runtime.Misc;
using slnShopping.Models;
using PagedList.Mvc;

using PagedList;

namespace slnShopping.Controllers
{
    [Authorize]//簡單的登入過濾
    public class MemberController : Controller
    {
        // GET: Member
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        //設定一頁可以顯示幾筆商品
        int pageSize = 6;
        /// <summary>
        /// 顯示商品清單
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Index(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            //取的所有產品
            var product = db.tProduct.OrderByDescending(m => m.fId).ToList();
            //設定商品要顯示幾頁，每頁6筆商品
            var result = product.ToPagedList(currentPage, pageSize);
            return View("../Home/Index","_LayoutMember",result);
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        //get:Member/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();      //登出
            return RedirectToAction("../Home/Login");
        }
        /// <summary>
        /// 增加商品至購物車
        /// </summary>
        /// <param name="fPId"></param>
        /// <returns></returns>
        //get:Member/AddCar
        public ActionResult AddCar(string fPId)
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //依fId 找到會員放入購屋車的商品指定給 currentCar
            var currentCar = db.tOrderDetail.Where(m => m.fPId == fPId && m.fIsApproved == "否"
                             && m.fUserId == fUserId).FirstOrDefault();
            //該商品如果沒有在購物車就新增一筆產品資料
            if(currentCar == null)
            {
                //依fId 找出目前商品指定給product
                var product = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();
                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);
            }
            else
            {
                //如果購物車有該商品，數量就加1
                currentCar.fQty += 1;
            }
            
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }
        /// <summary>
        /// 刪除購物車的商品
        /// </summary>
        /// <param name="fId"></param>
        /// <returns></returns>
        public ActionResult DeleteCar(int fId)
        {
            //依fId 判斷要刪除的購物車裡的商品
            var orderDetail = db.tOrderDetail.Where(m => m.fId == fId).FirstOrDefault();
            //如果商品大於1就刪除1個該商品，等於1個就把該商品從購物車刪除
            if (orderDetail.fQty == 1)
            {
                db.tOrderDetail.Remove(orderDetail);
            }
            else
            {
                orderDetail.fQty -= 1;
            }
           
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }
        /// <summary>
        /// 顯示購物車清單
        /// </summary>
        /// <returns></returns>
        //get:Member/ShoppingCar 
        public ActionResult ShoppingCar()
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //取得該會員購物車裡的產品
            var orderDetails = db.tOrderDetail.Where(m => m.fUserId == fUserId
                                && m.fIsApproved == "否").ToList();

            return View(orderDetails);
        }
        /// <summary>
        /// 建立購物訂單
        /// </summary>
        /// <param name="fRecceiver"></param>
        /// <param name="fEmail"></param>
        /// <param name="fAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShoppingCar(string fRecceiver, string fEmail, string fAddress)
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //建立隨機獨立的訂單號碼
            string guid = Guid.NewGuid().ToString();
            //建立訂單資料
            tOrder order = new tOrder();
            order.fOrderGuid = guid;
            order.fUserId = fUserId;
            order.fReceiver = fRecceiver;
            order.fEmail = fEmail;
            order.fAddress = fAddress;
            order.fDate = DateTime.Now;
            db.tOrder.Add(order);
            //找出該會員所有購物車裡的商品指定給carList
            var carList = db.tOrderDetail.Where(m => m.fIsApproved == "否" && m.fUserId == fUserId).ToList();
            //將購物車的商品狀態fIsApproved改為 "是" 表示成立訂單
            foreach (var item in carList)
            {
                item.fIsApproved = "是";
                item.fOrderGuid = guid;
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        /// <summary>
        /// 顯示購物定單
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderList()
        {
            //取得會員帳號並指定給fUserId
            string fUserId = User.Identity.Name;
            //找出該會員的訂單按成立訂單的日期遞減排序
            var order = db.tOrder.Where(m => m.fUserId == fUserId).OrderByDescending(m => m.fDate).ToList();
            //把訂單顯示到網頁上
            return View(order);
        }
        /// <summary>
        /// 顯示商品訂單明細
        /// </summary>
        /// <param name="fOrderGuid"></param>
        /// <returns></returns>
        public ActionResult OrderDetail(string fOrderGuid)
        {
            //依 fOrderGuid(訂單編號) 訂單編號找出商品明細
            var orderDetails = db.tOrderDetail.Where(m => m.fOrderGuid == fOrderGuid).ToList();
            return View(orderDetails);
        }
    }
}