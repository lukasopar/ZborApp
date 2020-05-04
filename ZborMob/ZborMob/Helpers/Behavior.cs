#region Copyright Syncfusion Inc. 2001-2020.
// Copyright Syncfusion Inc. 2001-2020. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.ListView.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using ZborMob.ViewModels;

namespace ZborMob.Helpers
{
    [Preserve(AllMembers = true)]


    public class DataTemplateSelectorBehavior : Behavior<Syncfusion.ListView.XForms.SfListView>
    {


        private RazgovorViewModel DataTemplateSelectorViewModel;
        private Syncfusion.ListView.XForms.SfListView ListView;
        private ScrollView scrollView;



        protected override void OnAttachedTo(Syncfusion.ListView.XForms.SfListView bindable)
        {
            ListView = bindable;
            base.OnAttachedTo(bindable);
            DataTemplateSelectorViewModel = (RazgovorViewModel)ListView.BindingContext;
            ListView.PropertyChanged += ListView_Loaded;
        }

        private void ListView_Loaded(object sender, EventArgs e)
        {
            var l = ListView.BindingContext as RazgovorViewModel;

            if (l==null || l.MessageInfo == null)
                return;
            scrollView = ListView.Parent as ScrollView;
            ListView.HeightRequest = scrollView.Height;
           
            if (Device.RuntimePlatform == Device.macOS)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ListView.ScrollTo(2500);
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    (ListView.LayoutManager as LinearLayout).ScrollToRowIndex(l.MessageInfo.Count - 1, Syncfusion.ListView.XForms.ScrollToPosition.Start);
                });
            }
        }

        protected override void OnDetachingFrom(Syncfusion.ListView.XForms.SfListView bindable)
        {
            ListView = null;
            base.OnDetachingFrom(bindable);
        }


    }

}
