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
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using ZborMob.Views;

namespace ZborMob.Helpers
{
    [Preserve(AllMembers = true)]
    public class MessageDataTemplateSelector : Xamarin.Forms.DataTemplateSelector
    {
        public DataTemplate IncomingTextTemplate { get; set; }
        public DataTemplate OutgoingTextTemplate { get; set; }
        public DataTemplate IncomingImageTemplate { get; set; }


        public MessageDataTemplateSelector()
        {
            IncomingTextTemplate = new DataTemplate(typeof(IncomingTextTemplate));
            OutgoingTextTemplate = new DataTemplate(typeof(OutgoingTextTemplate));
            IncomingImageTemplate = new DataTemplate(typeof(IncomingImageMessageTemplate));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (((MessageInfo)item).TemplateType == TemplateType.OutGoingText)
                return OutgoingTextTemplate;
            else if (((MessageInfo)item).TemplateType == TemplateType.IncomingText)
                return IncomingTextTemplate;
            else
                return IncomingImageTemplate;
        }

    }


    #region CustomContentView
    [Preserve(AllMembers = true)]
    public class IncomingCustomContentView : ContentView
    {

    }
    [Preserve(AllMembers = true)]
    public class OutgoingCustomContentView : ContentView
    {

    }

    #endregion
}
