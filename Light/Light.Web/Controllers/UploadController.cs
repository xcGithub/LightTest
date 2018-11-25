using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using xxoo.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Light.Web.Controllers
{
    public class UploadController : Controller
    {

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        private IHostingEnvironment hostingEnv;
        public UploadController(IHostingEnvironment env)
        {
            this.hostingEnv = env;
        }
        public IActionResult Uploadify()
        {
            Response.ContentType = "text/plain";
            //context.Response.Charset = "utf-8";

            //Response.ContentEncoding = Encoding.GetEncoding("UTF-8");

            IFormFile file = Request.Form.Files["Filedata"];
            var filename = ContentDispositionHeaderValue
                           .Parse(file.ContentDisposition)
                           .FileName
                         .Trim('"');
            filename = hostingEnv.WebRootPath + $@"\folder\{filename}";
            long size = file.Length;

            if (file == null)
            {
                //Response.Write("0");
                return Content("0");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filename));

            // 保存文件
            using (FileStream fs = System.IO.File.Create(filename))
            {
               file.CopyTo(fs);
               fs.Flush();
            }

            //Response.WriteAsync("1");

            return Content("0");
        }


        public IActionResult AjaxupIndex()
        {

            return View();
        }
        public IActionResult AjaxFileupload()
        {
            IFormFile file = Request.Form.Files["file"];
            string msg = null;
            string[] contentType = { "image/jpeg", "image/png", "image/bmp", "image/gif" };
            // 过滤文件类型
            if (!contentType.Contains(file.ContentType) || (file == null))
            {
                // string[] fileType = 
                msg = "文件格式不正确（必须为.jpg/.gif/.bmp/.png文件）";


                var o = new { state = 0, msg = msg };
                string s = JsonHelper.DateSerializeObject(o);
                return Content(s);
            }

            /*   
             */
            string filePath = hostingEnv.WebRootPath + @"\Content\thumbnail\" + DateTime.Now.ToString("yyyyMMddHHssmm") + Guid.NewGuid().ToString() + ".jpg";

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            long size = file.Length;
            // 保存文件
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            filePath = Request.PathBase + filePath;
            msg = " 成功! 文件大小为:" + size;

            var resobj = new { state = 1, msg = msg, imgurl = filePath };
            string res = JsonHelper.DateSerializeObject(resobj);
            return Content(res);
             
        }
         


    }
}
