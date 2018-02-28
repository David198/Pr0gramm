using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Pr0gramm.EventHandlers;
using Pr0gramm.Models.Enums;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Pr0gramm.Controls
{
    public sealed partial class VoteControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler UpVote;
        public event EventHandler DownVote;

        public VoteControl()
        {
            InitializeComponent();
            MainPanel.DataContext = this;
            PlusForeGround = ForegroundBrush;
            MinusForeGround = ForegroundBrush;
        }

        private Brush ForegroundBrush
        {
            get
            {
                if (ThemeSelectorService.Theme == ElementTheme.Dark)
                    return new SolidColorBrush(
                        (Color) Application.Current.Resources["SystemBaseHighColor"]);
                return new SolidColorBrush(
                    (Color) Application.Current.Resources["SystemAltHighColor"]);
            }
        }

        public static readonly DependencyProperty VoteStateProperty = DependencyProperty.Register(
            "VoteState", typeof(Vote), typeof(VoteControl), new PropertyMetadata(default(Vote), VoteStateChanged));

        private static void VoteStateChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((VoteControl) dependencyObject).SetButtonColor((Vote) dependencyPropertyChangedEventArgs.NewValue);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof(Double), typeof(VoteControl), new PropertyMetadata(default(Double)));

        public Double Size
        {
            get { return (Double) GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public Vote VoteState
        {
            get { return (Vote) GetValue(VoteStateProperty); }
            set { SetValue(VoteStateProperty, value); }
        }


        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private void SetButtonColor(Vote newVoteState)
        {
            switch (newVoteState)
            {
                case Vote.Down:
                    MinusForeGround = ForegroundBrush;
                    PlusForeGround = ForegroundBrush;
                    PlusForeGround.Opacity = 0.5;
                    break;
                case Vote.Neutral:
                    MinusForeGround = ForegroundBrush;
                    PlusForeGround = ForegroundBrush;
                    PlusForeGround.Opacity = 1;
                    MinusForeGround.Opacity = 1;
                    break;
                case Vote.Up:
                    PlusForeGround = (Brush) Application.Current.Resources["SystemControlHighlightAccentBrush"];
                    MinusForeGround = ForegroundBrush;
                    MinusForeGround.Opacity = 0.5;
                    break;
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(VoteControl),
            new PropertyMetadata(default(Orientation), OrientationChanged));

        private static void OrientationChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((VoteControl) dependencyObject).OnPropertyChanged(nameof(DynamicMargin));
        }

        private static Brush _plusForeGround;
        private static Brush _minusForeGround;

        public Thickness DynamicMargin
        {
            get
            {
                if (Orientation == Orientation.Horizontal)
                    return new Thickness(5, 0, 0, 0);
                return new Thickness(0, 5, 0, 0);
            }
        }

        public Brush PlusForeGround
        {
            get => _plusForeGround;
            set
            {
                _plusForeGround = value;
                OnPropertyChanged(nameof(PlusForeGround));
            }
        }

        private float RotationCenter => (float) Size / 2;

        public Brush MinusForeGround
        {
            get => _minusForeGround;
            set
            {
                _minusForeGround = value;
                OnPropertyChanged(nameof(MinusForeGround));
            }
        }

        private void UpVotePressed(object sender, RoutedEventArgs e)
        {
            OnUpVote();
            if (VoteState == Vote.Up)
            {
                UpButton.Rotate(720, RotationCenter, RotationCenter).Start();
                DownButton.Rotate(-720, RotationCenter, RotationCenter).Start();
            }

            if (VoteState == Vote.Neutral || VoteState == Vote.Down)
            {
                UpButton.Rotate(-720, RotationCenter, RotationCenter).Start();
            }
        }

        private void DownButtonPressed(object sender, RoutedEventArgs e)
        {
            OnDownVote();
            if (VoteState == Vote.Down)
            {
                DownButton.Rotate(720, RotationCenter, RotationCenter).Start();
                UpButton.Rotate(-720, RotationCenter, RotationCenter).Start();
            }

            if (VoteState == Vote.Neutral || VoteState == Vote.Up)
            {
                DownButton.Rotate(-720, RotationCenter, RotationCenter).Start();
            }
        }

        private void OnDownVote()
        {
            DownVote?.Invoke(this, EventArgs.Empty);
        }

        private void OnUpVote()
        {
            UpVote?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
