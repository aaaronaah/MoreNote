﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoreNote.Common.Util;
using MoreNote.Common.Utils;
using MoreNote.Logic.Entity;
using MoreNote.Logic.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MoreNote.Controllers
{

    /**
     * 源代码基本是从GO代码直接复制过来的
     * 
     * 只是简单的实现了API的功能
     * 
     * 2020年01月27日
     * */
    public class BaseController : Controller
    {
        public int pageSize = 1000;
        public string defaultSortField = "UpdatedTime";
        public string leanoteUserId = "admin";// 不能更改
        protected IHttpContextAccessor _accessor;
     
        public BaseController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;

        }
        public IActionResult action()
        {
            return Content("error");
        }
        //todo:得到token, 这个token是在AuthInterceptor设到Session中的
        public string GetToken()
        {
            /**
             *  软件从不假设某个IP或者使用者借助cookie获得永久的使用权
             *  任何访问，必须显式的提供token证明
             *  
             *  API服务不接受cookie中的信息，token总是显式提交的
             * 
             **/
            string token=null;
            if (_accessor.HttpContext.Request.Form!=null)
            {
                token= _accessor.HttpContext.Request.Form["token"];
            }
             
            if (string.IsNullOrEmpty(token))
            {
                token = _accessor.HttpContext.Request.Query["token"];
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
        public long GetUserIdBySession()
        {
            string userid_hex = _accessor.HttpContext.Session.GetString("_userId");
            long userid_number = MyConvert.HexToLong(userid_hex);
            return userid_number;
        }
        public User GetUserBySession()
        {
            string userid_hex = _accessor.HttpContext.Session.GetString("_userId");
            long userid_number = MyConvert.HexToLong(userid_hex);
            User user = UserService.GetUserByUserId(userid_number);
            return user;
        }
        // todo:得到用户信息
        public long getUserIdByToken(string token)
        {

            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }
            else
            {
                User user = TokenSerivce.GetUserByToken(token);
                long userid = (user == null ? 0 : user.UserId);
                return userid;
            }
        }
        public User getUserByToken(string token)
        {
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
        public long getUserIdByToken()
        {
            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                string userid_hex = _accessor.HttpContext.Session.GetString("userId");
                long userid_number = MyConvert.HexToLong(userid_hex);
                return userid_number;
            }
            else
            {
                User user = TokenSerivce.GetUserByToken(token);
                long userid = (user == null ? 0 : user.UserId);
                return userid;
            }
        }
        public void SetUserIdToSession(long userId)
        {
            _accessor.HttpContext.Session.SetString("userId", userId.ToString("x"));
        }
        public User getUserByToken()
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

        public long ConvertUserIdToLong()
        {
            string hex = _accessor.HttpContext.Request.Form["userId"];
            if (string.IsNullOrEmpty(hex))
            {
                hex = _accessor.HttpContext.Request.Query["userId"];
            }
            if (string.IsNullOrEmpty(hex))
            {
                return 0;
            }
            return MyConvert.HexToLong(hex);
        }
        // todo :上传附件
        public bool uploadAttach(string name,long userId, long noteId, out string msg, out long serverFileId)
        {
            msg = "";
            serverFileId = 0;

            var uploadDirPath = $"{RuntimeEnvironment.DirectorySeparatorChar}www/attachs/{userId.ToString("x")}/images/{DateTime.Now.ToString("yyyy_MM")}/";
            if (RuntimeEnvironment.IsWindows)
            {
                uploadDirPath = $@"upload\{userId.ToString("x")}\attachs\{DateTime.Now.ToString("yyyy_MM")}\";
            }
            var diskFileId = SnowFlake_Net.GenerateSnowFlakeID();
            serverFileId = diskFileId;
            var httpFiles = _accessor.HttpContext.Request.Form.Files;
            //检查是否登录
            if (userId == 0)
            {
                userId = GetUserIdBySession();
                if (userId == 0)
                {
                    msg = "NoLogin";
                    return false;
                }
            }

            if (httpFiles == null || httpFiles.Count < 1)
            {
                return false;
            }
            var httpFile = httpFiles[name];
            var fileEXT = Path.GetExtension(httpFile.FileName).Replace(".","");
            if (!IsAllowAttachExt(fileEXT))
            {
                msg = $"The_Attach_extension_{fileEXT}_is_blocked";
                return false;
            }
            var fileName = diskFileId.ToString("x") + "." + fileEXT;
            //判断合法性
            if (httpFiles == null || httpFile.Length < 0)
            {
                return false;
            }
            //将文件保存在磁盘
            Task<bool> task = SaveUploadFileOnDiskAsync(httpFile, uploadDirPath, fileName);
            bool result = task.Result;
            if (result)
            {
                //将结果保存在数据库
                AttachInfo attachInfo = new AttachInfo()
                {
                    AttachId = diskFileId,
                    UserId=userId,
                    NoteId = noteId,
                    UploadUserId = userId,
                    Name = fileName,
                    Title = httpFile.FileName,
                    Size = httpFile.Length,
                    Path = uploadDirPath + fileName,
                    Type=fileEXT.ToLower(),
                  
                    CreatedTime = DateTime.Now
                    //todo: 增加特性=图片管理

                };
                var AddResult = AttachService.AddAttach(attachInfo, true,out string AttachMsg);
                if (!AddResult)
                {
                    msg = "添加数据库失败";

                }
                return AddResult;
            }
            else
            {
                msg = "磁盘保存失败";
                return false;
            }
        }
        public long getUserId()
        {
            return 0;
        }

        public bool upload(string name,long userId,long noteId, bool isAttach, out long serverFileId, out string msg)
        {
            if (isAttach)
            {
                return uploadAttach(name,userId, noteId, out msg, out serverFileId);
            }
            msg = "";
            serverFileId = 0;
          
            var uploadDirPath = $"{RuntimeEnvironment.DirectorySeparatorChar}www/upload/{userId.ToString("x")}/images/{DateTime.Now.ToString("yyyy_MM")}/";
            if (RuntimeEnvironment.IsWindows)
            {
                uploadDirPath = $@"upload\{userId.ToString("x")}\images\{DateTime.Now.ToString("yyyy_MM")}\";
            }
            var diskFileId = SnowFlake_Net.GenerateSnowFlakeID();
            serverFileId=diskFileId;
            var httpFiles = _accessor.HttpContext.Request.Form.Files;
            //检查是否登录
            if (userId==0)
            {
                userId=GetUserIdBySession();
                if (userId==0)
                {
                    msg="NoLogin";
                    return false;
                }
            }

            if (httpFiles == null || httpFiles.Count < 1)
            {
                return false;
            }
            var httpFile = httpFiles[name];
            var fileEXT = Path.GetExtension(httpFile.FileName).Replace(".", "");
            if (!IsAllowImageExt(fileEXT))
            {
                msg= $"The_image_extension_{fileEXT}_is_blocked";
                return false;
            }
            var  fileName=diskFileId.ToString("x")+"."+fileEXT;
            //判断合法性
            if (httpFiles == null || httpFile.Length < 0)
            {
                return false;
            }
            //将文件保存在磁盘
            Task<bool> task = SaveUploadFileOnDiskAsync(httpFile, uploadDirPath, fileName);
            bool result = task.Result;
            if (result)
            {
                //将结果保存在数据库
                NoteFile noteFile=new NoteFile()
                {
                    FileId=diskFileId,
                    UserId=userId,
                    AlbumId=1,
                    Name =fileName,
                    Title=fileName,
                    Path= uploadDirPath+ fileName,
                    Size=httpFile.Length,
                    CreatedTime=DateTime.Now
                    //todo: 增加特性=图片管理
   
                };
                var AddResult=FileService.AddImage(noteFile,0,userId,true);
                if (!AddResult)
                {
                    msg="添加数据库失败";

                }
                return AddResult;
            }
            else
            {
                msg="磁盘保存失败";
                return false;
            }
        }
        //上传图片 png jpg 
        public bool UploadImage()
        {
            return false;
        }
        public void UploadVideo()
        {

        }
        public void UploadAudio()
        {

        }
        //检查上传图片后缀名
        private bool IsAllowImageExt(string ext)
        {
            HashSet<string> exts=new HashSet<string>() { "bmp","jpg","jpeg","png","tif","gif","pcx","tga","exif","fpx","svg","psd","cdr","pcd","dxf","ufo","eps","ai","raw","WMF","webp"};
            if (exts.Contains(ext.ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsAllowAttachExt(string ext)
        {
            //上传文件扩展名 白名单  后期会集中到一个类里面专门处理上传文件的问题
            HashSet<string> exts = new HashSet<string>() { "bmp","jpg","png","tif","gif","pcx","tga","exif","fpx","svg","psd","cdr","pcd","dxf","ufo","eps","ai","raw","WMF","webp",
                "zip","7z","rar"
                ,"mp4","mp3",
                "doc","docx","ppt","pptx","xls","xlsx"};
            if (exts.Contains(ext.ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task<bool> SaveUploadFileOnDiskAsync(IFormFile formFile, string uploadDirPath, string fileName)
        {
            if (formFile.Length > 0)
            {
                // string fileExt = Path.GetExtension(formFile.FileName); //文件扩展名，不含“.”
                // long fileSize = formFile.Length; //获得文件大小，以字节为单位

                if (!Directory.Exists(uploadDirPath))
                {
                    Directory.CreateDirectory(uploadDirPath);
                }
                var filePath = uploadDirPath + fileName;
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public enum FileTyte
        {
            /**
             * 文件分类
             * 视频 音频 图片 二进制 纯文本
             * */
            Video, Audio, Image, Binary, PlainText
        }



    }
}