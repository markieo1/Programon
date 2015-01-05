﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgramonEngine
{
    public class Item
    {
        public string name;
        public Sprite sprite;
        public Stats statChange;
        public Buff buffOnUse;
        public Item()
        { 
        this.name="";
        this.sprite=new Sprite();
        this.statChange=new Stats();
        this.buffOnUse=new Buff();
        }
        public Item(string name, Sprite sprite)
        {
            this.name = name;
            this.sprite =sprite;
            this.statChange = new Stats();
            this.buffOnUse = new Buff();
        }
        public Item(string name, Sprite sprite, Stats statChange, Buff buffOnUse)
        {
            this.name = name;
            this.sprite = sprite;
            this.statChange = statChange;
            this.buffOnUse = buffOnUse;
        }
    }
}