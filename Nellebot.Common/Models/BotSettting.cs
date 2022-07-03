﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nellebot.Common.Models
{
    public class BotSettting
    {
        [Key]
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
