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

        private string server;
        private string database;
        private string uid;
        private string password;

        protected long mInvoiceID;
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
        public BusinessLogic(string ConnName)
        {
            mConString = ConnName;
            db = new DatabaseProviderFactory().Create(ConnName);
        }

        public bool getOpenMessageHeaders()
        {
            try
            {
                string BatchNo = string.Empty;
                string str = "select * from BroadcastMessagesList where statusID=1";
                string Email = string.Empty;
                string Mobile = string.Empty;
                DataSet dsEmailList = ReturnDs(str);
                DataSet MemberDetails = new DataSet();
                DataSet EmailDetails = new DataSet();
                if (dsEmailList != null)
                {
                    BatchNo = dsEmailList.Tables[0].Rows[0]["ID"].ToString();
                    LogScriptor.WriteErrorLog("Sending Batch: " + BatchNo);
                    foreach (DataRow rwmh in dsEmailList.Tables[0].Rows)
                    {

                        str = "select * from BroadcastListContacts where BroadcastListID = '" + BatchNo + "' and StatusID = 1";
                        DataSet dsMgsNum = ReturnDs(str);
                        if (dsMgsNum != null)
                        {
                            LogScriptor.WriteErrorLog("Beginning to send emails to : " + dsMgsNum.Tables[0].Rows.Count.ToString() + " email addresses");

                            foreach (DataRow row in dsMgsNum.Tables[0].Rows)
                            {

                               EmailDetails = 
                                MemberDetails = getContactDetails(int.Parse(BatchNo));

                                if (MemberDetails != null)
                                {
                                    DataRow rws = MemberDetails.Tables[0].Rows[0];
                                    Email = rws["Email"].ToString();
                                    //Mobile = rws["MobileNo"].ToString();


                                }


                                //DateTime dt = DateTime.Now;
                                //string formattedDate = dt.ToString("MMMM yyyy");



                                //string Message = $"Here are your Login Credentials as at {formattedDate}";

                                //if (Email != "" || !string.IsNullOrEmpty(Email))
                                //{
                                //    SendEmail(Email, "Login  Credentials", Message);
                                //}

                                //str = "update BroadcastListContacts set StatusID=2 WHERE ID = " + int.Parse(row["ID"].ToString()) + " AND BroadcastListID='" + BatchNo + "' ";
                                //db.ExecuteNonQuery(CommandType.Text, str);
                            }

                            str = "update BroadcastMessagesList set statusID=2 WHERE ID='" + BatchNo + "'";
                            db.ExecuteNonQuery(CommandType.Text, str);

                        }
                        else
                        {
                            str = "update BroadcastMessagesList set statusID=2 WHERE ID='" + BatchNo + "'";
                            db.ExecuteNonQuery(CommandType.Text, str);
                            return true;
                        }
                    }
                    return true;

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
                string str = "select Bc.ID,Bc.MemberID,pp.LastName,pp.FirstName,pp.Email from BroadcastListContacts Bc inner join RegistrationMembers pp on pp.Id = Bc.MemberID  where BroadcastListID = " + broadcastID + "";
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
        private void SendEmail(string recepientEmail, string subject, string MessageBody)
        {
            try
            {
                SmtpClient Client = new SmtpClient()
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential()
                    {
                        UserName = "training@zapf.co.zw",
                        Password = "Fuq97442 "
                    }
                };

                MailAddress FromEmail = new MailAddress("training@zapf.co.zw", "ZAPF");
                MailAddress ToEmail = new MailAddress("" + recepientEmail + "", "Member");
                MailMessage Message = new MailMessage()
                {
                    From = FromEmail,
                    Subject = subject
                };


                string body = string.Empty;
                // Set the HTML body of the email
                body = $@"<html>
                    <body>

                        <p>Dear Member,</p>
                        <p>{MessageBody}</p>
                        <p>Looking forward to your submissions.</p>
                        <p>Regards,</p>
                        <p>ZAPF </p>
                    </body>
                      <footer>
                        <footer>
                            <div class = ""row"">
                                <div class = ""column"">
                                    <img src = ""https://zapf.co.zw/assets/images/logo.png"">
                                </div>
                                <div class = ""column"">
                                    <p class = ""details"" >

                                        Zimbabwe Association of Pension Funds (ZAPF)<br> 
                                        3 Penn Place Close<br>
                                        Strathaven<br>
                                        Harare<br>
                                        +263 242 333341<br>
                                        +263 774 000 040 / 715 000 040 / 776 174 138<br>
                                </div>
            
                            </div>
        
                        </footer>
                        </footer>
                </html>";

                // Create an alternate view with the HTML body
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                // Load the image from a file in the folder
                //string imagePath = @"C:\Systems\EmailingService\EconetLogo.png";
                string serviceDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Define the path to your image file within the "img" folder
                string imagePath = Path.Combine(serviceDirectory, "img", "EconetLogo.png");
                //string imagePath = Server.MapPath("~/img/EconetLogo.png");

                //string imagePath = "@..\Reports\AciveMembers.rpt";
                LinkedResource imageResource = new LinkedResource(imagePath);
                imageResource.ContentId = "imageId";


                // Add the image to the alternate view
                htmlView.LinkedResources.Add(imageResource);
                //System.Net.Mail.Attachment attachment;
                //attachment = new System.Net.Mail.Attachment(Server.MapPath(@"../Communications/Templates/wadii.pdf"));
                //Message.Attachments.Add(attachment);

                // Add the alternate view to the email message
                Message.AlternateViews.Add(htmlView);

                Message.To.Add(ToEmail);
                Client.Send(Message);
                //SuccessAlert("Message sent");

                //BroadcastMessagesList bc = new BroadcastMessagesList("cn", 1);
                //if (bc.UpdateEmailListStatus(int.Parse(txtID.Value), PensionNo))
                //{

                //}
            }
            catch (Exception ex)
            {
                throw;
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


        public void CheckAuditRecords()
        {
            try
            {
                LogScriptor.WriteErrorLog("Checking PayPensioner Table");

                string strSchools = "Select * from PayPensioner where DateUploaded>'2018-08-31' order by DateUploaded desc";
                DataSet ds = db.ExecuteDataSet(CommandType.Text, strSchools);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {

                    LogScriptor.WriteErrorLog("Audit data found:" + DateTime.Today);
                    foreach (DataRow rd in ds.Tables[0].Rows)
                    {

                        LogScriptor.WriteErrorLog(rd[0].ToString() + rd[1].ToString() + rd[2].ToString() + rd[3].ToString() + rd[4].ToString() + rd[5].ToString() + rd[6].ToString() + rd[7].ToString() + rd[8].ToString() + rd[9].ToString() + rd[10].ToString() + DateTime.Today);

                    }

                    LogScriptor.WriteErrorLog(ds.Tables[0].Rows.Count.ToString() + "records found");

                }
                else
                {
                    LogScriptor.WriteErrorLog("No records found: " + DateTime.Today);
                }
            }
            catch (Exception ex)
            {
                mMsgflg = ex.Message;
                LogScriptor.WriteErrorLog(mMsgflg);
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
