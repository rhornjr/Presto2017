﻿using System;

namespace PrestoCommon.EntityHelperClasses
{
    public class InstallationSummaryDto
    {
        public string Id { get; set; }
        public string ServerName { get; set; }
        public string ApplicationName { get; set; }
        public DateTime InstallationStart { get; set; }
        public DateTime InstallationEnd { get; set; }
        public string Result { get; set; }
    }
}