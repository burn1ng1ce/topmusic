using System;
using System.Collections.Generic;
using System.Text;
using DotRas;

namespace TopMusic
{
    public class RasReconnectRemoteCommand : RemoteCommand
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RasReconnectRemoteCommand));
        private const int MAX_RETRIES = 3;
        private string rasEntryName;
        private RasDialer dialer = null;
        private State state;
        private long runTime;

        public RasReconnectRemoteCommand(string rasEntryName)
        {
            this.rasEntryName = rasEntryName;
        }

        public string Type 
        {
            get { return "reconnect"; }
        }

        public string Scope
        {
            get { return ""; }
        }

        public string RasEntryName
        {
            get { return this.rasEntryName; }
            set { this.rasEntryName = value; }
        }

        public State State
        {
            get { return this.state; }
        }

        public long RunTime
        {
            get { return this.runTime; }
        }

        public void Run()
        {
            if (rasEntryName == null || rasEntryName.Length == 0)
            {
                this.state = State.FAILED;
                return;
            }

            this.state = State.RUNNING;
            this.runTime = DateTime.Now.Ticks;
            string activePhoneBookPath = null;
            RasConnection connection = this.GetActiveConnection();
            if (connection != null) 
            {
                // reconnect
                activePhoneBookPath = connection.PhoneBookPath;
                logger.Debug("hangup connection " + this.rasEntryName + "...");
                
                connection.HangUp();

                if (this.dialer == null)
                {
                    dialer = new RasDialer();
                    dialer.EntryName = this.rasEntryName;
                    dialer.AllowUseStoredCredentials = true;

                    dialer.PhoneBookPath = activePhoneBookPath;
                    dialer.Timeout = 30000;
                    /*
                    dialer.StateChanged += dialer_StateChanged;
                    dialer.DialCompleted += dialer_DialCompleted;
                    dialer.Error += dialer_Error;
                     */
                }

                // dial
                bool dialSuccess = false;
                for (int retries = 0; retries < MAX_RETRIES && !dialSuccess ; ++retries)
                {
                    try
                    {
                        if (!dialer.IsBusy)
                        {
                            logger.Debug("connecting " + this.rasEntryName + "...");
                            dialer.Dial();
                            logger.Debug(this.rasEntryName + " connected.");
                            dialSuccess = true;
                        }
                    }
                    catch (RasException ex)
                    {
                        logger.Error("RasException raised to dial " + this.rasEntryName + ": " + ex.ErrorCode + "(" + ex.Message + ")", ex);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("failed to dial " + this.rasEntryName + ": " + ex.Message, ex);
                    }
                }
            }

            this.state = State.COMPLETED;
        }
        /*
        void dialer_Error(object sender, System.IO.ErrorEventArgs e)
        {
            logger.Error("on dial error: " + (e.GetException() == null ? null : e.GetException().Message), e.GetException());
        }

        void dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            logger.Debug("on dial completed, connected: " + e.Connected + ", cancelled: " + e.Cancelled + "error: " + (e.Error == null ? null : e.Error.GetType() + ": " + e.Error.Message));
        }

        void dialer_StateChanged(object sender, StateChangedEventArgs e)
        {
            logger.Debug("on dial state changed, state: " + e.State + ", errorCdoe: " + e.ErrorCode + ", errorMessage: " + e.ErrorMessage);
        }
        */
        public string ToJson()
        {
            return "{\"type\":" + this.Type + ", \"name\":\"" + this.rasEntryName + "\"}";
        }

        private RasConnection GetActiveConnection()
        {
            foreach (RasConnection connection in RasConnection.GetActiveConnections())
            {
                if (connection.EntryName == rasEntryName)
                {
                    return connection;
                }
            }

            return null;
        }
    }
}
