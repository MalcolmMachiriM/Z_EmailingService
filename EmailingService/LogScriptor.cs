using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailingService
{
    public class LogScriptor
    {

        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {

                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\EmailingService.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ":" + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch
            {

            }

        }

        public static void WriteErrorLog(String Message)
        {
            StreamWriter sw = null;
            try
            {

                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\EmailingService.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ":" + Message);

                sw.Flush();
                sw.Close();

            }
            catch
            {

            }

        }
    }
}
