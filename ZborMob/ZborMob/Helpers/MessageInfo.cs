#region Copyright Syncfusion Inc. 2001-2020.
// Copyright Syncfusion Inc. 2001-2020. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ZborMob.Helpers
{
    public enum TemplateType
    {
        IncomingText,
        OutGoingText,
    }
    [Preserve(AllMembers = true)]
    public class MessageInfo : INotifyPropertyChanged
    {

        private string text;
        private string profileImage;
        private String dateTime;
        private string username;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        public string ProfileImage
        {
            get { return profileImage; }
            set
            {
                profileImage = value;
                OnPropertyChanged("ProfileImage");
            }
        }

        public String DateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged("Username");
            }
        }

        public TemplateType TemplateType
        {
            get;
            set;
        }


        public MessageInfo()
        {

        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string Property = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        }

    }
}
