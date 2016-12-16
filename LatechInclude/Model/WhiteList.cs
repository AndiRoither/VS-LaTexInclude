﻿
using System;
using System.ComponentModel;

namespace LatechInclude.HelperClasses
{
    public class WhiteList : INotifyPropertyChanged, IComparable<WhiteList>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string Obj)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(Obj));
            }
        }

        public int CompareTo(WhiteList that)
        {           
            return this.Extension.CompareTo(that.Extension);
        }

        public WhiteList()
        {
        }

        public WhiteList(string Language, string Extension)
        {
            this.Language = Language;
            this.Extension = Extension;
        }

        public string Language { get; set; }

        public string Extension { get; set; }

       
    }
}
