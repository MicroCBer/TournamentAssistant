﻿using System;

namespace TournamentAssistantShared.Models
{
    [Serializable]
    public class Coordinator : User
    {
        // -- The chips on the match coordinator view require this for the purpose of the little chip icon
        public string GetIcon
        {
            get
            {
                return Name.Substring(0, 1);
            }
        }

        public string UserId { get; set; } = null;
    }
}
