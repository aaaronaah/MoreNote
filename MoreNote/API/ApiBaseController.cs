﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreNote.Logic.Entity;
using MoreNote.Logic.Service;

namespace MoreNote.API
{
    public class ApiBaseController : Controller
    {
        public IActionResult action()
        {
            return Content("error");
        }
        //todo:得到token, 这个token是在AuthInterceptor设到Session中的
        public  string GetToken()
        {
            /**
             *  软件从不假设某个IP或者使用者借助cookie获得永久的使用权
             *  任何访问，必须显式的提供token证明
             *  
             *  API服务不接受cookie中的信息，token总是显式提交的
             *  
             *  
             *  
             **/
            string token = Request.Form[""];
            if (string.IsNullOrEmpty(token))
            {
                token=Request.Query["token"];
            }
            if (string.IsNullOrEmpty(token))
            {
                return "";
            }
            else
            {
                return token;
            }
           
        }
        // todo:得到用户信息
        public  long? getUserId()
        {
            string token=GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            else
            {
                User user= TokenSerivce.GetUserByToken(token);
                long? userid  = (user == null?0:user.UserId);
                return userid;
            }
           
        }
        public  User getUserByToken()
        {
            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            else
            {
                User user = TokenSerivce.GetUserByToken(token);
                return user;
            }

        }
        // todo :上传附件
        public IActionResult uploadAttach()
        {
            return null;
        }
        // todo :上传图片
        public IActionResult upload()
        {
            return null;
        }
    }
}