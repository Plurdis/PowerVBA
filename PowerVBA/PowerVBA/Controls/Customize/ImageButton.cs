﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using static PowerVBA.Global.Globals;
using System.ComponentModel;
using WPFExtension;

namespace PowerVBA.Controls.Customize
{
    [TemplatePart(Name = "SimpleButton", Type = typeof(Border))]
    [TemplatePart(Name = "ExButton", Type = typeof(Border))]
    [TemplatePart(Name = "Image")]
    [ContentProperty("Content")]
    [DefaultEvent("ButtonClick")]
    public class ImageButton : ListViewItem
    {
        public ImageButton()
        {
            this.Style = FindResource("ImageButtonStyle") as Style;
        }

        
        public static DependencyProperty BackImageProperty = DependencyHelper.Register();
        public static DependencyProperty ButtonModeProperty = DependencyHelper.Register();
        public static DependencyProperty TextAlignmentProperty = DependencyHelper.Register(new PropertyMetadata(TextAlignment.Center));

        public ImageSource BackImage
        {
            get => (ImageSource)GetValue(BackImageProperty);
            set => SetValue(BackImageProperty, value);
        }

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        public ButtonModes ButtonMode
        {
            get => (ButtonModes)GetValue(ButtonModeProperty);
            set => SetValue(ButtonModeProperty, value);
        }

        private Border SimpleBtn;
        private Border ExBtn;
        public event SenderEventHandler ButtonClick;
        public event BlankEventHandler ExButtonClicked;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SimpleBtn = GetTemplateChild("SimpleButton") as Border;
            ExBtn = GetTemplateChild("ExButton") as Border;

            SimpleBtn.MouseLeftButtonDown += SimpleBtn_LeftMouseDown;
            SimpleBtn.MouseUp += SimpleBtn_LeftButtonUp;
            if (ExBtn != null)
            {
                ExBtn.MouseLeftButtonDown += ExBtn_LeftButtonDown;
                ExBtn.MouseLeftButtonUp += ExBtn_LeftButtonUp;
            }   
        }

        bool ExDown, SimpleDown;

        private void ExBtn_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExDown = true;
        }

        private void ExBtn_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ExDown && ExButtonClicked != null) ExButtonClicked();
            ExDown = false;
        }



        private void SimpleBtn_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            SimpleDown = true;
        }
        private void SimpleBtn_LeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SimpleDown && ButtonClick != null) ButtonClick(this);
            SimpleDown = false;
        }
        



        public enum ButtonModes
        {
            /// <summary>
            /// 기본 버튼입니다.
            /// </summary>
            Default,
            /// <summary>
            /// 버튼과 자세히 보기 두개의 버튼이 있습니다.
            /// </summary>
            ButtonWithDetails,
            /// <summary>
            /// 오직 이미지만 있습니다.
            /// </summary>
            OnlyImage,
            /// <summary>
            /// 길이가 긴 버튼 형태입니다.
            /// </summary>
            LongWidth,
        }
    }
}
