﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Software_Requirement_Specification.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Software_Requirement_Specification.Areas.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }
       

        [HttpGet]
        public  async Task<string> TaoMaXacThuc(string _to,string sdt)
        {
            var email = await _context.TaiKhoan.Where(t=>t.Email==_to&&t.PhoneNumber==sdt).ToListAsync();
            if (email.Count > 0)
            {
                string UpperCase = "QWERTYUIOPASDFGHJKLZXCVBNM";
                string LowerCase = "qwertyuiopasdfghjklzxcvbnm";
                string Digits = "1234567890";
                string allCharacters = UpperCase + LowerCase + Digits;
                Random r = new Random();
                String password = "";
                for (int i = 0; i < 6; i++)
                {
                    double rand = r.NextDouble();
                    if (i == 0)
                    {
                        password += UpperCase.ToCharArray()[(int)Math.Floor(rand * UpperCase.Length)];
                    }
                    else
                    {
                        password += allCharacters.ToCharArray()[(int)Math.Floor(rand * allCharacters.Length)];
                    }
                }
                string token = password;
                string _from = "baophuc3008@gmail.com";
                string _subject = "Xác thực tài khoản E_Library";
                string _body = token;
                string _gmail = "baophuc3008@gmail.com";
                string _password = "Phuc01698669234";
                MailMessage message = new MailMessage(_from, _to, _subject, _body);
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.ReplyToList.Add(new MailAddress(_from));
                message.Sender = new MailAddress(_from);

                using var smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_gmail, _password);

                try
                {
                    await smtpClient.SendMailAsync(message);
                    //HttpContext.Session.SetString("OTP", _body);
                    //HttpContext.Session.SetString("Gmail", _to);
                    //HttpContext.Session.SetString("Sdt", sdt);
                    return "Da gui mail xac thuc";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return "Gui that bai";
            }
            return "Email va so dien thoai khong trung khop";
        
        }

 

        [HttpGet]
        [Route("/dangxuat")]
        public async Task<string> DangXuat()
        {
            HttpContext.Session.Clear();
            return "Dang xuat thanh cong";  //chuyen sang login
        }

        [HttpGet]
        public async Task<string> DoiMatKhau(string pass, string rePass)
        {
            if(HttpContext.Session.GetInt32("XacThuc")!=1)
            {
                return "Chua xac thuc"; //chuyen sang xac thuc
            }
            if(pass==rePass)
            {
                var tk = await _context.TaiKhoan.Where(t => t.Email == HttpContext.Session.GetString("Gmail")).FirstOrDefaultAsync();
                if(tk!=null)
                {
                    tk.PasswordHash = pass;
                }else
                {
                    return "Het phien";
                }

                var a = await _context.TaiKhoan.Where(n => n.Email == HttpContext.Session.GetString("Gmail")).ToListAsync();
                try
                {
                    _context.TaiKhoan.Update(tk);
                   await _context.SaveChangesAsync();
                    HttpContext.Session.Remove("XacThuc");
                    var tb = new ThongBao();
                    tb.LoaiThongBao = true;
                    tb.TaiKhoanId = "";
                    tb.NoiDung = tk.UserName + " đã thay đổi mật khẩu";
                    tb.ChuDe = "Thay đổi mật khẩu";
                    tb.ThoiGian = DateTime.Now;
                    _context.Add(tb);
                    await _context.SaveChangesAsync();

                    return "Da luu mat khau moi";
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }
            return "Mat khau khong trung khop";
        }

        [HttpGet]
        public async Task<string> XacThucOTP(string otp)
        {
            if (HttpContext.Session.GetString("OTP") == null)
            {
                return "Dang nhap lai";
            }else
            if (otp==HttpContext.Session.GetString("OTP"))
            {
                HttpContext.Session.SetInt32("XacThuc", 1);
                HttpContext.Session.Remove("OTP");
                return "Xac thuc thanh cong"; //chuyen sang doi mat khau
            }
     
            string a= await TaoMaXacThuc(HttpContext.Session.GetString("Gmail"),HttpContext.Session.GetString("Sdt"));
            
            return "Da gui lai ma xac thuc";
        }

    }
}
