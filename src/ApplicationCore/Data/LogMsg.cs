﻿namespace ApplicationCore.Data
{
    public class LogMsg
    {
        public string message { get; set; }
        public string message_template { get; set; }
        public string level { get; set; }
        public DateTime raise_date { get; set; }
        public string exception { get; set; }
        public string properties { get; set; }        
        public string machine_name { get; set; }
        public string user_name { get; set; }
    }
}
