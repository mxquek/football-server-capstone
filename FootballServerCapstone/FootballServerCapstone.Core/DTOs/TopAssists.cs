﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballServerCapstone.Core.DTOs
{
    public class TopAssists
    {
        public int SeasonId { get; set; }
        public string ClubName { get; set; }
        public string PlayerName { get; set; }
        public int TotalAssists { get; set; }
    }
}
