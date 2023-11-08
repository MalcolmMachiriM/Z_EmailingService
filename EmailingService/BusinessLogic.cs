using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Net.Mail;
using System.IO;
using System.Web;
using System.Runtime.Remoting.Messaging;

namespace EmailingService
{
    public enum SyncLookUp : long
    {
        AwaitingSync = 1,
        InvoiceHeaderSyncd = 2,
        InvoiceDetailLinesSyncd = 3


    }
    public class BusinessLogic
    {

        #region vars

        private string server;
        private string database;
        private string uid;
        private string password;
        string BatchNo = string.Empty;
        protected long mInvoiceID;

        #endregion

        #region props
        public long InvoiceID
        {
            get { return mInvoiceID; }
            set { mInvoiceID = value; }
        }
        protected string mMsgflg = "";
        public string Msgflg
        {
            get { return mMsgflg; }
            set { mMsgflg = value; }
        }
        protected Database db;
        protected Database Appdb;
        protected string mConString;

        public string Constring
        {
            get { return mConString; }

        }
        #endregion
        public BusinessLogic(string ConnName)
        {
            mConString = ConnName;
            db = new DatabaseProviderFactory().Create(ConnName);
        }

        public bool getOpenMessageHeaders()
        {
            try
            {
                
                string str = "select * from BroadcastMessagesList where statusID=2";
                string Email = string.Empty;
                string Mobile = string.Empty;
                string EmailBody = string.Empty;
                string EmailHeader = string.Empty;
                DataSet dsEmailList = ReturnDs(str);
                DataSet MemberDetails = new DataSet();
                DataSet EmailDetails = new DataSet();
                if (dsEmailList != null)
                {
                    BatchNo = dsEmailList.Tables[0].Rows[0]["ID"].ToString();
                    LogScriptor.WriteErrorLog("Sending Batch: " + BatchNo);
                    
                    str = "select * from BroadcastListContacts where BroadcastListID = '" + BatchNo + "' and StatusID = 2";
                    DataSet dsMgsNum = ReturnDs(str);
                    if (dsMgsNum != null)
                    {
                        LogScriptor.WriteErrorLog("Beginning to send emails to : " + dsMgsNum.Tables[0].Rows.Count.ToString() + " email addresses");


                        EmailDetails = getMessageDetails(int.Parse(BatchNo));
                                
                        MemberDetails = getContactDetails(int.Parse(BatchNo));

                        if (MemberDetails != null)
                        {
                            foreach (DataRow item in MemberDetails.Tables[0].Rows)
                            {
                                DataRow rws = MemberDetails.Tables[0].Rows[0];
                                Email = item["email"].ToString();

                                DataRow email = EmailDetails.Tables[0].Rows[0];
                                EmailBody = email["Message"].ToString();
                                EmailHeader = email["BroadcastMessgeTitle"].ToString();

                                SendHtmlFormattedEmail(Email, EmailHeader, EmailBody, EmailBody, int.Parse(rws["MemberID"].ToString()));
                                    
                            }
                            str = "update BroadcastMessagesList set statusID=3 WHERE ID='" + BatchNo + "'";
                            db.ExecuteNonQuery(CommandType.Text, str);
                            return true;


                        }
                        return true;

                    }
                    else
                    {
                        str = "update BroadcastMessagesList set statusID=3 WHERE ID='" + BatchNo + "'";
                        db.ExecuteNonQuery(CommandType.Text, str);
                        return true;
                    }
                    

                }
                else
                {
                    LogScriptor.WriteErrorLog("There are no open message headers: ");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogScriptor.WriteErrorLog("An error occured while getting message headers: " + ex.Message);
                return false;
            }
        }

        private void SendHtmlFormattedEmail(string recepientEmail, string subject, string body, string MessageBody, int MemberId)
        {
            string str = string.Empty;

            try
            {

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                var AccessLink = " https://www.comartononline.com/AGMSystem/Registration/PortalRegistration";

                SmtpClient Client = new SmtpClient()
                {
                    Credentials = new NetworkCredential("training@zapf.co.zw", "Fuq97442"),
                    Port = 587,
                    Host = "smtp.office365.com",
                    EnableSsl = true,
                };



                MailMessage Message = new MailMessage();
                Message.From = new MailAddress("training@zapf.co.zw", "ZAPF");
                Message.To.Add(recepientEmail);
                Message.Subject = subject;
                Message.IsBodyHtml = true;



                string filepath = "Select FilePath from EmailAttachments where BroadcastMessagesListID=" + BatchNo + ";";
                DataSet ds = ReturnDs(filepath);
                if (ds != null)
                {
                    foreach (DataRow item in ds.Tables[0].Rows)
                    {
                        string filePath = item["FilePath"].ToString();
                        Message.Attachments.Add(new Attachment(filePath));
                    }
                }


                //// Set the HTML body of the email
                DataSet EmailDets = getMessageDetails(int.Parse(BatchNo));
                DataRow dr  = EmailDets.Tables[0].Rows[0];
                string template = "001_ZAPF_General_Template.html";
                body = (dr["Format"].ToString() == "1")? PopulateBody(MemberId, template, MessageBody) :PopulateBody(MemberId, MessageBody);
                

                // Create an alternate view with the HTML body
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                Message.AlternateViews.Add(htmlView);
                try
                {

                    Client.Send(Message);


                    str = "update BroadcastMessagesList set statusID=3 WHERE ID='" + BatchNo + "'";
                    db.ExecuteNonQuery(CommandType.Text, str);
                    str = "update BroadcastListContacts set StatusID=3 WHERE MemberID = " + MemberId + " AND BroadcastListID='" + BatchNo + "' ";
                    db.ExecuteNonQuery(CommandType.Text, str);

                }
                catch (Exception e)
                {

                    LogScriptor.WriteErrorLog("Error reported @ SendHtmlFormattedEmail: " + e.Message); ;
                }

            }
            catch (Exception ex)
            {
                LogScriptor.WriteErrorLog("Error reported @ SendHtmlFormattedEmail: " + ex.Message); ;
                str = "update BroadcastMessagesList set statusID=3 WHERE ID='" + BatchNo + "'";
                db.ExecuteNonQuery(CommandType.Text, str);
            }
        }

        private string PopulateBody(int MemberID, string Template, string MessageBody)
        {
            string memberName = string.Empty;
            DataSet reg = ReturnDs("SELECT * FROM RegistrationMembers WHERE Id = " + MemberID + "");
            DataRow dets = reg.Tables[0].Rows[0];
            if (reg != null)
            {
                memberName = dets["FirstName"] + " " + dets["LastName"];
            }
            string link = "C:/comarsoft/AGMSystem/AGMSystem/communication/Templates/001_ZAPF_General_Template.html";
            string body = string.Empty;
            LogScriptor.WriteErrorLog(link);
            if (File.Exists(link))
            {
                body = File.ReadAllText(link);
            }
            else
            {
                LogScriptor.WriteErrorLog("template not found");

            }
            body = body.Replace("{BODY}", MessageBody);
            body = body.Replace("{MEMBER}", memberName);
            return body;


        }
        private string PopulateBody(int MemberID, string MessageBody)
        {
            string memberName = string.Empty;
            DataSet reg = ReturnDs("SELECT * FROM RegistrationMembers WHERE Id = " + MemberID+"");
            DataRow dets = reg.Tables[0].Rows[0];
            if (reg!=null)
            {
                memberName = dets["FirstName"] + " " + dets["LastName"];
            }

            DataSet ds = ReturnDs("Select * from BroadcastMessagesList where id = "+BatchNo+"");
            DataRow dr = ds.Tables[0].Rows[0];

            string body = string.Empty;
            string link = "C:/Comarsoft/AGMSystem/AGMSystem/communication/Templates/" + dr["Template"];
            LogScriptor.WriteErrorLog(link);
            if (File.Exists(link))
            {
                body = File.ReadAllText (link);
            }
            else
            {
                LogScriptor.WriteErrorLog("template not found");
                return null;
            }

            body = body.Replace("{MEMBER}", memberName);
            return body;


        }

        //protected void SendSMSAlert(string UserID, string Password, string SenderID, string MobileNo, string Message)
        //{

        //    try
        //    {

        //        WebRequest req = WebRequest.Create("http://etext.co.zw/sendsms.php?user=263772486127&password=cive15Um&senderid=" + SenderID + "&mobile=" + MobileNo + "&message=" + Message.Replace(" ", "+").ToString() + "");
        //        req.GetResponse();
        //        req.Timeout = Timeout.Infinite;
        //        WebResponse resp = req.GetResponse();
        //        resp.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogScriptor.WriteErrorLog("An error occured while sending a message: " + ex.Message);
        //    }
        //}
        public DataSet getContactDetails(int broadcastID)
        {
            try
            {
                string str = "select Bc.ID,Bc.MemberID,pp.LastName,pp.FirstName,pp.Email from BroadcastListContacts Bc inner join RegistrationMembers pp on pp.Id = Bc.MemberID  where BroadcastListID = " + broadcastID + " and bc.StatusID=2";
                DataSet ds = db.ExecuteDataSet(CommandType.Text, str);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }
        public DataSet getMessageDetails(int broadcastID)
        {
            try
            {
                string str = "select * from BroadcastMessagesList where ID = " + broadcastID + "";
                DataSet ds = db.ExecuteDataSet(CommandType.Text, str);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }
        protected DataSet ReturnDs(string str)
        {
            try
            {
                DataSet ds = new DataSet();
                ds = db.ExecuteDataSet(CommandType.Text, str);
                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogScriptor.WriteErrorLog("An error occured while getting message headers: " + ex.Message);
                return null;
            }
        }


        public DataSet Getdatalu(string str)
        {
            try
            {
                DataSet ds = db.ExecuteDataSet(CommandType.Text, str);
                return ds;
            }
            catch (Exception ex)
            {
                LogScriptor.WriteErrorLog(ex.Message);
                return null;
            }
        }


    }
}
