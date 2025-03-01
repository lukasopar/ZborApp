#region Copyright Syncfusion Inc. 2001-2020.
// Copyright Syncfusion Inc. 2001-2020. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace ZborMob.Views
{
    [Preserve(AllMembers = true)]
    public partial class OutgoingTextTemplate : ViewCell
    {
        #region OutgoingMessageTemplate
        public OutgoingTextTemplate()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
                this.gridLayout.ColumnSpacing = 0;
            else
                this.frame.BackgroundColor = Color.FromRgb(229, 245, 251);
        }

        #endregion
    }
}