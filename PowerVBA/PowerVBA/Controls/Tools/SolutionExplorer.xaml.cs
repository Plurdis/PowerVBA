﻿using System;
using System.Windows.Input;
using PowerVBA.Core.Connector;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using PowerVBA.Controls.Customize;
using PowerVBA.Resources;
using PowerVBA.Global;
using PowerVBA.Core.Wrap.WrapBase;
using static PowerVBA.Wrap.WrappedClassManager;
using static PowerVBA.Global.Globals;
using System.Windows;

namespace PowerVBA.Controls.Tools
{
    /// <summary>
    /// SolutionExplorer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SolutionExplorer : UserControl
    {
        
        public SolutionExplorer()
        {
            InitializeComponent();

            listBoxes.Add(lbClass);
            listBoxes.Add(lbModule);
            listBoxes.Add(lbBForms);
            listBoxes.Add(lbSlideDoc);


            foreach (ListBox lb in listBoxes)
            {
                lb.MouseDoubleClick += Item_DoubleClick;
            }


            // ContextMenu 초기화


            MenuItem itm1 = new MenuItem();
            MenuItem itm2 = new MenuItem();
            MenuItem itm3 = new MenuItem();

            itm1.Header = "열기";
            itm1.Click += Itm1_Click;
            itm2.Header = "복사";
            itm2.Click += Itm2_Click;
            itm3.Header = "삭제";
            itm3.Click += Itm3_Click;

            itmMenu.Items.Add(itm1);
            itmMenu.Items.Add(itm2);
            itmMenu.Items.Add(itm3);
        }

        public void Reset()
        {
            lbClass.Items.Clear();
            lbModule.Items.Clear();
            lbBForms.Items.Clear();
            lbSlideDoc.Items.Clear();


            classRun.Text = "0";
            moduleRun.Text = "0";
            formRun.Text = "0";
            slideDocRun.Text = "0";
        }



        public List<ListBox> listBoxes = new List<ListBox>();

        ContextMenu itmMenu = new ContextMenu();


        private void Itm1_Click(object sender, RoutedEventArgs e)
        {
            var itm = (ImageListViewItem)GetSelectedItem();
            VBComponentWrappingBase comp = (VBComponentWrappingBase)itm.Tag;
            
            Open?.Invoke(this, comp);
        }

        private void Itm2_Click(object sender, RoutedEventArgs e)
        {
            var itm = (ImageListViewItem)GetSelectedItem();
            VBComponentWrappingBase comp = (VBComponentWrappingBase)itm.Tag;

            Copy?.Invoke(this, comp);
        }
        
        private void Itm3_Click(object sender, RoutedEventArgs e)
        {
            var itm = (ImageListViewItem)GetSelectedItem();
            VBComponentWrappingBase comp = (VBComponentWrappingBase)itm.Tag;
            if (MessageBox.Show($"'{itm.Content}'가 영구적으로 삭제됩니다.", "삭제 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Delete?.Invoke(this, comp);
            }
        }

        public delegate void ComponentDelegate(object sender, VBComponentWrappingBase data);

        public event ComponentDelegate Open;
        public event ComponentDelegate Copy;
        public event ComponentDelegate Delete;

        public event BlankDelegate OpenProperty;
        public event BlankDelegate OpenObjectBrowser;
        public event BlankDelegate OpenShapeExplorer;

        private void Item_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            object source = (((FrameworkElement)e.OriginalSource).TemplatedParent);
            if (source.GetType() == typeof(ContentPresenter)) source = ((ContentPresenter)source).TemplatedParent;

            ImageListViewItem itm = (ImageListViewItem)source;
            VBComponentWrappingBase comp = (VBComponentWrappingBase)itm.Tag;


            Open?.Invoke(this, comp);
        }

        bool handled = false;

        private void ListBoxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handled)
            {
                handled = false;
                return;
            }

            foreach (ListBox lb in listBoxes)
            {
                if (sender == lb) continue;

                if (lb.SelectedIndex != -1)
                {
                    handled = true;
                    lb.SelectedIndex = -1;
                }
            }
        }

        private ListBoxItem GetSelectedItem()
        {
            foreach (ListBox lb in listBoxes)
            {
                foreach(ListBoxItem lbItm in lb.Items)
                {
                    if (lbItm.IsSelected) return lbItm;
                }
            }
            return null;
        }

        private void AddItem(VBComponentWrappingBase comp)
        {

            var t = comp.GetComponentType();

            ListBox AddLB = null;
            ImageSource img = null;
            
            switch (t)
            {
                case 1:
                    AddLB = lbClass; img = ResourceImage.GetIconImage("ClassIcon");
                    break;
                case 2:
                    AddLB = lbModule; img = ResourceImage.GetIconImage("ModuleIcon");
                    break;
                case 3:
                    AddLB = lbBForms; img = ResourceImage.GetIconImage("FormIcon");
                    break;
                case 4:
                    AddLB = lbSlideDoc; img = ResourceImage.GetIconImage("ClassIcon");
                    break;
            }

            var item = new ImageListViewItem() { Content = $"{comp.ToVBComponent2013().Name}{comp.GetExtension}", Tag = comp, Source = img, ContextMenu = itmMenu };
            item.KeyDown += Item_KeyDown;
            AddLB?.Items.Add(item);
        }

        private void Item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Itm3_Click(sender, e);
            }
        }

        private void RemoveItem(VBComponentWrappingBase comp)
        {
            
            foreach(ListBox lb in listBoxes)
            {
                foreach(ImageListViewItem itm in lb.Items)
                {
                    if (itm.Tag == comp)
                    {
                        lb.Items.Remove(lb.Items.Cast<ImageListViewItem>().Where(i => i.Tag == comp).First());
                        return;            
                    }
                }
            }

        }

        public void UpdateSlide(PPTConnectorBase pptConn)
        {
            btnOpenShapeExplorer.Text = $"도형 탐색기 ({pptConn.Slide} 슬라이드)";
        }

        public void Update(PPTConnectorBase pptConn)
        {
            IEnumerable<VBComponentWrappingBase> addComp = new List<VBComponentWrappingBase>();
            IEnumerable<VBComponentWrappingBase> removeComp = new List<VBComponentWrappingBase>();
            
            var LocalItm = lbClass.Items.Cast<ImageListViewItem>()
                .Concat(lbBForms.Items.Cast<ImageListViewItem>())
                .Concat(lbModule.Items.Cast<ImageListViewItem>())
                .Concat(lbSlideDoc.Items.Cast<ImageListViewItem>())
                .Select(i => (VBComponentWrappingBase)i.Tag);


            // 버전별 분류
            IEnumerable<VBComponentWrappingBase> PPTItm = null;

            PPTItm = pptConn.GetFiles();
            
            addComp = PPTItm.Where((i) => !LocalItm.Contains(i)).Copy();
            removeComp = LocalItm.Where(i => !PPTItm.Contains(i)).Copy();

            foreach(var itm in addComp) AddItem(itm);
            foreach (var itm in removeComp) RemoveItem(itm);
            
            classRun.Text = lbClass.Items.Count.ToString();
            moduleRun.Text = lbModule.Items.Count.ToString();
            formRun.Text = lbBForms.Items.Count.ToString();
            slideDocRun.Text = lbSlideDoc.Items.Count.ToString();
        }

        private void OpenProperty_Click(object sender, MouseButtonEventArgs e)
        {
            OpenProperty?.Invoke();
        }

        private void BtnOpenObjectBrowser_Click(object sender, MouseButtonEventArgs e)
        {
            OpenObjectBrowser?.Invoke();
        }

        private void OpenShapeExplorer_Click(object sender, MouseButtonEventArgs e)
        {
            OpenShapeExplorer?.Invoke();
        }
    }
}
