using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace UtilityTool
{
    #region 郵件
    public class MailHelper
    {
        public bool isBodyHtml { get; set; } = true;

        class MailServerInfo
        {
            public static readonly string MailServerIp = "twmail08.coretronic.com";
            public static readonly int MailServerPort = 25;
        }

        public void SendMail(string From, string To, string subject, string mailContext)
        {
            SendMail(From, subject, mailContext, new List<string>() { To });
        }
        public void SendMail(string From, string To, string subject, string mailContext, Attachment MailAttachment)
        {
            SendMail(From, subject, mailContext, new string[] { To }, MailAttachment: new Attachment[] { MailAttachment });
        }
        public void SendMail(string From, string To, string subject, string mailContext, IEnumerable<Attachment> MailAttachment)
        {
            SendMail(From, subject, mailContext, new string[] { To }, MailAttachment: MailAttachment);
        }
        public void SendMail(string From, IEnumerable<string> To, string subject, string mailContext)
        {
            SendMail(From, subject, mailContext, To);
        }
        public void SendMail(string From, IEnumerable<string> To, string subject, string mailContext, Attachment MailAttachment)
        {
            SendMail(From, subject, mailContext, To, MailAttachment: new Attachment[] { MailAttachment });
        }
        public void SendMail(string From, IEnumerable<string> To, string subject, string mailContext, IEnumerable<Attachment> MailAttachment)
        {
            SendMail(From, subject, mailContext, To, MailAttachment: MailAttachment);
        }

        /// <summary>
        /// 寄送電子郵件
        /// </summary>
        /// <param name="From">寄件者</param>
        /// <param name="To">收件者</param>
        /// <param name="subject">主旨</param>
        /// <param name="mailContext">內文</param>
        /// <param name="MailAttachment">附件</param>
        /// <param name="MailBackupPath">mail檔案備份位址</param>
        public void SendMail(string From,
            string subject,
            string mailContext,
            IEnumerable<string> To = null,
            IEnumerable<string> Cc = null,
            IEnumerable<string> Bcc = null,
            IEnumerable<Attachment> MailAttachment = null,
            string MailBackupPath = null)
        {
            using (var smtp = new SmtpClient(MailServerInfo.MailServerIp, MailServerInfo.MailServerPort))
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(From);
                if (To != null)
                {
                    foreach (var address in To)
                    {
                        if (string.IsNullOrWhiteSpace(address)) continue;
                        if (!IsValidEmail(address))
                        {
                            throw new Exception("To:" + address + ",maill格式錯誤");
                        }
                        mail.To.Add(address.Trim());//設定收件者Email
                    }
                }
                if (Cc != null)
                {
                    foreach (var address in Cc)
                    {
                        if (string.IsNullOrWhiteSpace(address)) continue;
                        if (!IsValidEmail(address))
                        {
                            throw new Exception("Cc:" + address + ",maill格式錯誤");
                        }
                        mail.CC.Add(address.Trim());//設定副本收件者Email
                    }
                }
                if (Bcc != null)
                {
                    foreach (var address in Bcc)
                    {
                        if (string.IsNullOrWhiteSpace(address)) continue;
                        if (!IsValidEmail(address))
                        {
                            throw new Exception("Bcc:" + address + ",maill格式錯誤");
                        }
                        mail.Bcc.Add(address.Trim());//設定密件副本收件者Email
                    }
                }
                if (!mail.To.Any() && !mail.CC.Any() && !mail.Bcc.Any()) throw new Exception("To、Cc及Bcc，三參數中須有一參數存在收件人");
                mail.Subject = subject;//信件標題
                mail.Body = mailContext; //設定信件內容
                mail.IsBodyHtml = isBodyHtml; //是否使用html格式
                mail.BodyEncoding = Encoding.UTF8;

                if (MailAttachment != null)
                {
                    foreach (var item in MailAttachment)
                    {
                        mail.Attachments.Add(item);
                    }
                }

                if (!string.IsNullOrWhiteSpace(MailBackupPath))
                {
                    if (!Directory.Exists(MailBackupPath))
                    {
                        Directory.CreateDirectory(MailBackupPath);
                    }
                    smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtp.PickupDirectoryLocation = MailBackupPath;
                    smtp.Send(mail);
                }

                smtp.UseDefaultCredentials = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(mail);
            }
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w-]+\.)*?[\w-]+@[\w-]+\.([\w-]+\.)*?[\w]+$");
        }
    }
    #endregion
}