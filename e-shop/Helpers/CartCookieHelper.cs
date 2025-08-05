using e_shop.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace e_shop.Helpers
{
   

    public static class CartCookieHelper
    {
        public static void SaveCart(HttpContext context, List<CartItem> cart)
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            string json = JsonConvert.SerializeObject(cart);
            context.Response.Cookies.Append("CartData", json, options);
        }

        public static List<CartItem> LoadCart(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("CartData", out string json))
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }

        public static void ClearCart(HttpContext context)
        {
            context.Response.Cookies.Delete("CartData");
        }
    }

}
