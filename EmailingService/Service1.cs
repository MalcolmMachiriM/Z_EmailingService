using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EmailingService
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer1 = (Timer)null;
        public Service1()
        {
            InitializeComponent();
        }

       
        protected override void OnStart(string[] args)
        {
            // Here are are going to call an event tied to the process of saving a new invoice 
            this.timer1 = new Timer();
            this.timer1.Interval = 60000.0; // 60000=5 minutes 120 minutes = 7200000.0  

            this.timer1.Elapsed += new ElapsedEventHandler(this.timer1_tick);
            this.timer1.Enabled = true;
            LogScriptor.WriteErrorLog("Comarsoft Communications App Service has started");
        }

        private void timer1_tick(object Sender, ElapsedEventArgs e)
        {
            try
            {
                BusinessLogic obj = new BusinessLogic("DBMS");

                LogScriptor.WriteErrorLog("Sending Pending SMS batch");
                obj.getOpenMessageHeaders();
                LogScriptor.WriteErrorLog("Completed Sending Session");


            }
            catch (Exception ex)
            {
                LogScriptor.WriteErrorLog("Error reported @ getting new open batch header: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            this.timer1.Enabled = false;
            LogScriptor.WriteErrorLog("Comarsoft comms service stopped");
        }
       
    }
}
