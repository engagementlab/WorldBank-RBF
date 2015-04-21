﻿/* 
World Bank RBF
Created by Engagement Lab, 2015
==============
 Models.cs
 Game data models.

 Created by Johnny Richardson on 4/21/15.
==============
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game data models.
/// </summary>
public class Models {

	/// <summary>
	/// Stores all game data.
	/// </summary>
    public class GameData {

        public Character[] characters { get; set; }
        public City[] cities { get; set; }
		public Dictionary<string, NPC[]> phase_one { get; set; }
        public Dictionary<string, object> phase_two { get; set; }

    }

    [System.Serializable]
    public class Character {

        public string symbol { get; set; }
        public string display_name { get; set; }
        public string description { get; set; }

    }

    [System.Serializable]
    public class City {

        public string symbol { get; set; }
        public string display_name { get; set; }
        public string description { get; set; }

    }

    public class NPC {

        public string symbol { get; set; }
        public string character { get; set; }
		public Dictionary<string, Dictionary<string, string>> dialogue { get; set; }

    }

    public class Dialogue {

        public string symbol { get; set; }
        public string character { get; set; }
        public Dictionary<string, object> dialogue { get; set; }

    }

	public class PhaseOne {
		public Dictionary<string, object>[] npcs { get; set; }
	}

    public class Characters {
        public List<Dictionary<string, object>> dialogue = new List<Dictionary<string, object>>();
    }

}
