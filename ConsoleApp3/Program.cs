using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using System.Threading;

namespace ConsoleApplication1
{
    class Run
    {
        static List<Thread> transactionThreads = new List<Thread>();

        public class ConnectionHolder : IDisposable
        {
            public void Dispose()
            {
            }

            public void executeLongTransaction()
            {
                Console.WriteLine("Starting a long running transaction.");
                using (SqlConnection _con = new SqlConnection("Server=.;Database=Test_SocialCee_Trunk;Trusted_Connection=True;Max Pool Size=200;MultipleActiveResultSets=True;Connect Timeout=30;Application Name=ConsoleApplication1.vshost"))
                {
                    try
                    {
                        SqlTransaction trans = null;
                        _con.Open();
                        trans = _con.BeginTransaction();

                        SqlCommand cmd = new SqlCommand("update Actor set Country = 'XXX' where ID = @0; waitfor delay '00:00:05'", _con, trans);
                        cmd.Parameters.Add(new SqlParameter("0", 9999));
                        cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();

                        Console.WriteLine("Finished the long running transaction.");
                    }
                    catch (ThreadAbortException tae)
                    {
                        Console.WriteLine("Thread - caught ThreadAbortException in executeLongTransaction - resetting.");
                        Console.WriteLine("Exception message: {0}", tae.Message);
                    }
                }
            }
        }

        static void killTransactionThread()
        {
            Thread.Sleep(2 * 1000);

            // We're not doing this anywhere in our real code.  This is for simulation
            // purposes only!
            transactionThreads.ForEach(s => s.Abort());

            Console.WriteLine("Killing the transaction thread...");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var connectionHolder = new ConnectionHolder())
            {
                for (var i = 0; i<100; i++)
                {
                    var thread = new Thread(connectionHolder.executeLongTransaction);
                    thread.Start();
                    transactionThreads.Add(thread);

                }
                new Thread(killTransactionThread).Start();

                transactionThreads.ForEach(s => s.Join());

                Console.WriteLine("The transaction thread has died.  Please run 'select * from sysprocesses where open_tran > 0' now while this window remains open. \n\n");

                Console.Read();
            }
        }
    }
}